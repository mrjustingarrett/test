#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LibraryGame.Editor.Tools
{
    public static class CreateInitialScenes
    {
        private const string ScenesDir = "Assets/_Project/Scenes";

        private static readonly string[] SceneNames =
        {
            "00_Boot",
            "01_Auth",
            "02_MainHub",
            "03_Loading"
        };

        [MenuItem("Library Game/Create Initial Scenes")]
        public static void Create()
        {
            Directory.CreateDirectory(ScenesDir);

            var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            foreach (var name in SceneNames)
            {
                var path = $"{ScenesDir}/{name}.unity";
                if (!File.Exists(path))
                {
                    var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                    EditorSceneManager.SaveScene(scene, path);
                }
                buildScenes.Add(new EditorBuildSettingsScene(path, true));
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Library Game] Created/wired scenes: {string.Join(", ", SceneNames)}");
        }
    }
}
#endif
