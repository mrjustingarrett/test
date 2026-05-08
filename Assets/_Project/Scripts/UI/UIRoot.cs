using LibraryGame.UI.Common;
using LibraryGame.UI.Panels;
using UnityEngine;
using UnityEngine.UI;

namespace LibraryGame.UI
{
    /// <summary>
    /// Persistent root canvas hosting the four menu panels (Library, Inventory,
    /// Shop, Settings/Build). Each panel is created lazily on first open.
    /// </summary>
    public sealed class UIRoot : MonoBehaviour
    {
        public Canvas Canvas { get; private set; }
        public RectTransform PanelHost { get; private set; }

        private LibraryPanel _libraryPanel;
        private InventoryPanel _inventoryPanel;
        private ShopPanel _shopPanel;
        private BuildModePanel _buildPanel;
        private GameObject _activePanel;

        public void Initialize(TouchHud hud)
        {
            BuildCanvas();

            hud.OnLibraryPressed   = () => Toggle(GetLibrary().gameObject);
            hud.OnInventoryPressed = () => Toggle(GetInventory().gameObject);
            hud.OnShopPressed      = () => Toggle(GetShop().gameObject);
            hud.OnBuildModePressed = () => Toggle(GetBuild().gameObject);
        }

        private void BuildCanvas()
        {
            var go = new GameObject("MenuCanvas");
            go.transform.SetParent(transform, false);
            Canvas = go.AddComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Canvas.sortingOrder = 50; // below TouchHud (100) so HUD buttons stay visible? actually we want menus above
            Canvas.sortingOrder = 200; // above HUD when open
            var s = go.AddComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(1080, 1920);
            s.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();

            // Full-screen panel host (the per-panel root will fill this).
            var hostGo = new GameObject("PanelHost");
            hostGo.transform.SetParent(go.transform, false);
            var rt = hostGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            PanelHost = rt;
        }

        private LibraryPanel GetLibrary() => _libraryPanel ??= LibraryPanel.Create(PanelHost);
        private InventoryPanel GetInventory() => _inventoryPanel ??= InventoryPanel.Create(PanelHost);
        private ShopPanel GetShop() => _shopPanel ??= ShopPanel.Create(PanelHost);
        private BuildModePanel GetBuild() => _buildPanel ??= BuildModePanel.Create(PanelHost);

        private void Toggle(GameObject panel)
        {
            if (_activePanel == panel)
            {
                panel.SetActive(false);
                _activePanel = null;
                return;
            }
            if (_activePanel != null) _activePanel.SetActive(false);
            panel.SetActive(true);
            _activePanel = panel;
        }
    }
}
