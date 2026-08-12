#if UNITY_EDITOR
using System.Collections.Generic;
using TW08.Presentation;
using UnityEditor;
using UnityEngine;

namespace TW08.Editor
{
    /// <summary>
    /// Promove a arte fatiada da pasta REFERENCIA (Tools/art/slice_reference_sheets.py)
    /// a arte de jogo: configura importadores e sobrescreve os DirectionalSpriteSets
    /// e retratos dos personagens. Quando a pasta Reference não existe, tudo cai de
    /// volta na arte starter procedural.
    /// </summary>
    public static class TW08ReferenceGameArt
    {
        public const string Root = "Assets/_Project/Art/Production/Reference";
        public const string RobertSpriteSetPath =
            "Assets/_Project/ScriptableObjects/Art/Robert_DirectionalSpriteSet.asset";

        private const float CharacterPixelsPerUnit = 160f;
        private const float ForkliftPixelsPerUnit = 200f;

        private static readonly string[] CharacterFolders = { "John", "Duda", "Robert" };

        public static bool HasReferenceArt => AssetDatabase.IsValidFolder(Root + "/Characters");

        [MenuItem("Tools/TW08/Art/Wire Reference Game Art")]
        public static void WireFromMenu()
        {
            bool ok = WireAll();
            EditorUtility.DisplayDialog(
                "The Warehouse Nº 08 — Reference Art",
                ok
                    ? "Arte de referência ligada: John, Duda e Robert usam os sprites fatiados da REFERENCIA."
                    : "Pasta de arte fatiada não encontrada em " + Root + ".\nRode Tools/art/slice_reference_sheets.py primeiro.",
                "OK");
        }

        /// <summary>Liga tudo. Retorna false quando não há arte fatiada no projeto.</summary>
        public static bool WireAll()
        {
            if (!HasReferenceArt)
            {
                return false;
            }

            ConfigureImporters();

            OverwriteSpriteSet(
                LoadOrCreateSet(TW08ProductionArtSetup.JohnSpriteSetPath), "John");
            OverwriteSpriteSet(
                LoadOrCreateSet(TW08ExpansionStarterArt.DudaSpriteSetPath), "Duda");
            OverwriteSpriteSet(
                LoadOrCreateSet(RobertSpriteSetPath), "Robert");

            AssetDatabase.SaveAssets();
            return true;
        }

        /// <summary>Retrato de referência do personagem, ou null quando ausente.</summary>
        public static Sprite LoadPortrait(string characterId)
        {
            string folder = characterId switch
            {
                "john" => "John",
                "duda" => "Duda",
                "robert" => "Robert",
                _ => null
            };
            if (folder == null)
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(
                $"{Root}/Characters/{folder}/{folder}_Portrait.png");
        }

        /// <summary>Sprite de empilhadeira ("Car1_Hero", "Car2_Left", ...), ou null.</summary>
        public static Sprite LoadForklift(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>($"{Root}/Forklift/{name}.png");
        }

        // ------------------------------------------------------------------ //

        private static void ConfigureImporters()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { Root }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
                {
                    continue;
                }

                bool portrait = path.EndsWith("_Portrait.png");
                bool forklift = path.Contains("/Forklift/");

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = forklift ? ForkliftPixelsPerUnit : CharacterPixelsPerUnit;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.maxTextureSize = 1024;

                TextureImporterSettings settings = new();
                importer.ReadTextureSettings(settings);
                if (portrait)
                {
                    settings.spriteAlignment = (int)SpriteAlignment.Center;
                }
                else
                {
                    settings.spriteAlignment = (int)SpriteAlignment.Custom;
                    settings.spritePivot = new Vector2(0.5f, 0.02f);
                }
                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }
        }

        private static DirectionalSpriteSet LoadOrCreateSet(string path)
        {
            DirectionalSpriteSet set = AssetDatabase.LoadAssetAtPath<DirectionalSpriteSet>(path);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<DirectionalSpriteSet>();
                AssetDatabase.CreateAsset(set, path);
            }

            return set;
        }

        private static void OverwriteSpriteSet(DirectionalSpriteSet set, string folder)
        {
            SerializedObject serialized = new(set);
            AssignSprite(serialized.FindProperty("idleDown"), folder, "IdleDown");
            AssignSprite(serialized.FindProperty("idleUp"), folder, "IdleUp");
            AssignSprite(serialized.FindProperty("idleLeft"), folder, "IdleLeft");
            AssignSprite(serialized.FindProperty("idleRight"), folder, "IdleRight");
            AssignWalk(serialized.FindProperty("walkDown"), folder, "Down");
            AssignWalk(serialized.FindProperty("walkUp"), folder, "Up");
            AssignWalk(serialized.FindProperty("walkLeft"), folder, "Left");
            AssignWalk(serialized.FindProperty("walkRight"), folder, "Right");
            SerializedProperty fps = serialized.FindProperty("framesPerSecond");
            if (fps != null)
            {
                fps.floatValue = 8f;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(set);
        }

        private static void AssignSprite(SerializedProperty property, string folder, string frame)
        {
            Sprite sprite = LoadFrame(folder, frame);
            if (property != null && sprite != null)
            {
                property.objectReferenceValue = sprite;
            }
        }

        private static void AssignWalk(SerializedProperty property, string folder, string direction)
        {
            if (property == null)
            {
                return;
            }

            List<Sprite> frames = new();
            for (int i = 1; i <= 3; i++)
            {
                Sprite sprite = LoadFrame(folder, $"Walk{i}{direction}");
                if (sprite != null)
                {
                    frames.Add(sprite);
                }
            }

            if (frames.Count == 0)
            {
                return;
            }

            property.arraySize = frames.Count;
            for (int i = 0; i < frames.Count; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
            }
        }

        private static Sprite LoadFrame(string folder, string frame)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(
                $"{Root}/Characters/{folder}/{folder}_{frame}.png");
        }
    }
}
#endif
