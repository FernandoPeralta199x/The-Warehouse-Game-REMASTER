using System;
using System.Collections.Generic;
using System.Linq;
using TW08.Common;

namespace TW08.Puzzle
{
    public sealed class PuzzleBoardModel
    {
        private readonly HashSet<GridCoordinate> walls;
        private readonly HashSet<GridCoordinate> goals;
        private readonly HashSet<GridCoordinate> costlyCells;
        private readonly HashSet<GridCoordinate> iceCells;
        private readonly Dictionary<GridCoordinate, GridCoordinate> conveyors;
        private readonly HashSet<GridCoordinate> dynamicBlockedCells = new();
        private readonly List<PuzzlePatrolDefinition> patrols;
        private readonly HashSet<GridCoordinate> directionButtons;
        private readonly List<PuzzleTimedBlockDefinition> timedBlocks;
        private readonly Dictionary<GridCoordinate, PuzzleEntityKind> goalRequirements;
        private readonly Dictionary<GridCoordinate, string> crateByPosition;
        private readonly Dictionary<string, PuzzleEntityKind> crateKinds;

        public int Width { get; }
        public int Height { get; }
        public GridCoordinate PlayerPosition { get; private set; }
        public int MoveCount { get; private set; }
        public IReadOnlyCollection<GridCoordinate> Walls => walls;
        public IReadOnlyCollection<GridCoordinate> Goals => goals;
        public IReadOnlyCollection<GridCoordinate> CostlyCells => costlyCells;
        public IReadOnlyCollection<GridCoordinate> IceCells => iceCells;
        public IReadOnlyDictionary<GridCoordinate, GridCoordinate> Conveyors => conveyors;
        public IReadOnlyList<PuzzlePatrolDefinition> Patrols => patrols;

        /// <summary>Comandos executados. É o relógio que move os robôs e abre os prazos.</summary>
        public int CommandCount { get; private set; }

        /// <summary>True quando um botão de direção já foi acionado um número ímpar de vezes.</summary>
        public bool ConveyorsInverted { get; private set; }

        public IReadOnlyCollection<GridCoordinate> DirectionButtons => directionButtons;
        public IReadOnlyList<PuzzleTimedBlockDefinition> TimedBlocks => timedBlocks;
        public IReadOnlyCollection<GridCoordinate> DynamicBlockedCells => dynamicBlockedCells;
        public IReadOnlyDictionary<GridCoordinate, PuzzleEntityKind> GoalRequirements => goalRequirements;
        public IReadOnlyDictionary<GridCoordinate, string> Crates => crateByPosition;
        public bool IsComplete => EvaluateCompletion();

        public PuzzleBoardModel(PuzzleLevelDefinition level)
            : this(
                Guard.NotNull(level, nameof(level)).Width,
                level.Height,
                level.Walls,
                level.Goals,
                level.PlayerStart,
                level.Crates.ToDictionary(c => c.Id, c => c.Position),
                level.Crates.ToDictionary(c => c.Id, c => c.Kind),
                level.CostlyCells,
                level.GoalRequirements
                    .Where(requirement => requirement != null)
                    .ToDictionary(requirement => requirement.Position, requirement => requirement.RequiredKind),
                level.IceCells,
                level.Conveyors
                    .Where(conveyor => conveyor != null)
                    .ToDictionary(conveyor => conveyor.Position, conveyor => conveyor.Step),
                level.Patrols,
                level.DirectionButtons,
                level.TimedBlocks)
        {
        }

        public PuzzleBoardModel(
            int width,
            int height,
            IEnumerable<GridCoordinate> walls,
            IEnumerable<GridCoordinate> goals,
            GridCoordinate playerStart,
            IReadOnlyDictionary<string, GridCoordinate> crates,
            IReadOnlyDictionary<string, PuzzleEntityKind> kinds = null,
            IEnumerable<GridCoordinate> costlyCells = null,
            IReadOnlyDictionary<GridCoordinate, PuzzleEntityKind> goalRequirements = null,
            IEnumerable<GridCoordinate> iceCells = null,
            IReadOnlyDictionary<GridCoordinate, GridCoordinate> conveyors = null,
            IEnumerable<PuzzlePatrolDefinition> patrols = null,
            IEnumerable<GridCoordinate> directionButtons = null,
            IEnumerable<PuzzleTimedBlockDefinition> timedBlocks = null)
        {
            Width = Guard.Positive(width, nameof(width));
            Height = Guard.Positive(height, nameof(height));
            this.walls = new HashSet<GridCoordinate>(walls ?? Array.Empty<GridCoordinate>());
            this.goals = new HashSet<GridCoordinate>(goals ?? Array.Empty<GridCoordinate>());
            this.costlyCells = new HashSet<GridCoordinate>(costlyCells ?? Array.Empty<GridCoordinate>());
            this.iceCells = new HashSet<GridCoordinate>(iceCells ?? Array.Empty<GridCoordinate>());
            this.conveyors = conveyors != null
                ? new Dictionary<GridCoordinate, GridCoordinate>(conveyors)
                : new Dictionary<GridCoordinate, GridCoordinate>();
            this.patrols = new List<PuzzlePatrolDefinition>(
                (patrols ?? Array.Empty<PuzzlePatrolDefinition>()).Where(patrol => patrol != null && patrol.Route.Count > 0));
            this.directionButtons = new HashSet<GridCoordinate>(directionButtons ?? Array.Empty<GridCoordinate>());
            this.timedBlocks = new List<PuzzleTimedBlockDefinition>(
                (timedBlocks ?? Array.Empty<PuzzleTimedBlockDefinition>()).Where(block => block != null));
            this.goalRequirements = goalRequirements != null
                ? new Dictionary<GridCoordinate, PuzzleEntityKind>(goalRequirements)
                : new Dictionary<GridCoordinate, PuzzleEntityKind>();
            crateByPosition = new Dictionary<GridCoordinate, string>();
            crateKinds = new Dictionary<string, PuzzleEntityKind>();
            PlayerPosition = playerStart;

            ValidateCell(playerStart, "player");

            foreach (KeyValuePair<string, GridCoordinate> crate in crates ?? new Dictionary<string, GridCoordinate>())
            {
                Guard.NotBlank(crate.Key, nameof(crates));
                ValidateCell(crate.Value, crate.Key);

                if (crateByPosition.ContainsKey(crate.Value))
                {
                    throw new InvalidOperationException($"Two crates occupy {crate.Value}.");
                }

                crateByPosition.Add(crate.Value, crate.Key);
                crateKinds[crate.Key] = kinds != null && kinds.TryGetValue(crate.Key, out PuzzleEntityKind kind)
                    ? kind
                    : PuzzleEntityKind.Crate;
            }

            if (this.walls.Contains(PlayerPosition) || crateByPosition.ContainsKey(PlayerPosition))
            {
                throw new InvalidOperationException("Player start cell is blocked.");
            }
        }

        public bool TryMove(GridCoordinate direction, out PuzzleMove move)
        {
            move = default;

            if (direction.ManhattanLength != 1)
            {
                return false;
            }

            GridCoordinate destination = PlayerPosition + direction;

            if (!IsInside(destination) || IsBlocked(destination))
            {
                return false;
            }

            GridCoordinate previousPlayer = PlayerPosition;
            int moveCost = GetMoveCost(destination);

            // Os robôs avançam junto com este comando, então tudo é validado
            // contra onde eles VÃO estar, nunca contra onde estão agora — senão
            // o jogador poderia terminar o passo dentro de um deles.
            HashSet<GridCoordinate> robots = GetPatrolCells(CommandCount + 1);
            if (robots.Contains(destination))
            {
                return false;
            }

            if (!crateByPosition.TryGetValue(destination, out string crateId))
            {
                GridCoordinate landing = Slide(destination, direction, null, robots);
                if (robots.Contains(landing))
                {
                    return false;
                }

                PlayerPosition = landing;
                MoveCount += moveCost;
                CommandCount++;
                ApplyDirectionButton(landing);
                move = new PuzzleMove(previousPlayer, PlayerPosition, moveCost, direction);
                return true;
            }

            GridCoordinate crateDestination = destination + direction;

            if (!IsFree(crateDestination) || robots.Contains(crateDestination))
            {
                return false;
            }

            // A carga desliza primeiro e só então o jogador entra: sem essa
            // ordem o jogador ocuparia a célula que a carga ainda vai cruzar.
            GridCoordinate crateFinal = Slide(crateDestination, direction, destination, robots);
            if (robots.Contains(crateFinal))
            {
                return false;
            }

            crateByPosition.Remove(destination);
            crateByPosition.Add(crateFinal, crateId);

            // O jogador para antes da carga — ele não a atravessa nem a empurra
            // uma segunda vez no mesmo comando.
            GridCoordinate playerFinal = Slide(destination, direction, null, robots);

            // Uma esteira invertida devolve a carga para a célula que ela acabou
            // de deixar — que é exatamente onde o operador vai entrar. Sem esta
            // checagem os dois terminam empilhados na mesma célula e o tabuleiro
            // fica corrompido. O comando é recusado: a correia venceu o empurrão.
            if (robots.Contains(playerFinal) || playerFinal == crateFinal)
            {
                // Desfaz o empurrão: o comando inteiro é recusado.
                crateByPosition.Remove(crateFinal);
                crateByPosition.Add(destination, crateId);
                return false;
            }

            PlayerPosition = playerFinal;
            MoveCount += moveCost;
            CommandCount++;
            ApplyDirectionButton(playerFinal);
            move = new PuzzleMove(previousPlayer, PlayerPosition, crateId, destination, crateFinal, moveCost, direction);
            return true;
        }

        /// <summary>
        /// O botão dispara ao FIM do comando, sobre a célula onde o operador
        /// parou. Inverter no meio do deslize mudaria a correia enquanto a carga
        /// ainda a percorre, e o resultado deixaria de ser legível.
        /// </summary>
        private void ApplyDirectionButton(GridCoordinate landing)
        {
            if (directionButtons.Contains(landing))
            {
                ConveyorsInverted = !ConveyorsInverted;
            }
        }

        /// <summary>Células ocupadas pelos robôs após <paramref name="step"/> comandos.</summary>
        public HashSet<GridCoordinate> GetPatrolCells(int step)
        {
            HashSet<GridCoordinate> cells = new();
            foreach (PuzzlePatrolDefinition patrol in patrols)
            {
                cells.Add(patrol.PositionAt(step));
            }

            return cells;
        }

        /// <summary>
        /// Resolve o deslizamento a partir de <paramref name="start"/>, que já é
        /// uma célula livre e ocupada pela entidade.
        ///
        /// Gelo mantém a direção de entrada; esteira impõe a própria. Quem entra
        /// segue até pisar em piso comum ou até a próxima célula estar ocupada.
        ///
        /// <paramref name="vacated"/> é a célula que a entidade acabou de deixar
        /// e que ainda consta como ocupada pelo dicionário durante o empurrão.
        ///
        /// O teto de iterações protege contra esteiras em circuito fechado: sem
        /// ele um anel de esteiras giraria para sempre.
        /// </summary>
        private GridCoordinate Slide(
            GridCoordinate start,
            GridCoordinate direction,
            GridCoordinate? vacated,
            HashSet<GridCoordinate> robots = null)
        {
            GridCoordinate current = start;
            int guard = Width * Height;

            while (guard-- > 0)
            {
                GridCoordinate step;
                if (conveyors.TryGetValue(current, out GridCoordinate conveyorStep))
                {
                    // O botão de direção inverte a correia inteira, não só o trecho.
                    step = ConveyorsInverted
                        ? new GridCoordinate(-conveyorStep.X, -conveyorStep.Y)
                        : conveyorStep;
                }
                else if (iceCells.Contains(current))
                {
                    step = direction;
                }
                else
                {
                    return current;
                }

                GridCoordinate next = current + step;
                if (!IsSlideTargetFree(next, vacated) || (robots != null && robots.Contains(next)))
                {
                    return current;
                }

                current = next;
                direction = step;
            }

            return current;
        }

        private bool IsSlideTargetFree(GridCoordinate cell, GridCoordinate? vacated)
        {
            if (!IsInside(cell) || IsBlocked(cell))
            {
                return false;
            }

            // A célula de origem do empurrão ainda aparece ocupada no dicionário
            // enquanto a carga desliza; tratá-la como livre evita travar o deslize
            // logo no primeiro passo.
            if (vacated.HasValue && cell == vacated.Value)
            {
                return true;
            }

            return !crateByPosition.ContainsKey(cell);
        }

        public bool IsIce(GridCoordinate cell) => iceCells.Contains(cell);

        public bool TryGetConveyor(GridCoordinate cell, out GridCoordinate step)
            => conveyors.TryGetValue(cell, out step);

        public bool TryUndo(PuzzleMove move)
        {
            if (PlayerPosition != move.PlayerTo)
            {
                return false;
            }

            if (move.CrateMoved)
            {
                if (!crateByPosition.TryGetValue(move.CrateTo, out string crateId) || crateId != move.CrateId)
                {
                    return false;
                }

                crateByPosition.Remove(move.CrateTo);
                crateByPosition.Add(move.CrateFrom, move.CrateId);
            }

            // O toggle é derivado de onde o comando terminou, então desfazer é
            // reaplicar a mesma inversão — não é preciso guardá-lo no movimento.
            ApplyDirectionButton(move.PlayerTo);

            PlayerPosition = move.PlayerFrom;
            MoveCount = Math.Max(0, MoveCount - Math.Max(1, move.MoveCost));
            CommandCount = Math.Max(0, CommandCount - 1);
            return true;
        }

        public bool SetDynamicBlocked(GridCoordinate cell, bool blocked)
        {
            if (!IsInside(cell) || walls.Contains(cell))
            {
                return false;
            }

            if (blocked)
            {
                if (PlayerPosition == cell || crateByPosition.ContainsKey(cell))
                {
                    // Invariante: nunca existe porta fechada com algo dentro.
                    //
                    // Sem o Remove, desfazer um movimento podia devolver a carga
                    // para uma célula que fechou depois — e o grupo continuava
                    // constando como fechado, prendendo a carga ali para sempre
                    // e tornando a fase invencível sem nenhum aviso.
                    dynamicBlockedCells.Remove(cell);
                    return false;
                }

                return dynamicBlockedCells.Add(cell);
            }

            return dynamicBlockedCells.Remove(cell);
        }

        public void ClearDynamicBlocked()
        {
            dynamicBlockedCells.Clear();
        }

        public bool IsInside(GridCoordinate cell)
        {
            return cell.X >= 0 && cell.Y >= 0 && cell.X < Width && cell.Y < Height;
        }

        public bool IsWall(GridCoordinate cell)
        {
            return walls.Contains(cell);
        }

        public bool IsBlocked(GridCoordinate cell)
        {
            if (walls.Contains(cell) || dynamicBlockedCells.Contains(cell))
            {
                return true;
            }

            foreach (PuzzleTimedBlockDefinition block in timedBlocks)
            {
                if (block.Position == cell && block.IsClosedAt(CommandCount))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsGoal(GridCoordinate cell)
        {
            return goals.Contains(cell);
        }

        public bool IsCostly(GridCoordinate cell)
        {
            return costlyCells.Contains(cell);
        }

        public bool TryGetGoalRequirement(GridCoordinate cell, out PuzzleEntityKind kind)
        {
            return goalRequirements.TryGetValue(cell, out kind);
        }

        public int GetMoveCost(GridCoordinate destination)
        {
            return costlyCells.Contains(destination) ? 2 : 1;
        }

        public bool IsFree(GridCoordinate cell)
        {
            return IsInside(cell) && !IsBlocked(cell) && !crateByPosition.ContainsKey(cell);
        }

        public bool TryGetCratePosition(string crateId, out GridCoordinate position)
        {
            foreach (KeyValuePair<GridCoordinate, string> crate in crateByPosition)
            {
                if (crate.Value == crateId)
                {
                    position = crate.Key;
                    return true;
                }
            }

            position = default;
            return false;
        }

        public PuzzleEntityKind GetCrateKind(string crateId)
        {
            return crateKinds.TryGetValue(crateId, out PuzzleEntityKind kind) ? kind : PuzzleEntityKind.Crate;
        }

        private bool EvaluateCompletion()
        {
            if (goals.Count == 0 || goals.Count != crateByPosition.Count)
            {
                return false;
            }

            foreach (GridCoordinate goal in goals)
            {
                if (!crateByPosition.TryGetValue(goal, out string crateId))
                {
                    return false;
                }

                if (goalRequirements.TryGetValue(goal, out PuzzleEntityKind requiredKind)
                    && GetCrateKind(crateId) != requiredKind)
                {
                    return false;
                }
            }

            return true;
        }

        private void ValidateCell(GridCoordinate cell, string label)
        {
            if (!IsInside(cell))
            {
                throw new ArgumentOutOfRangeException(label, cell, "Cell is outside the board.");
            }
        }
    }
}
