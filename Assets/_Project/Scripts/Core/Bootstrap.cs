using LibraryGame.Services.Achievements;
using LibraryGame.Services.Books;
using LibraryGame.Services.Inventory;
using LibraryGame.Services.Save;
using LibraryGame.Services.Weather;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LibraryGame.Core
{
    /// <summary>
    /// Single entry point. Spawned automatically before any scene loads via
    /// RuntimeInitializeOnLoadMethod. Persists for the session.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Bootstrap : MonoBehaviour
    {
        public static Bootstrap Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnGameStart()
        {
            if (Instance != null) return;
            var go = new GameObject("[Bootstrap]");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<Bootstrap>();
        }

        private void Awake()
        {
            // Local-only services for MVP; M2 swaps SaveSystem to dual-tier
            // and adds AuthService / RemoteConfig / Analytics / Crashlytics.
            var save = new SaveSystem();
            ServiceLocator.Register(save);

            ServiceLocator.Register(new OpenLibraryClient(
                userAgent: "LibraryGame/0.1 (https://github.com/mrjustingarrett/test)"));
            ServiceLocator.Register(new BookService(save));

            var inv = new InventorySystem(save);
            ServiceLocator.Register(inv);

            ServiceLocator.Register(new AchievementSystem(save, inv));
            ServiceLocator.Register(new WeatherTimeSystem(save));

            // Boot is intentionally empty. Skip to MainHub.
            var current = SceneManager.GetActiveScene().name;
            if (current == Constants.Scenes.Boot)
            {
                SceneManager.LoadScene(Constants.Scenes.MainHub);
            }
        }
    }
}
