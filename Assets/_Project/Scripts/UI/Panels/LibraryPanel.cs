using System.Collections.Generic;
using LibraryGame.Core;
using LibraryGame.Data.Models;
using LibraryGame.Services.Achievements;
using LibraryGame.Services.Books;
using LibraryGame.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace LibraryGame.UI.Panels
{
    /// <summary>
    /// "Library" menu panel. Three tabs: My Books, Search, Add Manually.
    /// </summary>
    public sealed class LibraryPanel : MonoBehaviour
    {
        private enum Tab { Mine, Search, Manual }

        private Tab _tab;
        private RectTransform _content;
        private InputField _searchInput;
        private InputField _manualTitle, _manualAuthor, _manualYear, _manualIsbn;
        private Text _statusText;
        private List<OpenLibraryClient.SearchHit> _lastResults = new();

        public static LibraryPanel Create(RectTransform host)
        {
            var bg = UIFactory.Panel(host, "LibraryPanel", Vector2.zero, Vector2.one, UIFactory.BgPanel);
            var panel = bg.AddComponent<LibraryPanel>();
            panel.Build(bg.transform);
            return panel;
        }

        private void Build(Transform root)
        {
            UIFactory.HeaderBar(root, "Library", () => gameObject.SetActive(false));

            // Tab row at top below header.
            var tabBar = UIFactory.Panel(root, "TabBar", new Vector2(0, 0.84f), new Vector2(1, 0.92f), UIFactory.BgDark);
            var hl = tabBar.AddComponent<HorizontalLayoutGroup>();
            hl.padding = new RectOffset(20, 20, 12, 12);
            hl.spacing = 8;
            hl.childForceExpandWidth = true;
            hl.childForceExpandHeight = true;
            UIFactory.Button(tabBar.transform, "My Books",     new Vector2(0, 80), () => Switch(Tab.Mine));
            UIFactory.Button(tabBar.transform, "Search",       new Vector2(0, 80), () => Switch(Tab.Search));
            UIFactory.Button(tabBar.transform, "Add Manually", new Vector2(0, 80), () => Switch(Tab.Manual));

            // Content area below tab bar (84% height of body).
            var body = UIFactory.Panel(root, "Body", new Vector2(0, 0.04f), new Vector2(1, 0.84f), UIFactory.BgPanel);

            // Status / hint line
            var status = UIFactory.Panel(root, "Status", new Vector2(0, 0f), new Vector2(1, 0.04f), UIFactory.BgDark);
            _statusText = UIFactory.Label(status.transform, "", 22, TextAnchor.MiddleCenter, UIFactory.TextDim);

            UIFactory.ScrollList(body.transform, out _content);
            Switch(Tab.Mine);
        }

        private void Switch(Tab t)
        {
            _tab = t;
            ClearContent();
            switch (t)
            {
                case Tab.Mine:    BuildMine();   break;
                case Tab.Search:  BuildSearch(); break;
                case Tab.Manual:  BuildManual(); break;
            }
        }

        private void ClearContent()
        {
            for (int i = _content.childCount - 1; i >= 0; i--) Destroy(_content.GetChild(i).gameObject);
            _statusText.text = "";
        }

        // ---------------- My Books ----------------

        private void BuildMine()
        {
            var svc = ServiceLocator.Get<BookService>();
            if (svc.Library.Count == 0)
            {
                UIFactory.Label(UIFactory.Row(_content.transform, 80).transform,
                    "No books yet. Try Search or Add Manually.",
                    26, TextAnchor.MiddleCenter, UIFactory.TextDim);
                return;
            }
            for (int i = 0; i < svc.Library.Count; i++)
            {
                var book = svc.Library[i];
                BuildBookRow(book);
            }
        }

        private void BuildBookRow(BookDoc book)
        {
            var row = UIFactory.Row(_content.transform, 130);
            var rowRT = (RectTransform)row.transform;

            // Title
            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(rowRT, false);
            var trt = titleGo.AddComponent<RectTransform>();
            trt.anchorMin = new Vector2(0f, 0.55f); trt.anchorMax = new Vector2(0.65f, 1f);
            trt.offsetMin = new Vector2(20, 0); trt.offsetMax = new Vector2(-10, -8);
            var tt = titleGo.AddComponent<Text>();
            tt.text = book.title ?? "(untitled)";
            tt.font = UIFactory.UiFont; tt.fontSize = 28; tt.color = UIFactory.TextLight;
            tt.alignment = TextAnchor.MiddleLeft;

            // Author / year
            var subGo = new GameObject("Sub");
            subGo.transform.SetParent(rowRT, false);
            var srt = subGo.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0f, 0f); srt.anchorMax = new Vector2(0.65f, 0.55f);
            srt.offsetMin = new Vector2(20, 8); srt.offsetMax = new Vector2(-10, 0);
            var st = subGo.AddComponent<Text>();
            string authors = book.authors != null && book.authors.Count > 0 ? string.Join(", ", book.authors) : "Unknown";
            string year = book.publishYear > 0 ? $"  ·  {book.publishYear}" : "";
            string status = $"  ·  {Pretty(book.status)}";
            st.text = authors + year + status;
            st.font = UIFactory.UiFont; st.fontSize = 22; st.color = UIFactory.TextDim;
            st.alignment = TextAnchor.MiddleLeft;

            // Action buttons on right
            var btnRow = UIFactory.Panel(rowRT, "Actions", new Vector2(0.65f, 0.05f), new Vector2(0.99f, 0.95f), new Color(0,0,0,0));
            var hl = btnRow.AddComponent<HorizontalLayoutGroup>();
            hl.padding = new RectOffset(4,4,4,4); hl.spacing = 6;
            hl.childForceExpandWidth = true; hl.childForceExpandHeight = true;

            var svc = ServiceLocator.Get<BookService>();
            UIFactory.Button(btnRow.transform, NextStatusLabel(book.status), new Vector2(0, 80), () =>
            {
                svc.SetStatus(book.bookId, NextStatus(book.status));
                if (NextStatus(book.status) == "read")
                {
                    AchievementSystem.Instance?.OnBookFinished(book);
                }
                Switch(Tab.Mine);
            });
            UIFactory.Button(btnRow.transform, "Remove", new Vector2(0, 80), () =>
            {
                svc.Remove(book.bookId);
                Switch(Tab.Mine);
            });
        }

        private static string Pretty(string status) => status switch
        {
            "read" => "Read",
            "reading" => "Reading",
            "want_to_read" => "Want to read",
            "dnf" => "DNF",
            _ => status ?? "?"
        };

        private static string NextStatus(string s) => s switch
        {
            "want_to_read" => "reading",
            "reading" => "read",
            "read" => "want_to_read",
            _ => "want_to_read"
        };

        private static string NextStatusLabel(string s) => s switch
        {
            "want_to_read" => "Start",
            "reading" => "Finish",
            "read" => "Reset",
            _ => "Start"
        };

        // ---------------- Search ----------------

        private void BuildSearch()
        {
            // Search bar row
            var bar = UIFactory.Row(_content.transform, 100);
            var hl = bar.AddComponent<HorizontalLayoutGroup>();
            hl.padding = new RectOffset(8,8,8,8); hl.spacing = 8;
            hl.childForceExpandWidth = true; hl.childForceExpandHeight = true;
            _searchInput = UIFactory.InputField(bar.transform, "Title or author…", new Vector2(0, 80));
            UIFactory.Button(bar.transform, "Search", new Vector2(220, 80), DoSearch);

            if (_lastResults.Count == 0)
            {
                UIFactory.Label(UIFactory.Row(_content.transform, 80).transform,
                    "Type a title or author and tap Search.",
                    24, TextAnchor.MiddleCenter, UIFactory.TextDim);
                return;
            }

            for (int i = 0; i < _lastResults.Count; i++)
                BuildSearchHitRow(_lastResults[i]);
        }

        private void DoSearch()
        {
            var query = _searchInput != null ? _searchInput.text : "";
            if (string.IsNullOrWhiteSpace(query))
            {
                _statusText.text = "Enter a search term.";
                return;
            }
            _statusText.text = "Searching…";
            var ol = ServiceLocator.Get<OpenLibraryClient>();
            StartCoroutine(ol.Search(query, 20, hits =>
            {
                _lastResults = hits ?? new List<OpenLibraryClient.SearchHit>();
                _statusText.text = _lastResults.Count > 0 ? $"{_lastResults.Count} results" : "No results.";
                Switch(Tab.Search); // rebuild content
            }, err =>
            {
                _statusText.text = err;
            }));
        }

        private void BuildSearchHitRow(OpenLibraryClient.SearchHit hit)
        {
            var row = UIFactory.Row(_content.transform, 120);
            var rt = (RectTransform)row.transform;

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(rt, false);
            var trt = titleGo.AddComponent<RectTransform>();
            trt.anchorMin = new Vector2(0f, 0.55f); trt.anchorMax = new Vector2(0.7f, 1f);
            trt.offsetMin = new Vector2(20, 0); trt.offsetMax = new Vector2(-10, -8);
            var tt = titleGo.AddComponent<Text>();
            tt.text = hit.title ?? "(untitled)";
            tt.font = UIFactory.UiFont; tt.fontSize = 26; tt.color = UIFactory.TextLight;
            tt.alignment = TextAnchor.MiddleLeft;

            var subGo = new GameObject("Sub");
            subGo.transform.SetParent(rt, false);
            var srt = subGo.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0f, 0f); srt.anchorMax = new Vector2(0.7f, 0.55f);
            srt.offsetMin = new Vector2(20, 8); srt.offsetMax = new Vector2(-10, 0);
            var st = subGo.AddComponent<Text>();
            string authors = hit.author_name != null && hit.author_name.Count > 0 ? string.Join(", ", hit.author_name) : "Unknown";
            string year = hit.first_publish_year > 0 ? $"  ·  {hit.first_publish_year}" : "";
            st.text = authors + year;
            st.font = UIFactory.UiFont; st.fontSize = 22; st.color = UIFactory.TextDim;
            st.alignment = TextAnchor.MiddleLeft;

            var btn = UIFactory.Button(rt, "Add", new Vector2(180, 80), () =>
            {
                var svc = ServiceLocator.Get<BookService>();
                svc.AddFromSearchHit(hit);
                _statusText.text = $"Added \"{hit.title}\"";
            });
            var brt = btn.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(1f, 0.5f); brt.anchorMax = new Vector2(1f, 0.5f);
            brt.pivot = new Vector2(1f, 0.5f);
            brt.anchoredPosition = new Vector2(-20, 0);
        }

        // ---------------- Manual Add ----------------

        private void BuildManual()
        {
            _manualTitle  = UIFactory.InputField(UIFactory.Row(_content.transform, 100).transform, "Title", new Vector2(0, 80));
            _manualAuthor = UIFactory.InputField(UIFactory.Row(_content.transform, 100).transform, "Author(s), comma-separated", new Vector2(0, 80));
            _manualYear   = UIFactory.InputField(UIFactory.Row(_content.transform, 100).transform, "Year (optional)", new Vector2(0, 80));
            _manualYear.contentType = InputField.ContentType.IntegerNumber;
            _manualIsbn   = UIFactory.InputField(UIFactory.Row(_content.transform, 100).transform, "ISBN (optional)", new Vector2(0, 80));

            var bar = UIFactory.Row(_content.transform, 100);
            var hl = bar.AddComponent<HorizontalLayoutGroup>();
            hl.padding = new RectOffset(8,8,8,8); hl.spacing = 8;
            hl.childForceExpandWidth = true; hl.childForceExpandHeight = true;
            UIFactory.Button(bar.transform, "Save", new Vector2(0, 80), () =>
            {
                if (string.IsNullOrWhiteSpace(_manualTitle.text)) { _statusText.text = "Title required."; return; }
                int.TryParse(_manualYear.text, out int year);
                ServiceLocator.Get<BookService>().AddManual(_manualTitle.text.Trim(),
                    _manualAuthor.text, year, _manualIsbn.text.Trim());
                _statusText.text = "Saved.";
                _manualTitle.text = ""; _manualAuthor.text = ""; _manualYear.text = ""; _manualIsbn.text = "";
            });
        }
    }
}
