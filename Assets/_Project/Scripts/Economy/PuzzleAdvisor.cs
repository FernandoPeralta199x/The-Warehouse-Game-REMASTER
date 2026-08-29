using System;
using System.Collections.Generic;
using TW08.Puzzle;

namespace TW08.Economy
{
    /// <summary>
    /// Inteligência de apoio das ferramentas de informação (Scanner Logístico e
    /// Assistente de Turno).
    ///
    /// Deliberadamente NÃO resolve a fase: a bíblia de design exige que as
    /// ferramentas ajudem o jogador a enxergar o problema, nunca a pular o
    /// desafio. Por isso o conselho aponta a carga em pior situação e o caminho
    /// até ela — decidir o que fazer continua com o jogador.
    ///
    /// Tudo aqui é puro e determinístico para poder ser testado sem cena.
    /// </summary>
    public static class PuzzleAdvisor
    {
        private static readonly GridCoordinate[] Directions =
        {
            new(0, 1), new(0, -1), new(-1, 0), new(1, 0)
        };

        /// <summary>
        /// Carga que mais merece atenção: primeiro as travadas em canto, depois
        /// a que está mais longe de um alvo livre. Devolve false quando toda
        /// carga já está posicionada.
        /// </summary>
        public static bool TryFindCriticalCrate(PuzzleBoardModel board, out GridCoordinate position)
        {
            position = default;
            if (board == null)
            {
                return false;
            }

            int bestScore = int.MinValue;
            bool found = false;

            foreach (KeyValuePair<GridCoordinate, string> crate in board.Crates)
            {
                if (IsSatisfied(board, crate.Key, crate.Value))
                {
                    continue;
                }

                int score = IsCornerLocked(board, crate.Key)
                    ? int.MaxValue / 2
                    : DistanceToNearestOpenGoal(board, crate.Key);

                // Empate resolve pela posição para o conselho não oscilar entre frames.
                if (score > bestScore
                    || (score == bestScore && found && ComparePosition(crate.Key, position) < 0))
                {
                    bestScore = score;
                    position = crate.Key;
                    found = true;
                }
            }

            return found;
        }

        /// <summary>Alvos ainda sem a carga correta — o que o Marcador de Rota destaca.</summary>
        public static IReadOnlyList<GridCoordinate> FindOpenGoals(PuzzleBoardModel board)
        {
            List<GridCoordinate> open = new();
            if (board == null)
            {
                return open;
            }

            foreach (GridCoordinate goal in board.Goals)
            {
                if (!board.Crates.TryGetValue(goal, out string crateId) || !IsSatisfied(board, goal, crateId))
                {
                    open.Add(goal);
                }
            }

            return open;
        }

        /// <summary>
        /// Dica do Assistente de Turno. <paramref name="tier"/> vai de 1 (vaga) a
        /// 3 (a mais direta); nem a camada 3 entrega a solução, apenas a direção
        /// do próximo passo até a carga crítica.
        /// </summary>
        public static string BuildHint(PuzzleBoardModel board, int tier)
        {
            if (board == null)
            {
                return "Terminal sem leitura do setor.";
            }

            if (!TryFindCriticalCrate(board, out GridCoordinate crate))
            {
                return "Toda a carga está posicionada. Confira o painel de saída.";
            }

            switch (Math.Clamp(tier, 1, 3))
            {
                case 1:
                    return $"A carga em {Describe(board, crate)} está travando o setor.";

                case 2:
                    IReadOnlyList<GridCoordinate> goals = FindOpenGoals(board);
                    if (goals.Count == 0)
                    {
                        return "Reposicione a carga destacada antes de seguir.";
                    }

                    GridCoordinate target = NearestGoal(crate, goals);
                    return $"Leve essa carga na direção de {Describe(board, target)} antes de mover as outras.";

                default:
                    return TryFindStepTowards(board, crate, out string step)
                        ? $"Primeiro passo sugerido: {step}."
                        : "Abra caminho até a carga destacada: não há rota livre agora.";
            }
        }

        private static bool IsSatisfied(PuzzleBoardModel board, GridCoordinate cell, string crateId)
        {
            if (!board.IsGoal(cell))
            {
                return false;
            }

            return !board.TryGetGoalRequirement(cell, out PuzzleEntityKind required)
                   || board.GetCrateKind(crateId) == required;
        }

        private static bool IsCornerLocked(PuzzleBoardModel board, GridCoordinate cell)
        {
            bool horizontal = IsSolid(board, cell + new GridCoordinate(-1, 0))
                              || IsSolid(board, cell + new GridCoordinate(1, 0));
            bool vertical = IsSolid(board, cell + new GridCoordinate(0, -1))
                            || IsSolid(board, cell + new GridCoordinate(0, 1));
            return horizontal && vertical;
        }

        private static bool IsSolid(PuzzleBoardModel board, GridCoordinate cell)
        {
            return !board.IsInside(cell) || board.IsBlocked(cell);
        }

        private static int DistanceToNearestOpenGoal(PuzzleBoardModel board, GridCoordinate crate)
        {
            IReadOnlyList<GridCoordinate> goals = FindOpenGoals(board);
            if (goals.Count == 0)
            {
                return 0;
            }

            GridCoordinate nearest = NearestGoal(crate, goals);
            return Math.Abs(nearest.X - crate.X) + Math.Abs(nearest.Y - crate.Y);
        }

        private static GridCoordinate NearestGoal(GridCoordinate from, IReadOnlyList<GridCoordinate> goals)
        {
            GridCoordinate best = goals[0];
            int bestDistance = int.MaxValue;
            foreach (GridCoordinate goal in goals)
            {
                int distance = Math.Abs(goal.X - from.X) + Math.Abs(goal.Y - from.Y);
                if (distance < bestDistance || (distance == bestDistance && ComparePosition(goal, best) < 0))
                {
                    bestDistance = distance;
                    best = goal;
                }
            }

            return best;
        }

        /// <summary>Primeiro passo de uma rota livre do jogador até uma célula vizinha da carga.</summary>
        private static bool TryFindStepTowards(PuzzleBoardModel board, GridCoordinate crate, out string step)
        {
            step = null;
            HashSet<GridCoordinate> targets = new();
            foreach (GridCoordinate direction in Directions)
            {
                GridCoordinate neighbour = crate + direction;
                if (board.IsFree(neighbour))
                {
                    targets.Add(neighbour);
                }
            }

            if (targets.Count == 0)
            {
                return false;
            }

            if (targets.Contains(board.PlayerPosition))
            {
                step = "empurre a carga destacada";
                return true;
            }

            Queue<GridCoordinate> queue = new();
            Dictionary<GridCoordinate, GridCoordinate> firstStep = new();
            queue.Enqueue(board.PlayerPosition);

            while (queue.Count > 0)
            {
                GridCoordinate current = queue.Dequeue();
                foreach (GridCoordinate direction in Directions)
                {
                    GridCoordinate next = current + direction;
                    if (!board.IsFree(next) || firstStep.ContainsKey(next))
                    {
                        continue;
                    }

                    firstStep[next] = current == board.PlayerPosition ? direction : firstStep[current];
                    if (targets.Contains(next))
                    {
                        step = DirectionName(firstStep[next]);
                        return true;
                    }

                    queue.Enqueue(next);
                }
            }

            return false;
        }

        private static string DirectionName(GridCoordinate direction)
        {
            if (direction.X > 0) return "direita";
            if (direction.X < 0) return "esquerda";
            return direction.Y > 0 ? "cima" : "baixo";
        }

        private static string Describe(PuzzleBoardModel board, GridCoordinate cell)
        {
            string vertical = cell.Y >= board.Height / 2 ? "norte" : "sul";
            string horizontal = cell.X >= board.Width / 2 ? "leste" : "oeste";
            return $"{vertical}-{horizontal}";
        }

        private static int ComparePosition(GridCoordinate a, GridCoordinate b)
        {
            int byY = a.Y.CompareTo(b.Y);
            return byY != 0 ? byY : a.X.CompareTo(b.X);
        }
    }
}
