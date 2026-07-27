using System;
using UnityEngine;

namespace TW08.Puzzle
{
    [Serializable]
    public sealed class PuzzleCrateDefinition
    {
        [SerializeField] private string id = "crate";
        [SerializeField] private PuzzleEntityKind kind = PuzzleEntityKind.Crate;
        [SerializeField] private GridCoordinate position;

        public string Id => id;
        public PuzzleEntityKind Kind => kind;
        public GridCoordinate Position => position;

        public PuzzleCrateDefinition(string id, PuzzleEntityKind kind, GridCoordinate position)
        {
            this.id = id;
            this.kind = kind;
            this.position = position;
        }
    }
}
