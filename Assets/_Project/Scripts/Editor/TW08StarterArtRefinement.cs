#if UNITY_EDITOR
using System;
using System.IO;
using TW08.Presentation;
using UnityEditor;
using UnityEngine;

namespace TW08.Editor
{
    /// <summary>
    /// Regenerates disposable starter pixel-art in place. These files are intentionally
    /// generated assets: their .meta files/GUIDs stay stable while the PNG payload evolves.
    /// Hand-authored production art must live outside GeneratedStarter.
    /// </summary>
    public static class TW08StarterArtRefinement
    {
        private const string Root = "Assets/_Project/Art/Production/GeneratedStarter";
        private const float PixelsPerUnit = 32f;

        [MenuItem("Tools/TW08/Production/Regenerate Starter Pixel Art")]
        public static void RegenerateFromMenu()
        {
            Regenerate();
            EditorUtility.DisplayDialog(
                "The Warehouse Nº 08 — Starter Art",
                "Starter pixel-art regenerado com os mesmos GUIDs.\n\n" +
                "Piso, parede, alvo, caixa e John foram atualizados. " +
                "Arte manual/final fora de GeneratedStarter não é sobrescrita.",
                "OK");
        }

        public static void Regenerate()
        {
            WriteSprite(Root + "/Environment/Floor_Primary.png", 32, 32, DrawFloorPrimary, new Vector2(0.5f, 0.5f));
            WriteSprite(Root + "/Environment/Floor_Secondary.png", 32, 32, DrawFloorSecondary, new Vector2(0.5f, 0.5f));
            WriteSprite(Root + "/Environment/Wall_N8.png", 32, 32, DrawWall, new Vector2(0.5f, 0.5f));
            WriteSprite(Root + "/Interactive/Goal_N8.png", 32, 32, DrawGoal, new Vector2(0.5f, 0.5f));
            WriteSprite(Root + "/Props/Crate_N8.png", 32, 32, DrawCrate, new Vector2(0.5f, 0.5f));

            foreach (FacingDirection direction in Enum.GetValues(typeof(FacingDirection)))
            {
                string key = direction.ToString();
                WriteJohn(key + "_Idle", direction, 0);
                WriteJohn(key + "_Walk01", direction, 1);
                WriteJohn(key + "_Walk02", direction, 2);
                WriteJohn(key + "_Walk03", direction, 3);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        }

        private static void WriteJohn(string fileName, FacingDirection direction, int phase)
        {
            WriteSprite(
                Root + "/Characters/John/John_" + fileName + ".png",
                32,
                48,
                texture => DrawJohn(texture, direction, phase),
                new Vector2(0.5f, 0.06f));
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
            if (AssetImporter.GetAtPath(assetPath) is not TextureImporter importer)
            {
                return;
            }

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

        private static void DrawFloorPrimary(Texture2D texture)
        {
            Color baseColor = Rgb(25, 31, 34);
            Color plate = Rgb(31, 38, 41);
            Color seam = Rgb(43, 51, 54);
            Color highlight = Rgb(52, 61, 64);

            Fill(texture, 0, 0, 32, 32, baseColor);
            Fill(texture, 1, 1, 30, 30, plate);
            Fill(texture, 31, 0, 1, 32, seam);
            Fill(texture, 0, 31, 32, 1, seam);
            Fill(texture, 2, 29, 28, 1, highlight);
            SetSafe(texture, 4, 4, Rgb(76, 84, 86));
            SetSafe(texture, 27, 27, Rgb(16, 20, 22));
        }

        private static void DrawFloorSecondary(Texture2D texture)
        {
            DrawFloorPrimary(texture);
            Color recess = Rgb(28, 34, 37);
            Color edge = Rgb(39, 47, 50);
            Fill(texture, 10, 10, 12, 12, recess);
            Fill(texture, 10, 21, 12, 1, edge);
            Fill(texture, 21, 10, 1, 12, edge);
        }

        private static void DrawWall(Texture2D texture)
        {
            Color outline = Rgb(12, 16, 18);
            Color frame = Rgb(57, 66, 69);
            Color panel = Rgb(30, 37, 40);
            Color highlight = Rgb(93, 103, 106);
            Color amber = Rgb(235, 148, 20);
            Color hazardDark = Rgb(24, 27, 28);

            Fill(texture, 0, 0, 32, 32, outline);
            Fill(texture, 2, 6, 28, 24, frame);
            Fill(texture, 4, 8, 24, 19, panel);
            Fill(texture, 3, 28, 26, 1, highlight);
            Fill(texture, 0, 0, 32, 6, hazardDark);
            for (int x = -3; x < 35; x += 8)
            {
                Fill(texture, x, 0, 4, 6, amber);
            }

            Bolt(texture, 5, 24);
            Bolt(texture, 25, 24);
            Fill(texture, 7, 10, 18, 1, Rgb(38, 45, 48));
        }

        private static void DrawGoal(Texture2D texture)
        {
            Color glow = new(0.12f, 0.95f, 0.48f, 0.24f);
            Color dark = Rgb(18, 30, 25);
            Color steel = Rgb(48, 72, 61);
            Color green = Rgb(65, 255, 139);
            Color whiteGreen = Rgb(174, 255, 203);

            // Transparent pad: floor remains visible underneath.
            Fill(texture, 5, 9, 22, 14, glow);
            Fill(texture, 8, 6, 16, 20, glow);
            Fill(texture, 7, 9, 18, 14, dark);
            Fill(texture, 9, 7, 14, 18, dark);
            Fill(texture, 8, 9, 16, 14, steel);
            Fill(texture, 10, 8, 12, 16, steel);
            Fill(texture, 10, 10, 12, 12, dark);

            for (int i = 0; i < 8; i++)
            {
                SetSafe(texture, 12 + i, 12 + i, green);
                SetSafe(texture, 19 - i, 12 + i, green);
            }
            Fill(texture, 14, 14, 4, 4, whiteGreen);
            SetSafe(texture, 7, 16, green);
            SetSafe(texture, 24, 16, green);
            SetSafe(texture, 16, 7, green);
            SetSafe(texture, 16, 24, green);
        }

        private static void DrawCrate(Texture2D texture)
        {
            Color outline = Rgb(48, 29, 14);
            Color darkWood = Rgb(111, 57, 16);
            Color wood = Rgb(194, 101, 22);
            Color bright = Rgb(239, 147, 35);
            Color metal = Rgb(76, 71, 59);

            Fill(texture, 2, 2, 28, 28, outline);
            Fill(texture, 4, 4, 24, 24, darkWood);
            Fill(texture, 6, 6, 20, 20, wood);
            Fill(texture, 5, 25, 22, 2, bright);
            Fill(texture, 5, 5, 2, 22, bright);

            for (int i = 0; i < 18; i++)
            {
                SetSafe(texture, 7 + i, 7 + i, darkWood);
                SetSafe(texture, 24 - i, 7 + i, darkWood);
            }

            Fill(texture, 13, 13, 6, 6, metal);
            Fill(texture, 14, 14, 4, 4, Rgb(35, 37, 34));
            Bolt(texture, 5, 5);
            Bolt(texture, 25, 5);
            Bolt(texture, 5, 25);
            Bolt(texture, 25, 25);
        }

        private static void DrawJohn(Texture2D texture, FacingDirection direction, int phase)
        {
            Color shadow = new(0f, 0f, 0f, 0.30f);
            Color outline = Rgb(37, 27, 21);
            Color cap = Rgb(48, 55, 61);
            Color capLight = Rgb(77, 85, 91);
            Color n8 = Rgb(239, 153, 25);
            Color skin = Rgb(207, 127, 62);
            Color skinLight = Rgb(239, 165, 87);
            Color beard = Rgb(73, 43, 24);
            Color shirt = Rgb(211, 132, 20);
            Color shirtLight = Rgb(239, 164, 34);
            Color belt = Rgb(42, 35, 29);
            Color pants = Rgb(44, 50, 57);
            Color pantsLight = Rgb(67, 75, 82);
            Color boot = Rgb(89, 55, 28);
            Color glove = Rgb(70, 43, 26);

            int legShift = phase == 1 ? -2 : phase == 3 ? 2 : 0;
            int armShift = phase == 1 ? 1 : phase == 3 ? -1 : 0;
            Fill(texture, 7, 2, 18, 3, shadow);

            if (direction == FacingDirection.Left || direction == FacingDirection.Right)
            {
                bool right = direction == FacingDirection.Right;
                int face = right ? 1 : -1;

                Fill(texture, 9 + legShift, 5, 6, 5, boot);
                Fill(texture, 18 - legShift, 5, 6, 5, boot);
                Fill(texture, 10 + legShift, 10, 6, 10, pants);
                Fill(texture, 16 - legShift, 10, 7, 10, pantsLight);
                Fill(texture, 9, 20, 14, 2, belt);
                Fill(texture, 8, 22, 16, 11, outline);
                Fill(texture, 9, 23, 14, 9, shirt);
                Fill(texture, right ? 21 : 6, 22 + armShift, 5, 10, glove);
                Fill(texture, right ? 6 : 21, 22 - armShift, 4, 9, shirtLight);

                Fill(texture, 10, 32, 12, 9, skin);
                Fill(texture, right ? 17 : 10, 33, 5, 5, beard);
                Fill(texture, right ? 19 : 10, 37, 2, 2, skinLight);
                SetSafe(texture, right ? 18 : 12, 38, outline);

                Fill(texture, 8, 40, 16, 6, cap);
                Fill(texture, 10, 45, 12, 2, capLight);
                Fill(texture, right ? 21 : 5, 39, 7, 3, cap);
                Fill(texture, right ? 14 : 12, 42, 5, 2, n8);
                return;
            }

            Fill(texture, 7 + legShift, 5, 7, 5, boot);
            Fill(texture, 18 - legShift, 5, 7, 5, boot);
            Fill(texture, 8 + legShift, 10, 7, 10, pants);
            Fill(texture, 17 - legShift, 10, 7, 10, pantsLight);
            Fill(texture, 7, 20, 18, 2, belt);
            Fill(texture, 7, 22, 18, 11, outline);
            Fill(texture, 8, 23, 16, 9, shirt);
            Fill(texture, 9, 30, 14, 2, shirtLight);
            Fill(texture, 3, 22 + armShift, 5, 10, glove);
            Fill(texture, 24, 22 - armShift, 5, 10, glove);

            if (direction == FacingDirection.Up)
            {
                Fill(texture, 10, 32, 12, 9, beard);
                Fill(texture, 11, 33, 10, 7, Rgb(57, 39, 28));
                Fill(texture, 8, 40, 16, 6, cap);
                Fill(texture, 10, 45, 12, 2, capLight);
                Fill(texture, 10, 39, 12, 2, Rgb(34, 39, 43));
                return;
            }

            Fill(texture, 10, 32, 12, 9, skin);
            Fill(texture, 10, 32, 12, 4, beard);
            Fill(texture, 12, 36, 8, 2, beard);
            Fill(texture, 11, 38, 10, 2, skinLight);
            SetSafe(texture, 13, 38, outline);
            SetSafe(texture, 18, 38, outline);
            Fill(texture, 8, 40, 16, 6, cap);
            Fill(texture, 10, 45, 12, 2, capLight);
            Fill(texture, 10, 39, 12, 2, cap);
            Fill(texture, 13, 42, 6, 2, n8);
            SetSafe(texture, 14, 42, outline);
            SetSafe(texture, 17, 42, outline);
        }

        private static void Bolt(Texture2D texture, int x, int y)
        {
            SetSafe(texture, x, y, Rgb(18, 21, 22));
            SetSafe(texture, x + 1, y + 1, Rgb(126, 135, 137));
        }

        private static void Clear(Texture2D texture)
        {
            Color[] pixels = new Color[texture.width * texture.height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }
            texture.SetPixels(pixels);
        }

        private static void Fill(Texture2D texture, int x, int y, int width, int height, Color color)
        {
            for (int py = y; py < y + height; py++)
            {
                for (int px = x; px < x + width; px++)
                {
                    SetSafe(texture, px, py, color);
                }
            }
        }

        private static void SetSafe(Texture2D texture, int x, int y, Color color)
        {
            if (x >= 0 && x < texture.width && y >= 0 && y < texture.height)
            {
                texture.SetPixel(x, y, color);
            }
        }

        private static Color Rgb(byte r, byte g, byte b)
        {
            return new Color32(r, g, b, 255);
        }

        private static string ToAbsoluteAssetPath(string assetPath)
        {
            string relative = assetPath.Substring("Assets/".Length).Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(Application.dataPath, relative);
        }
    }
}
#endif