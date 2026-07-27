using System;

namespace TW08.Race
{
    [Serializable]
    public readonly struct RaceResult
    {
        public readonly string RacerId;
        public readonly float TimeSeconds;
        public readonly float CargoDamage;
        public readonly int Medal;

        public RaceResult(string racerId, float timeSeconds, float cargoDamage, int medal)
        {
            RacerId = racerId;
            TimeSeconds = timeSeconds;
            CargoDamage = cargoDamage;
            Medal = medal;
        }
    }
}
