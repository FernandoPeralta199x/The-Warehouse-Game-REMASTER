#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using TW08.Presentation;
using UnityEditor;
using UnityEngine;

namespace TW08.Editor
{
    public static class TW08ExpansionStarterArt
    {
        public const string Root = "Assets/_Project/Art/Production/GeneratedExpansion";
        public const string DudaSpriteSetPath = "Assets/_Project/ScriptableObjects/Art/Duda_DirectionalSpriteSet.asset";
        private const float PixelsPerUnit = 32f;

        public static void EnsureAll()
        {
            EnsureFolder(Root);
            EnsureFolder(Root + "/Characters");
            EnsureFolder(Root + "/Characters/Duda");
            EnsureFolder(Root + "/Portraits");
            EnsureFolder(Root + "/Race");

            foreach (FacingDirection direction in Enum.GetValues(typeof(FacingDirection)))
            {
                string key = direction.ToString();
                WriteSprite(DudaPath(key + "_Idle"), 32, 48, texture => DrawDuda(texture, direction, 0), new Vector2(0.5f, 0.06f));
                WriteSprite(DudaPath(key + "_Walk01"), 32, 48, texture => DrawDuda(texture, direction, 1), new Vector2(0.5f, 0.06f));
                WriteSprite(DudaPath(key + "_Walk02"), 32, 48, texture => DrawDuda(texture, direction, 2), new Vector2(0.5f, 0.06f));
                WriteSprite(DudaPath(key + "_Walk03"), 32, 48, texture => DrawDuda(texture, direction, 3), new Vector2(0.5f, 0.06f));
            }

            WriteSprite(Root + "/Portraits/John.png", 96, 112, texture => DrawPortrait(texture, Rgb(222, 143, 29), Rgb(67, 48, 32), false), new Vector2(0.5f, 0.08f));
            WriteSprite(Root + "/Portraits/Duda.png", 96, 112, texture => DrawPortrait(texture, Rgb(231, 155, 34), Rgb(70, 39, 25), true), new Vector2(0.5f, 0.08f));
            WriteSprite(Root + "/Portraits/Robert.png", 96, 112, texture => DrawPortrait(texture, Rgb(185, 111, 31), Rgb(52, 43, 38), false), new Vector2(0.5f, 0.08f));

            WriteSprite(Root + "/Race/TrackFloor.png", 32, 32, DrawTrackFloor, new Vector2(0.5f, 0.5f));
            WriteSprite(Root + "/Race/Barrier.png", 32, 32, DrawBarrier, new Vector2(0.5f, 0.5f));
            WriteSprite(Root + "/Race/Checkpoint.png", 32, 32, DrawCheckpoint, new Vector2(0.5f, 0.5f));
            WriteSprite(Root + "/Race/Boost.png", 32, 32, DrawBoost, new Vector2(0.5f, 0.5f));
            WriteSprite(Root + "/Race/Ice.png", 32, 32, DrawIce, new Vector2(0.5f, 0.5f));
            WriteSprite(Root + "/Race/Oil.png", 32, 32, DrawOil, new Vector2(0.5f, 0.5f));
            WriteSprite(Root + "/Race/Forklift_John.png", 32, 48, texture => DrawForklift(texture, Rgb(224, 139, 19)), new Vector2(0.5f, 0.5f));
            WriteSprite(Root + "/Race/Forklift_Duda.png", 32, 48, texture => DrawForklift(texture, Rgb(38, 164, 181)), new Vector2(0.5f, 0.5f));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        }

        public static DirectionalSpriteSet EnsureDudaSpriteSet()
        {
            EnsureAll();
            EnsureFolder("Assets/_Project/ScriptableObjects/Art");
            DirectionalSpriteSet set = AssetDatabase.LoadAssetAtPath<DirectionalSpriteSet>(DudaSpriteSetPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<DirectionalSpriteSet>();
                AssetDatabase.CreateAsset(set, DudaSpriteSetPath);
            }

            SerializedObject serialized = new(set);
            Assign(serialized.FindProperty("idleDown"), DudaPath("Down_Idle"));
            Assign(serialized.FindProperty("idleUp"), DudaPath("Up_Idle"));
            Assign(serialized.FindProperty("idleLeft"), DudaPath("Left_Idle"));
            Assign(serialized.FindProperty("idleRight"), DudaPath("Right_Idle"));
            AssignArray(serialized.FindProperty("walkDown"), "Down");
            AssignArray(serialized.FindProperty("walkUp"), "Up");
            AssignArray(serialized.FindProperty("walkLeft"), "Left");
            AssignArray(serialized.FindProperty("walkRight"), "Right");
            SerializedProperty fps = serialized.FindProperty("framesPerSecond");
            if (fps != null) fps.floatValue = 8f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets();
            return set;
        }

        public static Sprite LoadPortrait(string characterId)
        {
            string file = characterId == "duda" ? "Duda" : characterId == "robert" ? "Robert" : "John";
            return AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Portraits/" + file + ".png");
        }

        public static Sprite LoadRaceSprite(string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Race/" + fileName + ".png");
        }

        private static string DudaPath(string name) => Root + "/Characters/Duda/Duda_" + name + ".png";

        private static void Assign(SerializedProperty property, string path)
        {
            if (property != null) property.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void AssignArray(SerializedProperty property, string direction)
        {
            if (property == null) return;
            property.arraySize = 3;
            property.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(DudaPath(direction + "_Walk01"));
            property.GetArrayElementAtIndex(1).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(DudaPath(direction + "_Walk02"));
            property.GetArrayElementAtIndex(2).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(DudaPath(direction + "_Walk03"));
        }

        private static void DrawDuda(Texture2D t, FacingDirection direction, int phase)
        {
            Color outline = Rgb(36, 29, 25);
            Color cap = Rgb(48, 54, 61);
            Color skin = Rgb(218, 145, 84);
            Color hair = Rgb(75, 43, 28);
            Color shirt = Rgb(225, 146, 27);
            Color vest = Rgb(38, 75, 83);
            Color pants = Rgb(43, 49, 57);
            Color boot = Rgb(91, 58, 31);
            Color glove = Rgb(66, 47, 35);
            int stride = phase == 1 ? -2 : phase == 3 ? 2 : 0;

            Fill(t, 8, 2, 16, 3, new Color(0f, 0f, 0f, 0.3f));
            Fill(t, 8 + stride, 5, 6, 6, boot);
            Fill(t, 18 - stride, 5, 6, 6, boot);
            Fill(t, 8 + stride, 10, 7, 10, pants);
            Fill(t, 17 - stride, 10, 7, 10, pants);
            Fill(t, 7, 20, 18, 15, outline);
            Fill(t, 8, 21, 16, 13, shirt);
            Fill(t, 12, 21, 8, 13, vest);
            Fill(t, 4, 22 - stride, 5, 10, glove);
            Fill(t, 23, 22 + stride, 5, 10, glove);

            if (direction == FacingDirection.Up)
            {
                Fill(t, 10, 32, 12, 9, hair);
                Fill(t, 9, 40, 14, 6, cap);
                Fill(t, 8, 35, 3, 7, hair);
                Fill(t, 21, 35, 3, 7, hair);
            }
            else if (direction == FacingDirection.Left || direction == FacingDirection.Right)
            {
                bool right = direction == FacingDirection.Right;
                Fill(t, 10, 32, 12, 9, skin);
                Fill(t, right ? 9 : 20, 33, 3, 8, hair);
                Fill(t, 9, 40, 14, 6, cap);
                Fill(t, right ? 21 : 5, 39, 7, 3, cap);
                SetSafe(t, right ? 18 : 13, 37, outline);
            }
            else
            {
                Fill(t, 10, 32, 12, 9, skin);
                Fill(t, 9, 33, 3, 8, hair);
                Fill(t, 20, 33, 3, 8, hair);
                Fill(t, 9, 40, 14, 6, cap);
                Fill(t, 12, 42, 8, 2, Rgb(236, 157, 30));
                SetSafe(t, 13, 37, outline);
                SetSafe(t, 18, 37, outline);
            }
        }

        private static void DrawPortrait(Texture2D t, Color uniform, Color hair, bool female)
        {
            Fill(t, 14, 4, 68, 5, new Color(0f, 0f, 0f, 0.28f));
            Fill(t, 20, 10, 56, 50, Rgb(38, 42, 48));
            Fill(t, 24, 14, 48, 42, uniform);
            Fill(t, 29, 52, 38, 38, Rgb(218, 145, 82));
            Fill(t, 26, 83, 44, 15, Rgb(50, 56, 63));
            Fill(t, 35, 93, 26, 5, Rgb(232, 150, 24));
            Fill(t, 27, 57, 8, 31, hair);
            Fill(t, 61, 57, 8, 31, hair);
            if (!female) Fill(t, 34, 53, 28, 9, hair);
            SetSafe(t, 40, 72, Rgb(27, 23, 20));
            SetSafe(t, 55, 72, Rgb(27, 23, 20));
        }

        private static void DrawTrackFloor(Texture2D t)
        {
            Fill(t, 0, 0, 32, 32, Rgb(31, 35, 38));
            Fill(t, 1, 1, 30, 30, Rgb(39, 44, 47));
            Fill(t, 3, 15, 26, 2, Rgb(71, 76, 77));
            Fill(t, 4, 4, 2, 2, Rgb(86, 91, 92));
            Fill(t, 26, 26, 2, 2, Rgb(86, 91, 92));
        }

        private static void DrawBarrier(Texture2D t)
        {
            Fill(t, 0, 0, 32, 32, Rgb(24, 27, 29));
            Fill(t, 2, 5, 28, 22, Rgb(65, 70, 73));
            for (int x = -4; x < 36; x += 8)
            {
                Fill(t, x, 8, 4, 16, Rgb(230, 145, 21));
            }
        }

        private static void DrawCheckpoint(Texture2D t)
        {
            Fill(t, 0, 0, 32, 32, new Color(0f, 0f, 0f, 0f));
            for (int y = 4; y < 28; y += 4)
            {
                Fill(t, 2, y, 4, 2, Rgb(63, 230, 141));
                Fill(t, 26, y, 4, 2, Rgb(63, 230, 141));
            }
            Fill(t, 5, 15, 22, 2, Rgb(63, 230, 141));
        }

        private static void DrawBoost(Texture2D t)
        {
            Fill(t, 2, 2, 28, 28, Rgb(23, 55, 61));
            Fill(t, 5, 5, 22, 22, Rgb(27, 109, 126));
            Fill(t, 7, 14, 18, 4, Rgb(75, 226, 244));
            Fill(t, 16, 8, 9, 4, Rgb(75, 226, 244));
            Fill(t, 16, 20, 9, 4, Rgb(75, 226, 244));
        }

        private static void DrawIce(Texture2D t)
        {
            Fill(t, 0, 0, 32, 32, Rgb(31, 55, 66));
            Fill(t, 2, 2, 28, 28, Rgb(77, 148, 171));
            Fill(t, 4, 23, 18, 2, Rgb(174, 229, 239));
            Fill(t, 12, 8, 16, 2, Rgb(174, 229, 239));
            Fill(t, 7, 14, 2, 10, Rgb(130, 205, 221));
        }

        private static void DrawOil(Texture2D t)
        {
            Fill(t, 5, 6, 22, 18, new Color(0.04f, 0.04f, 0.05f, 0.9f));
            Fill(t, 10, 3, 12, 5, new Color(0.04f, 0.04f, 0.05f, 0.9f));
            Fill(t, 8, 20, 6, 6, new Color(0.12f, 0.12f, 0.15f, 0.8f));
        }

        private static void DrawForklift(Texture2D t, Color accent)
        {
            Color dark = Rgb(32, 35, 38);
            Color steel = Rgb(74, 79, 80);
            Fill(t, 7, 5, 18, 32, dark);
            Fill(t, 9, 9, 14, 22, accent);
            Fill(t, 10, 24, 12, 9, steel);
            Fill(t, 5, 7, 4, 9, Rgb(19, 20, 22));
            Fill(t, 23, 7, 4, 9, Rgb(19, 20, 22));
            Fill(t, 5, 27, 4, 9, Rgb(19, 20, 22));
            Fill(t, 23, 27, 4, 9, Rgb(19, 20, 22));
            Fill(t, 12, 36, 3, 10, steel);
            Fill(t, 18, 36, 3, 10, steel);
            Fill(t, 11, 43, 11, 2, Rgb(226, 146, 21));
        }

        private static void WriteSprite(string assetPath, int width, int height, Action<Texture2D> draw, Vector2 pivot)
        {
            string absolute = ToAbsoluteAssetPath(assetPath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolute) ?? Application.dataPath);
            Texture2D texture = new(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            Clear(texture);
            draw(texture);
            texture.Apply(false, false);
            File.WriteAllBytes(absolute, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            ConfigureImporter(assetPath, pivot);
        }

        private static void ConfigureImporter(string assetPath, Vector2 pivot)
        {
            if (AssetImporter.GetAtPath(assetPath) is not TextureImporter importer) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.wrapMode = TextureWrapMode.Clamp;
            TextureImporterSettings settings = new();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private static void Clear(Texture2D texture)
        {
            Color[] pixels = new Color[texture.width * texture.height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
            texture.SetPixels(pixels);
        }

        private static void Fill(Texture2D t, int x, int y, int width, int height, Color color)
        {
            for (int py = y; py < y + height; py++)
            for (int px = x; px < x + width; px++)
                SetSafe(t, px, py, color);
        }

        private static void SetSafe(Texture2D t, int x, int y, Color color)
        {
            if (x >= 0 && y >= 0 && x < t.width && y < t.height) t.SetPixel(x, y, color);
        }

        private static Color Rgb(byte r, byte g, byte b) => new Color32(r, g, b, 255);

        private static string ToAbsoluteAssetPath(string assetPath)
        {
            string relative = assetPath.Substring("Assets/".Length).Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(Application.dataPath, relative);
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
