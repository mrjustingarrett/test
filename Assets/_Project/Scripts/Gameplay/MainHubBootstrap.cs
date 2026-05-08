using LibraryGame.Core;
using LibraryGame.Gameplay.Player;
using LibraryGame.Services.Weather;
using LibraryGame.UI.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LibraryGame.Gameplay
{
    /// <summary>
    /// Composes the 3D library scene at runtime when 02_MainHub loads.
    /// Triggered by Bootstrap via SceneManager.sceneLoaded.
    /// </summary>
    public static class MainHubBootstrap
    {
        public static GameObject RoomRoot { get; private set; }
        public static GameObject FurnitureRoot { get; private set; }
        public static PlayerController Player { get; private set; }
        public static CameraRig CameraRig { get; private set; }
        public static TouchHud Hud { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Hook()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            if (SceneManager.GetActiveScene().name == Constants.Scenes.MainHub)
                Compose();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == Constants.Scenes.MainHub) Compose();
        }

        public static void Compose()
        {
            if (RoomRoot != null) return;

            // Lighting (initial values; WeatherTimeSystem overrides each Tick).
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.62f, 0.75f);
            RenderSettings.ambientEquatorColor = new Color(0.50f, 0.50f, 0.55f);
            RenderSettings.ambientGroundColor = new Color(0.20f, 0.18f, 0.16f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.85f, 0.85f, 0.92f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 12f;
            RenderSettings.fogEndDistance = 50f;

            var sunGo = new GameObject("Sun");
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.96f, 0.86f);
            sun.intensity = 1.05f;
            sunGo.transform.rotation = Quaternion.Euler(50f, 30f, 0f);

            // Room geometry
            RoomRoot = RuntimeRoomBuilder.Build();

            // Furniture root + renderer
            FurnitureRoot = new GameObject("Furniture");
            FurnitureRoot.AddComponent<RoomLayoutRenderer>();

            // Player
            var playerGo = new GameObject("Player");
            var cc = playerGo.AddComponent<CharacterController>();
            cc.height = 1.7f; cc.radius = 0.3f; cc.center = new Vector3(0, 0.85f, 0);
            Player = playerGo.AddComponent<PlayerController>();

            // Camera rig
            var rigGo = new GameObject("CameraRig");
            CameraRig = rigGo.AddComponent<CameraRig>();
            CameraRig.target = playerGo.transform;
            CameraRig.headMount = Player.CameraMount;

            // HUD
            var hudGo = new GameObject("[HUD]");
            Hud = hudGo.AddComponent<TouchHud>();
            Hud.player = Player;
            Hud.cameraRig = CameraRig;

            // UI Root (menus)
            var uiRootGo = new GameObject("[UIRoot]");
            var uiRoot = uiRootGo.AddComponent<UI.UIRoot>();
            uiRoot.Initialize(Hud);

            // Weather + time-of-day binding
            if (ServiceLocator.TryGet<WeatherTimeSystem>(out var ws))
            {
                ws.Bind(sun, RoomRoot.transform);
                sunGo.AddComponent<WeatherTicker>();
            }
        }
    }
}
