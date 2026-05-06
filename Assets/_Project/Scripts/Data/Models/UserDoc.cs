using System;

namespace LibraryGame.Data.Models
{
    [Serializable]
    public sealed class UserDoc
    {
        public string uid;
        public string displayName;
        public string handle;            // @lowercase, unique via Cloud Function
        public string photoURL;
        public DateTime createdAt;
        public DateTime lastSeenAt;
        public string locale;
        public Privacy privacy = new();
        public Stats stats = new();
        public Currency currency = new();
        public Flags flags = new();      // server-only writable

        [Serializable]
        public sealed class Privacy
        {
            public bool profilePublic;
            public bool libraryPublic;
        }

        [Serializable]
        public sealed class Stats
        {
            public int booksRead;
            public int booksOwned;
            public int reviewsCount;
            public int streakDays;
        }

        [Serializable]
        public sealed class Currency
        {
            public int soft;
            public int hard;             // server-authoritative
        }

        [Serializable]
        public sealed class Flags
        {
            public bool isPremium;
            public string activeSeasonPassId;
        }
    }
}
