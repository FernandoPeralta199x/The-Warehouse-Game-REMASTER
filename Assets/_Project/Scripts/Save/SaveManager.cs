using TW08.Data;
using UnityEngine;

namespace TW08.Save
{
    [DisallowMultipleComponent]
    public sealed class SaveManager : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        private JsonSaveService service;

        public SaveGameData Data { get; private set; }

        private void Awake()
        {
            if (config == null)
            {
                Debug.LogError("SaveManager requires GameConfig.", this);
                enabled = false;
                return;
            }

            service = new JsonSaveService(config);
            Data = service.Load();
        }

        public void Save()
        {
            service?.Save(Data);
        }
    }
}
