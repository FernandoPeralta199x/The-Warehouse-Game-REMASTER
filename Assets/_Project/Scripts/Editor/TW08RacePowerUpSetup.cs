#if UNITY_EDITOR
using System.Collections.Generic;
using TW08.PowerUps;
using UnityEditor;
using UnityEngine;

namespace TW08.Editor
{
    /// <summary>
    /// Materializa os power-ups da corrida.
    ///
    /// Os doze efeitos já estavam implementados em <c>StandardPowerUpEffect</c>,
    /// o inventário existia e a roleta ponderada também — só nunca foi criado
    /// um único asset. O sistema inteiro rodava vazio.
    ///
    /// A distribuição segue a lógica de kart: quem está atrás tira item forte,
    /// quem lidera tira item defensivo. Sem isso a corrida decide no primeiro
    /// checkpoint e as outras duas voltas não têm graça.
    /// </summary>
    public static class TW08RacePowerUpSetup
    {
        public const string PowerUpRoot = "Assets/_Project/ScriptableObjects/PowerUps";
        public const string TablePath = PowerUpRoot + "/TW08_PowerUpTable.asset";
        private const string EffectPath = PowerUpRoot + "/StandardPowerUpEffect.asset";

        private readonly struct Spec
        {
            public Spec(
                string id, string name, PowerUpType type,
                float magnitude, float duration, float radius,
                float minRank, float maxRank, float weight, Color tint)
            {
                Id = id;
                Name = name;
                Type = type;
                Magnitude = magnitude;
                Duration = duration;
                Radius = radius;
                MinRank = minRank;
                MaxRank = maxRank;
                Weight = weight;
                Tint = tint;
            }

            public string Id { get; }
            public string Name { get; }
            public PowerUpType Type { get; }
            public float Magnitude { get; }
            public float Duration { get; }
            public float Radius { get; }

            /// <summary>Faixa de posição em que o item pode sair. 0 = líder, 1 = último.</summary>
            public float MinRank { get; }
            public float MaxRank { get; }
            public float Weight { get; }
            public Color Tint { get; }
        }

        private static readonly Spec[] Specs =
        {
            // --- Aceleração: quanto mais atrás, mais forte ---
            new("turbo-compressor", "Compressor Turbo", PowerUpType.TurboCompressor,
                1.35f, 2.2f, 0f, 0.15f, 1f, 1.4f, new Color(1f, 0.63f, 0.12f, 1f)),
            new("hydraulic-nitro", "Nitro Hidráulico", PowerUpType.HydraulicNitro,
                1.7f, 2.8f, 0f, 0.55f, 1f, 1.1f, new Color(1f, 0.36f, 0.12f, 1f)),

            // --- Defesa: sai mais para quem lidera ---
            new("safety-barrier", "Barreira de Segurança", PowerUpType.SafetyBarrier,
                1f, 6f, 0f, 0f, 0.7f, 1.2f, new Color(0.26f, 0.84f, 0.92f, 1f)),
            new("cargo-stabilizer", "Estabilizador de Carga", PowerUpType.CargoStabilizer,
                1f, 7f, 0f, 0f, 0.8f, 1f, new Color(0.25f, 0.95f, 0.58f, 1f)),
            new("reinforced-suspension", "Suspensão Reforçada", PowerUpType.ReinforcedSuspension,
                1f, 8f, 0f, 0f, 0.75f, 0.9f, new Color(0.55f, 0.72f, 0.80f, 1f)),

            // --- Controle: úteis em qualquer posição ---
            new("abs-brake", "Freio ABS", PowerUpType.AbsBrake,
                1f, 5f, 0f, 0f, 1f, 0.9f, new Color(0.66f, 0.96f, 1f, 1f)),
            new("magnetic-fork", "Garfo Magnético", PowerUpType.MagneticFork,
                1f, 6f, 0f, 0.2f, 1f, 0.8f, new Color(0.78f, 0.55f, 1f, 1f)),
            new("route-scanner", "Scanner de Rota", PowerUpType.RouteScanner,
                1f, 8f, 0f, 0.3f, 1f, 0.7f, new Color(0.26f, 0.84f, 0.92f, 1f)),

            // --- Ataque: nunca para o líder ---
            new("oil-canister", "Lata de Óleo", PowerUpType.OilCanister,
                1f, 9f, 0f, 0.25f, 1f, 1.2f, new Color(0.42f, 0.36f, 0.24f, 1f)),
            new("emp-signal", "Pulso EMP", PowerUpType.EmpSignal,
                1f, 1.6f, 7f, 0.5f, 1f, 0.7f, new Color(0.55f, 0.62f, 1f, 1f)),
            new("industrial-horn", "Buzina Industrial", PowerUpType.IndustrialHorn,
                1f, 1.2f, 5.5f, 0.35f, 1f, 0.9f, new Color(1f, 0.84f, 0.32f, 1f)),

            // --- Reparo: só faz sentido para quem se danificou ---
            new("repair-kit", "Kit de Reparo", PowerUpType.RepairKit,
                35f, 0f, 0f, 0.2f, 1f, 1f, new Color(0.25f, 0.95f, 0.58f, 1f)),
        };

        [MenuItem("Tools/TW08/Production/Build Race Power-Ups")]
        public static void BuildFromMenu()
        {
            WeightedPowerUpTable table = EnsureAll();
            EditorUtility.DisplayDialog(
                "The Warehouse Nº 08 — Corrida",
                $"Power-ups materializados: {Specs.Length}.\nRoleta: {table.name}",
                "OK");
        }

        /// <summary>Cria/atualiza os assets dos power-ups e a roleta ponderada.</summary>
        public static WeightedPowerUpTable EnsureAll()
        {
            TW08ProductionSceneUtility.EnsureFolder(PowerUpRoot);

            PowerUpEffect effect = LoadOrCreate<StandardPowerUpEffect>(EffectPath);

            List<PowerUpDefinition> created = new();
            foreach (Spec spec in Specs)
            {
                created.Add(EnsurePowerUp(spec, effect));
            }

            WeightedPowerUpTable table = LoadOrCreate<WeightedPowerUpTable>(TablePath);
            SerializedObject serialized = new(table);
            SerializedProperty entries = serialized.FindProperty("entries");
            entries.arraySize = created.Count;

            for (int i = 0; i < created.Count; i++)
            {
                Spec spec = Specs[i];
                SerializedProperty element = entries.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("definition").objectReferenceValue = created[i];
                element.FindPropertyRelative("minimumRank").floatValue = spec.MinRank;
                element.FindPropertyRelative("maximumRank").floatValue = spec.MaxRank;
                element.FindPropertyRelative("weight").floatValue = spec.Weight;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();

            return AssetDatabase.LoadAssetAtPath<WeightedPowerUpTable>(TablePath);
        }

        private static PowerUpDefinition EnsurePowerUp(Spec spec, PowerUpEffect effect)
        {
            string path = $"{PowerUpRoot}/PowerUp_{spec.Id}.asset";
            PowerUpDefinition definition = LoadOrCreate<PowerUpDefinition>(path);

            SerializedObject serialized = new(definition);
            serialized.FindProperty("id").stringValue = spec.Id;
            serialized.FindProperty("displayName").stringValue = spec.Name;
            serialized.FindProperty("type").enumValueIndex = TypeIndex(spec.Type);
            serialized.FindProperty("effect").objectReferenceValue = effect;
            serialized.FindProperty("magnitude").floatValue = spec.Magnitude;
            serialized.FindProperty("duration").floatValue = spec.Duration;
            serialized.FindProperty("radius").floatValue = spec.Radius;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        /// <summary>
        /// enumValueIndex é a POSIÇÃO no enum, não o valor. O enum tem buracos
        /// históricos, então converter direto gravaria o item errado.
        /// </summary>
        private static int TypeIndex(PowerUpType type)
        {
            PowerUpType[] values = (PowerUpType[])System.Enum.GetValues(typeof(PowerUpType));
            return System.Array.IndexOf(values, type);
        }

        /// <summary>Cor do item, para o HUD e a caixa na pista.</summary>
        public static Color TintFor(string powerUpId)
        {
            foreach (Spec spec in Specs)
            {
                if (spec.Id == powerUpId)
                {
                    return spec.Tint;
                }
            }

            return Color.white;
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
    }
}
#endif
