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

            if (_save.Owned.items.Count == 0)
            {
                // First ever run — grant a full starter library set silently (no save spam).
                var starterIds = new[]
                {
                    "starter_shelf", "oak_bookshelf", "reading_chair", "cozy_armchair",
                    "floor_lamp", "rug_red", "rug_blue", "end_table", "coffee_table",
                    "desk", "potted_fern", "big_plant", "globe_lamp"
                };
                foreach (var id in starterIds)
                {
                    if (!Has(id))
                        _save.Owned.items.Add(new OwnedItemDoc
                        {
                            itemId = id,
                            acquiredAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            source = "starter",
                            quantity = 1
                        });
                }
                _save.SaveOwned();
                InventoryChanged?.Invoke();
            }

            if (_save.Layout.placements.Count == 0)
                SeedDefaultLayout();
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

        private static float SuggestRotationForSlot(int i)
        {
            if (i >= 0 && i <= 3) return 180f;
            if (i == 4 || i == 5) return 270f;
            if (i >= 6 && i <= 9) return 0f;
            return 90f;
        }

        /// <summary>Fills the room with a curated starting layout on a brand-new save.</summary>
        private void SeedDefaultLayout()
        {
            int nextId = 0;
            void Add(string id, float x, float z, float rot, float scale = 1f)
            {
                if (!Has(id)) return;
                _save.Layout.placements.Add(new RoomLayoutDoc.Placement
                {
                    itemId = id, posX = x, posY = 0f, posZ = z,
                    rotationY = rot, scale = scale,
                    gridX = nextId++, gridZ = 0
                });
            }

            // Back wall — bookshelf row
            Add("oak_bookshelf",  -3.5f,  4.7f, 180f);
            Add("starter_shelf",  -1.2f,  4.7f, 180f);
            Add("starter_shelf",   1.2f,  4.7f, 180f);
            Add("oak_bookshelf",   3.5f,  4.7f, 180f);

            // West wall — tall plant + floor lamp alcove
            Add("big_plant",      -4.6f,  3.5f,  90f);
            Add("floor_lamp",     -4.6f,  1.0f,  90f);
            Add("desk",           -4.0f, -1.5f,  90f, 0.95f);

            // East wall — seating area
            Add("reading_chair",   3.8f,  1.8f, 270f);
            Add("cozy_armchair",   3.8f, -1.0f, 270f);
            Add("end_table",       4.1f,  0.4f, 270f, 0.85f);

            // Center — rug and coffee table
            Add("rug_red",         0f,    0.5f,   0f);
            Add("coffee_table",    0f,    0.5f,   0f, 0.9f);

            // Near south wall — a second lamp and plant
            Add("globe_lamp",      2.5f, -4.5f,   0f);
            Add("potted_fern",    -2.5f, -4.5f,   0f);

            _save.Layout.updatedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _save.SaveLayout();
            LayoutChanged?.Invoke();
        }
    }
}
