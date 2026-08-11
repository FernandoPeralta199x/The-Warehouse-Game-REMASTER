using System.Collections.Generic;
using NUnit.Framework;
using TW08.PowerUps;
using TW08.Race;
using UnityEngine;

namespace TW08.Tests.EditMode
{
    public sealed class RaceProductionSystemsTests
    {
        private readonly List<GameObject> objects = new();

        [TearDown]
        public void TearDown()
        {
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                GameObject go = objects[i];
                if (go != null)
                {
                    Object.DestroyImmediate(go);
                }
            }
            objects.Clear();
        }

        [Test]
        public void RaceManagerRanksRacerThatAdvancedCheckpointAhead()
        {
            RaceManager manager = Create<RaceManager>("Race Manager");
            List<RaceCheckpoint> checkpoints = new()
            {
                CreateCheckpoint(manager, 0, new Vector2(-5f, 0f)),
                CreateCheckpoint(manager, 1, new Vector2(0f, 0f)),
                CreateCheckpoint(manager, 2, new Vector2(5f, 0f))
            };
            manager.Configure(checkpoints, 2);

            RacerProgress leader = CreateRacer(manager, "leader");
            RacerProgress follower = CreateRacer(manager, "follower");
            manager.Register(leader);
            manager.Register(follower);
            manager.StartRace();

            manager.NotifyCheckpoint(leader, 1);

            Assert.That(manager.GetRacePosition(leader), Is.EqualTo(1));
            Assert.That(manager.GetRacePosition(follower), Is.EqualTo(2));
            Assert.That(manager.GetNormalizedRank(leader), Is.EqualTo(0f).Within(0.001f));
            Assert.That(manager.GetNormalizedRank(follower), Is.EqualTo(1f).Within(0.001f));
        }

        [Test]
        public void RaceManagerExposesCheckpointPositionForAiAndScanner()
        {
            RaceManager manager = Create<RaceManager>("Race Manager");
            RaceCheckpoint checkpoint = CreateCheckpoint(manager, 0, new Vector2(3.5f, -2.25f));
            manager.Configure(new[] { checkpoint }, 1);

            bool found = manager.TryGetCheckpointPosition(0, out Vector2 position);
            bool missing = manager.TryGetCheckpointPosition(7, out _);

            Assert.That(found, Is.True);
            Assert.That(position, Is.EqualTo(new Vector2(3.5f, -2.25f)));
            Assert.That(missing, Is.False);
        }

        [Test]
        public void PowerUpEnumPreservesLegacySerializedValues()
        {
            Assert.That((int)PowerUpType.TurboCompressor, Is.EqualTo(0));
            Assert.That((int)PowerUpType.SafetyBarrier, Is.EqualTo(1));
            Assert.That((int)PowerUpType.OilCanister, Is.EqualTo(2));
            Assert.That((int)PowerUpType.EmpSignal, Is.EqualTo(3));
            Assert.That((int)PowerUpType.RepairKit, Is.EqualTo(4));
            Assert.That((int)PowerUpType.CargoStabilizer, Is.GreaterThan(4));
        }

        private RaceCheckpoint CreateCheckpoint(RaceManager manager, int index, Vector2 position)
        {
            GameObject go = CreateObject("Checkpoint " + index);
            go.transform.position = position;
            BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            RaceCheckpoint checkpoint = go.AddComponent<RaceCheckpoint>();
            checkpoint.Configure(manager, index);
            return checkpoint;
        }

        private RacerProgress CreateRacer(RaceManager manager, string id)
        {
            GameObject go = CreateObject(id);
            RacerProgress progress = go.AddComponent<RacerProgress>();
            progress.Configure(manager, id);
            return progress;
        }

        private T Create<T>(string name) where T : Component
        {
            return CreateObject(name).AddComponent<T>();
        }

        private GameObject CreateObject(string name)
        {
            GameObject go = new(name);
            objects.Add(go);
            return go;
        }
    }
}
