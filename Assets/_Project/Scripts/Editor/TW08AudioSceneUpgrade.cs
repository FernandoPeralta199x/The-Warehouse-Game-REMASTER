#if UNITY_EDITOR
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
            if (catalog == null) return;
            if (menuPaths != null)
            {
                foreach (string path in menuPaths) AttachMusic(path, catalog.MenuMusic);
            }
            if (puzzlePaths != null)
            {
                foreach (string path in puzzlePaths) AttachPuzzle(path, catalog);
            }
            if (racePaths != null)
            {
                foreach (string path in racePaths) AttachRace(path, catalog);
            }
        }

        private static void AttachMusic(string path, MusicTrack track)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) return;
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            SceneMusicPresenter presenter = Object.FindFirstObjectByType<SceneMusicPresenter>()
                ?? new GameObject("Scene Music").AddComponent<SceneMusicPresenter>();
            presenter.Configure(track);
            EditorUtility.SetDirty(presenter);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void AttachPuzzle(string path, TW08AudioCatalog catalog)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) return;
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            PuzzleRuntime runtime = Object.FindFirstObjectByType<PuzzleRuntime>();
            if (runtime == null) return;
            PuzzleAudioFeedback feedback = runtime.GetComponent<PuzzleAudioFeedback>() ?? runtime.gameObject.AddComponent<PuzzleAudioFeedback>();
            feedback.Configure(runtime, catalog);
            SceneMusicPresenter music = Object.FindFirstObjectByType<SceneMusicPresenter>() ?? new GameObject("Scene Music").AddComponent<SceneMusicPresenter>();
            music.Configure(catalog.PuzzleMusic);
            EditorUtility.SetDirty(feedback);
            EditorUtility.SetDirty(music);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void AttachRace(string path, TW08AudioCatalog catalog)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) return;
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            RaceSessionController session = Object.FindFirstObjectByType<RaceSessionController>();
            if (session == null) return;
            RaceAudioFeedback feedback = session.GetComponent<RaceAudioFeedback>() ?? session.gameObject.AddComponent<RaceAudioFeedback>();
            feedback.Configure(session, catalog);
            SceneMusicPresenter music = Object.FindFirstObjectByType<SceneMusicPresenter>() ?? new GameObject("Scene Music").AddComponent<SceneMusicPresenter>();
            music.Configure(catalog.RaceMusic);
            EditorUtility.SetDirty(feedback);
            EditorUtility.SetDirty(music);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);
        }
    }
}
#endif
