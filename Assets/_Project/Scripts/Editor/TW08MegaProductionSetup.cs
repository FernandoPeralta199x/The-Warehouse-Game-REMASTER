#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TW08.Editor
{
    public static class TW08MegaProductionSetup
    {
        [MenuItem("Tools/TW08/Production/Build Mega Production Update")]
        public static void BuildMegaProductionUpdate()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "The Warehouse Nº 08 — Mega Update",
                    "Saia do Play Mode antes de aplicar a atualização de produção.",
                    "OK");
                return;
            }

            try
            {
                EditorUtility.DisplayProgressBar(
                    "The Warehouse Nº 08 — Mega Update",
                    "Criando perfis gráficos e conteúdo de corrida...",
                    0.15f);
                TW08MegaContentSetup.MegaContentData content = TW08MegaContentSetup.EnsureAll();

                EditorUtility.DisplayProgressBar(
                    "The Warehouse Nº 08 — Mega Update",
                    "Aplicando câmera, IA, itens e UI profissional...",
                    0.48f);
                TW08MegaSceneUpgrade.Apply(content);

                EditorUtility.DisplayProgressBar(
                    "The Warehouse Nº 08 — Mega Update",
                    "Salvando assets e atualizando o banco do Unity...",
                    0.86f);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorSceneManager.OpenScene(TW08MenuSceneBuilder.MainMenuPath, OpenSceneMode.Single);
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog(
                    "The Warehouse Nº 08 — Mega Update",
                    "Atualização de produção aplicada.\n\n" +
                    "GRÁFICOS\n" +
                    "- TW08 Graphics Profile + Graphics Director\n" +
                    "- câmera pixel-aware com look-ahead, speed zoom e screen shake\n\n" +
                    "CORRIDA\n" +
                    "- grid com jogador + 3 rivais IA\n" +
                    "- rubber-banding leve e drift da IA\n" +
                    "- 4 caixas de itens por pista\n" +
                    "- 8 Power Ups logísticos\n" +
                    "- posição 01/04 + item atual no HUD\n" +
                    "- scanner de rota e feedback de impacto\n\n" +
                    "UI/UX\n" +
                    "- entrada animada dos menus\n" +
                    "- foco visual para teclado/gamepad\n\n" +
                    "CONTEÚDO GRANDE\n" +
                    "- Addressables 2.7.6 preparado no Package Manager\n" +
                    "- nenhum arquivo artificial foi criado apenas para inflar o tamanho do projeto\n\n" +
                    "Gate obrigatório: Console sem erros, EditMode/PlayMode e playtest das 3 pistas.",
                    "OK");
            }
            catch (Exception exception)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogException(exception);
                EditorUtility.DisplayDialog(
                    "The Warehouse Nº 08 — Mega Update falhou",
                    "A atualização foi interrompida no primeiro erro. Corrija o erro antes de repetir o comando.\n\n" +
                    exception.Message,
                    "OK");
                throw;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
#endif
