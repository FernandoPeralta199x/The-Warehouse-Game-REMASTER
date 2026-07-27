using System;
using TW08.Core.Services;
using UnityEngine;

namespace TW08.Core
{
    [DisallowMultipleComponent]
    public sealed class GameStateMachine : MonoBehaviour, IGameService
    {
        public GameState Current { get; private set; } = GameState.Booting;
        public GameState Previous { get; private set; } = GameState.Booting;

        public event Action<GameState, GameState> StateChanged;

        public void Initialize(ServiceRegistry services)
        {
            TransitionTo(GameState.MainMenu);
        }

        public void Shutdown()
        {
            TransitionTo(GameState.ShuttingDown);
        }

        public bool TransitionTo(GameState next)
        {
            if (Current == next)
            {
                return false;
            }

            GameState old = Current;
            Previous = old;
            Current = next;
            StateChanged?.Invoke(old, next);
            return true;
        }
    }
}
