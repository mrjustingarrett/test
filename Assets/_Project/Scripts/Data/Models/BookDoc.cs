using System;
using System.Collections.Generic;

namespace LibraryGame.Data.Models
{
    [Serializable]
    public sealed class BookDoc
    {
        public string bookId;
        public string source;            // "openlibrary" | "manual" | "goodreads"
        public string olWorkKey;
        public string olEditionKey;
        public string isbn10;
        public string isbn13;
        public string title;
        public List<string> authors = new();
        public string coverUrl;
        public string publisher;
        public int? publishYear;
        public int? pageCount;
        public List<string> genres = new();
        public Series series;
        public string status;            // "want_to_read" | "reading" | "read" | "dnf"
        public List<string> shelves = new();
        public DateTime addedAt;
        public DateTime? startedAt;
        public DateTime? finishedAt;
        public float? rating;            // 0..5, 0.5 increments
        public Progress progress;

        [Serializable]
        public sealed class Series
        {
            public string name;
            public int index;
        }

        [Serializable]
        public sealed class Progress
        {
            public int page;
            public float percent;
        }
    }
}
