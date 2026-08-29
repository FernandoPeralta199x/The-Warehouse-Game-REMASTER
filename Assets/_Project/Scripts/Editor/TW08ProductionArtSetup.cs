#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using TW08.Presentation;
using UnityEditor;
using UnityEngine;

namespace TW08.Editor
{
    public static class TW08ProductionArtSetup
    {
        public const string ArtDataRoot = "Assets/_Project/ScriptableObjects/Art";
        public const string ProductionArtRoot = "Assets/_Project/Art/Production";
        public const string CatalogPath = ArtDataRoot + "/TW08_ArtCatalog.asset";
        public const string JohnSpriteSetPath = ArtDataRoot + "/John_DirectionalSpriteSet.asset";

        private const string GeneratedRoot = ProductionArtRoot + "/GeneratedStarter";
        private const float PixelsPerUnit = 32f;

        [MenuItem("Tools/TW08/Production/Create Art Runtime Assets")]
        public static void CreateArtRuntimeAssets()
        {
            TW08ArtCatalog catalog = EnsureProductionArtAssets();
            Selection.activeObject = catalog;
            EditorGUIUtility.PingObject(catalog);

            EditorUtility.DisplayDialog(
                "The Warehouse Nº 08 — Production Art",
                "Pipeline de arte de produção preparado.\n\n" +
                "Catálogo: " + CatalogPath + "\n" +
                "John: " + JohnSpriteSetPath + "\n" +
                "Starter pixel-art: " + GeneratedRoot + "\n\n" +
                "O starter usa a direção visual N-8 das referências: grafite, aço, âmbar e verde terminal. " +
                "Ele existe para substituir os quadrados do protótipo enquanto a arte final é refinada.",
                "OK");
        }

        public static TW08ArtCatalog EnsureProductionArtAssets()
        {
            EnsureFolder(ArtDataRoot);
            EnsureFolder(ProductionArtRoot);
            EnsureFolder(ProductionArtRoot + "/Characters");
            EnsureFolder(ProductionArtRoot + "/Characters/John");
            EnsureFolder(ProductionArtRoot + "/Environment");
            EnsureFolder(ProductionArtRoot + "/Props");
            EnsureFolder(ProductionArtRoot + "/Interactive");
            EnsureFolder(ProductionArtRoot + "/UI");
            EnsureFolder(ProductionArtRoot + "/VFX");
            EnsureFolder(GeneratedRoot);
            EnsureFolder(GeneratedRoot + "/Environment");
            EnsureFolder(GeneratedRoot + "/Props");
            EnsureFolder(GeneratedRoot + "/Interactive");
            EnsureFolder(GeneratedRoot + "/Characters");
            EnsureFolder(GeneratedRoot + "/Characters/John");

            DirectionalSpriteSet john = AssetDatabase.LoadAssetAtPath<DirectionalSpriteSet>(JohnSpriteSetPath);
            if (john == null)
            {
                john = ScriptableObject.CreateInstance<DirectionalSpriteSet>();
                AssetDatabase.CreateAsset(john, JohnSpriteSetPath);
            }

            TW08ArtCatalog catalog = AssetDatabase.LoadAssetAtPath<TW08ArtCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<TW08ArtCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            SerializedObject serializedCatalog = new(catalog);
            SerializedProperty johnProperty = serializedCatalog.FindProperty("john");
            if (johnProperty.objectReferenceValue == null)
            {
                johnProperty.objectReferenceValue = john;
                serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(catalog);
            }

            EnsureStarterPixelArt(catalog, john);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return catalog;
        }

        private static void EnsureStarterPixelArt(TW08ArtCatalog catalog, DirectionalSpriteSet john)
        {
            string floorPrimary = EnsureSprite(
                GeneratedRoot + "/Environment/Floor_Primary.png",
                32,
                32,
                DrawFloorPrimary,
                new Vector2(0.5f, 0.5f));
            string floorSecondary = EnsureSprite(
                GeneratedRoot + "/Environment/Floor_Secondary.png",
                32,
                32,
                DrawFloorSecondary,
                new Vector2(0.5f, 0.5f));
            string wall = EnsureSprite(
                GeneratedRoot + "/Environment/Wall_N8.png",
                32,
                32,
                DrawWall,
                new Vector2(0.5f, 0.5f));
            string goal = EnsureSprite(
                GeneratedRoot + "/Interactive/Goal_N8.png",
                32,
                32,
                DrawGoal,
                new Vector2(0.5f, 0.5f));
            string crate = EnsureSprite(
                GeneratedRoot + "/Props/Crate_N8.png",
                32,
                32,
                DrawCrate,
                new Vector2(0.5f, 0.5f));
            string arrow = EnsureSprite(
                GeneratedRoot + "/Interactive/Arrow_N8.png",
                32,
                32,
                DrawArrow,
                new Vector2(0.5f, 0.5f));
            string neutral = EnsureSprite(
                GeneratedRoot + "/Interactive/Block_Neutral.png",
                32,
                32,
                DrawNeutralBlock,
                new Vector2(0.5f, 0.5f));

            Dictionary<FacingDirection, string> idle = new();
            Dictionary<FacingDirection, string[]> walk = new();
            foreach (FacingDirection direction in Enum.GetValues(typeof(FacingDirection)))
            {
                string key = direction.ToString();
                idle[direction] = EnsureJohnFrame(key + "_Idle", direction, 0);
                walk[direction] = new[]
                {
                    EnsureJohnFrame(key + "_Walk01", direction, 1),
                    EnsureJohnFrame(key + "_Walk02", direction, 2),
                    EnsureJohnFrame(key + "_Walk03", direction, 3)
                };
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            AssignCatalogSpriteIfEmpty(catalog, "floorPrimary", floorPrimary);
            AssignCatalogSpriteIfEmpty(catalog, "floorSecondary", floorSecondary);
            AssignCatalogSpriteIfEmpty(catalog, "wall", wall);
            AssignCatalogSpriteIfEmpty(catalog, "goal", goal);
            AssignCatalogSpriteIfEmpty(catalog, "crateDefault", crate);
            AssignCatalogSpriteIfEmpty(catalog, "directionArrow", arrow);
            AssignCatalogSpriteIfEmpty(catalog, "neutralBlock", neutral);

            SerializedObject serializedJohn = new(john);
            AssignSpriteIfEmpty(serializedJohn.FindProperty("idleDown"), idle[FacingDirection.Down]);
            AssignSpriteIfEmpty(serializedJohn.FindProperty("idleUp"), idle[FacingDirection.Up]);
            AssignSpriteIfEmpty(serializedJohn.FindProperty("idleLeft"), idle[FacingDirection.Left]);
            AssignSpriteIfEmpty(serializedJohn.FindProperty("idleRight"), idle[FacingDirection.Right]);
            AssignSpriteArrayIfEmpty(serializedJohn.FindProperty("walkDown"), walk[FacingDirection.Down]);
            AssignSpriteArrayIfEmpty(serializedJohn.FindProperty("walkUp"), walk[FacingDirection.Up]);
            AssignSpriteArrayIfEmpty(serializedJohn.FindProperty("walkLeft"), walk[FacingDirection.Left]);
            AssignSpriteArrayIfEmpty(serializedJohn.FindProperty("walkRight"), walk[FacingDirection.Right]);
            serializedJohn.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(john);
        }

        private static string EnsureJohnFrame(string fileName, FacingDirection direction, int phase)
        {
            return EnsureSprite(
                GeneratedRoot + "/Characters/John/John_" + fileName + ".png",
                32,
                48,
                texture => DrawJohn(texture, direction, phase),
                new Vector2(0.5f, 0.08f));
        }

        private static string EnsureSprite(
            string assetPath,
            int width,
            int height,
            Action<Texture2D> drawer,
            Vector2 pivot)
        {
            string absolutePath = ToAbsoluteAssetPath(assetPath);
            if (!File.Exists(absolutePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(absolutePath) ?? Application.dataPath);
                Texture2D texture = new(width, height, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };
                Clear(texture);
                drawer(texture);
                texture.Apply(false, false);
                File.WriteAllBytes(absolutePath, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            ConfigureSpriteImporter(assetPath, pivot);
            return assetPath;
        }

        private static void ConfigureSpriteImporter(string assetPath, Vector2 pivot)
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

        private static void AssignCatalogSpriteIfEmpty(TW08ArtCatalog catalog, string propertyName, string assetPath)
        {
            SerializedObject serialized = new(catalog);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property != null && property.objectReferenceValue == null)
            {
                property.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(catalog);
            }
        }

        private static void AssignSpriteIfEmpty(SerializedProperty property, string assetPath)
        {
            if (property != null && property.objectReferenceValue == null)
            {
                property.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            }
        }

        private static void AssignSpriteArrayIfEmpty(SerializedProperty property, IReadOnlyList<string> assetPaths)
        {
            if (property == null || property.arraySize > 0)
            {
                return;
            }

            property.arraySize = assetPaths.Count;
            for (int i = 0; i < assetPaths.Count; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(assetPaths[i]);
            }
        }

        private static void DrawFloorPrimary(Texture2D texture)
        {
            Color baseColor = Rgb(29, 35, 38);
            Color seam = Rgb(56, 65, 68);
            Color inset = Rgb(37, 43, 46);
            Fill(texture, 0, 0, 32, 32, baseColor);
            Fill(texture, 1, 1, 30, 30, inset);
            Fill(texture, 2, 2, 28, 1, seam);
            Fill(texture, 2, 29, 28, 1, seam);
            Fill(texture, 2, 2, 1, 28, seam);
            Fill(texture, 29, 2, 1, 28, seam);
            Rivet(texture, 5, 5);
            Rivet(texture, 26, 5);
            Rivet(texture, 5, 26);
            Rivet(texture, 26, 26);
        }

        private static void DrawFloorSecondary(Texture2D texture)
        {
            DrawFloorPrimary(texture);
            Color plate = Rgb(43, 50, 53);
            Fill(texture, 8, 8, 16, 1, plate);
            Fill(texture, 8, 23, 16, 1, plate);
            Fill(texture, 8, 8, 1, 16, plate);
            Fill(texture, 23, 8, 1, 16, plate);
        }

        private static void DrawWall(Texture2D texture)
        {
            Color dark = Rgb(17, 21, 23);
            Color steel = Rgb(57, 64, 67);
            Color light = Rgb(91, 99, 101);
            Color amber = Rgb(222, 139, 22);
            Fill(texture, 0, 0, 32, 32, dark);
            Fill(texture, 1, 3, 30, 27, steel);
            Fill(texture, 3, 5, 26, 23, Rgb(35, 40, 43));
            Fill(texture, 1, 29, 30, 2, light);
            Fill(texture, 0, 0, 32, 4, Rgb(25, 28, 30));
            for (int x = -4; x < 36; x += 8)
            {
                Fill(texture, x, 0, 4, 4, amber);
            }
            Rivet(texture, 5, 25);
            Rivet(texture, 26, 25);
        }

        private static void DrawGoal(Texture2D texture)
        {
            Color glow = new(0.14f, 0.9f, 0.48f, 0.35f);
            Color green = Rgb(49, 221, 116);
            Fill(texture, 6, 6, 20, 20, glow);
            Fill(texture, 4, 4, 8, 2, green);
            Fill(texture, 4, 4, 2, 8, green);
            Fill(texture, 20, 4, 8, 2, green);
            Fill(texture, 26, 4, 2, 8, green);
            Fill(texture, 4, 26, 8, 2, green);
            Fill(texture, 4, 20, 2, 8, green);
            Fill(texture, 20, 26, 8, 2, green);
            Fill(texture, 26, 20, 2, 8, green);
            Fill(texture, 14, 10, 4, 12, green);
            Fill(texture, 10, 14, 12, 4, green);
        }

        /// <summary>
        /// Seta que aponta para CIMA, com assimetria real.
        ///
        /// O sprite de alvo era usado para isto, mas ele tem simetria de quatro
        /// dobras: rotacioná-lo em múltiplos de 90° não muda um pixel, e as
        /// quatro direções da esteira saíam idênticas na tela. A mecânica ficava
        /// só na documentação.
        /// </summary>
        private static void DrawArrow(Texture2D texture)
        {
            Color body = Rgb(255, 161, 31);
            Color edge = Rgb(120, 66, 8);

            // Ponta: triângulo cheio no topo.
            for (int row = 0; row < 11; row++)
            {
                int y = 26 - row;
                int half = row + 1;
                Fill(texture, 16 - half, y, half * 2, 1, body);
            }

            // Haste, mais estreita que a ponta para a direção ser lida de longe.
            Fill(texture, 12, 5, 8, 12, body);

            // Contorno escuro só na base: dá peso e separa a seta do piso.
            Fill(texture, 12, 4, 8, 1, edge);
            Fill(texture, 11, 5, 1, 12, edge);
            Fill(texture, 20, 5, 1, 12, edge);
        }

        /// <summary>
        /// Bloco cinza neutro para elementos que recebem tinta.
        ///
        /// A tinta do Unity é multiplicativa: pintar de azul um sprite de
        /// madeira alaranjada não dá azul, dá oliva. Todo elemento cuja cor
        /// carrega significado — carga por tipo, porta, portão, robô — precisa
        /// partir de cinza para a tinta chegar limpa.
        /// </summary>
        private static void DrawNeutralBlock(Texture2D texture)
        {
            Color outline = Rgb(38, 42, 44);
            Color body = Rgb(138, 146, 150);
            Color light = Rgb(178, 186, 190);
            Color shade = Rgb(104, 112, 116);

            Fill(texture, 2, 2, 28, 28, outline);
            Fill(texture, 4, 4, 24, 24, body);
            Fill(texture, 4, 26, 24, 2, light);
            Fill(texture, 4, 4, 24, 2, shade);
            Fill(texture, 4, 4, 2, 24, shade);
            Fill(texture, 26, 4, 2, 24, light);
        }

        private static void DrawCrate(Texture2D texture)
        {
            Color outline = Rgb(53, 31, 16);
            Color frame = Rgb(125, 66, 18);
            Color wood = Rgb(201, 112, 28);
            Color highlight = Rgb(240, 157, 45);
            Fill(texture, 2, 2, 28, 28, outline);
            Fill(texture, 4, 4, 24, 24, frame);
            Fill(texture, 7, 7, 18, 18, wood);
            Fill(texture, 6, 25, 20, 2, highlight);
            for (int i = 0; i < 18; i++)
            {
                SetSafe(texture, 7 + i, 7 + i, frame);
                SetSafe(texture, 24 - i, 7 + i, frame);
            }
            Rivet(texture, 5, 5);
            Rivet(texture, 26, 5);
            Rivet(texture, 5, 26);
            Rivet(texture, 26, 26);
        }

        private static void DrawJohn(Texture2D texture, FacingDirection direction, int phase)
        {
            Color shadow = new(0f, 0f, 0f, 0.28f);
            Color outline = Rgb(38, 29, 24);
            Color cap = Rgb(48, 54, 59);
            Color capLight = Rgb(72, 79, 84);
            Color skin = Rgb(212, 137, 73);
            Color skinLight = Rgb(238, 164, 87);
            Color beard = Rgb(74, 45, 25);
            Color shirt = Rgb(214, 137, 25);
            Color shirtLight = Rgb(238, 165, 38);
            Color pants = Rgb(45, 50, 56);
            Color pantsLight = Rgb(66, 72, 78);
            Color boot = Rgb(91, 58, 30);
            Color glove = Rgb(78, 48, 28);

            int stride = phase == 1 ? -2 : phase == 3 ? 2 : 0;
            Fill(texture, 7, 2, 18, 3, shadow);

            if (direction == FacingDirection.Left || direction == FacingDirection.Right)
            {
                bool right = direction == FacingDirection.Right;
                int front = right ? 1 : -1;
                int bodyX = 11;

                Fill(texture, bodyX + stride, 5, 7, 6, boot);
                Fill(texture, bodyX - stride, 5, 7, 6, boot);
                Fill(texture, 10 + stride, 10, 7, 11, pants);
                Fill(texture, 15 - stride, 10, 7, 11, pantsLight);
                Fill(texture, 9, 20, 14, 15, outline);
                Fill(texture, 10, 21, 12, 13, shirt);
                Fill(texture, 11, 31, 10, 10, skin);
                Fill(texture, 12 + (right ? 3 : 0), 34, 4, 3, skinLight);
                Fill(texture, right ? 17 : 11, 31, 4, 5, beard);
                Fill(texture, 9, 39, 14, 6, cap);
                Fill(texture, 11, 44, 10, 2, capLight);
                Fill(texture, right ? 21 : 6, 38, 7, 3, cap);
                Fill(texture, 6 + (front > 0 ? 2 : 0), 22 + stride, 5, 10, glove);
                Fill(texture, 22 - (front > 0 ? 2 : 0), 22 - stride, 5, 10, glove);
                return;
            }

            Fill(texture, 7 + stride, 5, 7, 6, boot);
            Fill(texture, 18 - stride, 5, 7, 6, boot);
            Fill(texture, 8 + stride, 10, 7, 11, pants);
            Fill(texture, 17 - stride, 10, 7, 11, pantsLight);
            Fill(texture, 7, 20, 18, 15, outline);
            Fill(texture, 8, 21, 16, 13, shirt);
            Fill(texture, 4, 21 - stride, 5, 11, glove);
            Fill(texture, 23, 21 + stride, 5, 11, glove);
            Fill(texture, 10, 32, 12, 10, direction == FacingDirection.Up ? beard : skin);
            if (direction == FacingDirection.Down)
            {
                Fill(texture, 11, 33, 10, 7, skin);
                Fill(texture, 12, 33, 8, 3, beard);
                SetSafe(texture, 13, 38, outline);
                SetSafe(texture, 18, 38, outline);
            }
            Fill(texture, 8, 40, 16, 6, cap);
            Fill(texture, 10, 45, 12, 2, capLight);
            if (direction == FacingDirection.Down)
            {
                Fill(texture, 9, 39, 14, 2, cap);
                Fill(texture, 12, 42, 8, 2, Rgb(235, 154, 28));
            }
        }

        private static void Rivet(Texture2D texture, int x, int y)
        {
            Color dark = Rgb(18, 21, 22);
            Color light = Rgb(119, 128, 130);
            SetSafe(texture, x, y, dark);
            SetSafe(texture, x + 1, y + 1, light);
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

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
#endif