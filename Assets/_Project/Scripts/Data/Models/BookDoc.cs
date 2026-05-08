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
        public int publishYear = -1;     // -1 = unknown
        public int pageCount = -1;
        public List<string> genres = new();
        public string seriesName;
        public int seriesIndex = -1;
        public string status = "want_to_read"; // "want_to_read" | "reading" | "read" | "dnf"
        public List<string> shelves = new();
        public long addedAtUnixMs;
        public long startedAtUnixMs;     // 0 if not set
        public long finishedAtUnixMs;
        public float rating = -1f;       // -1 = unrated, else 0..5
        public int progressPage;
        public float progressPercent;
    }
}
