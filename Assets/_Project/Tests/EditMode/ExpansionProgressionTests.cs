using System.Reflection;
using NUnit.Framework;
using TW08.Race;
using TW08.Save;
using UnityEngine;

namespace TW08.Tests.EditMode
{
    public sealed class ExpansionProgressionTests
    {
        [Test]
        public void SaveMigrationV1ToV2InitializesNewFields()
        {
            SaveGameData legacy = new()
            {
                version = 1,
                selectedCharacterId = null,
                masterVolume = 0f,
                musicVolume = 0f,
                sfxVolume = 0f
            };

            SaveGameData migrated = new SaveMigrationV1ToV2().Migrate(legacy);

            Assert.That(migrated.version, Is.EqualTo(2));
            Assert.That(migrated.selectedCharacterId, Is.EqualTo("john"));
            Assert.That(migrated.unlockedCharacters, Does.Contain("john"));
            Assert.That(migrated.unlockedCharacters, Does.Contain("duda"));
            Assert.That(migrated.masterVolume, Is.EqualTo(1f));
            Assert.That(migrated.musicVolume, Is.EqualTo(0.8f));
            Assert.That(migrated.sfxVolume, Is.EqualTo(1f));
        }

        [Test]
        public void RaceDefinitionAwardsExpectedMedals()
        {
            RaceDefinition race = ScriptableObject.CreateInstance<RaceDefinition>();
            SetField(race, "bronzeTime", 75f);
            SetField(race, "silverTime", 65f);
            SetField(race, "goldTime", 58f);
            SetField(race, "platinumTime", 52f);
            SetField(race, "maximumCargoDamageForGold", 5f);

            Assert.That(race.EvaluateMedal(50f, 0f), Is.EqualTo(4));
            Assert.That(race.EvaluateMedal(56f, 3f), Is.EqualTo(3));
            Assert.That(race.EvaluateMedal(63f, 8f), Is.EqualTo(2));
            Assert.That(race.EvaluateMedal(72f, 9f), Is.EqualTo(1));
            Assert.That(race.EvaluateMedal(90f, 0f), Is.Zero);
            Object.DestroyImmediate(race);
        }

        private static void SetField<T>(object target, string name, T value)
        {
            FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing field '{name}'.");
            field.SetValue(target, value);
        }
    }
}
