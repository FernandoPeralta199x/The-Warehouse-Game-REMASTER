using System;
using System.Collections.Generic;
using System.Linq;
using TW08.Puzzle;
using UnityEngine;

namespace TW08.Presentation
{
    [DisallowMultipleComponent]
    public sealed class PuzzleMechanicView : MonoBehaviour
    {
        [Serializable]
        private sealed class DoorGroupVisual
        {
            [SerializeField] private string groupId;
            [SerializeField] private List<GameObject> doors = new();

            public string GroupId => groupId;
            public IReadOnlyList<GameObject> Doors => doors;

            public DoorGroupVisual(string id)
            {
                groupId = id;
            }

            public void AddDoor(GameObject door)
            {
                if (door != null && !doors.Contains(door))
                {
                    doors.Add(door);
                }
            }
        }

        [SerializeField] private PuzzleRuntime runtime;
        [SerializeField] private List<DoorGroupVisual> groups = new();

        public void Configure(PuzzleRuntime puzzleRuntime)
        {
            runtime = puzzleRuntime;
            MarkDirtyInEditor();
        }

        public void RegisterDoor(string groupId, GameObject door)
        {
            if (string.IsNullOrWhiteSpace(groupId) || door == null)
            {
                return;
            }

            DoorGroupVisual group = groups.FirstOrDefault(candidate => candidate.GroupId == groupId);
            if (group == null)
            {
                group = new DoorGroupVisual(groupId);
                groups.Add(group);
            }

            group.AddDoor(door);
            MarkDirtyInEditor();
        }

        private void OnEnable()
        {
            if (runtime != null)
            {
                runtime.SwitchGroupStateChanged += OnSwitchGroupStateChanged;
            }
        }

        private void Start()
        {
            Refresh();
        }

        private void OnDisable()
        {
            if (runtime != null)
            {
                runtime.SwitchGroupStateChanged -= OnSwitchGroupStateChanged;
            }
        }

        private void Refresh()
        {
            if (runtime == null)
            {
                return;
            }

            foreach (DoorGroupVisual group in groups)
            {
                Apply(group, runtime.IsSwitchGroupOpen(group.GroupId));
            }
        }

        private void OnSwitchGroupStateChanged(string groupId, bool open)
        {
            DoorGroupVisual group = groups.FirstOrDefault(candidate => candidate.GroupId == groupId);
            if (group != null)
            {
                Apply(group, open);
            }
        }

        private static void Apply(DoorGroupVisual group, bool open)
        {
            foreach (GameObject door in group.Doors)
            {
                if (door != null)
                {
                    door.SetActive(!open);
                }
            }
        }

        private void MarkDirtyInEditor()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
                if (gameObject.scene.IsValid())
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
                }
            }
#endif
        }
    }
}
