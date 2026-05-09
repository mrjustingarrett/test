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

            // Ambient — warm interior feel, not outdoor blue sky.
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor     = new Color(0.62f, 0.56f, 0.46f);  // warm ceiling bounce
            RenderSettings.ambientEquatorColor = new Color(0.52f, 0.48f, 0.42f);
            RenderSettings.ambientGroundColor  = new Color(0.18f, 0.15f, 0.12f);  // dark floor shadow
            RenderSettings.fog = false; // fog inside a small room looks wrong

            // Sun — comes through the east window at a shallow angle.
            var sunGo = new GameObject("Sun");
            var sun = sunGo.AddComponent<Light>();
            sun.type      = LightType.Directional;
            sun.color     = new Color(1.00f, 0.94f, 0.78f);
            sun.intensity = 0.85f;
            sun.shadows   = LightShadows.Soft;
            sunGo.transform.rotation = Quaternion.Euler(38f, -110f, 0f); // shines in from east wall window

            // Fill light — soft bluish sky fill from the window side, no shadows.
            var fillGo = new GameObject("FillLight");
            var fill = fillGo.AddComponent<Light>();
            fill.type      = LightType.Directional;
            fill.color     = new Color(0.60f, 0.72f, 0.90f);
            fill.intensity = 0.25f;
            fill.shadows   = LightShadows.None;
            fillGo.transform.rotation = Quaternion.Euler(20f, 70f, 0f);

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
