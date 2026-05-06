using UnityEngine;

namespace LibraryGame.Core
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Library Game/Game Config")]
    public sealed class GameConfig : ScriptableObject
    {
        [Header("Open Library")]
        [Tooltip("Required by Open Library ToS. Include contact email.")]
        public string openLibraryUserAgent = "LibraryGame/1.0 (contact@example.com)";
        public int openLibraryMaxRequestsPerSecond = 5;

        [Header("Save")]
        public bool useCloudSave = true;
        public bool firestoreOfflinePersistence = true;

        [Header("Build")]
        public string version = "0.1.0";
    }
}
