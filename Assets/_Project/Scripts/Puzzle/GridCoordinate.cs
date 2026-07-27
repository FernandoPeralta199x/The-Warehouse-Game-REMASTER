using System;
using UnityEngine;

namespace TW08.Puzzle
{
    [Serializable]
    public struct GridCoordinate : IEquatable<GridCoordinate>
    {
        [SerializeField] private int x;
        [SerializeField] private int y;

        public int X => x;
        public int Y => y;
        public int ManhattanLength => Mathf.Abs(x) + Mathf.Abs(y);

        public GridCoordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2Int ToVector2Int() => new(x, y);
        public Vector3 ToWorld(float cellSize) => new(x * cellSize, y * cellSize, 0f);

        public bool Equals(GridCoordinate other) => x == other.x && y == other.y;
        public override bool Equals(object obj) => obj is GridCoordinate other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(x, y);
        public override string ToString() => $"({x}, {y})";

        public static GridCoordinate operator +(GridCoordinate a, GridCoordinate b) => new(a.x + b.x, a.y + b.y);
        public static GridCoordinate operator -(GridCoordinate a, GridCoordinate b) => new(a.x - b.x, a.y - b.y);
        public static bool operator ==(GridCoordinate a, GridCoordinate b) => a.Equals(b);
        public static bool operator !=(GridCoordinate a, GridCoordinate b) => !a.Equals(b);

        public static readonly GridCoordinate Up = new(0, 1);
        public static readonly GridCoordinate Down = new(0, -1);
        public static readonly GridCoordinate Left = new(-1, 0);
        public static readonly GridCoordinate Right = new(1, 0);
    }
}
