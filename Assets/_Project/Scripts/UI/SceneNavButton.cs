using TW08.Core;
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

        public void Configure(string targetSceneName)
        {
            sceneName = targetSceneName;
        }

        public void Navigate()
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogWarning("SceneNavButton sem cena configurada.", this);
                return;
            }

            SceneLoader.TryLoadImmediate(sceneName, sceneName);
        }
    }
}
