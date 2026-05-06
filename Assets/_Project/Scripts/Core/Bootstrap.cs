using UnityEngine;

namespace LibraryGame.Core
{
    [DisallowMultipleComponent]
    public sealed class Bootstrap : MonoBehaviour
    {
        [SerializeField] private GameConfig config;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            // M2: init Firebase, AuthService, RemoteConfig, Analytics, Crashlytics
            // M2: branch to 01_Auth or 02_MainHub based on signed-in state
        }
    }
}
