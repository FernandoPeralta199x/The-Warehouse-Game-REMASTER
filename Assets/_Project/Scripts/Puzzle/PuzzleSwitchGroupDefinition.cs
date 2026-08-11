using System;
using System.Collections.Generic;
using UnityEngine;

namespace TW08.Puzzle
{
    [Serializable]
    public sealed class PuzzleSwitchGroupDefinition
    {
        [SerializeField] private string id = "switch-a";
        [SerializeField] private List<GridCoordinate> sensors = new();
        [SerializeField] private List<GridCoordinate> doors = new();

        public string Id => id;
        public IReadOnlyList<GridCoordinate> Sensors => sensors;
        public IReadOnlyList<GridCoordinate> Doors => doors;
    }
}
