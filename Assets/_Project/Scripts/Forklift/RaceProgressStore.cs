using UnityEngine;

namespace TW08.Race
{
    public static class RaceProgressStore
    {
        private const string Prefix = "tw08.race.";

        public static void Record(RaceTrackDefinition track, float timeSeconds, float cargoDamage = 0f)
        {
            if (track == null || timeSeconds <= 0f)
            {
                return;
            }

            string id = Normalize(track.TrackId);
            PlayerPrefs.SetInt(Prefix + id + ".completed", 1);

            float best = PlayerPrefs.GetFloat(Prefix + id + ".best", 0f);
            if (best <= 0f || timeSeconds < best)
            {
                PlayerPrefs.SetFloat(Prefix + id + ".best", timeSeconds);
            }

            int medal = track.GetMedal(timeSeconds, cargoDamage);
            int previousMedal = PlayerPrefs.GetInt(Prefix + id + ".medal", 0);
            if (medal > previousMedal)
            {
                PlayerPrefs.SetInt(Prefix + id + ".medal", medal);
            }

            PlayerPrefs.Save();
        }

        public static bool IsCompleted(string trackId)
        {
            return PlayerPrefs.GetInt(Prefix + Normalize(trackId) + ".completed", 0) == 1;
        }

        public static float GetBestTime(string trackId)
        {
            return PlayerPrefs.GetFloat(Prefix + Normalize(trackId) + ".best", 0f);
        }

        public static int GetMedal(string trackId)
        {
            return PlayerPrefs.GetInt(Prefix + Normalize(trackId) + ".medal", 0);
        }

        public static bool IsUnlocked(RaceCampaignDefinition campaign, int index)
        {
            if (campaign == null || index < 0 || index >= campaign.Tracks.Count)
            {
                return false;
            }

            if (index == 0)
            {
                return true;
            }

            RaceTrackDefinition previous = campaign.Tracks[index - 1];
            return previous != null && IsCompleted(previous.TrackId);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "unknown" : value.Trim().ToLowerInvariant();
        }
    }
}
