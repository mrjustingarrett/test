using LibraryGame.Core;
using LibraryGame.Services.Inventory;
using LibraryGame.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace LibraryGame.UI.Panels
{
    public sealed class ShopPanel : MonoBehaviour
    {
        private RectTransform _content;
        private Text _statusText;
        private Text _coinsText;

        public static ShopPanel Create(RectTransform host)
        {
            var bg = UIFactory.Panel(host, "ShopPanel", Vector2.zero, Vector2.one, UIFactory.BgPanel);
            var p = bg.AddComponent<ShopPanel>();
            p.Build(bg.transform);
            p.OnEnable();
            return p;
        }

        private void Build(Transform root)
        {
            UIFactory.HeaderBar(root, "Shop", () => gameObject.SetActive(false));

            var info = UIFactory.Panel(root, "CurrencyBar", new Vector2(0, 0.86f), new Vector2(1, 0.92f), UIFactory.BgDark);
            _coinsText = UIFactory.Label(info.transform, "", 28, TextAnchor.MiddleCenter, UIFactory.Accent);

            var body = UIFactory.Panel(root, "Body", new Vector2(0, 0.04f), new Vector2(1, 0.86f), UIFactory.BgPanel);
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
            _coinsText.text = $"Coins: {inv.SoftCurrency}";

            for (int i = 0; i < FurnitureCatalog.All.Count; i++)
            {
                var item = FurnitureCatalog.All[i];
                if (item.priceSoft <= 0) continue; // achievement-only items hidden from shop
                BuildItemRow(item, inv);
            }
        }

        private void BuildItemRow(FurnitureCatalog.Item item, InventorySystem inv)
        {
            var row = UIFactory.Row(_content.transform, 110);
            var rt = (RectTransform)row.transform;

            var swatch = new GameObject("Swatch");
            swatch.transform.SetParent(rt, false);
            var srt = swatch.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0, 0.15f); srt.anchorMax = new Vector2(0, 0.85f);
            srt.pivot = new Vector2(0, 0.5f);
            srt.sizeDelta = new Vector2(80, 0);
            srt.anchoredPosition = new Vector2(20, 0);
            var simg = swatch.AddComponent<Image>();
            simg.color = item.color;

            var info = new GameObject("Info");
            info.transform.SetParent(rt, false);
            var irt = info.AddComponent<RectTransform>();
            irt.anchorMin = new Vector2(0, 0); irt.anchorMax = new Vector2(0.65f, 1);
            irt.offsetMin = new Vector2(120, 8); irt.offsetMax = new Vector2(-10, -8);
            var t = info.AddComponent<Text>();
            t.text = $"{item.displayName}\n<color=#cc7d3c>{item.priceSoft} coins</color>";
            t.supportRichText = true;
            t.font = UIFactory.UiFont; t.fontSize = 24; t.color = UIFactory.TextLight;
            t.alignment = TextAnchor.MiddleLeft;

            string btnLabel = inv.Has(item.id) ? "Owned" : "Buy";
            var btn = UIFactory.Button(rt, btnLabel, new Vector2(180, 80), () =>
            {
                if (inv.Has(item.id)) { _statusText.text = "Already owned."; return; }
                if (inv.Buy(item.id, item.priceSoft))
                {
                    _statusText.text = $"Bought {item.displayName}.";
                    BuildList();
                }
                else
                {
                    _statusText.text = "Not enough coins.";
                }
            });
            var brt = btn.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(1, 0.5f); brt.anchorMax = new Vector2(1, 0.5f);
            brt.pivot = new Vector2(1, 0.5f);
            brt.anchoredPosition = new Vector2(-20, 0);
        }
    }
}
