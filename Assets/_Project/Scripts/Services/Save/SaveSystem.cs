using System;
using System.Collections.Generic;
using LibraryGame.Data.Models;

namespace LibraryGame.Services.Save
{
    /// <summary>
    /// Local-only save coordinator for pre-M2. When M2 lands, swap to a
    /// two-tier (local + Firestore) implementation behind this same API.
    /// </summary>
    public sealed class SaveSystem
    {
        private const string LibraryDocId = "library";
        private const string OwnedDocId = "owned";
        private const string LayoutDocId = "roomLayout";
        private const string AchievementsDocId = "achievements";
        private const string PlayerDocId = "player";

        private readonly LocalSaveStore _store = new();

        public event Action LibraryChanged;
        public event Action OwnedItemsChanged;
        public event Action LayoutChanged;
        public event Action AchievementsChanged;
        public event Action PlayerChanged;

        public LibraryDoc Library { get; private set; }
        public OwnedItemsDoc Owned { get; private set; }
        public RoomLayoutDoc Layout { get; private set; }
        public AchievementsDoc Achievements { get; private set; }
        public PlayerDoc Player { get; private set; }

        public SaveSystem()
        {
            Library = _store.Load<LibraryDoc>(LibraryDocId) ?? new LibraryDoc();
            Owned = _store.Load<OwnedItemsDoc>(OwnedDocId) ?? new OwnedItemsDoc();
            Layout = _store.Load<RoomLayoutDoc>(LayoutDocId) ?? new RoomLayoutDoc { layoutId = "main", schemaVersion = 1 };
            Achievements = _store.Load<AchievementsDoc>(AchievementsDocId) ?? new AchievementsDoc();
            Player = _store.Load<PlayerDoc>(PlayerDocId) ?? new PlayerDoc { softCurrency = 50 };
        }

        public void SaveLibrary() { _store.Save(LibraryDocId, Library); LibraryChanged?.Invoke(); }
        public void SaveOwned() { _store.Save(OwnedDocId, Owned); OwnedItemsChanged?.Invoke(); }
        public void SaveLayout() { _store.Save(LayoutDocId, Layout); LayoutChanged?.Invoke(); }
        public void SaveAchievements() { _store.Save(AchievementsDocId, Achievements); AchievementsChanged?.Invoke(); }
        public void SavePlayer() { _store.Save(PlayerDocId, Player); PlayerChanged?.Invoke(); }

        public void SaveAll()
        {
            SaveLibrary();
            SaveOwned();
            SaveLayout();
            SaveAchievements();
            SavePlayer();
        }
    }

    [Serializable]
    public sealed class LibraryDoc
    {
        public List<BookDoc> books = new();
    }

    [Serializable]
    public sealed class OwnedItemsDoc
    {
        public List<OwnedItemDoc> items = new();
    }

    [Serializable]
    public sealed class AchievementsDoc
    {
        public List<AchievementDoc> entries = new();
    }

    [Serializable]
    public sealed class PlayerDoc
    {
        public int softCurrency;
        public int booksReadCount;
        public string lastWeather = "clear";
    }
}
