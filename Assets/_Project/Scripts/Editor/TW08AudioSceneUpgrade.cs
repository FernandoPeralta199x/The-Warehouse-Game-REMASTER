#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TW08.Audio;
using TW08.Puzzle;
using TW08.Race;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TW08.Editor
{
    internal static class TW08AudioSceneUpgrade
    {
        internal static void Apply(
            TW08AudioCatalog catalog,
            IEnumerable<string> menuPaths,
            IEnumerable<string> puzzlePaths,
            IEnumerable<string> racePaths)
        {
            // The caller may hold a catalog reference across scene saves/import callbacks. Treat it as
            // an existence hint only; always reload a live catalog before mutating each generated scene.
            if (catalog == null && LoadCatalog() == null) return;

            if (menuPaths != null)
            {
                foreach (string path in menuPaths)
                {
                    TW08AudioCatalog liveCatalog = RequireCatalog();
                    AttachMusic(path, liveCatalog.MenuMusic);
                }
            }
            if (puzzlePaths != null)
            {
                foreach (string path in puzzlePaths)
                {
                    AttachPuzzle(path, RequireCatalog());
                }
            }
            if (racePaths != null)
            {
                foreach (string path in racePaths)
                {
                    AttachRace(path, RequireCatalog());
                }
            }
        }

        private static TW08AudioCatalog LoadCatalog()
        {
            return AssetDatabase.LoadAssetAtPath<TW08AudioCatalog>(TW08StarterAudioSetup.CatalogPath);
        }

        private static TW08AudioCatalog RequireCatalog()
        {
            TW08AudioCatalog catalog = LoadCatalog();
            if (catalog == null)
            {
                throw new InvalidOperationException(
                    $"TW08 audio catalog could not be reloaded from '{TW08StarterAudioSetup.CatalogPath}' during scene upgrade.");
            }
            return catalog;
        }

        private static void AttachMusic(string path, MusicTrack track)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) return;
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            SceneMusicPresenter presenter = UnityEngine.Object.FindFirstObjectByType<SceneMusicPresenter>();
            if (presenter == null)
            {
                presenter = new GameObject("Scene Music").AddComponent<SceneMusicPresenter>();
            }
            if (presenter == null)
            {
                throw new InvalidOperationException($"TW08 failed to attach SceneMusicPresenter in '{path}'.");
            }

            presenter.Configure(track);
            EditorUtility.SetDirty(presenter);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, path))
            {
                throw new InvalidOperationException($"Unity failed to save menu audio upgrade for '{path}'.");
            }
        }

        private static void AttachPuzzle(string path, TW08AudioCatalog catalog)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) return;
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            PuzzleRuntime runtime = UnityEngine.Object.FindFirstObjectByType<PuzzleRuntime>();
            if (runtime == null) return;

            // PuzzleAudioDirector substitui o feedback legado e cobre tudo que
            // ele cobria. Manter os dois fazia cada passo e cada empurrão tocar
            // duas vezes, com pitch sorteado em separado — e na carga pesada o
            // Director tocava o som grave enquanto o legado tocava o comum, o
            // que transformava a distinção de peso em "mais alto".
            PuzzleAudioDirector director = UnityEngine.Object.FindFirstObjectByType<PuzzleAudioDirector>();
            PuzzleAudioFeedback feedback = runtime.GetComponent<PuzzleAudioFeedback>();

            if (director != null)
            {
                if (feedback != null)
                {
                    UnityEngine.Object.DestroyImmediate(feedback);
                }
            }
            else if (feedback == null)
            {
                feedback = runtime.gameObject.AddComponent<PuzzleAudioFeedback>();
                if (feedback == null)
                {
                    throw new InvalidOperationException($"TW08 failed to attach PuzzleAudioFeedback in '{path}'.");
                }
            }

            SceneMusicPresenter music = UnityEngine.Object.FindFirstObjectByType<SceneMusicPresenter>();
            if (music == null)
            {
                music = new GameObject("Scene Music").AddComponent<SceneMusicPresenter>();
            }
            if (music == null)
            {
                throw new InvalidOperationException($"TW08 failed to attach SceneMusicPresenter in puzzle scene '{path}'.");
            }

            // feedback é nulo quando o Director assumiu a cena — ele cobre tudo
            // que o legado cobria, então não há o que configurar aqui.
            if (feedback != null)
            {
                feedback.Configure(runtime, catalog);
                EditorUtility.SetDirty(feedback);
            }

            music.Configure(catalog.PuzzleMusic);
            EditorUtility.SetDirty(music);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, path))
            {
                throw new InvalidOperationException($"Unity failed to save puzzle audio upgrade for '{path}'.");
            }
        }

        private static void AttachRace(string path, TW08AudioCatalog catalog)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) return;
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            RaceSessionController session = UnityEngine.Object.FindFirstObjectByType<RaceSessionController>();
            if (session == null) return;

            RaceAudioFeedback feedback = session.GetComponent<RaceAudioFeedback>();
            if (feedback == null)
            {
                feedback = session.gameObject.AddComponent<RaceAudioFeedback>();
            }
            if (feedback == null)
            {
                throw new InvalidOperationException($"TW08 failed to attach RaceAudioFeedback in '{path}'.");
            }

            SceneMusicPresenter music = UnityEngine.Object.FindFirstObjectByType<SceneMusicPresenter>();
            if (music == null)
            {
                music = new GameObject("Scene Music").AddComponent<SceneMusicPresenter>();
            }
            if (music == null)
            {
                throw new InvalidOperationException($"TW08 failed to attach SceneMusicPresenter in race scene '{path}'.");
            }

            feedback.Configure(session, catalog);
            music.Configure(catalog.RaceMusic);
            EditorUtility.SetDirty(feedback);
            EditorUtility.SetDirty(music);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, path))
            {
                throw new InvalidOperationException($"Unity failed to save race audio upgrade for '{path}'.");
            }
        }
    }
}
#endif