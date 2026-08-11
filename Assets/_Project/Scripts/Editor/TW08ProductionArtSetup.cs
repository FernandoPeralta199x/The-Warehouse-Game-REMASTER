#if UNITY_EDITOR
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
                "Sprites finais: " + ProductionArtRoot + "\n\n" +
                "ReferenceSource continua sendo apenas direção visual. Preencha o DirectionalSpriteSet com sprites recortados/limpos antes de promover a arte para gameplay.",
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

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return catalog;
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
