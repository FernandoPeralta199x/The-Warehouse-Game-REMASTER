#if UNITY_EDITOR
using System.Collections.Generic;
using TW08.Data;
using TW08.Narrative;
using UnityEditor;
using UnityEngine;

namespace TW08.Editor
{
    /// <summary>
    /// Materializa o roteiro do The Warehouse Nº 08 em assets.
    ///
    /// O texto vive aqui, no código, e não em um JSON solto: é a única forma de
    /// garantir que regenerar os assets não perca fala nem inverta ordem. As
    /// falas seguem a história central (abertura, ato 1 a 3, virada do Robert e
    /// desfecho no Núcleo Logístico) e não repetem as falas curtas que já estão
    /// em Docs/level-specs.json — aquelas são pistas de fase, estas são cenas.
    ///
    /// O catálogo é gravado dentro de uma pasta Resources para que qualquer cena
    /// de puzzle consiga se autoconfigurar sem os construtores de cena saberem
    /// que narrativa existe.
    /// </summary>
    public static class TW08NarrativeSetup
    {
        public const string NarrativeRoot = "Assets/_Project/ScriptableObjects/Narrative";
        public const string SequenceRoot = NarrativeRoot + "/Sequences";
        public const string ResourceRoot = NarrativeRoot + "/Resources";
        public const string CatalogPath = ResourceRoot + "/" + NarrativeCatalog.ResourceName + ".asset";

        private readonly struct SequenceSpec
        {
            public SequenceSpec(
                string fileName,
                string id,
                string title,
                NarrativeTriggerKind trigger,
                string sectorId,
                string levelId,
                NarrativeLine[] lines,
                float speed = 38f,
                int priority = 0)
            {
                FileName = fileName;
                Id = id;
                Title = title;
                Trigger = trigger;
                SectorId = sectorId;
                LevelId = levelId;
                Lines = lines;
                Speed = speed;
                Priority = priority;
            }

            public string FileName { get; }
            public string Id { get; }
            public string Title { get; }
            public NarrativeTriggerKind Trigger { get; }
            public string SectorId { get; }
            public string LevelId { get; }
            public NarrativeLine[] Lines { get; }
            public float Speed { get; }
            public int Priority { get; }
        }

        // ------------------------------------------------------------ Menu --

        [MenuItem("Tools/TW08/Production/Build Narrative")]
        public static void BuildFromMenu()
        {
            NarrativeCatalog catalog = EnsureCatalog();
            if (catalog == null)
            {
                EditorUtility.DisplayDialog(
                    "The Warehouse Nº 08 — Narrativa",
                    $"O catálogo não pôde ser lido de {CatalogPath} depois da geração.",
                    "OK");
                return;
            }

            int lineCount = 0;
            foreach (NarrativeSequence sequence in catalog.Sequences)
            {
                if (sequence != null)
                {
                    lineCount += sequence.Lines.Count;
                }
            }

            Selection.activeObject = catalog;
            EditorUtility.DisplayDialog(
                "The Warehouse Nº 08 — Narrativa",
                $"Roteiro materializado.\n\n" +
                $"Sequências: {catalog.Sequences.Count}\n" +
                $"Falas: {lineCount}\n" +
                $"Elenco: {(catalog.Roster != null ? "ligado" : "AUSENTE — rode Build Full Expansion Data")}",
                "OK");
        }

        [MenuItem("Tools/TW08/Production/Reset Narrative Progress")]
        public static void ResetProgressFromMenu()
        {
            NarrativeCatalog catalog = AssetDatabase.LoadAssetAtPath<NarrativeCatalog>(CatalogPath);
            if (catalog == null)
            {
                EditorUtility.DisplayDialog(
                    "The Warehouse Nº 08 — Narrativa",
                    "Catálogo ausente. Rode Build Narrative antes.",
                    "OK");
                return;
            }

            NarrativeProgressStore.ClearAll(catalog.Sequences);
            EditorUtility.DisplayDialog(
                "The Warehouse Nº 08 — Narrativa",
                "Progresso narrativo zerado. Todas as cutscenes voltam a tocar.",
                "OK");
        }

        // ----------------------------------------------------------- Build --

        /// <summary>Cria/atualiza os assets de sequência e o catálogo. Devolve o catálogo já relido do disco.</summary>
        public static NarrativeCatalog EnsureCatalog()
        {
            TW08ProductionSceneUtility.EnsureFolder(SequenceRoot);
            TW08ProductionSceneUtility.EnsureFolder(ResourceRoot);

            SequenceSpec[] specs = BuildScript();
            List<NarrativeSequence> sequences = new();
            foreach (SequenceSpec spec in specs)
            {
                sequences.Add(EnsureSequence(spec));
            }

            CharacterRoster roster = AssetDatabase.LoadAssetAtPath<CharacterRoster>(
                TW08ExpansionDataSetup.RosterPath);

            NarrativeCatalog catalog = LoadOrCreate<NarrativeCatalog>(CatalogPath);
            catalog.ConfigureAuthoring(roster, sequences, true);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Refresh invalida os wrappers nativos criados antes dele, mesmo com os
            // assets intactos no disco. Recarregar pelo caminho é obrigatório aqui.
            return AssetDatabase.LoadAssetAtPath<NarrativeCatalog>(CatalogPath);
        }

        private static NarrativeSequence EnsureSequence(SequenceSpec spec)
        {
            string path = $"{SequenceRoot}/{spec.FileName}.asset";
            NarrativeSequence sequence = LoadOrCreate<NarrativeSequence>(path);
            sequence.ConfigureAuthoring(
                spec.Id,
                spec.Title,
                spec.Trigger,
                spec.SectorId,
                spec.LevelId,
                true,
                spec.Priority,
                spec.Speed,
                spec.Lines);
            EditorUtility.SetDirty(sequence);
            return sequence;
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        // --------------------------------------------------------- Roteiro --

        private static NarrativeLine John(string text, NarrativeTone tone = NarrativeTone.Neutro)
        {
            return new NarrativeLine("john", text, tone);
        }

        private static NarrativeLine Duda(string text, NarrativeTone tone = NarrativeTone.Memoria)
        {
            return new NarrativeLine("duda", text, tone);
        }

        private static NarrativeLine Robert(string text, NarrativeTone tone = NarrativeTone.Neutro)
        {
            return new NarrativeLine("robert", text, tone);
        }

        private static NarrativeLine Automacao(string text)
        {
            return new NarrativeLine("sistema", text, NarrativeTone.Sistema);
        }

        private static NarrativeLine Terminal(string text)
        {
            return new NarrativeLine("terminal", text, NarrativeTone.Sistema);
        }

        private static SequenceSpec[] BuildScript()
        {
            return new[]
            {
                // ---------------------------------------------- Abertura --
                new SequenceSpec(
                    "NARR_00_Abertura",
                    "narr-abertura",
                    "Turno de Emergência",
                    NarrativeTriggerKind.Opening,
                    string.Empty,
                    "TW08_Level01_FirstShift",
                    new[]
                    {
                        Automacao("Armazém Nº 08. Falha operacional detectada."),
                        Automacao("Rota não autorizada. Setor indisponível."),
                        Automacao("Operador manual necessário."),
                        John("Três e dez da manhã. Claro que sou eu.", NarrativeTone.Seco),
                        Robert("Bom, John... pelo menos hoje não tá monótono."),
                        John("Só me diga que ainda temos energia."),
                        Robert("Temos algo parecido com energia. Não recomendo elogiar."),
                        John("Me abre o Recebimento. Eu empurro o resto."),
                        Robert("Já tá aberto. Foi a única porta que o sistema não teve coragem de trancar."),
                        John("A escala do turno ainda está pregada na parede?"),
                        Robert("Está. Com dois nomes."),
                        Robert("O meu e o seu.", NarrativeTone.Seco)
                    },
                    priority: 10),

                // ------------------------------------- Setor 01 — entrada --
                new SequenceSpec(
                    "NARR_01_S01_Recebimento",
                    "narr-setor-s01",
                    "Setor 01 — Recebimento",
                    NarrativeTriggerKind.SectorEntry,
                    "S01",
                    string.Empty,
                    new[]
                    {
                        John("Recebimento. Cada caixa aqui devia ter uma etiqueta e um destino."),
                        John("Metade não tem nem uma coisa nem outra.", NarrativeTone.Seco),
                        Robert("O sistema ficou remanejando carga a noite inteira, John. Sozinho, no escuro."),
                        John("Remanejando ou procurando?"),
                        Robert("..."),
                        Robert("Empurra a caixa, veterano. Pergunta difícil a gente faz depois do café.")
                    }),

                // ------------------- Primeira mensagem da Duda (fase 01) --
                new SequenceSpec(
                    "NARR_02_Duda_PrimeiraMensagem",
                    "narr-duda-primeira-mensagem",
                    "Não confie no painel",
                    NarrativeTriggerKind.LevelCompleted,
                    string.Empty,
                    "TW08_Level01_FirstShift",
                    new[]
                    {
                        Automacao("Terminal manual restaurado. Registro de operador anterior: pendente."),
                        Duda("John, se você está ouvindo isso, não confie no painel principal."),
                        Duda("Ele mostra o armazém que a empresa quer que exista."),
                        John("Duda?", NarrativeTone.Tenso),
                        Automacao("Mensagem sem origem. Descartada."),
                        John("Descartada uma ova.", NarrativeTone.Seco)
                    }),

                // ------------------------------------- Setor 02 — entrada --
                new SequenceSpec(
                    "NARR_03_S02_Expedicao",
                    "narr-setor-s02",
                    "Setor 02 — Expedição",
                    NarrativeTriggerKind.SectorEntry,
                    "S02",
                    string.Empty,
                    new[]
                    {
                        Automacao("Rota ideal recalculada. Prioridade de saída: doca B-12."),
                        John("Ele repete B-12 desde que eu entrei no setor."),
                        John("B-12."),
                        John("A B-12 foi lacrada em dois mil e dezenove. Eu ajudei a lacrar.", NarrativeTone.Tenso),
                        John("Ela deixou num terminal de expedição."),
                        Robert("Lugar que ninguém olha."),
                        John("Ela sabia que eu olho.")
                    }),

                // ------------------------------------- Setor 03 — entrada --
                new SequenceSpec(
                    "NARR_04_S03_CamaraFria",
                    "narr-setor-s03",
                    "Setor 03 — Câmara Fria",
                    NarrativeTriggerKind.SectorEntry,
                    "S03",
                    string.Empty,
                    new[]
                    {
                        Robert("Câmara fria. Veste o casaco e não confia no chão."),
                        John("O gelo não é o problema. O problema é carga que não para onde eu mando."),
                        Robert("Tem uma coisa esquisita aqui, John. O compressor roda em setor vazio faz seis meses."),
                        John("Alguém está pagando conta de luz para congelar nada."),
                        Duda("Nem toda carga parada está esperando destino. Algumas estão segurando caminho."),
                        John("Ou escondendo um.", NarrativeTone.Tenso)
                    }),

                // ------------------------------------- Setor 04 — entrada --
                new SequenceSpec(
                    "NARR_05_S04_Automacao",
                    "narr-setor-s04",
                    "Setor 04 — Automação",
                    NarrativeTriggerKind.SectorEntry,
                    "S04",
                    string.Empty,
                    new[]
                    {
                        Automacao("Bem-vindo ao setor de automação. Eficiência operacional: 99,4%."),
                        John("Noventa e nove vírgula quatro. E o armazém inteiro travado.", NarrativeTone.Seco),
                        Robert("Essas esteiras ligam sozinhas de madrugada. Rodam vazias e desligam."),
                        John("Vazias no mapa. Alguém devia conferir se estão vazias no chão."),
                        Robert("As autônomas nunca entram no corredor C. Contornam. Sempre contornaram."),
                        John("Máquina não tem medo. Máquina tem instrução.", NarrativeTone.Tenso),
                        Duda("Erro não se repete com esse capricho. Isso não é falha, é padrão."),
                        John("Padrão. Ela falava essa palavra que nem quem reza.")
                    }),

                // ------------------------------------- Setor 05 — entrada --
                new SequenceSpec(
                    "NARR_06_S05_ManutencaoPesada",
                    "narr-setor-s05",
                    "Setor 05 — Manutenção Pesada",
                    NarrativeTriggerKind.SectorEntry,
                    "S05",
                    string.Empty,
                    new[]
                    {
                        Robert("Oficina N-8. Cuidado com o degrau — ele nunca foi automatizado."),
                        John("Você está aqui embaixo esse tempo todo?"),
                        Robert("Desde o lockdown. O sistema esqueceu de me apagar. Eu não reclamei."),
                        John("As máquinas velhas ainda andam?"),
                        Robert("Andam. São as únicas aqui que não aprenderam a mentir."),
                        Robert("Pega a chave inglesa, John. A partir daqui a gente abre no braço.")
                    }),

                // ----------------------- Virada do Ato 2 — Robert confessa --
                new SequenceSpec(
                    "NARR_07_Robert_Confissao",
                    "narr-robert-confissao",
                    "A porta que Robert abriu",
                    NarrativeTriggerKind.LevelCompleted,
                    string.Empty,
                    "TW08_Level24_OldGenerator",
                    new[]
                    {
                        Robert("Desliga o gerador um minuto, John. Eu falo melhor no escuro."),
                        Robert("Eu abri aquela porta para ela. Achei que era só mais uma teimosia da Duda."),
                        Robert("Não era.", NarrativeTone.Tenso),
                        John("Quanto tempo depois?", NarrativeTone.Tenso),
                        Robert("Onze minutos. Depois o nome dela sumiu da escala do turno."),
                        Robert("Como se ela nunca tivesse batido ponto aqui."),
                        John("Você carregou isso sozinho esse tempo todo."),
                        Robert("Eu carrego peso, John. É o que eu faço."),
                        John("Esse não. Esse a gente divide.")
                    }),

                // ------------------------------------- Setor 06 — entrada --
                new SequenceSpec(
                    "NARR_08_S06_RotasFantasma",
                    "narr-setor-s06",
                    "Setor 06 — Rotas Fantasma",
                    NarrativeTriggerKind.SectorEntry,
                    "S06",
                    string.Empty,
                    new[]
                    {
                        Automacao("Setor não consta no mapa oficial. Retorne à rota autorizada."),
                        John("O mapa oficial mente desde a primeira caixa.", NarrativeTone.Seco),
                        Terminal("Origem: apagada. Destino: Setor 08. Operador anterior: inexistente."),
                        John("Inexistente."),
                        John("Tinha um Elias nesse turno. Cauteloso. Anotava tudo à mão.", NarrativeTone.Tenso),
                        John("Agora ele é um campo vazio.", NarrativeTone.Seco),
                        Duda("Não deixei nomes nos arquivos. Deixei nas rotas."),
                        John("Então me leva pela rota, Duda."),
                        John("Chão eu leio. Foi pra isso que me chamaram.")
                    }),

                // --------------------------------------------- Desfecho --
                new SequenceSpec(
                    "NARR_09_Desfecho_Nucleo",
                    "narr-desfecho-nucleo",
                    "Núcleo Logístico",
                    NarrativeTriggerKind.Ending,
                    string.Empty,
                    "TW08_Level30_LogisticsCore",
                    new[]
                    {
                        Automacao("Núcleo logístico restaurado. Rotas consolidadas."),
                        Automacao("Registro histórico recuperado: escala do turno da noite."),
                        Terminal("Hayes, R.  —  Miller, J.  —  Rocha, M. E.  —  Elias."),
                        John("Quatro nomes."),
                        John("De manhã tinha dois.", NarrativeTone.Tenso),
                        Duda("Eles não esconderam os dados em arquivos. Esconderam nas rotas."),
                        Duda("Eu sabia que você ia demorar."),
                        Duda("Você nunca deixou um turno pela metade. Nem quando devia."),
                        John("Demorei, Duda. Demorei demais.", NarrativeTone.Tenso),
                        Robert("A oficina está com energia, John. O que você quiser abrir daqui, abre."),
                        John("Modo manual."),
                        Automacao("Comando não reconhecido."),
                        John("Vai reconhecer.", NarrativeTone.Seco)
                    },
                    priority: 10)
            };
        }
    }
}
#endif
