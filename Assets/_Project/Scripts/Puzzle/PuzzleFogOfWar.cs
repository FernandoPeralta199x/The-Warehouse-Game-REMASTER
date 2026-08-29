using System.Collections.Generic;
using UnityEngine;

namespace TW08.Puzzle
{
    /// <summary>Como o setor esconde o que está longe do operador.</summary>
    public enum PuzzleFogMode
    {
        /// <summary>Sem névoa.</summary>
        None = 0,

        /// <summary>Só o que está no raio da lanterna aparece. O resto volta a escurecer.</summary>
        Flashlight = 1,

        /// <summary>O que já foi visto continua visível, mais apagado. É o mapa parcial.</summary>
        Memory = 2
    }

    /// <summary>
    /// Névoa de guerra das fases de mapa escuro e mapa parcial.
    ///
    /// É apresentação pura: não altera o tabuleiro nem o que o solver considera
    /// possível. Esconder informação muda a dificuldade percebida, não a
    /// solvabilidade — e por isso as fases com névoa continuam valendo as mesmas
    /// provas do solver.
    ///
    /// A escuridão é aplicada por tinta nos renderizadores, e não desligando
    /// objetos: o alvo e a carga precisam continuar recebendo suas animações
    /// mesmo enquanto estão fora do alcance da luz.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PuzzleFogOfWar : MonoBehaviour
    {
        [SerializeField] private PuzzleRuntime runtime;
        [SerializeField] private PuzzleFogMode mode = PuzzleFogMode.Flashlight;
        [SerializeField, Min(1)] private int radius = 2;
        [SerializeField, Range(0f, 1f)] private float hiddenAlpha = 0.06f;
        [SerializeField, Range(0f, 1f)] private float rememberedAlpha = 0.34f;
        [SerializeField, Min(1f)] private float fadeSpeed = 9f;

        private readonly Dictionary<GridCoordinate, List<SpriteRenderer>> cellRenderers = new();
        private readonly Dictionary<SpriteRenderer, Color> baseColors = new();
        private readonly HashSet<GridCoordinate> seen = new();
        private readonly Dictionary<SpriteRenderer, float> currentAlpha = new();

        private float cellSize = 1f;

        public void Configure(PuzzleRuntime puzzleRuntime, PuzzleFogMode fogMode, int visionRadius, float cell)
        {
            runtime = puzzleRuntime;
            mode = fogMode;
            radius = Mathf.Max(1, visionRadius);
            cellSize = Mathf.Max(0.1f, cell);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        private void OnEnable()
        {
            if (mode == PuzzleFogMode.None)
            {
                enabled = false;
                return;
            }

            IndexScene();

            if (runtime != null)
            {
                runtime.Initialized += OnBoardReset;
                runtime.LevelRestarted += OnBoardReset;
            }

            OnBoardReset();
        }

        private void OnDisable()
        {
            if (runtime != null)
            {
                runtime.Initialized -= OnBoardReset;
                runtime.LevelRestarted -= OnBoardReset;
            }

            RestoreAll();
        }

        /// <summary>
        /// Mapeia cada renderizador para a célula em que ele está.
        ///
        /// A varredura é feita uma vez: as fases são estáticas fora carga e
        /// jogador, que são tratados à parte por posição a cada frame.
        /// </summary>
        private void IndexScene()
        {
            cellRenderers.Clear();
            baseColors.Clear();
            currentAlpha.Clear();

            if (runtime != null && runtime.Level != null)
            {
                cellSize = Mathf.Max(0.1f, runtime.Level.CellSize);
            }

            foreach (SpriteRenderer renderer in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
            {
                if (renderer == null)
                {
                    continue;
                }

                GridCoordinate cell = WorldToCell(renderer.transform.position);
                if (!cellRenderers.TryGetValue(cell, out List<SpriteRenderer> list))
                {
                    list = new List<SpriteRenderer>();
                    cellRenderers[cell] = list;
                }

                list.Add(renderer);
                baseColors[renderer] = renderer.color;
                currentAlpha[renderer] = renderer.color.a;
            }
        }

        private void OnBoardReset()
        {
            seen.Clear();
        }

        private void LateUpdate()
        {
            if (runtime?.Board == null || cellRenderers.Count == 0)
            {
                return;
            }

            GridCoordinate player = runtime.Board.PlayerPosition;
            float step = 1f - Mathf.Exp(-fadeSpeed * Time.deltaTime);

            foreach (KeyValuePair<GridCoordinate, List<SpriteRenderer>> entry in cellRenderers)
            {
                float target = TargetAlphaFor(entry.Key, player);

                foreach (SpriteRenderer renderer in entry.Value)
                {
                    if (renderer == null || !baseColors.TryGetValue(renderer, out Color baseColor))
                    {
                        continue;
                    }

                    float from = currentAlpha.TryGetValue(renderer, out float value) ? value : baseColor.a;
                    float next = Mathf.Lerp(from, baseColor.a * target, step);
                    currentAlpha[renderer] = next;

                    Color color = baseColor;
                    color.a = next;
                    renderer.color = color;
                }
            }
        }

        private float TargetAlphaFor(GridCoordinate cell, GridCoordinate player)
        {
            // Distância de Chebyshev: a lanterna ilumina um quadrado, não um
            // losango. Com Manhattan os cantos do raio ficariam escuros e o
            // jogador leria isso como parede.
            int distance = Mathf.Max(Mathf.Abs(cell.X - player.X), Mathf.Abs(cell.Y - player.Y));

            if (distance <= radius)
            {
                seen.Add(cell);
                return 1f;
            }

            if (mode == PuzzleFogMode.Memory && seen.Contains(cell))
            {
                return rememberedAlpha;
            }

            return hiddenAlpha;
        }

        private GridCoordinate WorldToCell(Vector3 world)
        {
            return new GridCoordinate(
                Mathf.RoundToInt(world.x / cellSize),
                Mathf.RoundToInt(world.y / cellSize));
        }

        private void RestoreAll()
        {
            foreach (KeyValuePair<SpriteRenderer, Color> entry in baseColors)
            {
                if (entry.Key != null)
                {
                    entry.Key.color = entry.Value;
                }
            }
        }
    }
}
