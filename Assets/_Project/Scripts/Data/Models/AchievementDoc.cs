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
        public DateTime? unlockedAt;
        public DateTime? rewardClaimedAt;
        public List<string> rewardItemIds = new();
    }
}
