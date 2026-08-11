using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Il2CppTMPro;
using PaleoPinesDinoStudio.UI.Tabs;

namespace PaleoPinesDinoStudio.UI
{
    /// <summary>
    /// The in-game UGUI editor overlay. Builds the window once and drives all input manually
    /// (no EventSystem routing), which avoids stripped native-delegate marshalling in IL2CPP.
    /// </summary>
    public static class GameUI
    {
        public static readonly List<UiButton> Buttons = new List<UiButton>();
        public static readonly List<UiSlider> Sliders = new List<UiSlider>();
        public static readonly List<UiTextField> TextFields = new List<UiTextField>();
        public static readonly List<UiScroll> Scrolls = new List<UiScroll>();
        public static readonly List<UiPaint> Paints = new List<UiPaint>();
        public static readonly List<UiDrag> Drags = new List<UiDrag>();

        private const float WinW = 1540f;
        private const float WinH = 850f;
        private const float HeaderH = 46f;
        private const float TabBarH = 42f;
        private const float ContentTop = HeaderH + TabBarH;
        private const float FooterH = 26f;
        public const float ContentH = WinH - ContentTop - FooterH;
        public const float ContentW = WinW - 24f;

        private static bool _built;
        private static int _canvasGen = -1;
        private static RectTransform _windowRT;
        private static RectTransform _contentRoot;
        private static TextMeshProUGUI _statusLabel;
        private static GameObject _loadingPanel;
        private static TextMeshProUGUI _loadingMsg;
        private static Vector2 _winPos = new Vector2(20f, 20f);
        private static int _activeTab = -1;

        private static readonly string[] TabNames = { "Dino", "Catalog", "Color", "Preview", "Apply" };

        public static RectTransform ContentRoot { get { return _contentRoot; } }

        public static Vector2 WindowOrigin() { return _winPos; }

        public static Vector2 ContentOrigin()
        {
            return new Vector2(_winPos.x + 12f, _winPos.y + ContentTop);
        }

        public static void EnsureBuilt()
        {
            if (_built && (UiFactory.CanvasGO == null || UiFactory.CanvasGeneration != _canvasGen || _windowRT == null))
            {
                _built = false;
            }
            if (_built) return;
            UiFactory.EnsureCanvas();
            BuildWindow();
            _canvasGen = UiFactory.CanvasGeneration;
            _built = true;
        }

        private static void BuildWindow()
        {
            _chrome.Clear();
            var winImg = UiFactory.Panel(UiFactory.RootRT, "Window", _winPos.x, _winPos.y, WinW, WinH, UiPalette.Window);
            _windowRT = winImg.rectTransform;

            // Title
            UiFactory.Label(_windowRT, "Title", "Dino Studio", 10f, 8f, 220f, 30f, 26f, Color.white, UiPalette.LeftMid);

            // Tab buttons
            for (int i = 0; i < TabNames.Length; i++)
            {
                int idx = i;
                var tb = UiFactory.Toggle(_windowRT, "Tab_" + idx, TabNames[i], 200f + i * 110f, 6f, 100f, 34f,
                    () => _activeTab == idx, () => SwitchTab(idx), () => WindowOrigin());
                _chrome.Add(tb);
            }

            // Close button
            var close = UiFactory.Button(_windowRT, "CloseBtn", "X", WinW - 44f, 6f, 36f, 34f, () => Main.State.ToggleEditor(), () => WindowOrigin());
            _chrome.Add(close);

            // Header drag
            Drags.Add(dragRef);

            // Content root
            var contentPanel = UiFactory.Panel(_windowRT, "Content", 12f, ContentTop, ContentW, ContentH, new Color(0f, 0f, 0f, 0f));
            _contentRoot = contentPanel.rectTransform;

            // Footer / status
            _statusLabel = UiFactory.Label(_windowRT, "Status", "", 12f, WinH - FooterH + 4f, ContentW, 20f, 16f, UiPalette.Warn, UiPalette.LeftMid);

            // Loading overlay
            _loadingPanel = UiFactory.Panel(_windowRT, "Loading", 12f, ContentTop, ContentW, ContentH, UiPalette.Panel).gameObject;
            _loadingMsg = UiFactory.Label(_loadingPanel.transform, "LoadingMsg", "", 60f, 200f, ContentW - 120f, 60f, 24f, UiPalette.Text, UiPalette.Center);
            _retryBtn = UiFactory.Button(_loadingPanel.transform, "Retry", "Retry", 200f, 300f, 160f, 40f, () => Core.GameCatalog.Refresh(), () => WindowOrigin());

            SwitchTab(0);
        }

        private static void MoveWindow(Vector2 delta)
        {
            if (_windowRT == null) return;
            try
            {
                _winPos.x = Mathf.Clamp(_winPos.x + delta.x, 0f, UiFactory.DesignW - WinW);
                _winPos.y = Mathf.Clamp(_winPos.y + delta.y, 0f, UiFactory.DesignH - WinH);
                _windowRT.anchoredPosition = new Vector2(_winPos.x, -_winPos.y);
            }
            catch (System.Exception e)
            {
                MelonLoader.MelonLogger.Error("MoveWindow failed: " + e);
            }
        }

        private static void ClearInteractables()
        {
            Buttons.Clear();
            Sliders.Clear();
            TextFields.Clear();
            Scrolls.Clear();
            Paints.Clear();
            Drags.Clear();
            Drags.Add(dragRef);
            for (int i = 0; i < _chrome.Count; i++) Buttons.Add(_chrome[i]);
            if (_retryBtn != null) Buttons.Add(_retryBtn);
        }

        private static UiButton _retryBtn;
        private static readonly List<UiButton> _chrome = new List<UiButton>();

        private static readonly UiDrag dragRef = new UiDrag
        {
            r = new Rect(0f, 0f, WinW, HeaderH),
            Origin = () => WindowOrigin(),
            CanStart = p => !(p.x >= 200f && p.x <= 200f + TabNames.Length * 110f + 10f) || p.x >= WinW - 50f,
            OnDragDelta = d => MoveWindow(d)
        };

        public static void SwitchTab(int index)
        {
            if (index < 0 || index >= TabNames.Length) return;
            _activeTab = index;

            ClearInteractables();
            UiFactory.ClearChildren(_contentRoot);

            UiFactory.WidgetOrigin = () => ContentOrigin();
            try
            {
                switch (index)
                {
                    case 0: DinoTab.Build(Main.State, _contentRoot); break;
                    case 1: CatalogTab.Build(Main.State, _contentRoot); break;
                    case 2: ColorTab.Build(Main.State, _contentRoot); break;
                    case 3: ViewportTab.Build(Main.State, _contentRoot); break;
                    case 4: ApplyTab.Build(Main.State, _contentRoot); break;
                }
            }
            finally
            {
                UiFactory.WidgetOrigin = () => Vector2.zero;
            }
        }

        public static void RefreshTab()
        {
            SwitchTab(_activeTab);
        }

        /// <summary>Removes all buttons living inside a scroll's content (used before rebuilding a list).</summary>
        public static void RemoveScrollButtons(UiScroll sc)
        {
            if (sc == null) return;
            for (int i = Buttons.Count - 1; i >= 0; i--)
            {
                if (Buttons[i].InScroll == sc) Buttons.RemoveAt(i);
            }
        }

        public static void Tick(StudioState state)
        {
            if (state == null || !state.EditorOpen) return;
            EnsureBuilt();

            Vector2 mp = UiFactory.DesignPoint(Input.mousePosition);
            bool mdL = Input.GetMouseButtonDown(0);
            bool mhL = Input.GetMouseButton(0);
            bool muL = Input.GetMouseButtonUp(0);
            bool mdR = Input.GetMouseButtonDown(1);
            bool mhR = Input.GetMouseButton(1);
            bool muR = Input.GetMouseButtonUp(1);

            // ---- Paint canvases (pattern + 3D viewport) ----
            for (int i = 0; i < Paints.Count; i++)
            {
                var p = Paints[i];
                if (p.Active)
                {
                    if (muL || muR) { if (p.OnUp != null) p.OnUp(mp); p.Active = false; }
                    else if (mhL || mhR) { if (p.OnDrag != null) p.OnDrag(mp); }
                }
                else if ((mdL || mdR) && UiFactory.InRect(p.EffRect(), mp))
                {
                    p.Active = true;
                    if (p.OnDown != null) p.OnDown(mp);
                }
            }

            // ---- Window header drag ----
            for (int i = 0; i < Drags.Count; i++)
            {
                var d = Drags[i];
                if (d.Active)
                {
                    if (mhL) { d.OnDragDelta(mp - d.Last); d.Last = mp; }
                    else if (muL) d.Active = false;
                }
                else if (mdL && UiFactory.InRect(d.EffRect(), mp) && (d.CanStart == null || d.CanStart(mp)))
                {
                    d.Active = true;
                    d.Last = mp;
                    d.OnDragDelta(Vector2.zero);
                }
            }

            // ---- Sliders ----
            for (int i = 0; i < Sliders.Count; i++)
            {
                var s = Sliders[i];
                if (s.Dragging)
                {
                    if (mhL) UpdateSlider(s, mp);
                    else s.Dragging = false;
                }
                else if (mdL && UiFactory.InRect(s.EffRect(), mp))
                {
                    s.Dragging = true;
                    UpdateSlider(s, mp);
                }
            }

            // ---- Buttons ----
            if (mdL)
            {
                for (int i = 0; i < Buttons.Count; i++)
                {
                    var b = Buttons[i];
                    if (b.Disabled) continue;
                    Rect r = b.GetRect();
                    if (UiFactory.InRect(r, mp))
                    {
                        if (b.OnClick != null) b.OnClick();
                        break;
                    }
                }
            }

            // ---- Text fields: focus ----
            for (int i = 0; i < TextFields.Count; i++)
            {
                var tf = TextFields[i];
                if (mdL) tf.Focused = UiFactory.InRect(tf.EffRect(), mp);
            }

            // ---- Text fields: keyboard input ----
            for (int i = 0; i < TextFields.Count; i++)
            {
                var tf = TextFields[i];
                if (!tf.Focused) continue;
                string str = Input.inputString;
                if (str.Length == 0) continue;
                string cur = tf.Get != null ? tf.Get() : "";
                string result = cur;
                foreach (char c in str)
                {
                    if (c == '\b' || c == '\n' || c == '\r')
                    {
                        if (c == '\b' && result.Length > 0) result = result.Substring(0, result.Length - 1);
                        continue;
                    }
                    if (c < 32) continue;
                    if (result.Length >= tf.MaxLen) continue;
                    if (!string.IsNullOrEmpty(tf.Filter) && tf.Filter.IndexOf(c) < 0) continue;
                    result += c;
                }
                if (result != cur && tf.Set != null)
                {
                    tf.Set(result);
                    if (tf.Label != null) tf.Label.text = result;
                }
            }

            // ---- Scroll wheel ----
            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.001f)
            {
                for (int i = 0; i < Scrolls.Count; i++)
                {
                    var sc = Scrolls[i];
                    if (UiFactory.InRect(sc.EffView(), mp))
                    {
                        sc.Scroll = Mathf.Clamp(sc.Scroll - wheel * 48f, 0f, sc.MaxScroll);
                    }
                }
            }

            RefreshVisuals(mp, mdL);

            // ---- Loading overlay ----
            if (_loadingPanel != null)
            {
                _loadingPanel.SetActive(!Core.GameCatalog.Loaded);
                if (!Core.GameCatalog.Loaded)
                {
                    _loadingMsg.text = Core.GameCatalog.LoadFailed
                        ? "Could not load game data. Are you in a world?"
                        : "Loading game data... (ensure you are in a world, not the main menu)";
                }
            }

            UpdateStatus(state);
            TickActiveTab(state);
        }

        private static void UpdateSlider(UiSlider s, Vector2 mp)
        {
            Rect er = s.EffRect();
            float v = Mathf.Clamp01((mp.x - er.x) / er.width);
            if (s.Set != null) s.Set(v);
        }

        private static void RefreshVisuals(Vector2 mp, bool mouseDown)
        {
            for (int i = 0; i < Buttons.Count; i++)
            {
                var b = Buttons[i];
                if (b.Img == null) continue;
                bool hovered = UiFactory.InRect(b.GetRect(), mp);
                b.IsHovered = hovered;
                if (b.Disabled)
                {
                    b.Img.color = UiPalette.Dim;
                    continue;
                }
                if (b.IsActive != null && b.IsActive())
                {
                    b.Img.color = UiPalette.BtnActive;
                }
                else if (hovered)
                {
                    b.Img.color = UiPalette.BtnHover;
                }
                else
                {
                    b.Img.color = UiPalette.Btn;
                }
            }

            for (int i = 0; i < Sliders.Count; i++)
            {
                var s = Sliders[i];
                if (s.Fill == null) continue;
                float v = s.Value;
                if (s.Fill != null) s.Fill.rectTransform.sizeDelta = new Vector2(Mathf.Max(2f, v * s.r.width), s.r.height);
                if (s.Knob != null) s.Knob.rectTransform.anchoredPosition = new Vector2(Mathf.Clamp(v * s.r.width - 7f, 0f, s.r.width - 14f), 4f);
                if (s.ValueLabel != null && s.Get != null)
                {
                    s.ValueLabel.text = string.Format(s.Format ?? "{0:0.00}", s.Get());
                }
            }

            for (int i = 0; i < TextFields.Count; i++)
            {
                var tf = TextFields[i];
                if (tf.Label != null && tf.Get != null && !tf.Focused) tf.Label.text = tf.Get() ?? "";
                if (tf.Bg != null) tf.Bg.color = tf.Focused ? UiPalette.BtnActive : UiPalette.Field;
            }

            for (int i = 0; i < Scrolls.Count; i++)
            {
                var sc = Scrolls[i];
                if (sc.Content == null) continue;
                sc.Scroll = Mathf.Clamp(sc.Scroll, 0f, sc.MaxScroll);
                sc.Content.anchoredPosition = new Vector2(0f, sc.Scroll);
                sc.Content.sizeDelta = new Vector2(sc.view.width, sc.ContentHeight);
            }
        }

        private static void UpdateStatus(StudioState state)
        {
            if (_statusLabel == null) return;
            if (string.IsNullOrEmpty(state.StatusMessage) || Time.unscaledTime - state.StatusMessageTime > 4f)
            {
                if (_statusLabel.text.Length > 0) _statusLabel.text = "";
                return;
            }
            if (_statusLabel.text != state.StatusMessage) _statusLabel.text = state.StatusMessage;
        }

        private static void TickActiveTab(StudioState state)
        {
            switch (_activeTab)
            {
                case 0: DinoTab.Tick(state); break;
                case 1: CatalogTab.Tick(state); break;
                case 2: ColorTab.Tick(state); break;
                case 3: ViewportTab.Tick(state); break;
                case 4: ApplyTab.Tick(state); break;
            }
        }
    }
}
