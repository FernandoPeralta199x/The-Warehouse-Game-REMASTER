#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TW08.Editor
{
    /// <summary>
    /// Imports the project's approved visual reference folder into Assets without
    /// treating concept art as final game-ready sprites. Source files remain untouched.
    /// </summary>
    public static class TW08ReferenceArtImporter
    {
        private const string RepositorySourceRoot = "REFERENCIA";
        private const string DestinationRoot = "Assets/_Project/Art/ReferenceSource";
        private const float CandidatePixelsPerUnit = 32f;

        [MenuItem("Tools/TW08/Art/Import Repository Reference Folder")]
        public static void ImportRepositoryReferenceFolder()
        {
            string projectRoot = ResolveProjectRoot();
            if (string.IsNullOrWhiteSpace(projectRoot))
            {
                return;
            }

            ImportFromSource(Path.Combine(projectRoot, RepositorySourceRoot), projectRoot);
        }

        [MenuItem("Tools/TW08/Art/Import Reference Folder...")]
        public static void ImportExternalReferenceFolder()
        {
            string projectRoot = ResolveProjectRoot();
            if (string.IsNullOrWhiteSpace(projectRoot))
            {
                return;
            }

            string selectedFolder = EditorUtility.OpenFolderPanel(
                "Selecione a pasta REFERENCIA extraída",
                projectRoot,
                RepositorySourceRoot);

            if (string.IsNullOrWhiteSpace(selectedFolder))
            {
                return;
            }

            ImportFromSource(selectedFolder, projectRoot);
        }

        private static string ResolveProjectRoot()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (!string.IsNullOrWhiteSpace(projectRoot))
            {
                return projectRoot;
            }

            Debug.LogError("TW08: Could not resolve the Unity project root.");
            return null;
        }

        private static void ImportFromSource(string sourceRoot, string projectRoot)
        {
            if (!Directory.Exists(sourceRoot))
            {
                EditorUtility.DisplayDialog(
                    "The Warehouse Nº 08 — Referências",
                    "A pasta de referências selecionada não existe.",
                    "OK");
                return;
            }

            string[] pngFiles = Directory
                .GetFiles(sourceRoot, "*.png", SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (pngFiles.Length == 0)
            {
                EditorUtility.DisplayDialog(
                    "The Warehouse Nº 08 — Referências",
                    "Nenhuma imagem PNG foi encontrada na pasta selecionada.",
                    "OK");
                return;
            }

            List<string> importedAssets = new();
            List<string> lfsPointers = new();

            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (string sourceFile in pngFiles)
                {
                    string relative = GetRelativePath(sourceRoot, sourceFile);
                    if (IsGitLfsPointer(sourceFile))
                    {
                        lfsPointers.Add(relative);
                        continue;
                    }

                    string destinationAssetPath = $"{DestinationRoot}/{relative}".Replace('\\', '/');
                    string destinationAbsolutePath = Path.Combine(
                        projectRoot,
                        destinationAssetPath.Replace('/', Path.DirectorySeparatorChar));
                    string destinationDirectory = Path.GetDirectoryName(destinationAbsolutePath);
                    if (!string.IsNullOrWhiteSpace(destinationDirectory))
                    {
                        Directory.CreateDirectory(destinationDirectory);
                    }

                    File.Copy(sourceFile, destinationAbsolutePath, true);
                    importedAssets.Add(destinationAssetPath);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            foreach (string assetPath in importedAssets)
            {
                ConfigureImportedTexture(assetPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (lfsPointers.Count > 0)
            {
                string sample = string.Join("\n", lfsPointers.Take(6));
                Debug.LogWarning(
                    $"TW08: {lfsPointers.Count} arquivo(s) de referência são ponteiros Git LFS e não contêm a imagem real. " +
                    "Use 'git lfs pull' ou escolha a pasta extraída do RAR pelo menu de importação externa.\n" + sample);
            }

            string result =
                $"Referências importadas: {importedAssets.Count}\n" +
                $"Ponteiros Git LFS ignorados: {lfsPointers.Count}\n\n" +
                "Destino: Assets/_Project/Art/ReferenceSource\n\n" +
                "Personagens e empilhadeiras permanecem material de direção visual. " +
                "Somente arquivos provenientes de /Sprites/ são configurados como candidatos de sprite pixel-art; " +
                "nada é promovido automaticamente a arte final sem revisão.";

            EditorUtility.DisplayDialog("The Warehouse Nº 08 — Referências", result, "OK");
        }

        private static void ConfigureImportedTexture(string assetPath)
        {
            if (AssetImporter.GetAtPath(assetPath) is not TextureImporter importer)
            {
                return;
            }

            bool spriteCandidate = assetPath.IndexOf("/Sprites/", StringComparison.OrdinalIgnoreCase) >= 0;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;

            if (spriteCandidate)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = CandidatePixelsPerUnit;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;
            }
            else
            {
                importer.textureType = TextureImporterType.Default;
                importer.filterMode = FilterMode.Bilinear;
            }

            importer.SaveAndReimport();
        }

        private static bool IsGitLfsPointer(string path)
        {
            FileInfo info = new(path);
            if (!info.Exists || info.Length > 1024)
            {
                return false;
            }

            using StreamReader reader = new(path);
            string firstLine = reader.ReadLine();
            return string.Equals(
                firstLine,
                "version https://git-lfs.github.com/spec/v1",
                StringComparison.Ordinal);
        }

        private static string GetRelativePath(string root, string fullPath)
        {
            string normalizedRoot = Path.GetFullPath(root)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            string normalizedPath = Path.GetFullPath(fullPath);

            if (!normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Reference file is outside the configured source root: {fullPath}");
            }

            return normalizedPath
                .Substring(normalizedRoot.Length)
                .Replace(Path.DirectorySeparatorChar, '/');
        }
    }
}
#endif
