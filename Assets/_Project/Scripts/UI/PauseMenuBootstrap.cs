using TW08.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TW08.UI
{
    /// <summary>
    /// Instala o menu de pausa em toda cena que tenha entrada de jogo.
    ///
    /// Auto-instalação em vez de fiação por cena: são 49 cenas geradas por
    /// pipeline, e um painel montado à mão em cada uma sairia de sincronia na
    /// primeira mudança de layout. A presença de <see cref="GameInput"/> é o que
    /// distingue cena de jogo de cena de menu — menu não pausa.
    /// </summary>
    public static class PauseMenuBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            InstallInActiveScene();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => InstallInActiveScene();

        private static void InstallInActiveScene()
        {
            if (Object.FindFirstObjectByType<PauseMenuController>() != null)
            {
                return;
            }

            if (Object.FindFirstObjectByType<GameInput>() == null)
            {
                return;
            }

            new GameObject("Pause Menu").AddComponent<PauseMenuController>();
        }
    }
}
