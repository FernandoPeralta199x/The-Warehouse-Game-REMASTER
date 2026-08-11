using System;
using TW08.Core;
using TW08.Puzzle;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TW08.Input
{
    [DefaultExecutionOrder(-900)]
    [DisallowMultipleComponent]
    public sealed class GameInput : MonoBehaviour
    {
        private InputActionMap puzzleMap;
        private InputActionMap raceMap;
        private InputAction puzzleMove;
        private InputAction puzzleUndo;
        private InputAction puzzleRedo;
        private InputAction puzzleRestart;
        private InputAction puzzlePause;
        private InputAction raceSteer;
        private InputAction raceThrottle;
        private InputAction raceDrift;
        private InputAction racePowerUp;
        private InputAction racePause;

        public event Action<GridCoordinate> PuzzleMoveRequested;
        public event Action PuzzleUndoRequested;
        public event Action PuzzleRedoRequested;
        public event Action PuzzleRestartRequested;
        public event Action PauseRequested;
        public event Action RacePowerUpRequested;

        public float RaceSteer => raceSteer?.ReadValue<float>() ?? 0f;
        public float RaceThrottle => raceThrottle?.ReadValue<float>() ?? 0f;
        public bool RaceDriftHeld => raceDrift?.IsPressed() ?? false;

        private void Awake()
        {
            BuildActions();
        }

        private void OnEnable()
        {
            puzzleMove.performed += OnPuzzleMove;
            puzzleUndo.performed += OnPuzzleUndo;
            puzzleRedo.performed += OnPuzzleRedo;
            puzzleRestart.performed += OnPuzzleRestart;
            puzzlePause.performed += OnPause;
            racePowerUp.performed += OnRacePowerUp;
            racePause.performed += OnPause;
            SetMode(GameMode.Puzzle);
        }

        private void OnDisable()
        {
            puzzleMove.performed -= OnPuzzleMove;
            puzzleUndo.performed -= OnPuzzleUndo;
            puzzleRedo.performed -= OnPuzzleRedo;
            puzzleRestart.performed -= OnPuzzleRestart;
            puzzlePause.performed -= OnPause;
            racePowerUp.performed -= OnRacePowerUp;
            racePause.performed -= OnPause;
            puzzleMap.Disable();
            raceMap.Disable();
        }

        private void OnDestroy()
        {
            puzzleMap?.Dispose();
            raceMap?.Dispose();
        }

        public void SetMode(GameMode mode)
        {
            puzzleMap.Disable();
            raceMap.Disable();

            if (mode == GameMode.Puzzle)
            {
                puzzleMap.Enable();
            }
            else if (mode == GameMode.Race)
            {
                raceMap.Enable();
            }
        }

        private void BuildActions()
        {
            puzzleMap = new InputActionMap("Puzzle");
            puzzleMove = puzzleMap.AddAction("Move", InputActionType.Value);
            AddVector2Bindings(puzzleMove);

            puzzleUndo = puzzleMap.AddAction("Undo", InputActionType.Button, "<Keyboard>/z");
            puzzleUndo.AddBinding("<Keyboard>/backspace");
            puzzleUndo.AddBinding("<Gamepad>/buttonWest");

            puzzleRedo = puzzleMap.AddAction("Redo", InputActionType.Button, "<Keyboard>/y");
            puzzleRedo.AddBinding("<Gamepad>/buttonNorth");

            puzzleRestart = puzzleMap.AddAction("Restart", InputActionType.Button, "<Keyboard>/r");
            puzzleRestart.AddBinding("<Gamepad>/select");

            puzzlePause = puzzleMap.AddAction("Pause", InputActionType.Button, "<Keyboard>/escape");
            puzzlePause.AddBinding("<Gamepad>/start");

            raceMap = new InputActionMap("Race");
            raceSteer = raceMap.AddAction("Steer", InputActionType.Value);
            AddAxisBindings(raceSteer, "<Keyboard>/a", "<Keyboard>/d");
            AddAxisBindings(raceSteer, "<Keyboard>/leftArrow", "<Keyboard>/rightArrow");
            raceSteer.AddBinding("<Gamepad>/leftStick/x");

            raceThrottle = raceMap.AddAction("Throttle", InputActionType.Value);
            AddAxisBindings(raceThrottle, "<Keyboard>/s", "<Keyboard>/w");
            AddAxisBindings(raceThrottle, "<Keyboard>/downArrow", "<Keyboard>/upArrow");
            raceThrottle.AddCompositeBinding("1DAxis")
                .With("Negative", "<Gamepad>/leftTrigger")
                .With("Positive", "<Gamepad>/rightTrigger");

            raceDrift = raceMap.AddAction("Drift", InputActionType.Button, "<Keyboard>/leftShift");
            raceDrift.AddBinding("<Gamepad>/buttonSouth");

            racePowerUp = raceMap.AddAction("PowerUp", InputActionType.Button, "<Keyboard>/space");
            racePowerUp.AddBinding("<Gamepad>/buttonEast");

            racePause = raceMap.AddAction("Pause", InputActionType.Button, "<Keyboard>/escape");
            racePause.AddBinding("<Gamepad>/start");
        }

        private static void AddVector2Bindings(InputAction action)
        {
            action.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            action.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            action.AddBinding("<Gamepad>/dpad");
        }

        private static void AddAxisBindings(InputAction action, string negative, string positive)
        {
            action.AddCompositeBinding("1DAxis")
                .With("Negative", negative)
                .With("Positive", positive);
        }

        private void OnPuzzleMove(InputAction.CallbackContext context)
        {
            Vector2 raw = context.ReadValue<Vector2>();
            GridCoordinate direction;

            if (Mathf.Abs(raw.x) > Mathf.Abs(raw.y))
            {
                direction = new GridCoordinate(raw.x > 0f ? 1 : -1, 0);
            }
            else if (Mathf.Abs(raw.y) > 0.01f)
            {
                direction = new GridCoordinate(0, raw.y > 0f ? 1 : -1);
            }
            else
            {
                return;
            }

            PuzzleMoveRequested?.Invoke(direction);
        }

        private void OnPuzzleUndo(InputAction.CallbackContext _) => PuzzleUndoRequested?.Invoke();
        private void OnPuzzleRedo(InputAction.CallbackContext _) => PuzzleRedoRequested?.Invoke();
        private void OnPuzzleRestart(InputAction.CallbackContext _) => PuzzleRestartRequested?.Invoke();
        private void OnPause(InputAction.CallbackContext _) => PauseRequested?.Invoke();
        private void OnRacePowerUp(InputAction.CallbackContext _) => RacePowerUpRequested?.Invoke();
    }
}
