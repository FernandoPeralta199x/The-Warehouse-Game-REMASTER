using System;
using UnityEngine;

namespace TW08.Puzzle
{
    [Serializable]
    public sealed class PuzzleGoalRequirementDefinition
    {
        [SerializeField] private GridCoordinate position;
        [SerializeField] private PuzzleEntityKind requiredKind = PuzzleEntityKind.Crate;

        public GridCoordinate Position => position;
        public PuzzleEntityKind RequiredKind => requiredKind;
    }
}
