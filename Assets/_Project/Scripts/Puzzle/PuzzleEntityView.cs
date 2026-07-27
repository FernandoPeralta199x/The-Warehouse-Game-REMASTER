using UnityEngine;

namespace TW08.Puzzle
{
    [DisallowMultipleComponent]
    public sealed class PuzzleEntityView : MonoBehaviour
    {
        [SerializeField] private string entityId = "player";
        [SerializeField] private PuzzleEntityKind kind = PuzzleEntityKind.Player;
        [SerializeField] private Vector3 worldOffset;

        public string EntityId => entityId;
        public PuzzleEntityKind Kind => kind;

        public void Configure(string id, PuzzleEntityKind entityKind)
        {
            entityId = id;
            kind = entityKind;
        }

        public void Snap(GridCoordinate cell, float cellSize)
        {
            transform.position = cell.ToWorld(cellSize) + worldOffset;
        }
    }
}
