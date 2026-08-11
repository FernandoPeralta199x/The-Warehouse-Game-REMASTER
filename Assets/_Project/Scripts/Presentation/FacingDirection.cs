using UnityEngine;

namespace TW08.Presentation
{
    public enum FacingDirection
    {
        Down = 0,
        Up = 1,
        Left = 2,
        Right = 3
    }

    public static class FacingDirectionUtility
    {
        public static FacingDirection FromDelta(Vector2Int delta, FacingDirection fallback = FacingDirection.Down)
        {
            if (delta == Vector2Int.zero)
            {
                return fallback;
            }

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                return delta.x < 0 ? FacingDirection.Left : FacingDirection.Right;
            }

            return delta.y < 0 ? FacingDirection.Down : FacingDirection.Up;
        }
    }
}
