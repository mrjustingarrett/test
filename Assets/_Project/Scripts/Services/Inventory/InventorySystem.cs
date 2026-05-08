using System;
using System.Collections.Generic;
using LibraryGame.Data.Models;
using LibraryGame.Services.Save;
using UnityEngine;

namespace LibraryGame.Services.Inventory
{
    /// <summary>
    /// Authoritative source for what the player owns. Backed by SaveSystem.
    /// Currency operations live here too (soft only — hard is server-only).
    /// </summary>
    public sealed class InventorySystem
    {
        private readonly SaveSystem _save;

        public event Action InventoryChanged;
        public event Action LayoutChanged;
        public event Action CurrencyChanged;

        public InventorySystem(SaveSystem save)
        {
            _save = save;
            // Grant the starter shelf if the user has nothing yet.
            if (_save.Owned.items.Count == 0)
            {
                Grant("starter_shelf", "achievement");
            }
        }

        public IReadOnlyList<OwnedItemDoc> Owned => _save.Owned.items;
        public IReadOnlyList<RoomLayoutDoc.Placement> Placements => _save.Layout.placements;
        public int SoftCurrency => _save.Player.softCurrency;

        public bool Has(string itemId) => _save.Owned.items.Exists(x => x.itemId == itemId);

        public void Grant(string itemId, string source)
        {
            if (Has(itemId)) return;
            _save.Owned.items.Add(new OwnedItemDoc
            {
                itemId = itemId,
                acquiredAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                source = source,
                quantity = 1
            });
            _save.SaveOwned();
            InventoryChanged?.Invoke();
        }

        public bool Buy(string itemId, int price)
        {
            if (Has(itemId)) return false;
            if (_save.Player.softCurrency < price) return false;
            _save.Player.softCurrency -= price;
            _save.SavePlayer();
            CurrencyChanged?.Invoke();
            Grant(itemId, "purchase_soft");
            return true;
        }

        public void AddCurrency(int amount)
        {
            _save.Player.softCurrency += amount;
            _save.SavePlayer();
            CurrencyChanged?.Invoke();
        }

        // ---------- Placement ----------

        // 12 preset slots arranged around the room walls. (x, z) on the floor.
        private static readonly Vector2[] Slots =
        {
            new(-3.8f, 4.0f), new(-1.5f, 4.5f), new( 1.5f, 4.5f), new( 3.8f, 4.0f),
            new( 4.2f, 1.8f), new( 4.2f,-1.8f),
            new( 3.8f,-4.0f), new( 1.5f,-4.5f), new(-1.5f,-4.5f), new(-3.8f,-4.0f),
            new(-4.2f,-1.8f), new(-4.2f, 1.8f),
        };

        /// <summary>Places the item at the next free preset slot. Returns false if all slots full.</summary>
        public bool Place(string itemId)
        {
            for (int i = 0; i < Slots.Length; i++)
            {
                if (!IsSlotUsed(i))
                {
                    _save.Layout.placements.Add(new RoomLayoutDoc.Placement
                    {
                        itemId = itemId,
                        posX = Slots[i].x,
                        posY = 0f,
                        posZ = Slots[i].y,
                        rotationY = SuggestRotationForSlot(i),
                        gridX = i,
                        gridZ = 0,
                        scale = 1f
                    });
                    _save.Layout.updatedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    _save.SaveLayout();
                    LayoutChanged?.Invoke();
                    return true;
                }
            }
            return false;
        }

        public void Unplace(int slotIndex)
        {
            var idx = _save.Layout.placements.FindIndex(p => p.gridX == slotIndex);
            if (idx >= 0)
            {
                _save.Layout.placements.RemoveAt(idx);
                _save.SaveLayout();
                LayoutChanged?.Invoke();
            }
        }

        public void ClearLayout()
        {
            _save.Layout.placements.Clear();
            _save.SaveLayout();
            LayoutChanged?.Invoke();
        }

        public bool IsPlaced(string itemId) => _save.Layout.placements.Exists(p => p.itemId == itemId);

        private bool IsSlotUsed(int i) => _save.Layout.placements.Exists(p => p.gridX == i);

        // Rotate so items face into the room.
        private static float SuggestRotationForSlot(int i)
        {
            // Slots 0..3 along +Z wall → face -Z (rotate 180)
            if (i >= 0 && i <= 3) return 180f;
            // Slots 4..5 along +X wall → face -X (rotate 270)
            if (i == 4 || i == 5) return 270f;
            // Slots 6..9 along -Z wall → face +Z (rotate 0)
            if (i >= 6 && i <= 9) return 0f;
            // Slots 10..11 along -X wall → face +X (rotate 90)
            return 90f;
        }
    }
}
