using TW08.Audio;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TW08.UI.Menus
{
    /// <summary>
    /// Som de navegação de menu: um toque curto sempre que o foco muda.
    ///
    /// Escuta o EventSystem em vez de cada botão porque o foco muda por teclado,
    /// gamepad e mouse, e os controladores de seleção chamam
    /// <c>RemoveAllListeners</c> ao religar a tela — um listener por botão seria
    /// apagado.
    ///
    /// O primeiro foco da tela não soa: ele acontece sozinho na abertura, e um
    /// bipe sem ação do jogador soa como erro.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MenuNavigationAudio : MonoBehaviour
    {
        private GameObject lastSelected;
        private bool primed;

        private void OnEnable()
        {
            primed = false;
            lastSelected = null;
        }

        private void Update()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                return;
            }

            GameObject selected = eventSystem.currentSelectedGameObject;
            if (selected == lastSelected)
            {
                return;
            }

            lastSelected = selected;

            if (!primed)
            {
                primed = true;
                return;
            }

            if (selected != null)
            {
                GameAudio.Focus();
            }
        }
    }
}
