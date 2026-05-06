using System;

namespace LibraryGame.Data.Models
{
    [Serializable]
    public sealed class OwnedItemDoc
    {
        public string itemId;
        public DateTime acquiredAt;
        public string source;            // "achievement" | "purchase_soft" | "purchase_hard" | "season_pass" | "gift"
        public string seasonPassId;
        public string variantId;
        public int quantity = 1;
    }
}
