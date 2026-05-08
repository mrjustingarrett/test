using System.Collections.Generic;
using LibraryGame.Core;
using LibraryGame.Data.Models;
using LibraryGame.Services.Inventory;
using UnityEngine;

namespace LibraryGame.Gameplay
{
    /// <summary>
    /// Watches InventorySystem.Layout and spawns/removes furniture
    /// GameObjects to match. One MonoBehaviour, one parent transform.
    /// </summary>
    public sealed class RoomLayoutRenderer : MonoBehaviour
    {
        private readonly Dictionary<int, GameObject> _spawned = new();
        private InventorySystem _inv;

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _inv)) return;
            _inv.LayoutChanged += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_inv != null) _inv.LayoutChanged -= Refresh;
        }

        public void Refresh()
        {
            if (_inv == null) return;
            // Remove items no longer in the layout.
            var live = new HashSet<int>();
            foreach (var p in _inv.Placements) live.Add(p.gridX);
            var stale = new List<int>();
            foreach (var kv in _spawned) if (!live.Contains(kv.Key)) stale.Add(kv.Key);
            foreach (var k in stale)
            {
                if (_spawned[k] != null) Destroy(_spawned[k]);
                _spawned.Remove(k);
            }

            // Add new items.
            foreach (var p in _inv.Placements)
            {
                if (_spawned.ContainsKey(p.gridX)) continue;
                var item = FurnitureCatalog.Get(p.itemId);
                if (item == null) continue;
                var go = RuntimeFurnitureBuilder.Build(item, transform);
                go.transform.localPosition = new Vector3(p.posX, p.posY, p.posZ);
                go.transform.localRotation = Quaternion.Euler(0, p.rotationY, 0);
                go.transform.localScale = Vector3.one * (p.scale > 0 ? p.scale : 1f);
                _spawned[p.gridX] = go;
            }
        }
    }
}
