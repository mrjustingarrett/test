using System;
using System.Collections.Generic;

namespace LibraryGame.Data.Models
{
    [Serializable]
    public sealed class AchievementDoc
    {
        public string achievementId;
        public int progress;
        public int target;
        public long unlockedAtUnixMs; // 0 if locked
        public long rewardClaimedAtUnixMs;
        public List<string> rewardItemIds = new();
    }
}
