using System;

namespace LibraryGame.Data.Models
{
    [Serializable]
    public sealed class ReviewDoc
    {
        public string reviewId;
        public string bookId;
        public string body;              // markdown
        public float rating;
        public bool spoiler;
        public DateTime createdAt;
        public DateTime updatedAt;
        public string visibility;        // "private" | "friends" | "public"
    }
}
