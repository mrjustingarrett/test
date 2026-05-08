using System;
using System.Collections.Generic;
using LibraryGame.Core;
using LibraryGame.Data.Models;
using LibraryGame.Services.Save;
using UnityEngine;

namespace LibraryGame.Services.Books
{
    /// <summary>
    /// Library-side façade. Holds the user's library doc (via SaveSystem),
    /// converts Open Library hits into BookDoc, and notifies listeners.
    /// </summary>
    public sealed class BookService
    {
        private readonly SaveSystem _save;

        public event Action LibraryChanged;
        public IReadOnlyList<BookDoc> Library => _save.Library.books;

        public BookService(SaveSystem save)
        {
            _save = save;
            _save.LibraryChanged += () => LibraryChanged?.Invoke();
        }

        public BookDoc AddFromSearchHit(OpenLibraryClient.SearchHit hit)
        {
            string isbn13 = FirstIsbn(hit.isbn, len: 13);
            string isbn10 = FirstIsbn(hit.isbn, len: 10);
            string id = !string.IsNullOrEmpty(isbn13) ? $"isbn:{isbn13}"
                       : !string.IsNullOrEmpty(isbn10) ? $"isbn:{isbn10}"
                       : $"ol:{hit.key}";
            if (Find(id) != null) return Find(id);

            var doc = new BookDoc
            {
                bookId = id,
                source = "openlibrary",
                olWorkKey = hit.key,
                isbn10 = isbn10,
                isbn13 = isbn13,
                title = hit.title,
                authors = hit.author_name ?? new List<string>(),
                coverUrl = OpenLibraryClient.CoverUrl(hit.cover_i),
                publisher = hit.publisher != null && hit.publisher.Count > 0 ? hit.publisher[0] : null,
                publishYear = hit.first_publish_year > 0 ? hit.first_publish_year : -1,
                pageCount = hit.number_of_pages_median > 0 ? hit.number_of_pages_median : -1,
                genres = hit.subject ?? new List<string>(),
                addedAtUnixMs = NowMs()
            };

            _save.Library.books.Add(doc);
            _save.SaveLibrary();
            return doc;
        }

        public BookDoc AddManual(string title, string author, int year, string isbn = null)
        {
            string id = !string.IsNullOrEmpty(isbn) ? $"isbn:{isbn}" : $"manual:{Guid.NewGuid():N}";
            if (Find(id) != null) return Find(id);

            var doc = new BookDoc
            {
                bookId = id,
                source = "manual",
                isbn13 = isbn != null && isbn.Length == 13 ? isbn : null,
                isbn10 = isbn != null && isbn.Length == 10 ? isbn : null,
                title = title,
                authors = string.IsNullOrEmpty(author)
                    ? new List<string>()
                    : new List<string>(author.Split(',')),
                publishYear = year > 0 ? year : -1,
                addedAtUnixMs = NowMs()
            };
            // Trim author list whitespace.
            for (int i = 0; i < doc.authors.Count; i++) doc.authors[i] = doc.authors[i].Trim();

            _save.Library.books.Add(doc);
            _save.SaveLibrary();
            return doc;
        }

        public void SetStatus(string bookId, string status)
        {
            var b = Find(bookId);
            if (b == null) return;
            b.status = status;
            if (status == "read" && b.finishedAtUnixMs == 0) b.finishedAtUnixMs = NowMs();
            if (status == "reading" && b.startedAtUnixMs == 0) b.startedAtUnixMs = NowMs();
            _save.SaveLibrary();
        }

        public void Remove(string bookId)
        {
            var idx = _save.Library.books.FindIndex(b => b.bookId == bookId);
            if (idx >= 0)
            {
                _save.Library.books.RemoveAt(idx);
                _save.SaveLibrary();
            }
        }

        public BookDoc Find(string bookId) => _save.Library.books.Find(b => b.bookId == bookId);

        private static string FirstIsbn(List<string> isbns, int len)
        {
            if (isbns == null) return null;
            for (int i = 0; i < isbns.Count; i++)
            {
                var raw = isbns[i] ?? string.Empty;
                var stripped = raw.Replace("-", "").Replace(" ", "");
                if (stripped.Length == len) return stripped;
            }
            return null;
        }

        public static long NowMs() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
