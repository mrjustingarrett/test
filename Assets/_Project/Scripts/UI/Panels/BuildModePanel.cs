using System.Text;
using LibraryGame.Core;
using LibraryGame.Services.Inventory;
using LibraryGame.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace LibraryGame.UI.Panels
{
    /// <summary>
    /// Minimal "Build / Settings" panel: shows current room layout, weather
    /// toggle, and an info panel. Placement happens via Inventory → Place.
    /// Fuller drag/move/rotate UI is post-MVP.
    /// </summary>
    public sealed class BuildModePanel : MonoBehaviour
    {
        private Text _layoutText;

        public static BuildModePanel Create(RectTransform host)
        {
            var bg = UIFactory.Panel(host, "BuildModePanel", Vector2.zero, Vector2.one, UIFactory.BgPanel);
            var p = bg.AddComponent<BuildModePanel>();
            p.Build(bg.transform);
            p.OnEnable();
            return p;
        }

        private void Build(Transform root)
        {
            UIFactory.HeaderBar(root, "Edit Room", () => gameObject.SetActive(false));

            var body = UIFactory.Panel(root, "Body", new Vector2(0, 0.04f), new Vector2(1, 0.92f), UIFactory.BgPanel);

            var layoutTextRow = UIFactory.Panel(body.transform, "Info",
                new Vector2(0.04f, 0.55f), new Vector2(0.96f, 0.95f), UIFactory.BgRow);
            _layoutText = UIFactory.Label(layoutTextRow.transform, "", 24, TextAnchor.UpperLeft, UIFactory.TextLight);

            // Buttons: Clear Room, Weather row
            var clearRow = UIFactory.Panel(body.transform, "ClearRow",
                new Vector2(0.04f, 0.45f), new Vector2(0.96f, 0.53f), new Color(0,0,0,0));
            var hl = clearRow.AddComponent<HorizontalLayoutGroup>();
            hl.padding = new RectOffset(0,0,0,0); hl.spacing = 12;
            hl.childForceExpandWidth = true; hl.childForceExpandHeight = true;
            UIFactory.Button(clearRow.transform, "Clear Room", new Vector2(0, 80), () =>
            {
                ServiceLocator.Get<InventorySystem>().ClearLayout();
                Refresh();
            });

            var weatherLabelRow = UIFactory.Panel(body.transform, "WeatherLabel",
                new Vector2(0.04f, 0.36f), new Vector2(0.96f, 0.43f), new Color(0,0,0,0));
            UIFactory.Label(weatherLabelRow.transform, "Weather", 28, TextAnchor.MiddleLeft);

            var weatherRow = UIFactory.Panel(body.transform, "WeatherRow",
                new Vector2(0.04f, 0.27f), new Vector2(0.96f, 0.35f), new Color(0,0,0,0));
            var whl = weatherRow.AddComponent<HorizontalLayoutGroup>();
            whl.spacing = 10; whl.childForceExpandWidth = true; whl.childForceExpandHeight = true;
            UIFactory.Button(weatherRow.transform, "Clear", new Vector2(0, 80), () => SetWeather("clear"));
            UIFactory.Button(weatherRow.transform, "Rain",  new Vector2(0, 80), () => SetWeather("rain"));
            UIFactory.Button(weatherRow.transform, "Snow",  new Vector2(0, 80), () => SetWeather("snow"));
        }

        private void OnEnable() => Refresh();

        private void Refresh()
        {
            if (_layoutText == null) return;
            var inv = ServiceLocator.Get<InventorySystem>();
            var sb = new StringBuilder();
            sb.AppendLine($"Coins: {inv.SoftCurrency}");
            sb.AppendLine($"Items owned: {inv.Owned.Count}");
            sb.AppendLine($"Items in room: {inv.Placements.Count} / 12");
            sb.AppendLine();
            sb.AppendLine("Inventory → Place to add items to the room.");
            sb.AppendLine("Read books to unlock more.");
            _layoutText.text = sb.ToString();
        }

        private void SetWeather(string w)
        {
            if (ServiceLocator.TryGet<Services.Weather.WeatherTimeSystem>(out var ws))
            {
                ws.SetWeather(w);
            }
        }
    }
}
