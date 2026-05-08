using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LibraryGame.Services.Books
{
    /// <summary>
    /// Thin async wrapper over Open Library's public APIs.
    ///   /search.json?q=...
    ///   /isbn/{isbn}.json
    ///   covers.openlibrary.org/b/id/{coverId}-M.jpg
    /// User-Agent must contain contact info per OL ToS.
    /// </summary>
    public sealed class OpenLibraryClient
    {
        private const string SearchBase = "https://openlibrary.org/search.json";
        private const string IsbnBase = "https://openlibrary.org/isbn/";
        private const string CoverBase = "https://covers.openlibrary.org/b/id/";
        private readonly string _userAgent;

        public OpenLibraryClient(string userAgent)
        {
            _userAgent = string.IsNullOrEmpty(userAgent) ? "LibraryGame/1.0" : userAgent;
        }

        public IEnumerator Search(string query, int limit, Action<List<SearchHit>> onResult, Action<string> onError = null)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                onResult?.Invoke(new List<SearchHit>());
                yield break;
            }

            var url = $"{SearchBase}?q={UnityWebRequest.EscapeURL(query)}&limit={limit}";
            using var req = UnityWebRequest.Get(url);
            req.SetRequestHeader("User-Agent", _userAgent);
            req.timeout = 15;
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Open Library search failed: {req.error}");
                onResult?.Invoke(new List<SearchHit>());
                yield break;
            }

            try
            {
                var resp = JsonUtility.FromJson<SearchResponse>(req.downloadHandler.text);
                onResult?.Invoke(resp != null && resp.docs != null ? resp.docs : new List<SearchHit>());
            }
            catch (Exception ex)
            {
                onError?.Invoke($"Parse failed: {ex.Message}");
                onResult?.Invoke(new List<SearchHit>());
            }
        }

        public static string CoverUrl(int coverId, string size = "M")
            => coverId > 0 ? $"{CoverBase}{coverId}-{size}.jpg" : null;

        // Open Library /search.json subset. We only deserialize what we use.
        [Serializable]
        public sealed class SearchResponse
        {
            public List<SearchHit> docs;
        }

        [Serializable]
        public sealed class SearchHit
        {
            public string key;                  // /works/OLxxxxW
            public string title;
            public List<string> author_name;
            public int first_publish_year;
            public int cover_i;                 // cover edition id
            public List<string> isbn;           // multiple isbns possible
            public List<string> publisher;
            public int number_of_pages_median;
            public List<string> subject;
            public List<string> ia_collection_s;
        }
    }
}
