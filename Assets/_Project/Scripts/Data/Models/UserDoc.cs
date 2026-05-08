using System;

namespace LibraryGame.Data.Models
{
    [Serializable]
    public sealed class UserDoc
    {
        public string uid;
        public string displayName;
        public string handle;
        public string photoURL;
        public long createdAtUnixMs;
        public long lastSeenAtUnixMs;
        public string locale;
        public bool privacyProfilePublic;
        public bool privacyLibraryPublic;
        public int booksRead;
        public int booksOwned;
        public int reviewsCount;
        public int streakDays;
        public int currencySoft;
        public int currencyHard;        // server-authoritative
        public bool isPremium;
        public string activeSeasonPassId;
    }
}
