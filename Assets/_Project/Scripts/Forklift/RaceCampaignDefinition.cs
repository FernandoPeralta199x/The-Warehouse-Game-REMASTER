using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TW08.Race
{
    [CreateAssetMenu(fileName = "RaceCampaign", menuName = "TW08/Race/Campaign Definition")]
    public sealed class RaceCampaignDefinition : ScriptableObject
    {
        [SerializeField] private List<RaceTrackDefinition> tracks = new();

        public IReadOnlyList<RaceTrackDefinition> Tracks => tracks;

        public RaceTrackDefinition Find(string trackId)
        {
            if (string.IsNullOrWhiteSpace(trackId))
            {
                return null;
            }

            return tracks.FirstOrDefault(track =>
                track != null && string.Equals(track.TrackId, trackId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
