using System;
using System.Collections.Generic;

namespace LibraryGame.Data.Models
{
    [Serializable]
    public sealed class SeasonPassDoc
    {
        public string seasonId;
        public int tier;
        public int xp;
        public bool isPremium;
        public List<int> claimedFreeTiers = new();
        public List<int> claimedPremiumTiers = new();
        public DateTime startedAt;
        public DateTime expiresAt;
    }
}
