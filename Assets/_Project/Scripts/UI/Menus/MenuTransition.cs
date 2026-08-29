using System.Collections.Generic;
using TW08.Core;
using TW08.Motion;
using UnityEngine;
using UnityEngine.UI;

namespace TW08.UI.Menus
{
    /// <summary>
    /// Saída animada de um menu seguida da troca de cena.
    ///
    /// A contagem até o carregamento roda no <c>Update</c> deste runner e não numa
    /// corrotina do serviço de movimento: se a animação for interrompida (Kill,
    /// objeto destruído, runner do UIMotion perdido), o jogador ainda navega.
    /// Nenhum menu pode prender o jogador numa tela que não avança.
    ///
    /// O host é <c>DontDestroyOnLoad</c> porque ele precisa sobreviver ao próprio
    /// carregamento que dispara.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MenuTransition : MonoBehaviour
    {
        /// <summary>Duração padrão da saída. Curta de propósito: menu não é cutscene.</summary>
        public const float DefaultExitDuration = 0.24f;

        private const float ExitSlide = 34f;

        private static MenuTransition active;

        private readonly List<CanvasGroup> dimmedGroups = new();
        private readonly List<RectTransform> movedRects = new();
        private readonly List<Vector2> movedOrigins = new();
        private readonly List<GraphicRaycaster> blockedRaycasters = new();
        private readonly List<MotionHandle> handles = new();

        private string targetScene;
        private string contextLabel;
        private float remaining;
        private bool fired;

        /// <summary>True enquanto uma saída de menu estiver em andamento.</summary>
        public static bool IsTransitioning => active != null;

        /// <summary>Regra pura de destino válido — usada pelos menus antes de animar.</summary>
        public static bool IsValidDestination(string sceneName)
        {
            return !string.IsNullOrWhiteSpace(sceneName);
        }

        /// <summary>
        /// Fecha o menu com animação e carrega <paramref name="sceneName"/>.
        /// Devolve false quando o destino é inválido ou já existe uma saída rodando.
        /// </summary>
        public static bool Go(string sceneName, string context = "menu", float duration = DefaultExitDuration)
        {
            if (!IsValidDestination(sceneName))
            {
                Debug.LogWarning($"MenuTransition: destino de cena vazio para '{context}'.");
                return false;
            }

            if (active != null)
            {
                // Segunda confirmação no mesmo frame (mouse + gamepad, por exemplo).
                return false;
            }

            // Fora do Play Mode não há loop de frames: navegar direto mantém os
            // builders e ferramentas de editor funcionando.
            if (!Application.isPlaying)
            {
                return SceneLoader.TryLoadImmediate(sceneName, context);
            }

            GameObject host = new("TW08 Menu Transition");
            DontDestroyOnLoad(host);
            MenuTransition transition = host.AddComponent<MenuTransition>();
            active = transition;
            transition.Begin(sceneName, context, Mathf.Max(0f, duration));
            return true;
        }

        private void Begin(string sceneName, string context, float duration)
        {
            targetScene = sceneName;
            contextLabel = context;
            remaining = duration;

            if (duration <= 0f)
            {
                return;
            }

            foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (canvas == null || canvas.transform.parent != null)
                {
                    continue;
                }

                if (canvas.TryGetComponent(out GraphicRaycaster raycaster) && raycaster.enabled)
                {
                    raycaster.enabled = false;
                    blockedRaycasters.Add(raycaster);
                }

                FadeCanvasChildren(canvas, duration);
            }
        }

        private void FadeCanvasChildren(Canvas canvas, float duration)
        {
            foreach (Transform child in canvas.transform)
            {
                if (child == null || !child.gameObject.activeSelf)
                {
                    continue;
                }

                // A tela de carregamento é justamente o que deve aparecer quando o
                // resto some — apagá-la junto deixaria o jogador olhando o vazio.
                if (child.GetComponentInChildren<LoadingScreenPresenter>(true) != null)
                {
                    continue;
                }

                CanvasGroup group = child.TryGetComponent(out CanvasGroup existing)
                    ? existing
                    : child.gameObject.AddComponent<CanvasGroup>();
                group.interactable = false;
                group.blocksRaycasts = false;
                dimmedGroups.Add(group);
                handles.Add(UIMotion.FadeTo(group, 0f, duration, Ease.InQuad));

                if (child is not RectTransform rect)
                {
                    continue;
                }

                // Painéis que cobrem a tela inteira (fundo, grade do terminal) não
                // deslizam: revelariam uma faixa vazia na borda.
                if (rect.anchorMin == Vector2.zero && rect.anchorMax == Vector2.one)
                {
                    continue;
                }

                movedRects.Add(rect);
                movedOrigins.Add(rect.anchoredPosition);
                handles.Add(UIMotion.MoveTo(
                    rect, rect.anchoredPosition + new Vector2(0f, -ExitSlide), duration, Ease.InQuad));
            }
        }

        private void Update()
        {
            if (fired)
            {
                return;
            }

            remaining -= Time.unscaledDeltaTime;
            if (remaining > 0f)
            {
                return;
            }

            Fire();
        }

        private void Fire()
        {
            fired = true;
            if (active == this)
            {
                active = null;
            }

            if (SceneLoader.TryLoadImmediate(targetScene, contextLabel))
            {
                Destroy(gameObject);
                return;
            }

            // Cena não registrada: devolver o menu ao jogador é obrigatório. Uma
            // tela apagada sem navegação encerraria a sessão na prática.
            RestoreMenu();
            Destroy(gameObject);
        }

        private void RestoreMenu()
        {
            foreach (MotionHandle handle in handles)
            {
                handle?.Kill();
            }

            handles.Clear();

            foreach (CanvasGroup group in dimmedGroups)
            {
                if (group == null)
                {
                    continue;
                }

                group.alpha = 1f;
                group.interactable = true;
                group.blocksRaycasts = true;
            }

            for (int i = 0; i < movedRects.Count && i < movedOrigins.Count; i++)
            {
                if (movedRects[i] != null)
                {
                    movedRects[i].anchoredPosition = movedOrigins[i];
                }
            }

            foreach (GraphicRaycaster raycaster in blockedRaycasters)
            {
                if (raycaster != null)
                {
                    raycaster.enabled = true;
                }
            }

            blockedRaycasters.Clear();
        }

        private void OnDestroy()
        {
            if (active == this)
            {
                active = null;
            }
        }
    }
}
