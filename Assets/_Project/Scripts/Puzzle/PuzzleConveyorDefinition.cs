using System;
using UnityEngine;

namespace TW08.Puzzle
{
    /// <summary>Direção fixa de uma esteira.</summary>
    public enum ConveyorDirection
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3
    }

    /// <summary>
    /// Trecho de esteira. Quem entra nesta célula é levado na direção dela até
    /// sair do trecho ou encostar em algo.
    /// </summary>
    [Serializable]
    public sealed class PuzzleConveyorDefinition
    {
        [SerializeField] private GridCoordinate position;
        [SerializeField] private ConveyorDirection direction = ConveyorDirection.Right;

        public PuzzleConveyorDefinition()
        {
        }

        public PuzzleConveyorDefinition(GridCoordinate position, ConveyorDirection direction)
        {
            this.position = position;
            this.direction = direction;
        }

        public GridCoordinate Position => position;
        public ConveyorDirection Direction => direction;

        public GridCoordinate Step => direction switch
        {
            ConveyorDirection.Up => GridCoordinate.Up,
            ConveyorDirection.Down => GridCoordinate.Down,
            ConveyorDirection.Left => GridCoordinate.Left,
            _ => GridCoordinate.Right
        };
    }
}
