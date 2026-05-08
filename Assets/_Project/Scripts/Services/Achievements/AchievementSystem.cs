using System;
using System.Collections.Generic;
using LibraryGame.Data.Models;
using LibraryGame.Services.Inventory;
using LibraryGame.Services.Save;
using UnityEngine;

namespace LibraryGame.Services.Achievements
{
    public sealed class AchievementSystem
    {
        public static AchievementSystem Instance { get; private set; }

        private readonly SaveSystem _save;
        private readonly InventorySystem _inv;

        public event Action<AchievementCatalog.Def> Unlocked;

        private int _lastPlacementCount;

        public AchievementSystem(SaveSystem save, InventorySystem inv)
        {
            _save = save;
            _inv = inv;
            Instance = this;
            EnsureEntries();
            _lastPlacementCount = inv.Placements.Count;
            _inv.LayoutChanged += OnLayoutChanged;
        }

        private void OnLayoutChanged()
        {
            int current = _inv.Placements.Count;
            int delta = current - _lastPlacementCount;
            _lastPlacementCount = current;
            if (delta > 0) OnItemPlaced(delta);
        }

        private void EnsureEntries()
        {
            foreach (var def in AchievementCatalog.All)
            {
                if (_save.Achievements.entries.Find(e => e.achievementId == def.id) == null)
                {
                    _save.Achievements.entries.Add(new AchievementDoc
                    {
                        achievementId = def.id,
                        progress = 0,
                        target = def.target,
                        rewardItemIds = string.IsNullOrEmpty(def.rewardItemId)
                            ? new List<string>()
                            : new List<string> { def.rewardItemId }
                    });
                }
            }
            _save.SaveAchievements();
        }

        public void OnBookFinished(BookDoc book)
        {
            // BooksFinished
            ProgressBy(AchievementCatalog.TriggerKind.BooksFinished, null, 1);

            // Genre matches
            if (book.genres != null)
            {
                foreach (var g in book.genres)
                {
                    if (string.IsNullOrEmpty(g)) continue;
                    var lc = g.ToLowerInvariant();
                    foreach (var def in AchievementCatalog.All)
                    {
                        if (def.trigger == AchievementCatalog.TriggerKind.GenreFinished
                            && !string.IsNullOrEmpty(def.filter)
                            && lc.Contains(def.filter))
                        {
                            ProgressOne(def, 1);
                        }
                    }
                }
            }

            _save.Player.booksReadCount += 1;
            _save.SavePlayer();
        }

        public void OnItemPlaced(int delta = 1)
        {
            ProgressBy(AchievementCatalog.TriggerKind.ItemsPlaced, null, delta);
        }

        private void ProgressBy(AchievementCatalog.TriggerKind kind, string filter, int delta)
        {
            foreach (var def in AchievementCatalog.All)
            {
                if (def.trigger != kind) continue;
                if (kind == AchievementCatalog.TriggerKind.GenreFinished
                    && (filter == null || def.filter != filter)) continue;
                ProgressOne(def, delta);
            }
        }

        private void ProgressOne(AchievementCatalog.Def def, int delta)
        {
            var entry = _save.Achievements.entries.Find(e => e.achievementId == def.id);
            if (entry == null) return;
            if (entry.unlockedAtUnixMs > 0) return; // already unlocked

            entry.progress += delta;
            if (entry.progress >= entry.target)
            {
                entry.progress = entry.target;
                entry.unlockedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                if (def.rewardSoft > 0) _inv.AddCurrency(def.rewardSoft);
                if (!string.IsNullOrEmpty(def.rewardItemId)) _inv.Grant(def.rewardItemId, "achievement");

                Unlocked?.Invoke(def);
                Debug.Log($"[Achievement] Unlocked: {def.title}");
            }
            _save.SaveAchievements();
        }
    }
}
