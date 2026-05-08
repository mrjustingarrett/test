using System.Collections.Generic;
using LibraryGame.Gameplay;
using LibraryGame.Gameplay.Player;
using UnityEngine;
using UnityEngine.UI;

namespace LibraryGame.UI.Common
{
    /// <summary>
    /// On-screen mobile controls. Left half of screen = virtual move-stick,
    /// right half = drag-to-look. Reads input from both Input System
    /// (Touchscreen) and legacy Input so it works whichever input handling
    /// mode the project is set to. Plus a small uGUI button column for
    /// camera toggle and the four HUD menus.
    /// </summary>
    public sealed class TouchHud : MonoBehaviour
    {
        public PlayerController player;
        public CameraRig cameraRig;

        public System.Action OnLibraryPressed;
        public System.Action OnInventoryPressed;
        public System.Action OnShopPressed;
        public System.Action OnBuildModePressed;

        private const float StickRadiusPx = 110f;
        private const float HudReservedWidthFactor = 0.32f;
        private const float HudReservedHeightPx = 460f;

        // Move-stick state (left side)
        private bool _moveActive;
        private int _moveFingerId = -1;
        private Vector2 _moveOrigin;
        private Vector2 _moveCurrent;

        // Look state (right side)
        private bool _lookActive;
        private int _lookFingerId = -1;
        private Vector2 _lookLast;

        private Rect _hudReservedRect;
        private Canvas _canvas;

        private static readonly List<TouchSnap> _scratch = new();

        private void Start()
        {
            BuildCanvas();
            UpdateHudReservedRect();
        }

        private void OnRectTransformDimensionsChange() => UpdateHudReservedRect();

        private void UpdateHudReservedRect()
        {
            float w = Mathf.Max(260f, Screen.width * HudReservedWidthFactor);
            float h = HudReservedHeightPx;
            _hudReservedRect = new Rect(Screen.width - w, Screen.height - h, w, h);
        }

        private void Update()
        {
            ReadTouches(_scratch);

            Vector2 moveInput = Vector2.zero;
            Vector2 lookDelta = Vector2.zero;
            bool moveStillActive = false;
            bool lookStillActive = false;

            foreach (var t in _scratch)
            {
                if (_hudReservedRect.Contains(t.position)) continue; // HUD button area

                bool isLeft = t.position.x < Screen.width * 0.5f;

                if (isLeft)
                {
                    if ((t.began || !_moveActive) && (_moveFingerId == -1 || t.fingerId == _moveFingerId))
                    {
                        if (!_moveActive || t.began)
                        {
                            _moveActive = true;
                            _moveFingerId = t.fingerId;
                            _moveOrigin = t.position;
                        }
                        _moveCurrent = t.position;
                        moveStillActive = true;
                        continue;
                    }
                    if (_moveActive && t.fingerId == _moveFingerId)
                    {
                        _moveCurrent = t.position;
                        moveStillActive = true;
                    }
                }
                else
                {
                    if ((t.began || !_lookActive) && (_lookFingerId == -1 || t.fingerId == _lookFingerId))
                    {
                        if (!_lookActive || t.began)
                        {
                            _lookActive = true;
                            _lookFingerId = t.fingerId;
                            _lookLast = t.position;
                        }
                        else
                        {
                            lookDelta += t.position - _lookLast;
                            _lookLast = t.position;
                        }
                        lookStillActive = true;
                        continue;
                    }
                    if (_lookActive && t.fingerId == _lookFingerId)
                    {
                        lookDelta += t.position - _lookLast;
                        _lookLast = t.position;
                        lookStillActive = true;
                    }
                }
            }

            if (!moveStillActive) { _moveActive = false; _moveFingerId = -1; }
            if (!lookStillActive) { _lookActive = false; _lookFingerId = -1; }

            if (_moveActive)
            {
                var v = (_moveCurrent - _moveOrigin) / StickRadiusPx;
                if (v.magnitude > 1f) v = v.normalized;
                moveInput = v;
            }

            if (player != null)
            {
                player.MoveInput = moveInput;
                player.LookInput = lookDelta;
            }
        }

        private void ReadTouches(List<TouchSnap> list)
        {
            list.Clear();
#if ENABLE_INPUT_SYSTEM
            var ts = UnityEngine.InputSystem.Touchscreen.current;
            if (ts != null)
            {
                foreach (var t in ts.touches)
                {
                    if (!t.press.isPressed) continue;
                    list.Add(new TouchSnap
                    {
                        fingerId = t.touchId.ReadValue(),
                        position = t.position.ReadValue(),
                        began = t.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began
                    });
                }
                if (list.Count > 0) return;
            }
#endif
            for (int i = 0; i < UnityEngine.Input.touchCount; i++)
            {
                var t = UnityEngine.Input.GetTouch(i);
                list.Add(new TouchSnap
                {
                    fingerId = t.fingerId,
                    position = t.position,
                    began = t.phase == UnityEngine.TouchPhase.Began
                });
            }

#if UNITY_EDITOR
            if (list.Count == 0 && UnityEngine.Input.GetMouseButton(0))
            {
                list.Add(new TouchSnap
                {
                    fingerId = 0,
                    position = (Vector2)UnityEngine.Input.mousePosition,
                    began = UnityEngine.Input.GetMouseButtonDown(0)
                });
            }
#endif
        }

        private struct TouchSnap
        {
            public int fingerId;
            public Vector2 position;
            public bool began;
        }

        // ---------- Canvas / Buttons ----------

        private void BuildCanvas()
        {
            var canvasGo = new GameObject("HudCanvas");
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            EnsureEventSystem();

            float yStart = -40f;
            const float gap = 12f;
            const float btnW = 220f, btnH = 64f;
            yStart = MakeTopRightButton(canvasGo.transform, "Library",       () => OnLibraryPressed?.Invoke(),   yStart, btnW, btnH) - gap;
            yStart = MakeTopRightButton(canvasGo.transform, "Inventory",     () => OnInventoryPressed?.Invoke(), yStart, btnW, btnH) - gap;
            yStart = MakeTopRightButton(canvasGo.transform, "Shop",          () => OnShopPressed?.Invoke(),      yStart, btnW, btnH) - gap;
            yStart = MakeTopRightButton(canvasGo.transform, "Edit Room",     () => OnBuildModePressed?.Invoke(), yStart, btnW, btnH) - gap;
            MakeTopRightButton(canvasGo.transform, "Toggle Camera",          () => cameraRig?.Toggle(),         yStart, btnW, btnH);
        }

        private void EnsureEventSystem()
        {
            if (UnityEngine.EventSystems.EventSystem.current != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<UnityEngine.EventSystems.EventSystem>();
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
        }

        private float MakeTopRightButton(Transform parent, string label, System.Action onClick, float yOffset, float w, float h)
        {
            var go = new GameObject($"Btn_{label}");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-20f, yOffset);
            rt.sizeDelta = new Vector2(w, h);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.10f, 0.12f, 0.16f, 0.85f);
            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var trt = textGo.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
            var t = textGo.AddComponent<Text>();
            t.text = label;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            t.fontSize = 22;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            return yOffset - h;
        }
    }
}
