#if UNITY_EDITOR
using System;
using System.IO;
using TW08.Audio;
using UnityEditor;
using UnityEngine;

namespace TW08.Editor
{
    internal static class TW08StarterAudioSetup
    {
        internal const string AudioRoot = "Assets/_Project/Audio/GeneratedStarter";
        internal const string DataRoot = "Assets/_Project/Audio/ScriptableObjects/Generated";
        internal const string CatalogPath = DataRoot + "/TW08_AudioCatalog.asset";
        private const int SampleRate = 22050;

        internal static TW08AudioCatalog EnsureAll()
        {
            TW08ProductionSceneUtility.EnsureFolder(AudioRoot);
            TW08ProductionSceneUtility.EnsureFolder(DataRoot);

            AudioClip ui = EnsureTone("ui_confirm.wav", 0.08f, 660f, 990f, 0.28f, false);
            AudioClip step = EnsureTone("puzzle_step.wav", 0.07f, 110f, 88f, 0.22f, true);
            AudioClip push = EnsureTone("puzzle_push.wav", 0.13f, 95f, 52f, 0.38f, true);
            AudioClip success = EnsureTone("puzzle_success.wav", 0.42f, 330f, 990f, 0.30f, false);
            AudioClip error = EnsureTone("puzzle_error.wav", 0.25f, 180f, 70f, 0.32f, true);
            AudioClip countdown = EnsureTone("race_countdown.wav", 0.10f, 440f, 440f, 0.28f, false);
            AudioClip finish = EnsureTone("race_finish.wav", 0.52f, 392f, 1176f, 0.30f, false);

            AudioClip menuMusicClip = EnsureMusic("music_menu.wav", 55f, 0.095f, 72f);
            AudioClip puzzleMusicClip = EnsureMusic("music_puzzle.wav", 44f, 0.080f, 60f);
            AudioClip raceMusicClip = EnsureMusic("music_race.wav", 110f, 0.10f, 140f);

            AudioEvent uiEvent = EnsureEvent("UI_Confirm", ui, 0.75f, 0.95f, 1.04f);
            AudioEvent stepEvent = EnsureEvent("Puzzle_Step", step, 0.62f, 0.94f, 1.06f);
            AudioEvent pushEvent = EnsureEvent("Puzzle_Push", push, 0.82f, 0.92f, 1.03f);
            AudioEvent successEvent = EnsureEvent("Puzzle_Success", success, 0.88f, 1f, 1f);
            AudioEvent errorEvent = EnsureEvent("Puzzle_Error", error, 0.80f, 0.96f, 1f);
            AudioEvent countdownEvent = EnsureEvent("Race_Countdown", countdown, 0.82f, 1f, 1f);
            AudioEvent finishEvent = EnsureEvent("Race_Finish", finish, 0.88f, 1f, 1f);

            MusicTrack menuMusic = EnsureMusicTrack("Music_Menu", menuMusicClip, 0.58f);
            MusicTrack puzzleMusic = EnsureMusicTrack("Music_Puzzle", puzzleMusicClip, 0.48f);
            MusicTrack raceMusic = EnsureMusicTrack("Music_Race", raceMusicClip, 0.55f);

            TW08AudioCatalog catalog = AssetDatabase.LoadAssetAtPath<TW08AudioCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<TW08AudioCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            SerializedObject s = new(catalog);
            SetObject(s, "uiConfirm", uiEvent);
            SetObject(s, "puzzleStep", stepEvent);
            SetObject(s, "puzzlePush", pushEvent);
            SetObject(s, "puzzleSuccess", successEvent);
            SetObject(s, "puzzleError", errorEvent);
            SetObject(s, "raceCountdown", countdownEvent);
            SetObject(s, "raceFinish", finishEvent);
            SetObject(s, "menuMusic", menuMusic);
            SetObject(s, "puzzleMusic", puzzleMusic);
            SetObject(s, "raceMusic", raceMusic);
            s.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return catalog;
        }

        private static AudioClip EnsureTone(
            string fileName,
            float duration,
            float startFrequency,
            float endFrequency,
            float amplitude,
            bool addNoise)
        {
            string path = AudioRoot + "/" + fileName;
            int count = Mathf.Max(64, Mathf.RoundToInt(duration * SampleRate));
            float[] samples = new float[count];
            System.Random random = new(fileName.GetHashCode());
            double phase = 0d;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Mathf.Max(1, count - 1);
                float frequency = Mathf.Lerp(startFrequency, endFrequency, t);
                phase += Math.PI * 2d * frequency / SampleRate;
                float attack = Mathf.Clamp01(t / 0.06f);
                float release = Mathf.Clamp01((1f - t) / 0.22f);
                float envelope = Mathf.Min(attack, release);
                float signal = Mathf.Sin((float)phase) * amplitude;
                if (addNoise)
                {
                    signal += ((float)random.NextDouble() * 2f - 1f) * amplitude * 0.18f * (1f - t);
                }
                samples[i] = Mathf.Clamp(signal * envelope, -1f, 1f);
            }
            WriteWave(path, samples);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }

        private static AudioClip EnsureMusic(string fileName, float rootFrequency, float amplitude, float bpm)
        {
            string path = AudioRoot + "/" + fileName;
            const float duration = 8f;
            int count = Mathf.RoundToInt(duration * SampleRate);
            float[] samples = new float[count];
            float beatSeconds = 60f / bpm;
            for (int i = 0; i < count; i++)
            {
                float time = i / (float)SampleRate;
                float beatPhase = (time % beatSeconds) / beatSeconds;
                float pulse = Mathf.Lerp(1f, 0.45f, Mathf.Clamp01(beatPhase * 4f));
                float bass = Mathf.Sin(2f * Mathf.PI * rootFrequency * time);
                float fifth = Mathf.Sin(2f * Mathf.PI * rootFrequency * 1.5f * time) * 0.35f;
                float octave = Mathf.Sin(2f * Mathf.PI * rootFrequency * 2f * time) * 0.18f;
                float texture = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * rootFrequency * 0.5f * time)) * 0.08f;
                samples[i] = Mathf.Clamp((bass + fifth + octave + texture) * amplitude * pulse, -0.75f, 0.75f);
            }
            WriteWave(path, samples);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }

        private static AudioEvent EnsureEvent(string id, AudioClip clip, float volume, float pitchMin, float pitchMax)
        {
            string path = DataRoot + "/AudioEvent_" + id + ".asset";
            AudioEvent audioEvent = AssetDatabase.LoadAssetAtPath<AudioEvent>(path);
            if (audioEvent == null)
            {
                audioEvent = ScriptableObject.CreateInstance<AudioEvent>();
                AssetDatabase.CreateAsset(audioEvent, path);
            }

            SerializedObject s = new(audioEvent);
            s.FindProperty("eventId").stringValue = id;
            SerializedProperty clips = s.FindProperty("clips");
            clips.arraySize = clip != null ? 1 : 0;
            if (clip != null) clips.GetArrayElementAtIndex(0).objectReferenceValue = clip;
            s.FindProperty("volumeRange").vector2Value = new Vector2(volume * 0.94f, volume);
            s.FindProperty("pitchRange").vector2Value = new Vector2(pitchMin, pitchMax);
            s.FindProperty("spatialBlend").floatValue = 0f;
            s.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(audioEvent);
            return audioEvent;
        }

        private static MusicTrack EnsureMusicTrack(string id, AudioClip clip, float volume)
        {
            string path = DataRoot + "/MusicTrack_" + id + ".asset";
            MusicTrack track = AssetDatabase.LoadAssetAtPath<MusicTrack>(path);
            if (track == null)
            {
                track = ScriptableObject.CreateInstance<MusicTrack>();
                AssetDatabase.CreateAsset(track, path);
            }

            SerializedObject s = new(track);
            s.FindProperty("trackId").stringValue = id;
            s.FindProperty("clip").objectReferenceValue = clip;
            s.FindProperty("volume").floatValue = volume;
            s.FindProperty("loop").boolValue = true;
            s.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(track);
            return track;
        }

        private static void SetObject(SerializedObject serialized, string propertyName, UnityEngine.Object value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property != null) property.objectReferenceValue = value;
        }

        private static void WriteWave(string assetPath, float[] samples)
        {
            string absolute = ToAbsoluteAssetPath(assetPath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolute) ?? Application.dataPath);
            using FileStream stream = File.Create(absolute);
            using BinaryWriter writer = new(stream);
            int dataLength = samples.Length * 2;
            writer.Write(new[] { 'R', 'I', 'F', 'F' });
            writer.Write(36 + dataLength);
            writer.Write(new[] { 'W', 'A', 'V', 'E' });
            writer.Write(new[] { 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(SampleRate);
            writer.Write(SampleRate * 2);
            writer.Write((short)2);
            writer.Write((short)16);
            writer.Write(new[] { 'd', 'a', 't', 'a' });
            writer.Write(dataLength);
            foreach (float sample in samples)
            {
                short pcm = (short)Mathf.RoundToInt(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
                writer.Write(pcm);
            }
            writer.Flush();
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        }

        private static string ToAbsoluteAssetPath(string assetPath)
        {
            string relative = assetPath.Substring("Assets/".Length).Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(Application.dataPath, relative);
        }
    }
}
#endif
