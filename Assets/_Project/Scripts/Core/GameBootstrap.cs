using System;
using System.Collections.Generic;
using TW08.Core.Services;
using TW08.Data;
using UnityEngine;

namespace TW08.Core
{
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    public sealed class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }

        [SerializeField] private GameConfig config;
        [SerializeField] private bool persistAcrossScenes = true;
        [SerializeField] private List<MonoBehaviour> serviceComponents = new();

        private readonly List<IGameService> initializedServices = new();

        public GameConfig Config => config;
        public GameMode CurrentMode { get; private set; } = GameMode.None;
        public ServiceRegistry Services { get; } = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }

            InitializeServices();
        }

        public void SetMode(GameMode mode)
        {
            CurrentMode = mode;
        }

        public T GetService<T>() where T : class
        {
            return Services.Get<T>();
        }

        private void InitializeServices()
        {
            if (serviceComponents.Count == 0)
            {
                serviceComponents.AddRange(GetComponentsInChildren<MonoBehaviour>(true));
            }

            foreach (MonoBehaviour component in serviceComponents)
            {
                if (component == null || component == this || component is not IGameService service)
                {
                    continue;
                }

                try
                {
                    Services.Register(service);
                    initializedServices.Add(service);
                }
                catch (Exception exception)
                {
                    Debug.LogError($"Unable to register service '{component.GetType().Name}': {exception.Message}", component);
                }
            }

            foreach (IGameService service in initializedServices)
            {
                service.Initialize(Services);
            }
        }

        private void OnDestroy()
        {
            for (int i = initializedServices.Count - 1; i >= 0; i--)
            {
                initializedServices[i].Shutdown();
            }

            initializedServices.Clear();
            Services.Clear();

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
