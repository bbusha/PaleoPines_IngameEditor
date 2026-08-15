using Il2CppItalicPig.PaleoPines.Inventories;
using System;
using UnityEngine;
using UnityEngine.Android;
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
            new Entry { Get = w => w.EyeColor,      Set = (w, c) => w.EyeColor = c },

            //new Entry { Get = w => w.JournalColor, Set = (w, c) => w.JournalColor = c }, // this is how the journal color is set in the game, but it doesn't seem to be used for anything other than base body color
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
                "Any marking with a * indicates a region that is tinted, not fully changed.",
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
                if (idx == Entries.Length - 1) displayName += "*";
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

        // Returns the default color region name for a given index.
        private static string ColorRegionDefault(int idx)
        {
            switch (idx)
            {
                case 0: return "Base";
                case 1: return "Pattern Colour 1";
                case 2: return "Pattern Colour 2";
                case 3: return "Pattern Colour 3";
                case 4: return "Pattern Colour 4";
                case 5: return "Eye Colour";
                default: return "Unknown";
            }
        }

        // Returns the color region name for a given index, taking into account species and pattern.
        private static string SetColorRegions(StudioState state, int idx)
        {
            var w = state != null ? state.Working : null;
            string speciesId = w != null ? w.SpeciesId : "";
            string patternId = w != null ? w.PatternUid : "";

            switch (speciesId)
            {
                case "ALLOS":
                    if (patternId.Contains("Pattern 1") || patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly";
                        case 1: return "Body";
                        case 2: return "Claws/Nose";
                        case 3: return "Stripes";
                        case 4: return "Details";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "ANKYL":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor3;
                        switch (idx)
                        {
                        case 0: return "Belly";
                        case 1: return "Body";
                        case 2: return "Horns";
                        case 3: return "Armour/Face";
                        case 4: return "UNUSED REGION";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else if (patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor3;
                        switch (idx)
                        {
                        case 0: return "Belly";
                        case 1: return "Body";
                        case 2: return "Horns";
                        case 3: return "Face";
                        case 4: return "Armour";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "ARCHA":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor2;
                        switch (idx)
                        {
                        case 0: return "Claws/Wings";
                        case 1: return "Belly";
                        case 2: return "Body";
                        case 3: return "Feet/Nose";
                        case 4: return "Details";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else if (patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor2;
                        switch (idx)
                        {
                        case 0: return "Claws";
                        case 1: return "Belly/Neck/Tail/Wings";
                        case 2: return "Body/Head/Tail Stripes";
                        case 3: return "Crest";
                        case 4: return "Details";
                        case 5: return "Back Stripes/Feet/Nose";
                        case 6: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "BARYO":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly/Claws";
                        case 1: return "Body";
                        case 2: return "Details";
                        case 3: return "Tail Circles";
                        case 4: return "Chin/Eye Oval";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else if (patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly/Claws";
                        case 1: return "Body";
                        case 2: return "Details/Eye Stripe";
                        case 3: return "Feet/Neck Gradient/Stripes";
                        case 4: return "Nose";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "CARNO":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly";
                        case 1: return "Body";
                        case 2: return "Horns/Stripes";
                        case 3: return "Chin Gradient";
                        case 4: return "Chest Gradient";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else if (patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly";
                        case 1: return "Body";
                        case 2: return "Horns/Scales";
                        case 3: return "Chin Gradient/Spines";
                        case 4: return "Chest Gradient";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "CENTR":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly";
                        case 1: return "Body";
                        case 2: return "Spots";
                        case 3: return "Beak";
                        case 4: return "Details";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "CERAT":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly/Feet";
                        case 1: return "Body";
                        case 2: return "Spines/Spots";
                        case 3: return "Nose Horn";
                        case 4: return "Claws/Details";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else if (patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor2;
                        switch (idx)
                        {
                        case 0: return "Splotches";
                        case 1: return "Belly/Feet";
                        case 2: return "Body";
                        case 3: return "Claws/Horns/Tail Stripes";
                        case 4: return "Leg Stripe/Horn Tip";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "COELO":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly";
                        case 1: return "Body";
                        case 2: return "Legs";
                        case 3: return "Head/Tail";
                        case 4: return "Claws/Details";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "COMPS":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor2;
                        switch (idx)
                        {
                        case 0: return "UNUSED REGION";
                        case 1: return "Belly";
                        case 2: return "Back";
                        case 3: return "Face/Feet";
                        case 4: return "UNUSED REGION";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    if (patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "UNUSED REGION";
                        case 1: return "Belly";
                        case 2: return "Back";
                        case 3: return "Face/Feet";
                        case 4: return "UNUSED REGION";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "CORYT":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly/Nose*";
                        case 1: return "Body";
                        case 2: return "Tail";
                        case 3: return "Crest Bottom";
                        case 4: return "Crest Top";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else if (patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Claws*/Nose*";
                        case 1: return "Hood";
                        case 2: return "Legs";
                        case 3: return "Belly";
                        case 4: return "Light Gradient";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        return ColorRegionDefault(idx);
                    }
                case "DEINO":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly";
                        case 1: return "Body";
                        case 2: return "Loops";
                        case 3: return "Beak/Legs";
                        case 4: return "Claws/Details";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else if (patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly/Tail Stripes";
                        case 1: return "Body";
                        case 2: return "Head/Stripes";
                        case 3: return "Stripes";
                        case 4: return "Claws/Details";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "DENON":
                    if (patternId.Contains("Pattern 1") || patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly/Claws";
                        case 1: return "Body";
                        case 2: return "Markings";
                        case 3: return "Beak/Details/Legs";
                        case 4: return "UNUSED REGION";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "DESMA":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly/Feet";
                        case 1: return "Body";
                        case 2: return "Armour/Claws/Markings";
                        case 3: return "Claw Details/Spine";
                        case 4: return "Details";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else if (patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly/Feet";
                        case 1: return "Body";
                        case 2: return "Stripes";
                        case 3: return "Armour/Claws";
                        case 4: return "Details";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    if (patternId.Contains("Pattern 3"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly/Feet";
                        case 1: return "Body/Nose";
                        case 2: return "Armour/Claws/Markings";
                        case 3: return "UNUSED REGION";
                        case 4: return "Details";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "DILOP":
                    if (patternId.Contains("Pattern 1") || patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly/Claws";
                        case 1: return "Body";
                        case 2: return "Feet/Markings/Tail";
                        case 3: return "Crest";
                        case 4: return "Details";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "DIMET":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly/Eye Marking";
                        case 1: return "Body";
                        case 2: return "Bottom Spine Gradient";
                        case 3: return "Claws";
                        case 4: return "Top Spine Gradient";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "EUOPL":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor3;
                        switch (idx)
                        {
                        case 0: return "Beak/Club/Horns";
                        case 1: return "Belly/Claws";
                        case 2: return "Body";
                        case 3: return "Armour/Details";
                        case 4: return "UNUSED REGION";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "GALLI":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Beak*/Belly/Legs*";
                        case 1: return "Body";
                        case 2: return "Head/Tail Stripes";
                        case 3: return "Under Tail/Wing Details";
                        case 4: return "UNUSED REGION";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else if (patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Beak*/Belly/Legs*";
                        case 1: return "Body";
                        case 2: return "Head/Tail Stripes";
                        case 3: return "Under Tail/Wing Details";
                        case 4: return "Eye Marking";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "KENTR":
                    if (patternId.Contains("Pattern 1") || patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly";
                        case 1: return "Body";
                        case 2: return "Plate Tips";
                        case 3: return "Plate Bottom";
                        case 4: return "Stripes";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "LUCKY":
                    switch (idx)
                    {
                        case 0: return "Body*";
                        case 1: return "UNUSED REGION";
                        case 2: return "UNUSED REGION";
                        case 3: return "UNUSED REGION";
                        case 4: return "UNUSED REGION";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                    }
                case "MEGAL":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor2;
                        switch (idx)
                        {
                        case 0: return "Claws";
                        case 1: return "Belly/Spots";
                        case 2: return "Body";
                        case 3: return "Nose/Tail Tip";
                        case 4: return "Details";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else if (patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor2;
                        switch (idx)
                        {
                        case 0: return "Nose*";
                        case 1: return "Belly/Head Stripe/Tail";
                        case 2: return "Body";
                        case 3: return "Claws/Head/Spots 1";
                        case 4: return "Spots 2";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "MICRO":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Claws/Wing Tips";
                        case 1: return "Body";
                        case 2: return "Bottom Gradient";
                        case 3: return "Crest";
                        case 4: return "Top Gradient";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "OURAN":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly";
                        case 1: return "Body";
                        case 2: return "Markings";
                        case 3: return "Details";
                        case 4: return "Beak/Claws";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "OVIRA":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor2;
                        switch (idx)
                        {
                        case 0: return "Claws";
                        case 1: return "Neck/Tail";
                        case 2: return "Body";
                        case 3: return "Crest";
                        case 4: return "Head/Legs/Tail Stripes";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "PACHY":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly/Horns/Tail Stripes";
                        case 1: return "Body";
                        case 2: return "Skull Dome";
                        case 3: return "Beak/Claws";
                        case 4: return "Details";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else if (patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly/Horns/Feet/Tail Gradient";
                        case 1: return "Body";
                        case 2: return "Markings/Skull Dome";
                        case 3: return "Beak/Claws";
                        case 4: return "Details";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "PARAS":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly";
                        case 1: return "Body";
                        case 2: return "Nose";
                        case 3: return "Spine Gradient";
                        case 4: return "Details";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else if (patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly";
                        case 1: return "Body";
                        case 2: return "Back Stripes";
                        case 3: return "Spine Gradient";
                        case 4: return "Details";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else if (patternId.Contains("Pattern 3"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly";
                        case 1: return "Body";
                        case 2: return "Spots";
                        case 3: return "Spine Gradient";
                        case 4: return "Details";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "PINAC":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly";
                        case 1: return "Armour";
                        case 2: return "Beak/Claws/Club/Horns/Armour Gradiant";
                        case 3: return "Head Spots";
                        case 4: return "Details";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "POSTO":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "UNUSED REGION";
                        case 1: return "Body";
                        case 2: return "Belly";
                        case 3: return "Claws/Spine";
                        case 4: return "UNUSED REGION";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "PSITT":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Beak/Belly";
                        case 1: return "Body/Quill Tip";
                        case 2: return "Claws/Head/Quill Middle/Spine Outline";
                        case 3: return "UNUSED REGION";
                        case 4: return "Details/Quill Bottom/Spine";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "SARCO":
                    if (patternId.Contains("Pattern 1") || patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor2;
                        switch (idx)
                        {
                        case 0: return "UNUSED REGION";
                        case 1: return "Belly";
                        case 2: return "Body";
                        case 3: return "Claws/Markings";
                        case 4: return "UNUSED REGION";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else if (patternId.Contains("Pattern 3"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor2;
                        switch (idx)
                        {
                        case 0: return "UNUSED REGION";
                        case 1: return "Belly";
                        case 2: return "Body";
                        case 3: return "Back";
                        case 4: return "Spines";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "SCELI":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor2;
                        switch (idx)
                        {
                        case 0: return "UNUSED REGION";
                        case 1: return "Body";
                        case 2: return "Back";
                        case 3: return "UNUSED REGION";
                        case 4: return "Horns/Tail Gradient";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "SPINO":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly";
                        case 1: return "Body";
                        case 2: return "Neck Marking";
                        case 3: return "Sail Stripes";
                        case 4: return "UNUSED REGION";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else if (patternId.Contains("Pattern 2") || patternId.Contains("Pattern 3"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly";
                        case 1: return "Body";
                        case 2: return "Neck Marking";
                        case 3: return "Bottom Sail Gradient";
                        case 4: return "Top Sail Gradient";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "STEGO":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Beak*/Belly/Claws/Thagomizer";
                        case 1: return "Body";
                        case 2: return "Plates/Stripes";
                        case 3: return "Plate Gradient";
                        case 4: return "UNUSED REGION";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "STYRA":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor2;
                        switch (idx)
                        {
                        case 0: return "Claws*/Horns*/Nostrils*";
                        case 1: return "Belly";
                        case 2: return "Body";
                        case 3: return "Beak/Back/Eye Marking/Feet/Frill Outline";
                        case 4: return "Frill Markings";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else if (patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor2;
                        switch (idx)
                        {
                        case 0: return "Claws*/Horns*/Nostrils*";
                        case 1: return "Belly/Body Dots/Center Frill Ring";
                        case 2: return "Body/Outer Frill Ring";
                        case 3: return "Beak/Back/Feet/Frill";
                        case 4: return "Frill Dots";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "THERO":
                    if (patternId.Contains("Pattern 1") || patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly*/Claws";
                        case 1: return "Body/Feather Details";
                        case 2: return "Beak/Feet";
                        case 3: return "Back Markings/Eye Detail";
                        case 4: return "Details";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "TRICE":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Claws/Horns";
                        case 1: return "Body";
                        case 2: return "Beak/Face Details/Leg Stripes";
                        case 3: return "Back Gradient/Belly/Spines";
                        case 4: return "Details";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    if (patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Claws/Horns";
                        case 1: return "Body";
                        case 2: return "Beak/Blush/Details";
                        case 3: return "Back Gradient/Belly/Spines/Markings";
                        case 4: return "Details";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "TROOD":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Head/Legs";
                        case 1: return "Body";
                        case 2: return "Tail Stripes/Wing Tips";
                        case 3: return "Beak/Claws";
                        case 4: return "Feet";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else if (patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Head/Legs/Tail Fan";
                        case 1: return "Body";
                        case 2: return "Stripes/Wing Tips";
                        case 3: return "Beak/Claws";
                        case 4: return "Feet";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else if (patternId.Contains("Pattern 3"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Head/Legs/Tail Fan";
                        case 1: return "Body";
                        case 2: return "Heart/Stripes/Wing Tips";
                        case 3: return "Beak/Claws";
                        case 4: return "Feet";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "TYRAN":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly/Tail Gradient";
                        case 1: return "Body";
                        case 2: return "Details/Head/Stripes";
                        case 3: return "Claws";
                        case 4: return "Face Gradient";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    if (patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly";
                        case 1: return "Body";
                        case 2: return "Details/Head/Outer Stripes";
                        case 3: return "Claws";
                        case 4: return "Face Stripe/Inner Stripes";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "UTAHR":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly/Fan Gradient/Feet";
                        case 1: return "Body";
                        case 2: return "Beak/Claws/Markings";
                        case 3: return "UNUSED REGION";
                        case 4: return "Details";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    if (patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly/Feet";
                        case 1: return "Body";
                        case 2: return "Beak/Claws/Markings";
                        case 3: return "Details";
                        case 4: return "UNUSED REGION";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "VELOC":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor2;
                        switch (idx)
                        {
                        case 0: return "UNUSED REGION";
                        case 1: return "Head/Belly";
                        case 2: return "Body/Legs/Wings";
                        case 3: return "Beak Tip/Claws/Tail/Wing Tips";
                        case 4: return "Beak/Shoulders";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    if (patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor2;
                        switch (idx)
                        {
                        case 0: return "UNUSED REGION";
                        case 1: return "Head/Belly";
                        case 2: return "Body/Wings";
                        case 3: return "Beak Tip/Claws/Tail/Wing Tips";
                        case 4: return "Beak/Feet/Shoulders";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "WUERH":
                    if (patternId.Contains("Pattern 1"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly/Claws/Thagomizer";
                        case 1: return "Body";
                        case 2: return "Plates/Spots";
                        case 3: return "UNUSED REGION";
                        case 4: return "UNUSED REGION";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    if (patternId.Contains("Pattern 2"))
                    {
                        state.Working.JournalColor = state.Working.PatternColor1;
                        switch (idx)
                        {
                        case 0: return "Belly/Claws/Thagomizer";
                        case 1: return "Body";
                        case 2: return "Plates";
                        case 3: return "Dots/Blush";
                        case 4: return "UNUSED REGION";
                        case 5: return "Eye Colour";
                        default: return "Unknown";
                        }
                    }
                    else
                    {
                        state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                default:
                    return ColorRegionDefault(idx);
            }
        }
    }
}
