using System;
using UnityEngine;
using UnityEngine.UI;

namespace PaleoPinesDinoStudio.UI.Tabs
{
    public static class ColorTab
    {
        private class Entry
        {
            public Func<Core.WorkingAssets, Color> Get;
            public Action<Core.WorkingAssets, Color> Set;
        }

        private static readonly Entry[] Entries =
        {
            new Entry { Get = w => w.BaseColor,     Set = (w, c) => w.BaseColor = c },
            new Entry { Get = w => w.PatternColor1, Set = (w, c) => w.PatternColor1 = c },
            new Entry { Get = w => w.PatternColor2, Set = (w, c) => w.PatternColor2 = c },
            new Entry { Get = w => w.PatternColor3, Set = (w, c) => w.PatternColor3 = c },
            new Entry { Get = w => w.PatternColor4, Set = (w, c) => w.PatternColor4 = c },
            new Entry { Get = w => w.JournalColor,  Set = (w, c) => w.JournalColor = c },
            new Entry { Get = w => w.EyeColor,      Set = (w, c) => w.EyeColor = c },
        };

        private static RawImage[] _swatches;
        private static bool _builtWithContent;
        private static bool _hsvMode;
        private static Texture2D _whiteTex;
        

        private static Texture2D WhiteTex()
        {
            if (_whiteTex == null)
            {
                _whiteTex = new Texture2D(2, 2);
                _whiteTex.name = "DinoStudio_White";
                _whiteTex.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
                _whiteTex.Apply();
            }
            return _whiteTex;
        }

        public static void Build(StudioState state, RectTransform parent)
        {
            _swatches = new RawImage[Entries.Length];

            UiFactory.Label(parent, "ColorIntro",
                "Colour Studio - drag sliders or type hex to change the dino colours.\n" +
                "Base Colour paints the body; Pattern Colours 1-4 tint the markings (which keep the setup's own pattern); " +
                "Eye Colour tints the eyes.",
                0f, 0f, 1300f, 46f, 19f, UiPalette.Text, UiPalette.LeftMid);

            UiFactory.Toggle(parent, "HsvToggle", _hsvMode ? "Basic (RGB)" : "Advanced (HSV)", 1180f, 0f, 240f, 30f,
                () => _hsvMode,
                () => { _hsvMode = !_hsvMode; GameUI.RefreshTab(); });

            var w = state.Working;
            bool has = w != null && w.HasContent;
            _builtWithContent = has;

            if (!has)
            {
                UiFactory.Label(parent, "NoContent", "Pick a species + setup in the Catalog tab first.",
                    0f, 50f, 900f, 40f, 24f, UiPalette.Warn, UiPalette.LeftMid);
                return;
            }

            for (int i = 0; i < Entries.Length; i++)
            {
                int idx = i;
                Entry e = Entries[i];
                float y = 58f + i * 96f;

                string displayName = SetColorRegions(state, idx);
                if (idx == 0) displayName += " (Base)";
                UiFactory.Label(parent, "ColorName_" + idx, displayName, 0f, y, 210f, 30f, 21f, UiPalette.Text, UiPalette.LeftMid);

                if (_hsvMode)
                {
                    BuildHsvSliders(parent, e, w, idx, y);
                }
                else
                {
                    BuildRgbSliders(parent, e, w, idx, y);
                }

                UiFactory.TextField(parent, "Color_" + idx + "_Hex", 1150f, y, 96f, 32f,
                    () => HexOf(Entries[idx].Get(w)),
                    hex => OnHex(idx, hex),
                    "0123456789abcdefABCDEF");

                _swatches[idx] = UiFactory.Raw(parent, "Color_" + idx + "_Swatch", 1260f, y, 96f, 32f, WhiteTex());
                _swatches[idx].color = Entries[idx].Get(w);
            }
        }

        private static void BuildRgbSliders(RectTransform parent, Entry e, Core.WorkingAssets w, int idx, float y)
        {
            UiFactory.Label(parent, "ColorR_" + idx + "_Lbl", "R", 210f, y - 4f, 16f, 26f, 18f, UiPalette.Dim, UiPalette.LeftMid);
            var rVal = UiFactory.Label(parent, "ColorR_" + idx + "_Val", "", 482f, y, 40f, 30f, 17f, UiPalette.Dim, UiPalette.LeftMid);
            var sR = UiFactory.Slider(parent, "Color_" + idx + "_R", 228f, y + 6f, 252f, 16f,
                () => Clamp01(e.Get(w).r), v => ApplyChannel(idx, 0, v), rVal, "{0:0}");
            sR.DisplayGet = () => Mathf.RoundToInt(Clamp01(e.Get(w).r) * 255f);

            UiFactory.Label(parent, "ColorG_" + idx + "_Lbl", "G", 520f, y - 4f, 16f, 26f, 18f, UiPalette.Dim, UiPalette.LeftMid);
            var gVal = UiFactory.Label(parent, "ColorG_" + idx + "_Val", "", 792f, y, 40f, 30f, 17f, UiPalette.Dim, UiPalette.LeftMid);
            var sG = UiFactory.Slider(parent, "Color_" + idx + "_G", 538f, y + 6f, 252f, 16f,
                () => Clamp01(e.Get(w).g), v => ApplyChannel(idx, 1, v), gVal, "{0:0}");
            sG.DisplayGet = () => Mathf.RoundToInt(Clamp01(e.Get(w).g) * 255f);

            UiFactory.Label(parent, "ColorB_" + idx + "_Lbl", "B", 830f, y - 4f, 16f, 26f, 18f, UiPalette.Dim, UiPalette.LeftMid);
            var bVal = UiFactory.Label(parent, "ColorB_" + idx + "_Val", "", 1102f, y, 40f, 30f, 17f, UiPalette.Dim, UiPalette.LeftMid);
            var sB = UiFactory.Slider(parent, "Color_" + idx + "_B", 848f, y + 6f, 252f, 16f,
                () => Clamp01(e.Get(w).b), v => ApplyChannel(idx, 2, v), bVal, "{0:0}");
            sB.DisplayGet = () => Mathf.RoundToInt(Clamp01(e.Get(w).b) * 255f);
        }

        private static void BuildHsvSliders(RectTransform parent, Entry e, Core.WorkingAssets w, int idx, float y)
        {
            float HueDeg(Core.WorkingAssets ww)
            {
                Color.RGBToHSV(e.Get(ww), out float h, out _, out _);
                return h * 360f;
            }

            UiFactory.Label(parent, "ColorH_" + idx + "_Lbl", "H", 210f, y - 4f, 16f, 26f, 18f, UiPalette.Dim, UiPalette.LeftMid);
            var hVal = UiFactory.Label(parent, "ColorH_" + idx + "_Val", "", 482f, y, 40f, 30f, 17f, UiPalette.Dim, UiPalette.LeftMid);
            var sH = UiFactory.Slider(parent, "Color_" + idx + "_H", 228f, y + 6f, 252f, 16f,
                () => { Color.RGBToHSV(e.Get(w), out float h, out _, out _); return h; },
                v => ApplyHsvChannel(idx, 0, v), hVal, "{0:0}");
            sH.DisplayGet = () => Mathf.RoundToInt(HueDeg(w));

            UiFactory.Label(parent, "ColorS_" + idx + "_Lbl", "S", 520f, y - 4f, 16f, 26f, 18f, UiPalette.Dim, UiPalette.LeftMid);
            var sVal = UiFactory.Label(parent, "ColorS_" + idx + "_Val", "", 792f, y, 40f, 30f, 17f, UiPalette.Dim, UiPalette.LeftMid);
            var sS = UiFactory.Slider(parent, "Color_" + idx + "_S", 538f, y + 6f, 252f, 16f,
                () => { Color.RGBToHSV(e.Get(w), out _, out float s, out _); return s; },
                v => ApplyHsvChannel(idx, 1, v), sVal, "{0:0}");
            sS.DisplayGet = () => { Color.RGBToHSV(e.Get(w), out _, out float s, out _); return Mathf.RoundToInt(s * 100f); };

            UiFactory.Label(parent, "ColorV_" + idx + "_Lbl", "V", 830f, y - 4f, 16f, 26f, 18f, UiPalette.Dim, UiPalette.LeftMid);
            var vVal = UiFactory.Label(parent, "ColorV_" + idx + "_Val", "", 1102f, y, 40f, 30f, 17f, UiPalette.Dim, UiPalette.LeftMid);
            var sV = UiFactory.Slider(parent, "Color_" + idx + "_V", 848f, y + 6f, 252f, 16f,
                () => { Color.RGBToHSV(e.Get(w), out _, out _, out float v); return v; },
                v => ApplyHsvChannel(idx, 2, v), vVal, "{0:0}");
            sV.DisplayGet = () => { Color.RGBToHSV(e.Get(w), out _, out _, out float v); return Mathf.RoundToInt(v * 100f); };
        }

        public static void Tick(StudioState state)
        {
            var w = state.Working;
            bool has = w != null && w.HasContent;
            if (has != _builtWithContent)
            {
                GameUI.RefreshTab();
            }
        }

        private static float Clamp01(float v) { return Mathf.Clamp01(v); }

        private static string HexOf(Color c)
        {
            int r = Mathf.RoundToInt(Mathf.Clamp01(c.r) * 255f);
            int g = Mathf.RoundToInt(Mathf.Clamp01(c.g) * 255f);
            int b = Mathf.RoundToInt(Mathf.Clamp01(c.b) * 255f);
            return r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
        }

        private static bool TryHex(string hex, out Color c)
        {
            c = Color.white;
            if (hex == null || hex.Length != 6) return false;
            for (int i = 0; i < 6; i++)
            {
                if ("0123456789abcdefABCDEF".IndexOf(hex[i]) < 0) return false;
            }
            try
            {
                int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                c = new Color(r / 255f, g / 255f, b / 255f, 1f);
                return true;
            }
            catch { return false; }
        }

        private static void ApplyChannel(int idx, int channel, float v)
        {
            var w = Main.State.Working;
            if (w == null) return;
            var e = Entries[idx];
            Color c = e.Get(w);
            if (channel == 0) c.r = v;
            else if (channel == 1) c.g = v;
            else c.b = v;
            e.Set(w, new Color(c.r, c.g, c.b, 1f));
            w.SyncWorkingObjects();
            UpdateSwatch(idx);
        }

        private static void ApplyHsvChannel(int idx, int channel, float v)
        {
            var w = Main.State.Working;
            if (w == null) return;
            var e = Entries[idx];
            Color c = e.Get(w);
            Color.RGBToHSV(c, out float h, out float s, out float val);
            if (channel == 0) h = v;
            else if (channel == 1) s = v;
            else val = v;
            c = Color.HSVToRGB(h, s, val);
            e.Set(w, new Color(c.r, c.g, c.b, 1f));
            w.SyncWorkingObjects();
            UpdateSwatch(idx);
        }

        private static void OnHex(int idx, string hex)
        {
            var w = Main.State.Working;
            if (w == null || !TryHex(hex, out var parsed)) return;
            var e = Entries[idx];
            e.Set(w, new Color(parsed.r, parsed.g, parsed.b, 1f));
            w.SyncWorkingObjects();
            UpdateSwatch(idx);
        }

        private static void UpdateSwatch(int idx)
        {
            if (_swatches == null || _swatches[idx] == null) return;
            var w = Main.State.Working;
            if (w == null) return;
            var c = Entries[idx].Get(w);
            _swatches[idx].color = new Color(c.r, c.g, c.b, 1f);
        }

        private static string SetColorRegions(StudioState state, int idx)
        {
            var w = state != null ? state.Working : null;
            string speciesId = w != null ? w.SpeciesId : "";

            switch (speciesId)
            {
                case "ALLOS":
                    switch (idx)
                    {
                        case 0: return "Belly";
                        case 1: return "Body";
                        case 2: return "Nose/Claws";
                        case 3: return "Stripes";
                        case 4: return "Details";
                        case 5: return "Journal Display";
                        case 6: return "Eye Colour";
                        default: return "Unknown";
                    }
                default:
                    switch (idx)
                    {
                        case 0: return "Base";
                        case 1: return "Pattern Colour 1";
                        case 2: return "Pattern Colour 2";
                        case 3: return "Pattern Colour 3";
                        case 4: return "Pattern Colour 4";
                        case 5: return "Journal Display";
                        case 6: return "Eye Colour";
                        default: return "Unknown";
                    }
            }
        }
    }
}
