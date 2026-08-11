using System;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PaleoPinesDinoStudio.UI
{
    public static class UiPalette
    {
        public static Color Window = new Color(0.09f, 0.10f, 0.13f, 0.97f);
        public static Color Header = new Color(0.16f, 0.18f, 0.24f, 1f);
        public static Color Panel = new Color(0.13f, 0.14f, 0.18f, 0.98f);
        public static Color PanelDeep = new Color(0.08f, 0.09f, 0.12f, 1f);
        public static Color Btn = new Color(0.22f, 0.26f, 0.34f, 1f);
        public static Color BtnHover = new Color(0.30f, 0.35f, 0.45f, 1f);
        public static Color BtnActive = new Color(0.40f, 0.55f, 0.42f, 1f);
        public static Color Accent = new Color(0.30f, 0.55f, 0.85f, 1f);
        public static Color Track = new Color(0.05f, 0.06f, 0.09f, 1f);
        public static Color Knob = new Color(0.65f, 0.70f, 0.78f, 1f);
        public static Color Field = new Color(0.10f, 0.11f, 0.15f, 1f);
        public static Color Text = new Color(0.92f, 0.93f, 0.96f, 1f);
        public static Color Dim = new Color(0.55f, 0.57f, 0.62f, 1f);
        public static Color Warn = new Color(1f, 0.95f, 0.7f, 1f);
        public static Color Row = new Color(0.16f, 0.17f, 0.22f, 1f);
        public static Color RowSel = new Color(0.30f, 0.40f, 0.55f, 1f);
        public static Color Border = new Color(0.28f, 0.32f, 0.42f, 1f);

        public static TextAlignmentOptions LeftMid = (TextAlignmentOptions)513;   // Left + Middle
        public static TextAlignmentOptions TopLeft = (TextAlignmentOptions)257;   // Left + Top
        public static TextAlignmentOptions Center = (TextAlignmentOptions)514;    // Center + Middle
        public static TextAlignmentOptions CenterTop = (TextAlignmentOptions)258; // Center + Top
        public static TextAlignmentOptions RightMid = (TextAlignmentOptions)516;  // Right + Middle
    }

    public class UiButton
    {
        public Rect r;
        public Func<Vector2> Origin;
        public Func<bool> IsActive;
        public Action OnClick;
        public Image Img;
        public TextMeshProUGUI Label;
        public bool Disabled;
        public UiScroll InScroll;
        public bool IsHovered;

        public Rect EffRect()
        {
            Vector2 o = Origin != null ? Origin() : Vector2.zero;
            return new Rect(o.x + r.x, o.y + r.y, r.width, r.height);
        }

        public Rect GetRect()
        {
            if (InScroll != null)
            {
                return new Rect(InScroll.EffView().x + r.x, InScroll.EffView().y - InScroll.Scroll + r.y, r.width, r.height);
            }
            return EffRect();
        }
    }

    public class UiSlider
    {
        public Rect r;
        public Func<Vector2> Origin;
        public Func<float> Get;
        public Action<float> Set;
        public Image Fill;
        public Image Knob;
        public TextMeshProUGUI ValueLabel;
        public string Format = "{0:0.00}";
        public bool Dragging;

        public float Value { get { return Get != null ? Mathf.Clamp01(Get()) : 0f; } }

        public Rect EffRect()
        {
            Vector2 o = Origin != null ? Origin() : Vector2.zero;
            return new Rect(o.x + r.x, o.y + r.y, r.width, r.height);
        }
    }

    public class UiTextField
    {
        public Rect r;
        public Func<Vector2> Origin;
        public Func<string> Get;
        public Action<string> Set;
        public TextMeshProUGUI Label;
        public Image Bg;
        public bool Focused;
        public int MaxLen = 48;
        public string Filter; // if set, only these chars are accepted (e.g. "0123456789abcdefABCDEF")

        public Rect EffRect()
        {
            Vector2 o = Origin != null ? Origin() : Vector2.zero;
            return new Rect(o.x + r.x, o.y + r.y, r.width, r.height);
        }
    }

    public class UiScroll
    {
        public Rect view;
        public Func<Vector2> Origin;
        public RectTransform Content;
        public float ContentHeight;
        public float Scroll;
        public float MaxScroll { get { return Mathf.Max(0f, ContentHeight - view.height); } }

        public Rect EffView()
        {
            Vector2 o = Origin != null ? Origin() : Vector2.zero;
            return new Rect(o.x + view.x, o.y + view.y, view.width, view.height);
        }
    }

    public class UiPaint
    {
        public Rect r;
        public Func<Vector2> Origin;
        public Action<Vector2> OnDown;
        public Action<Vector2> OnDrag;
        public Action<Vector2> OnUp;
        public bool Active;
        public RawImage Raw;

        public Rect EffRect()
        {
            Vector2 o = Origin != null ? Origin() : Vector2.zero;
            return new Rect(o.x + r.x, o.y + r.y, r.width, r.height);
        }
    }

    public class UiDrag
    {
        public Rect r;
        public Func<Vector2> Origin;
        public Action<Vector2> OnDragDelta;
        public Func<Vector2, bool> CanStart;
        public bool Active;
        public Vector2 Last;

        public Rect EffRect()
        {
            Vector2 o = Origin != null ? Origin() : Vector2.zero;
            return new Rect(o.x + r.x, o.y + r.y, r.width, r.height);
        }
    }

    /// <summary>
    /// Builds UGUI (game UI) elements on a runtime overlay canvas.
    /// All coordinates are in "design units" (1600x900 reference) and y grows DOWN.
    /// Every interactive widget registers itself with GameUI's manual input loop.
    /// </summary>
    public static class UiFactory
    {
        public const float DesignW = 1600f;
        public const float DesignH = 900f;
        public static float Scale = 1f;
        public static Func<Vector2> WidgetOrigin = () => Vector2.zero;
        public static int CanvasGeneration;

        private static GameObject _canvasGO;
        private static RectTransform _rootRT;
        private static bool _fontTried;
        private static TMP_FontAsset _font;
        private static Material _fontMat;

        public static RectTransform RootRT { get { return _rootRT; } }
        public static GameObject CanvasGO { get { return _canvasGO; } }

        public static void EnsureCanvas()
        {
            if (_canvasGO != null) return;
            _fontTried = false;

            _canvasGO = new GameObject("DinoStudio_Canvas");
            var canvas = _canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            _canvasGO.AddComponent<GraphicRaycaster>();

            // Survive scene loads; otherwise the window dies with the world and GameUI
            // keeps referencing the destroyed RectTransform.
            UnityEngine.Object.DontDestroyOnLoad(_canvasGO);
            CanvasGeneration++;

            // Full-screen invisible blocker: stops the game's EventSystem from routing
            // clicks to the game UI underneath the editor. Our input is manual.
            var blocker = Panel(_canvasGO.transform, "Blocker", 0f, 0f, Screen.width, Screen.height, new Color(0f, 0f, 0f, 0f));
            blocker.raycastTarget = true;

            Scale = Mathf.Min((float)Screen.width / DesignW, (float)Screen.height / DesignH);

            var rootGo = new GameObject("DinoStudio_Root");
            _rootRT = rootGo.AddComponent<RectTransform>();
            _rootRT.SetParent(_canvasGO.transform, false);
            _rootRT.anchorMin = _rootRT.anchorMax = new Vector2(0f, 1f);
            _rootRT.pivot = new Vector2(0f, 1f);
            _rootRT.anchoredPosition = Vector2.zero;
            _rootRT.sizeDelta = new Vector2(DesignW / Scale, DesignH / Scale);
            _rootRT.localScale = new Vector3(Scale, Scale, 1f);

            EnsureFont();
        }

        public static Vector2 DesignPoint(Vector2 screenPos)
        {
            return new Vector2(screenPos.x / Scale, (Screen.height - screenPos.y) / Scale);
        }

        public static bool InRect(Rect r, Vector2 p)
        {
            return p.x >= r.x && p.x <= r.x + r.width && p.y >= r.y && p.y <= r.y + r.height;
        }

        public static void ClearChildren(Transform parent)
        {
            if (parent == null) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                var c = parent.GetChild(i);
                if (c != null) UnityEngine.Object.Destroy(c.gameObject);
            }
        }

        public static RectTransform Rect(Transform parent, string name, float x, float y, float w, float h)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        public static Image Panel(Transform parent, string name, float x, float y, float w, float h, Color color)
        {
            var rt = Rect(parent, name, x, y, w, h);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        public static RawImage Raw(Transform parent, string name, float x, float y, float w, float h, Texture tex)
        {
            var rt = Rect(parent, name, x, y, w, h);
            var ri = rt.gameObject.AddComponent<RawImage>();
            ri.texture = tex;
            ri.color = Color.white;
            ri.raycastTarget = false;
            return ri;
        }

        private static void EnsureFont()
        {
            if (_font != null || _fontTried) return;
            _fontTried = true;
            try
            {
                var all = UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>(true);
                for (int i = 0; i < all.Length; i++)
                {
                    if (all[i] == null) continue;
                    var f = all[i].font;
                    if (f != null)
                    {
                        _font = f;
                        _fontMat = all[i].fontSharedMaterial;
                        return;
                    }
                }
                var assets = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
                for (int i = 0; i < assets.Length; i++)
                {
                    if (assets[i] != null) { _font = assets[i]; return; }
                }
            }
            catch (System.Exception e)
            {
                MelonLoader.MelonLogger.Error("UiFactory.EnsureFont failed: " + e);
            }
        }

        public static TextMeshProUGUI Label(Transform parent, string name, string text, float x, float y, float w, float h,
            float size, Color color, TextAlignmentOptions align = default)
        {
            EnsureFont();
            var rt = Rect(parent, name, x, y, w, h);
            var txt = rt.gameObject.AddComponent<TextMeshProUGUI>();
            txt.raycastTarget = false;
            if (_font != null)
            {
                txt.font = _font;
                if (_fontMat != null) txt.fontSharedMaterial = _fontMat;
            }
            txt.fontSize = size;
            txt.color = color;
            txt.alignment = align == default ? UiPalette.LeftMid : align;
            txt.text = text;
            return txt;
        }

        public static UiButton Button(Transform parent, string name, string label, float x, float y, float w, float h, Action onClick, Func<Vector2> origin = null)
        {
            var img = Panel(parent, name, x, y, w, h, UiPalette.Btn);
            img.raycastTarget = true;
            var txt = Label(img.transform, name + "_Label", label, 6f, 0f, w - 12f, h, 20f, Color.white, UiPalette.Center);
            var b = new UiButton { r = new Rect(x, y, w, h), OnClick = onClick, Img = img, Label = txt, Origin = origin ?? WidgetOrigin };
            GameUI.Buttons.Add(b);
            return b;
        }

        public static UiButton Toggle(Transform parent, string name, string label, float x, float y, float w, float h,
            Func<bool> isActive, Action onClick, Func<Vector2> origin = null)
        {
            var img = Panel(parent, name, x, y, w, h, UiPalette.Btn);
            img.raycastTarget = true;
            var txt = Label(img.transform, name + "_Label", label, 6f, 0f, w - 12f, h, 20f, Color.white, UiPalette.Center);
            var b = new UiButton { r = new Rect(x, y, w, h), OnClick = onClick, Img = img, Label = txt, IsActive = isActive, Origin = origin ?? WidgetOrigin };
            GameUI.Buttons.Add(b);
            return b;
        }

        public static UiSlider Slider(Transform parent, string name, float x, float y, float w, float h,
            Func<float> get, Action<float> set, TextMeshProUGUI valueLabel = null, string format = "{0:0.00}", Func<Vector2> origin = null)
        {
            var track = Panel(parent, name + "_Track", x, y, w, h, UiPalette.Track);
            track.raycastTarget = true;
            var fill = Panel(track.transform, name + "_Fill", 0f, 0f, w, h, UiPalette.Accent);
            fill.raycastTarget = false;
            var knob = Panel(track.transform, name + "_Knob", 0f, -4f, 14f, h + 8f, UiPalette.Knob);
            knob.raycastTarget = false;
            var s = new UiSlider { r = new Rect(x, y, w, h), Get = get, Set = set, Fill = fill, Knob = knob, ValueLabel = valueLabel, Format = format, Origin = origin ?? WidgetOrigin };
            GameUI.Sliders.Add(s);
            return s;
        }

        public static UiTextField TextField(Transform parent, string name, float x, float y, float w, float h,
            Func<string> get, Action<string> set, string filter = null, Func<Vector2> origin = null)
        {
            var bg = Panel(parent, name, x, y, w, h, UiPalette.Field);
            bg.raycastTarget = true;
            var txt = Label(bg.transform, name + "_Label", get != null ? get() : "", 8f, 0f, w - 16f, h, 22f, Color.white, UiPalette.LeftMid);
            var f = new UiTextField { r = new Rect(x, y, w, h), Get = get, Set = set, Label = txt, Bg = bg, Filter = filter, Origin = origin ?? WidgetOrigin };
            GameUI.TextFields.Add(f);
            return f;
        }

        public static UiScroll Scroll(Transform parent, string name, float x, float y, float w, float h, Func<Vector2> origin = null)
        {
            var viewport = Panel(parent, name + "_Viewport", x, y, w, h, UiPalette.PanelDeep);
            viewport.raycastTarget = true;
            viewport.gameObject.AddComponent<RectMask2D>();
            var content = Rect(viewport.transform, name + "_Content", 0f, 0f, w, 0f);
            var s = new UiScroll { view = new Rect(x, y, w, h), Content = content, Origin = origin ?? WidgetOrigin };
            GameUI.Scrolls.Add(s);
            return s;
        }

        /// <summary>Creates a row inside a scroll's content. Returns the rect transform for a label/row.</summary>
        public static RectTransform ScrollItem(UiScroll scroll, string name, float itemY, float w, float h)
        {
            var rt = Rect(scroll.Content, name, 0f, itemY, w, h);
            scroll.ContentHeight = Mathf.Max(scroll.ContentHeight, itemY + h);
            return rt;
        }

        public static UiButton ScrollButton(UiScroll scroll, string name, string label, float itemY, float w, float h, Action onClick)
        {
            var b = Button(scroll.Content, name, label, 2f, itemY, w - 4f, h, onClick);
            b.InScroll = scroll;
            scroll.ContentHeight = Mathf.Max(scroll.ContentHeight, itemY + h);
            return b;
        }

        public static UiPaint PaintRaw(Transform parent, string name, float x, float y, float w, float h, Texture tex, Func<Vector2> origin = null)
        {
            var rt = Rect(parent, name, x, y, w, h);
            var ri = rt.gameObject.AddComponent<RawImage>();
            ri.texture = tex;
            ri.color = Color.white;
            ri.raycastTarget = true;
            var p = new UiPaint { r = new Rect(x, y, w, h), Raw = ri, Origin = origin ?? WidgetOrigin };
            GameUI.Paints.Add(p);
            return p;
        }

        public static UiPaint PaintArea(Transform parent, string name, float x, float y, float w, float h, Func<Vector2> origin = null)
        {
            var img = Panel(parent, name, x, y, w, h, new Color(1f, 1f, 1f, 0.001f));
            img.raycastTarget = true;
            var p = new UiPaint { r = new Rect(x, y, w, h), Origin = origin ?? WidgetOrigin };
            GameUI.Paints.Add(p);
            return p;
        }
    }
}
