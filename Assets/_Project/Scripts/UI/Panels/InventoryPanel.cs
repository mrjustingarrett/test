using LibraryGame.Core;
using LibraryGame.Services.Inventory;
using LibraryGame.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace LibraryGame.UI.Panels
{
    public sealed class InventoryPanel : MonoBehaviour
    {
        private RectTransform _content;
        private Text _statusText;

        public static InventoryPanel Create(RectTransform host)
        {
            var bg = UIFactory.Panel(host, "InventoryPanel", Vector2.zero, Vector2.one, UIFactory.BgPanel);
            var p = bg.AddComponent<InventoryPanel>();
            p.Build(bg.transform);
            p.OnEnable();
            return p;
        }

        private void Build(Transform root)
        {
            UIFactory.HeaderBar(root, "Inventory", () => gameObject.SetActive(false));
            var actionBar = UIFactory.Panel(root, "ActionBar", new Vector2(0, 0.84f), new Vector2(1, 0.92f), UIFactory.BgDark);
            var hl = actionBar.AddComponent<HorizontalLayoutGroup>();
            hl.padding = new RectOffset(20, 20, 12, 12); hl.spacing = 8;
            hl.childForceExpandWidth = true; hl.childForceExpandHeight = true;
            UIFactory.Button(actionBar.transform, "Clear Room", new Vector2(0, 80), () =>
            {
                ServiceLocator.Get<InventorySystem>().ClearLayout();
                BuildList();
            });

            var body = UIFactory.Panel(root, "Body", new Vector2(0, 0.04f), new Vector2(1, 0.84f), UIFactory.BgPanel);
            var status = UIFactory.Panel(root, "Status", new Vector2(0, 0f), new Vector2(1, 0.04f), UIFactory.BgDark);
            _statusText = UIFactory.Label(status.transform, "", 22, TextAnchor.MiddleCenter, UIFactory.TextDim);

            UIFactory.ScrollList(body.transform, out _content);
        }

        private void OnEnable()
        {
            if (_content != null) BuildList();
        }

        private void BuildList()
        {
            for (int i = _content.childCount - 1; i >= 0; i--) Destroy(_content.GetChild(i).gameObject);

            var inv = ServiceLocator.Get<InventorySystem>();
            if (inv.Owned.Count == 0)
            {
                UIFactory.Label(UIFactory.Row(_content.transform, 80).transform,
                    "No items yet. Read books to unlock furniture or buy from the Shop.",
                    24, TextAnchor.MiddleCenter, UIFactory.TextDim);
                return;
            }

            for (int i = 0; i < inv.Owned.Count; i++)
            {
                var owned = inv.Owned[i];
                var item = FurnitureCatalog.Get(owned.itemId);
                if (item == null) continue;
                BuildItemRow(item, inv);
            }
        }

        private void BuildItemRow(FurnitureCatalog.Item item, InventorySystem inv)
        {
            var row = UIFactory.Row(_content.transform, 110);
            var rt = (RectTransform)row.transform;

            // Color swatch
            var swatch = new GameObject("Swatch");
            swatch.transform.SetParent(rt, false);
            var srt = swatch.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0, 0.15f); srt.anchorMax = new Vector2(0, 0.85f);
            srt.pivot = new Vector2(0, 0.5f);
            srt.sizeDelta = new Vector2(80, 0);
            srt.anchoredPosition = new Vector2(20, 0);
            var simg = swatch.AddComponent<Image>();
            simg.color = item.color;

            // Name + source
            var info = new GameObject("Info");
            info.transform.SetParent(rt, false);
            var irt = info.AddComponent<RectTransform>();
            irt.anchorMin = new Vector2(0, 0); irt.anchorMax = new Vector2(0.65f, 1);
            irt.offsetMin = new Vector2(120, 8); irt.offsetMax = new Vector2(-10, -8);
            var t = info.AddComponent<Text>();
            string placedSuffix = inv.IsPlaced(item.id) ? "  ·  In room" : "";
            t.text = $"{item.displayName}{placedSuffix}";
            t.font = UIFactory.UiFont; t.fontSize = 28; t.color = UIFactory.TextLight;
            t.alignment = TextAnchor.MiddleLeft;

            // Place / Unplace button
            string label = inv.IsPlaced(item.id) ? "Placed" : "Place";
            var btn = UIFactory.Button(rt, label, new Vector2(180, 80), () =>
            {
                if (inv.IsPlaced(item.id))
                {
                    _statusText.text = "Already in your room — Clear Room to reset.";
                    return;
                }
                if (!inv.Place(item.id))
                {
                    _statusText.text = "All slots full — Clear Room to reset.";
                    return;
                }
                _statusText.text = $"Placed {item.displayName} in your room.";
                BuildList();
            });
            var brt = btn.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(1, 0.5f); brt.anchorMax = new Vector2(1, 0.5f);
            brt.pivot = new Vector2(1, 0.5f);
            brt.anchoredPosition = new Vector2(-20, 0);
        }
    }
}
