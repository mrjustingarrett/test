using UnityEngine;
using UnityEngine.UI;

namespace LibraryGame.UI.Common
{
    /// <summary>
    /// Helpers for building uGUI panels in code without prefabs.
    /// Programmer-art-grade visuals — replace at M9 polish.
    /// </summary>
    public static class UIFactory
    {
        public static readonly Color BgDark = new(0.10f, 0.12f, 0.16f, 0.96f);
        public static readonly Color BgPanel = new(0.16f, 0.18f, 0.22f, 1f);
        public static readonly Color BgRow = new(0.22f, 0.24f, 0.28f, 1f);
        public static readonly Color Accent = new(0.92f, 0.66f, 0.36f, 1f);
        public static readonly Color TextLight = new(0.95f, 0.95f, 0.95f, 1f);
        public static readonly Color TextDim = new(0.70f, 0.72f, 0.76f, 1f);

        public static Font UiFont => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public static GameObject Panel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        public static Text Label(Transform parent, string text, int fontSize, TextAnchor anchor = TextAnchor.MiddleLeft, Color? color = null)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var t = go.AddComponent<Text>();
            t.text = text;
            t.fontSize = fontSize;
            t.alignment = anchor;
            t.color = color ?? TextLight;
            t.font = UiFont;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        public static Button Button(Transform parent, string label, Vector2 size, System.Action onClick)
        {
            var go = new GameObject($"Btn_{label}");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.color = Accent;
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
            t.color = new Color(0.10f, 0.10f, 0.14f);
            t.fontSize = 28;
            t.font = UiFont;
            t.fontStyle = FontStyle.Bold;
            return btn;
        }

        public static InputField InputField(Transform parent, string placeholder, Vector2 size)
        {
            var go = new GameObject("Input");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.10f);
            var input = go.AddComponent<InputField>();

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var trt = textGo.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(16, 8); trt.offsetMax = new Vector2(-16, -8);
            var t = textGo.AddComponent<Text>();
            t.font = UiFont;
            t.fontSize = 32;
            t.color = TextLight;
            t.alignment = TextAnchor.MiddleLeft;
            t.supportRichText = false;

            var phGo = new GameObject("Placeholder");
            phGo.transform.SetParent(go.transform, false);
            var prt = phGo.AddComponent<RectTransform>();
            prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
            prt.offsetMin = new Vector2(16, 8); prt.offsetMax = new Vector2(-16, -8);
            var p = phGo.AddComponent<Text>();
            p.font = UiFont;
            p.fontSize = 32;
            p.color = TextDim;
            p.text = placeholder;
            p.fontStyle = FontStyle.Italic;
            p.alignment = TextAnchor.MiddleLeft;

            input.textComponent = t;
            input.placeholder = p;
            return input;
        }

        public static ScrollRect ScrollList(Transform parent, out RectTransform content)
        {
            var go = new GameObject("Scroll");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.15f);
            var sr = go.AddComponent<ScrollRect>();
            go.AddComponent<RectMask2D>();

            var viewportGo = new GameObject("Viewport");
            viewportGo.transform.SetParent(go.transform, false);
            var vrt = viewportGo.AddComponent<RectTransform>();
            vrt.anchorMin = Vector2.zero; vrt.anchorMax = Vector2.one;
            vrt.offsetMin = Vector2.zero; vrt.offsetMax = Vector2.zero;
            var vimg = viewportGo.AddComponent<Image>();
            vimg.color = new Color(1f, 1f, 1f, 0f);
            viewportGo.AddComponent<Mask>().showMaskGraphic = false;

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewportGo.transform, false);
            var crt = contentGo.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0, 1);
            crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(0.5f, 1f);
            crt.anchoredPosition = Vector2.zero;
            crt.sizeDelta = new Vector2(0, 0);
            var vlg = contentGo.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            vlg.padding = new RectOffset(12, 12, 12, 12);
            vlg.spacing = 8;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            var fitter = contentGo.AddComponent<UnityEngine.UI.ContentSizeFitter>();
            fitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

            sr.content = crt;
            sr.viewport = vrt;
            sr.horizontal = false;
            sr.vertical = true;

            content = crt;
            return sr;
        }

        public static GameObject Row(Transform parent, float height, Color? bg = null)
        {
            var go = new GameObject("Row");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, height);
            var img = go.AddComponent<Image>();
            img.color = bg ?? BgRow;
            var le = go.AddComponent<UnityEngine.UI.LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            return go;
        }

        public static GameObject HeaderBar(Transform parent, string title, System.Action onClose)
        {
            var go = Panel(parent, "Header", new Vector2(0, 0.92f), new Vector2(1, 1f), BgDark);

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(go.transform, false);
            var trt = titleGo.AddComponent<RectTransform>();
            trt.anchorMin = new Vector2(0, 0); trt.anchorMax = new Vector2(1, 1);
            trt.offsetMin = new Vector2(40, 0); trt.offsetMax = new Vector2(-200, 0);
            var t = titleGo.AddComponent<Text>();
            t.text = title;
            t.fontSize = 44;
            t.color = TextLight;
            t.alignment = TextAnchor.MiddleLeft;
            t.font = UiFont;
            t.fontStyle = FontStyle.Bold;

            var btn = Button(go.transform, "Close", new Vector2(140, 80), () => onClose?.Invoke());
            var brt = btn.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(1, 0.5f); brt.anchorMax = new Vector2(1, 0.5f);
            brt.pivot = new Vector2(1, 0.5f);
            brt.anchoredPosition = new Vector2(-30, 0);

            return go;
        }
    }
}
