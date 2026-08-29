using TW08.UI.Menus;
using UnityEngine;

namespace TW08.UI
{
    /// <summary>
    /// Botão de navegação simples entre cenas de menu.
    /// Ligado via UnityEvent persistente pelos builders de cena do Editor.
    /// </summary>
    public sealed class SceneNavButton : MonoBehaviour
    {
        [SerializeField] private string sceneName = string.Empty;
        [SerializeField] private string context = string.Empty;

        public void Configure(string targetSceneName, string contextLabel = null)
        {
            sceneName = targetSceneName;
            context = string.IsNullOrWhiteSpace(contextLabel) ? targetSceneName : contextLabel;
        }

        public void Navigate()
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogWarning("SceneNavButton sem cena configurada.", this);
                return;
            }

            MenuFeedback.Click(this);
            MenuTransition.Go(sceneName, string.IsNullOrWhiteSpace(context) ? sceneName : context);
        }
    }
}
