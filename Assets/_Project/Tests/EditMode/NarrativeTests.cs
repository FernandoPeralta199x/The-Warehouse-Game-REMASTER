using System.Collections.Generic;
using NUnit.Framework;
using TW08.Narrative;
using UnityEngine;

namespace TW08.Tests.EditMode
{
    /// <summary>
    /// Cobre a lógica pura da narrativa: cursor de falas, casamento de gatilho
    /// por setor/fase, resolução no catálogo, persistência de playOnce e o que
    /// acontece quando o dado chega faltando.
    /// </summary>
    public sealed class NarrativeTests
    {
        private const string PersistentId = "narr-teste-persistencia";

        private readonly List<Object> spawned = new();
        private readonly List<string> touchedIds = new();

        [TearDown]
        public void TearDown()
        {
            // playOnce grava em PlayerPrefs de verdade: sem esta limpeza a segunda
            // execução da suíte encontraria as sequências já marcadas como vistas.
            NarrativeProgressStore.Clear(PersistentId);
            foreach (string id in touchedIds)
            {
                NarrativeProgressStore.Clear(id);
            }

            touchedIds.Clear();

            foreach (Object item in spawned)
            {
                if (item != null)
                {
                    Object.DestroyImmediate(item);
                }
            }

            spawned.Clear();
        }

        // -------------------------------------------------------- Playback --

        [Test]
        public void Playback_SkipsBlankLinesAndStopsAtTheEnd()
        {
            NarrativePlayback playback = new(new[]
            {
                new NarrativeLine("john", "   "),
                new NarrativeLine("robert", "Temos algo parecido com energia."),
                null,
                new NarrativeLine("john", "Só me diga que ainda temos energia.")
            });

            Assert.AreEqual("robert", playback.Current.SpeakerId);
            Assert.IsTrue(playback.Advance());
            Assert.AreEqual("john", playback.Current.SpeakerId);
            Assert.IsFalse(playback.Advance());
            Assert.IsTrue(playback.IsFinished);
            Assert.IsNull(playback.Current);
        }

        [Test]
        public void Playback_WithoutUsableContentIsFinishedImmediately()
        {
            Assert.IsTrue(new NarrativePlayback(null).IsFinished);
            Assert.IsTrue(new NarrativePlayback(new NarrativeLine[] { null }).IsFinished);
            Assert.IsTrue(new NarrativePlayback(new[] { new NarrativeLine("john", string.Empty) }).IsFinished);
        }

        [Test]
        public void Playback_RewindReturnsToTheFirstUsableLine()
        {
            NarrativePlayback playback = new(new[]
            {
                new NarrativeLine("duda", "Não deixei nomes nos arquivos."),
                new NarrativeLine("john", "Deixou nas rotas.")
            });

            playback.Advance();
            playback.Rewind();

            Assert.AreEqual(0, playback.Index);
            Assert.AreEqual("duda", playback.Current.SpeakerId);
        }

        // -------------------------------------------------------- Matching --

        [Test]
        public void Matching_EmptyFilterActsAsAWildcard()
        {
            NarrativeContext context = new(NarrativeTriggerKind.SectorEntry, "S03", "TW08_Level13_RefrigeratedCargo");

            Assert.IsTrue(NarrativeMatching.Matches(NarrativeTriggerKind.SectorEntry, "S03", string.Empty, context));
            Assert.IsTrue(NarrativeMatching.Matches(NarrativeTriggerKind.SectorEntry, null, null, context));
        }

        [Test]
        public void Matching_IsCaseAndWhitespaceInsensitive()
        {
            NarrativeContext context = new(NarrativeTriggerKind.LevelCompleted, "s05", " TW08_Level24_OldGenerator ");

            Assert.IsTrue(NarrativeMatching.Matches(
                NarrativeTriggerKind.LevelCompleted, "S05", "tw08_level24_oldgenerator", context));
        }

        [Test]
        public void Matching_RejectsAnotherSectorAnotherLevelAndAnotherMoment()
        {
            NarrativeContext context = new(NarrativeTriggerKind.SectorEntry, "S02", "TW08_Level06_WeightGate");

            Assert.IsFalse(NarrativeMatching.Matches(NarrativeTriggerKind.SectorEntry, "S04", string.Empty, context));
            Assert.IsFalse(NarrativeMatching.Matches(
                NarrativeTriggerKind.SectorEntry, "S02", "TW08_Level07_OutboundOrder", context));
            Assert.IsFalse(NarrativeMatching.Matches(NarrativeTriggerKind.Opening, "S02", string.Empty, context));
        }

        [Test]
        public void Matching_ManualNeverFiresByContext()
        {
            NarrativeContext context = new(NarrativeTriggerKind.Manual, "S01", "TW08_Level01_FirstShift");

            Assert.IsFalse(NarrativeMatching.Matches(NarrativeTriggerKind.Manual, "S01", string.Empty, context));
        }

        [Test]
        public void Matching_LevelFilterOutweighsSectorFilter()
        {
            Assert.Greater(
                NarrativeMatching.Specificity(string.Empty, "TW08_Level30_LogisticsCore"),
                NarrativeMatching.Specificity("S06", string.Empty));
            Assert.AreEqual(0, NarrativeMatching.Specificity(null, null));
        }

        // --------------------------------------------------------- Catálogo --

        [Test]
        public void Catalog_PrefersTheLevelSpecificSequenceOverTheSectorOne()
        {
            NarrativeSequence sector = MakeSequence(
                "s06-entry", NarrativeTriggerKind.SectorEntry, "S06", string.Empty);
            NarrativeSequence level = MakeSequence(
                "s06-core", NarrativeTriggerKind.SectorEntry, "S06", "TW08_Level30_LogisticsCore");
            NarrativeCatalog catalog = MakeCatalog(sector, level);

            NarrativeContext context = new(
                NarrativeTriggerKind.SectorEntry, "S06", "TW08_Level30_LogisticsCore");

            Assert.AreSame(level, catalog.Resolve(context));
        }

        [Test]
        public void Catalog_FallsBackToTheNextCandidateWhenThePreferredOneIsNotEligible()
        {
            NarrativeSequence sector = MakeSequence(
                "s06-entry", NarrativeTriggerKind.SectorEntry, "S06", string.Empty);
            NarrativeSequence level = MakeSequence(
                "s06-core", NarrativeTriggerKind.SectorEntry, "S06", "TW08_Level30_LogisticsCore");
            NarrativeCatalog catalog = MakeCatalog(sector, level);

            NarrativeContext context = new(
                NarrativeTriggerKind.SectorEntry, "S06", "TW08_Level30_LogisticsCore");

            Assert.AreSame(sector, catalog.Resolve(context, candidate => candidate != level));
        }

        [Test]
        public void Catalog_IgnoresNullEntriesAndSequencesWithoutLines()
        {
            NarrativeSequence empty = ScriptableObject.CreateInstance<NarrativeSequence>();
            spawned.Add(empty);
            empty.ConfigureAuthoring(
                "vazia", "Vazia", NarrativeTriggerKind.SectorEntry, "S01", string.Empty, true, 0, 38f, null);

            NarrativeCatalog catalog = MakeCatalog(null, empty);
            NarrativeContext context = new(NarrativeTriggerKind.SectorEntry, "S01", "TW08_Level01_FirstShift");

            Assert.IsNull(catalog.Resolve(context));
            Assert.IsNull(catalog.Find("nao-existe"));
            Assert.IsNull(catalog.Find(null));
        }

        [Test]
        public void Catalog_FindLocatesBySequenceIdIgnoringCase()
        {
            NarrativeSequence opening = MakeSequence(
                "narr-abertura", NarrativeTriggerKind.Opening, string.Empty, "TW08_Level01_FirstShift");
            NarrativeCatalog catalog = MakeCatalog(opening);

            Assert.AreSame(opening, catalog.Find("NARR-ABERTURA"));
        }

        // ---------------------------------------------------------- Serviço --

        [Test]
        public void Service_WalksEveryLineAndThenCompletes()
        {
            NarrativeService service = MakeService();
            NarrativeSequence sequence = MakeSequence(
                "avanco", NarrativeTriggerKind.Manual, string.Empty, string.Empty, lineCount: 3);

            int completed = 0;
            service.SequenceCompleted += _ => completed++;

            Assert.IsTrue(service.TryStart(sequence));
            Assert.AreEqual(0, service.Playback.Index);
            Assert.IsTrue(service.Advance());
            Assert.IsTrue(service.Advance());
            Assert.IsFalse(service.Advance());

            Assert.AreEqual(1, completed);
            Assert.IsFalse(service.IsPlaying);
            Assert.IsNull(service.Playback);
        }

        [Test]
        public void Service_RefusesASecondSequenceWhileOneIsOnScreen()
        {
            NarrativeService service = MakeService();
            NarrativeSequence first = MakeSequence("um", NarrativeTriggerKind.Manual, string.Empty, string.Empty);
            NarrativeSequence second = MakeSequence("dois", NarrativeTriggerKind.Manual, string.Empty, string.Empty);

            Assert.IsTrue(service.TryStart(first));
            Assert.IsFalse(service.TryStart(second));
            Assert.AreSame(first, service.Current);
        }

        [Test]
        public void Service_QueueChainsTheOpeningIntoTheSectorEntry()
        {
            NarrativeService service = MakeService();
            NarrativeSequence opening = MakeSequence("abertura", NarrativeTriggerKind.Opening, string.Empty, string.Empty);
            NarrativeSequence sector = MakeSequence("setor", NarrativeTriggerKind.SectorEntry, "S01", string.Empty);

            Assert.IsTrue(service.Enqueue(opening));
            Assert.IsTrue(service.Enqueue(sector));
            // Enfileirar duas vezes não duplica a cutscene.
            Assert.IsFalse(service.Enqueue(sector));

            Assert.IsTrue(service.PlayQueued());
            Assert.AreSame(opening, service.Current);
            Assert.IsTrue(service.HasPending);

            service.CompleteCurrent();

            Assert.AreSame(sector, service.Current);
            Assert.IsFalse(service.HasPending);
        }

        [Test]
        public void Service_SkipAllDropsTheQueueAndClosesTheScene()
        {
            NarrativeService service = MakeService();
            NarrativeSequence onScreen = MakeSequence("a", NarrativeTriggerKind.Manual, string.Empty, string.Empty);
            NarrativeSequence queued = MakeSequence("b", NarrativeTriggerKind.Manual, string.Empty, string.Empty);
            service.Enqueue(onScreen);
            service.Enqueue(queued);
            service.PlayQueued();

            service.SkipAll();

            Assert.IsFalse(service.IsPlaying);
            Assert.IsFalse(service.HasPending);
            // Quem pulou não quer reencontrar a cena pulada na próxima fase.
            Assert.IsTrue(service.HasPlayed(onScreen));
            Assert.IsTrue(service.HasPlayed(queued));
        }

        [Test]
        public void Service_HandlesNullAndEmptySequencesWithoutThrowing()
        {
            NarrativeService service = MakeService();
            NarrativeSequence empty = ScriptableObject.CreateInstance<NarrativeSequence>();
            spawned.Add(empty);
            empty.ConfigureAuthoring(
                "sem-falas", "Sem falas", NarrativeTriggerKind.Manual, string.Empty, string.Empty, true, 0, 38f, null);

            Assert.IsFalse(service.TryStart(null));
            Assert.IsFalse(service.TryStart(empty));
            Assert.IsFalse(service.Enqueue(null));
            Assert.IsFalse(service.Advance());
            Assert.DoesNotThrow(service.CompleteCurrent);
            Assert.IsFalse(service.HasPlayed(null));
        }

        // ------------------------------------------------------ Persistência --

        [Test]
        public void PlayOnce_SurvivesAcrossServiceInstances()
        {
            NarrativeSequence sequence = MakeSequence(
                PersistentId, NarrativeTriggerKind.Opening, string.Empty, string.Empty);

            NarrativeService first = MakeService();
            Assert.IsTrue(first.TryStart(sequence));
            first.CompleteCurrent();

            // Nova sessão: o serviço em memória não sabe de nada, o PlayerPrefs sabe.
            NarrativeService second = MakeService();
            Assert.IsTrue(second.HasPlayed(sequence));
            Assert.IsFalse(second.IsEligible(sequence));
            Assert.IsFalse(second.TryStart(sequence));
        }

        [Test]
        public void PlayOnce_ResetPutsTheSequenceBackInRotation()
        {
            NarrativeSequence sequence = MakeSequence(
                PersistentId, NarrativeTriggerKind.Opening, string.Empty, string.Empty);

            NarrativeProgressStore.MarkPlayed(sequence.SequenceId);
            Assert.IsTrue(NarrativeProgressStore.HasPlayed(sequence.SequenceId));

            NarrativeProgressStore.Clear(sequence.SequenceId);

            Assert.IsFalse(NarrativeProgressStore.HasPlayed(sequence.SequenceId));
            Assert.IsTrue(MakeService().IsEligible(sequence));
        }

        [Test]
        public void RepeatableSequence_IgnoresTheAlreadyPlayedMark()
        {
            NarrativeSequence sequence = ScriptableObject.CreateInstance<NarrativeSequence>();
            spawned.Add(sequence);
            sequence.ConfigureAuthoring(
                PersistentId,
                "Repetível",
                NarrativeTriggerKind.LevelStart,
                string.Empty,
                string.Empty,
                false,
                0,
                38f,
                new[] { new NarrativeLine("john", "Só mais um turno.") });

            NarrativeProgressStore.MarkPlayed(PersistentId);

            NarrativeService service = MakeService();
            Assert.IsTrue(service.HasPlayed(sequence));
            Assert.IsTrue(service.IsEligible(sequence));
        }

        // -------------------------------------------------------- Sequência --

        [Test]
        public void Sequence_UsesTheLineSpeedWhenItOverridesTheDefault()
        {
            NarrativeSequence sequence = ScriptableObject.CreateInstance<NarrativeSequence>();
            spawned.Add(sequence);

            NarrativeLine slow = new("duda", "Eu sabia que você ia demorar.", NarrativeTone.Memoria, 12f);
            NarrativeLine plain = new("john", "Demorei.");
            sequence.ConfigureAuthoring(
                "ritmo", "Ritmo", NarrativeTriggerKind.Manual, string.Empty, string.Empty, true, 0, 40f,
                new[] { slow, plain });

            Assert.AreEqual(12f, sequence.ResolveSpeed(slow), 0.001f);
            Assert.AreEqual(40f, sequence.ResolveSpeed(plain), 0.001f);
            Assert.AreEqual(40f, sequence.ResolveSpeed(null), 0.001f);
        }

        // ---------------------------------------------------------- Fixtures --

        private NarrativeService MakeService()
        {
            GameObject host = new("Narrative Service Test");
            spawned.Add(host);
            return host.AddComponent<NarrativeService>();
        }

        private NarrativeSequence MakeSequence(
            string id, NarrativeTriggerKind trigger, string sectorId, string levelId, int lineCount = 2)
        {
            NarrativeSequence sequence = ScriptableObject.CreateInstance<NarrativeSequence>();
            spawned.Add(sequence);
            touchedIds.Add(id);

            NarrativeLine[] lines = new NarrativeLine[lineCount];
            for (int i = 0; i < lineCount; i++)
            {
                lines[i] = new NarrativeLine("john", $"Fala {i + 1} de {id}.");
            }

            sequence.ConfigureAuthoring(id, id, trigger, sectorId, levelId, true, 0, 38f, lines);
            return sequence;
        }

        private NarrativeCatalog MakeCatalog(params NarrativeSequence[] sequences)
        {
            NarrativeCatalog catalog = ScriptableObject.CreateInstance<NarrativeCatalog>();
            spawned.Add(catalog);
            catalog.ConfigureAuthoring(null, sequences, false);
            return catalog;
        }
    }
}
