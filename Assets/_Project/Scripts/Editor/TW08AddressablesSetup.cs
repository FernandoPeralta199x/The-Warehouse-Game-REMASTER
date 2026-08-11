#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TW08.Editor
{
    internal static class TW08AddressablesSetup
    {
        private const string ContentRoot = "Assets/_Project/ContentPacks";

        private static readonly ContentGroupSpec[] Groups =
        {
            new("TW08-Optional-Art", ContentRoot + "/Art", "tw08/content/art"),
            new("TW08-Optional-Audio", ContentRoot + "/Audio", "tw08/content/audio"),
            new("TW08-Race-Packs", ContentRoot + "/Race", "tw08/content/race"),
            new("TW08-Narrative-Packs", ContentRoot + "/Narrative", "tw08/content/narrative")
        };

        [MenuItem("Tools/TW08/Production/Initialize Content Streaming")]
        internal static void InitializeFromMenu()
        {
            try
            {
                EnsureProductionGroups();
                EditorUtility.DisplayDialog(
                    "The Warehouse Nº 08 — Content Streaming",
                    "Addressables inicializado.\n\n" +
                    "Grupos preparados:\n" +
                    "- TW08-Optional-Art\n" +
                    "- TW08-Optional-Audio\n" +
                    "- TW08-Race-Packs\n" +
                    "- TW08-Narrative-Packs\n\n" +
                    "Os grupos começam locais. Um profile remoto/CDN deve ser configurado apenas quando existir um endpoint de distribuição real.",
                    "OK");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog(
                    "The Warehouse Nº 08 — Content Streaming falhou",
                    exception.Message,
                    "OK");
                throw;
            }
        }

        internal static void EnsureProductionGroups()
        {
            EnsureFolders();

            Type defaultObjectType = FindType("UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject");
            Type settingsType = FindType("UnityEditor.AddressableAssets.Settings.AddressableAssetSettings");
            Type groupType = FindType("UnityEditor.AddressableAssets.Settings.AddressableAssetGroup");
            Type schemaType = FindType("UnityEditor.AddressableAssets.Settings.AddressableAssetGroupSchema");
            Type bundledSchemaType = FindType("UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema");

            if (defaultObjectType == null || settingsType == null || groupType == null || schemaType == null || bundledSchemaType == null)
            {
                throw new InvalidOperationException(
                    "Addressables API não está disponível. Aguarde o Package Manager resolver com.unity.addressables e tente novamente.");
            }

            MethodInfo getSettings = defaultObjectType.GetMethod(
                "GetSettings",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(bool) },
                null);
            if (getSettings == null)
            {
                throw new MissingMethodException(defaultObjectType.FullName, "GetSettings(bool)");
            }

            object settings = getSettings.Invoke(null, new object[] { true });
            if (settings == null)
            {
                throw new InvalidOperationException("Addressables retornou settings nulo após solicitar criação.");
            }

            MethodInfo findGroup = settingsType.GetMethod(
                "FindGroup",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(string) },
                null);
            MethodInfo createEntry = settingsType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(method => method.Name == "CreateOrMoveEntry" && method.GetParameters().Length == 4);
            MethodInfo createGroup = settingsType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(method => method.Name == "CreateGroup" && method.GetParameters().Length == 6);

            if (findGroup == null || createEntry == null || createGroup == null)
            {
                throw new MissingMethodException("Addressables settings API expected by TW08 was not found.");
            }

            Type schemaListType = typeof(System.Collections.Generic.List<>).MakeGenericType(schemaType);

            foreach (ContentGroupSpec spec in Groups)
            {
                object group = findGroup.Invoke(settings, new object[] { spec.GroupName });
                if (group == null)
                {
                    object schemasToCopy = Activator.CreateInstance(schemaListType);
                    group = createGroup.Invoke(
                        settings,
                        new object[]
                        {
                            spec.GroupName,
                            false,
                            false,
                            true,
                            schemasToCopy,
                            new[] { bundledSchemaType }
                        });
                }

                if (group == null || !groupType.IsInstanceOfType(group))
                {
                    throw new InvalidOperationException($"Não foi possível criar/encontrar o grupo Addressables '{spec.GroupName}'.");
                }

                string guid = AssetDatabase.AssetPathToGUID(spec.AssetFolder);
                if (string.IsNullOrWhiteSpace(guid))
                {
                    throw new InvalidOperationException($"Pasta de content pack não possui GUID: {spec.AssetFolder}");
                }

                object entry = createEntry.Invoke(settings, new[] { (object)guid, group, false, true });
                if (entry == null)
                {
                    throw new InvalidOperationException($"Addressables não criou entry para '{spec.AssetFolder}'.");
                }

                MethodInfo setAddress = entry.GetType().GetMethod(
                    "SetAddress",
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new[] { typeof(string), typeof(bool) },
                    null);
                if (setAddress != null)
                {
                    setAddress.Invoke(entry, new object[] { spec.Address, true });
                }
            }

            if (settings is UnityEngine.Object unitySettings)
            {
                EditorUtility.SetDirty(unitySettings);
            }
            AssetDatabase.SaveAssets();
        }

        private static void EnsureFolders()
        {
            TW08ProductionSceneUtility.EnsureFolder(ContentRoot);
            foreach (ContentGroupSpec spec in Groups)
            {
                TW08ProductionSceneUtility.EnsureFolder(spec.AssetFolder);
            }
            AssetDatabase.SaveAssets();
        }

        private static Type FindType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName, false);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }

        private readonly struct ContentGroupSpec
        {
            public ContentGroupSpec(string groupName, string assetFolder, string address)
            {
                GroupName = groupName;
                AssetFolder = assetFolder;
                Address = address;
            }

            public string GroupName { get; }
            public string AssetFolder { get; }
            public string Address { get; }
        }
    }
}
#endif
