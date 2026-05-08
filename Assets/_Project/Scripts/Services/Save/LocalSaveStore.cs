using System;
using System.IO;
using UnityEngine;

namespace LibraryGame.Services.Save
{
    /// <summary>
    /// JSON file store under Application.persistentDataPath. Each "doc" is a
    /// separate file so partial corruption is recoverable.
    /// </summary>
    public sealed class LocalSaveStore
    {
        private readonly string _root;

        public LocalSaveStore()
        {
            _root = System.IO.Path.Combine(Application.persistentDataPath, "saves");
            Directory.CreateDirectory(_root);
        }

        public bool Exists(string docId) => File.Exists(Path(docId));

        public T Load<T>(string docId) where T : class
        {
            var path = Path(docId);
            if (!File.Exists(path)) return null;
            try
            {
                var json = File.ReadAllText(path);
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LocalSaveStore] Load failed for {docId}: {ex.Message}");
                return null;
            }
        }

        public void Save<T>(string docId, T data) where T : class
        {
            var path = Path(docId);
            var tmp = path + ".tmp";
            try
            {
                var json = JsonUtility.ToJson(data, prettyPrint: false);
                File.WriteAllText(tmp, json);
                if (File.Exists(path)) File.Delete(path);
                File.Move(tmp, path);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LocalSaveStore] Save failed for {docId}: {ex.Message}");
            }
        }

        public void Delete(string docId)
        {
            var path = Path(docId);
            if (File.Exists(path)) File.Delete(path);
        }

        private string Path(string docId) => System.IO.Path.Combine(_root, $"{docId}.json");
    }
}
