#if UNITY_INCLUDE_TESTS
using System.Collections;
using NUnit.Framework;
using TW08.Core;
using UnityEngine;
using UnityEngine.TestTools;

namespace TW08.Tests.PlayMode
{
    public sealed class BootstrapSmokeTests
    {
        [UnityTest]
        public IEnumerator GameStateMachineTransitions()
        {
            GameObject host = new("State Machine Test");
            GameStateMachine stateMachine = host.AddComponent<GameStateMachine>();
            stateMachine.Initialize(new TW08.Core.Services.ServiceRegistry());

            Assert.That(stateMachine.Current, Is.EqualTo(GameState.MainMenu));
            Assert.That(stateMachine.TransitionTo(GameState.Puzzle), Is.True);
            Assert.That(stateMachine.Current, Is.EqualTo(GameState.Puzzle));

            Object.Destroy(host);
            yield return null;
        }
    }
}
#endif
