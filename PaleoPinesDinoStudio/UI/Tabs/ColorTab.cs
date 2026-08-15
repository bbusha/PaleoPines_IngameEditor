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

        private static int CheckPatternNum(string patternId)
        {
            if(patternId.Contains("Pattern 1")) {return 1;}
            else if (patternId.Contains("Pattern 2")) {return 2;}
            else if (patternId.Contains("Pattern 3")) {return 3;}
            else {return 0;}
        }

        // Returns the color region name for a given index, taking into account species and pattern.
        private static string SetColorRegions(StudioState state, int idx)
        {
            var w = state != null ? state.Working : null;
            string speciesId = w != null ? w.SpeciesId : "";
            string patternId = w != null ? w.PatternUid : "";
            int patternNum = CheckPatternNum(patternId);

            switch (speciesId)
            {
                case "ALLOS":
                    if (patternNum == 1 || patternNum == 2 )
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch{0 => "Belly", 1 => "Body", 2 => "Claws/Nose",
                        3 => "Stripes", 4 => "Details", 5 => "Eye Colour",
                        _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "ANKYL":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor3;
                        return idx switch {0 => "Belly", 1 => "Body", 2 => "Horns",
                            3 => "Armour/Face", 4 => "UNUSED REGION", 5 => "Eye Colour",
                            _ => "Unknown",};}
                    else if (patternNum == 2) {
                        state.Working.JournalColor = state.Working.PatternColor3;
                        return idx switch {0 => "Belly", 1 => "Body", 2 => "Horns",
                            3 => "Face", 4 => "Armour", 5 => "Eye Colour",
                            _ => "Unknown",};}
                    else
                    {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "ARCHA":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor2;
                        return idx switch{0 => "Claws/Wings", 1 => "Belly", 2 => "Body",
                            3 => "Feet/Nose", 4 => "Details", 5 => "Eye Colour",
                            _ => "Unknown",};}
                    else if (patternNum == 2)
                    {state.Working.JournalColor = state.Working.PatternColor2;
                        return idx switch { 0 => "Claws", 1 => "Belly/Neck/Tail/Wings",
                            2 => "Body/Head/Tail Stripes", 3 => "Crest",
                            4 => "Details", 5 => "Back Stripes/Feet/Nose",
                            6 => "Eye Colour", _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "BARYO":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly/Claws", 1 => "Body",
                            2 => "Details", 3 => "Tail Circles", 4 => "Chin/Eye Oval",
                            5 => "Eye Colour", _ => "Unknown",};}
                    else if (patternNum == 2)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly/Claws", 1 => "Body",
                            2 => "Details/Eye Stripe", 3 => "Feet/Neck Gradient/Stripes",
                            4 => "Nose", 5 => "Eye Colour", _ => "Unknown",};}
                    else{state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "CARNO":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly", 1 => "Body", 2 => "Horns/Stripes",
                            3 => "Chin Gradient", 4 => "Chest Gradient",
                            5 => "Eye Colour", _ => "Unknown",};}
                    else if (patternNum == 2)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly", 1 => "Body", 2 => "Horns/Scales",
                            3 => "Chin Gradient/Spines", 4 => "Chest Gradient",
                            5 => "Eye Colour", _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "CENTR":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly", 1 => "Body", 2 => "Spots",
                            3 => "Beak", 4 => "Details", 5 => "Eye Colour",
                            _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "CERAT":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly/Feet",1 => "Body", 2 => "Spines/Spots",
                            3 => "Nose Horn", 4 => "Claws/Details", 5 => "Eye Colour",
                            _ => "Unknown",};}
                    else if (patternNum == 2)
                    {state.Working.JournalColor = state.Working.PatternColor2;
                        return idx switch {0 => "Splotches", 1 => "Belly/Feet", 2 => "Body",
                            3 => "Claws/Horns/Tail Stripes", 4 => "Leg Stripe/Horn Tip",
                            5 => "Eye Colour", _ => "Unknown", };}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "COELO":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch { 0 => "Belly", 1 => "Body", 2 => "Legs",
                            3 => "Head/Tail", 4 => "Claws/Details",
                            5 => "Eye Colour", _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "COMPS":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor2;
                        return idx switch {0 => "UNUSED REGION", 1 => "Belly",
                            2 => "Back", 3 => "Face/Feet", 4 => "UNUSED REGION",
                            5 => "Eye Colour", _ => "Unknown",};}
                    else if (patternNum == 2)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "UNUSED REGION", 1 => "Belly",
                            2 => "Back", 3 => "Face/Feet", 4 => "UNUSED REGION",
                            5 => "Eye Colour", _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "CORYT":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly/Nose*", 1 => "Body", 2 => "Tail",
                            3 => "Crest Bottom",4 => "Crest Top", 5 => "Eye Colour",
                            _ => "Unknown",};}
                    else if (patternNum == 2)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Claws*/Nose*", 1 => "Hood", 2 => "Legs",
                            3 => "Belly", 4 => "Light Gradient",5 => "Eye Colour",
                            _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);
                    }
                case "DEINO":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly", 1 => "Body", 2 => "Loops",
                            3 => "Beak/Legs", 4 => "Claws/Details", 5 => "Eye Colour",
                            _ => "Unknown",};}
                    else if (patternNum == 2)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly/Tail Stripes", 1 => "Body",
                            2 => "Head/Stripes", 3 => "Stripes", 4 => "Claws/Details",
                            5 => "Eye Colour", _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "DENON":
                    if (patternNum == 1 || patternNum == 2)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch { 0 => "Belly/Claws", 1 => "Body", 2 => "Markings",
                            3 => "Beak/Details/Legs", 4 => "UNUSED REGION", 5 => "Eye Colour",
                            _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "DESMA":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly/Feet", 1 => "Body",
                            2 => "Armour/Claws/Markings", 3 => "Claw Details/Spine",
                            4 => "Details", 5 => "Eye Colour", _ => "Unknown",};}
                    else if (patternNum == 2)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly/Feet", 1 => "Body", 2 => "Stripes",
                            3 => "Armour/Claws", 4 => "Details", 5 => "Eye Colour",
                            _ => "Unknown",};}
                    else if (patternNum == 3)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly/Feet", 1 => "Body/Nose",
                            2 => "Armour/Claws/Markings", 3 => "UNUSED REGION", 4 => "Details",
                            5 => "Eye Colour", _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "DILOP":
                    if (patternNum == 1 || patternNum == 2)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly/Claws", 1 => "Body",
                            2 => "Feet/Markings/Tail", 3 => "Crest", 4 => "Details",
                            5 => "Eye Colour", _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "DIMET":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly/Eye Marking", 1 => "Body",
                            2 => "Bottom Spine Gradient", 3 => "Claws", 4 => "Top Spine Gradient",
                            5 => "Eye Colour", _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "EUOPL":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor3;
                        return idx switch {0 => "Beak/Club/Horns", 1 => "Belly/Claws",
                            2 => "Body", 3 => "Armour/Details", 4 => "UNUSED REGION",
                            5 => "Eye Colour", _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "GALLI":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Beak*/Belly/Legs*", 1 => "Body",
                            2 => "Head/Tail Stripes", 3 => "Under Tail/Wing Details",
                            4 => "UNUSED REGION", 5 => "Eye Colour", _ => "Unknown",};}
                    else if (patternNum == 2)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Beak*/Belly/Legs*", 1 => "Body",
                            2 => "Head/Tail Stripes", 3 => "Under Tail/Wing Details",
                            4 => "Eye Marking", 5 => "Eye Colour", _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "KENTR":
                    if (patternNum == 1 || patternNum == 2)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly", 1 => "Body", 2 => "Plate Tips",
                            3 => "Plate Bottom", 4 => "Stripes", 5 => "Eye Colour",
                            _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "LUCKY":
                    return idx switch {0 => "Body*", 1 => "UNUSED REGION",
                        2 => "UNUSED REGION", 3 => "UNUSED REGION", 4 => "UNUSED REGION",
                        5 => "Eye Colour", _ => "Unknown",};
                case "MEGAL":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor2;
                        return idx switch {0 => "Claws", 1 => "Belly/Spots", 2 => "Body",
                            3 => "Nose/Tail Tip", 4 => "Details", 5 => "Eye Colour",
                            _ => "Unknown",};}
                    else if (patternNum == 2)
                    {state.Working.JournalColor = state.Working.PatternColor2;
                        return idx switch {0 => "Nose*", 1 => "Belly/Head Stripe/Tail",
                            2 => "Body", 3 => "Claws/Head/Spots 1", 4 => "Spots 2",
                            5 => "Eye Colour", _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "MICRO":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Claws/Wing Tips", 1 => "Body",
                            2 => "Bottom Gradient", 3 => "Crest", 4 => "Top Gradient",
                            5 => "Eye Colour", _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "OURAN":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly", 1 => "Body", 2 => "Markings",
                            3 => "Details", 4 => "Beak/Claws", 5 => "Eye Colour",
                            _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "OVIRA":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor2;
                        return idx switch {0 => "Claws", 1 => "Neck/Tail", 2 => "Body",
                            3 => "Crest", 4 => "Head/Legs/Tail Stripes", 5 => "Eye Colour",
                            _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "PACHY":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly/Horns/Tail Stripes", 1 => "Body",
                            2 => "Skull Dome", 3 => "Beak/Claws", 4 => "Details",
                            5 => "Eye Colour", _ => "Unknown",};}
                    else if (patternNum == 2)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly/Horns/Feet/Tail Gradient",
                            1 => "Body", 2 => "Markings/Skull Dome", 3 => "Beak/Claws",
                            4 => "Details", 5 => "Eye Colour", _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "PARAS":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly", 1 => "Body", 2 => "Nose",
                            3 => "Spine Gradient", 4 => "Details", 5 => "Eye Colour",
                            _ => "Unknown",};}
                    else if (patternNum == 2)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly", 1 => "Body", 2 => "Back Stripes",
                            3 => "Spine Gradient", 4 => "Details", 5 => "Eye Colour",
                            _ => "Unknown",};}
                    else if (patternNum == 3)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly", 1 => "Body", 2 => "Spots",
                            3 => "Spine Gradient", 4 => "Details", 5 => "Eye Colour",
                            _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "PINAC":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly", 1 => "Armour",
                            2 => "Beak/Claws/Club/Horns/Armour Gradiant", 3 => "Head Spots",
                            4 => "Details", 5 => "Eye Colour", _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "POSTO":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "UNUSED REGION", 1 => "Body", 2 => "Belly",
                            3 => "Claws/Spine", 4 => "UNUSED REGION", 5 => "Eye Colour",
                            _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "PSITT":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Beak/Belly", 1 => "Body/Quill Tip",
                            2 => "Claws/Head/Quill Middle/Spine Outline", 3 => "UNUSED REGION",
                            4 => "Details/Quill Bottom/Spine", 5 => "Eye Colour",
                            _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "SARCO":
                    if (patternNum == 1 || patternNum == 2)
                    {state.Working.JournalColor = state.Working.PatternColor2;
                        return idx switch {0 => "UNUSED REGION", 1 => "Belly", 2 => "Body",
                            3 => "Claws/Markings", 4 => "UNUSED REGION", 5 => "Eye Colour",
                            _ => "Unknown",};}
                    else if (patternNum == 3)
                    {state.Working.JournalColor = state.Working.PatternColor2;
                        return idx switch {0 => "UNUSED REGION", 1 => "Belly", 2 => "Body",
                            3 => "Back", 4 => "Spines", 5 => "Eye Colour", _ => "Unknown",};}
                    else
                    {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "SCELI":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor2;
                        return idx switch {0 => "UNUSED REGION", 1 => "Body", 2 => "Back",
                            3 => "UNUSED REGION", 4 => "Horns/Tail Gradient", 5 => "Eye Colour",
                            _ => "Unknown",};}
                    else
                    {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "SPINO":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly", 1 => "Body", 2 => "Neck Marking",
                            3 => "Sail Stripes", 4 => "UNUSED REGION", 5 => "Eye Colour",
                            _ => "Unknown",};}
                    else if (patternNum == 2 || patternNum == 3)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly", 1 => "Body", 2 => "Neck Marking",
                            3 => "Bottom Sail Gradient", 4 => "Top Sail Gradient", 5 => "Eye Colour",
                            _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "STEGO":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Beak*/Belly/Claws/Thagomizer",
                            1 => "Body", 2 => "Plates/Stripes", 3 => "Plate Gradient",
                            4 => "UNUSED REGION", 5 => "Eye Colour", _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "STYRA":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor2;
                        return idx switch {0 => "Claws*/Horns*/Nostrils*", 1 => "Belly",
                            2 => "Body", 3 => "Beak/Back/Eye Marking/Feet/Frill Outline",
                            4 => "Frill Markings", 5 => "Eye Colour", _ => "Unknown",};}
                    else if (patternNum == 2)
                    {state.Working.JournalColor = state.Working.PatternColor2;
                        return idx switch {0 => "Claws*/Horns*/Nostrils*",
                            1 => "Belly/Body Dots/Center Frill Ring", 2 => "Body/Outer Frill Ring",
                            3 => "Beak/Back/Feet/Frill", 4 => "Frill Dots", 5 => "Eye Colour",
                            _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "THERO":
                    if (patternNum == 1 || patternNum == 2)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly*/Claws", 1 => "Body/Feather Details",
                            2 => "Beak/Feet", 3 => "Back Markings/Eye Detail", 4 => "Details",
                            5 => "Eye Colour", _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "TRICE":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Claws/Horns", 1 => "Body",
                            2 => "Beak/Face Details/Leg Stripes", 3 => "Back Gradient/Belly/Spines",
                            4 => "Details", 5 => "Eye Colour", _ => "Unknown",};}
                    if (patternNum == 2)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Claws/Horns", 1 => "Body", 2 => "Beak/Blush/Details",
                            3 => "Back Gradient/Belly/Spines/Markings", 4 => "Details",
                            5 => "Eye Colour", _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "TROOD":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Head/Legs", 1 => "Body", 2 => "Tail Stripes/Wing Tips",
                            3 => "Beak/Claws", 4 => "Feet", 5 => "Eye Colour", _ => "Unknown",};}
                    else if (patternNum == 2)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Head/Legs/Tail Fan", 1 => "Body",
                            2 => "Stripes/Wing Tips", 3 => "Beak/Claws", 4 => "Feet",
                            5 => "Eye Colour", _ => "Unknown",};}
                    else if (patternNum == 3)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Head/Legs/Tail Fan", 1 => "Body",
                            2 => "Heart/Stripes/Wing Tips", 3 => "Beak/Claws", 4 => "Feet",
                            5 => "Eye Colour", _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "TYRAN":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly/Tail Gradient", 1 => "Body",
                            2 => "Details/Head/Stripes", 3 => "Claws", 4 => "Face Gradient",
                            5 => "Eye Colour", _ => "Unknown",};}
                    if (patternNum == 2)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly", 1 => "Body",
                            2 => "Details/Head/Outer Stripes", 3 => "Claws",
                            4 => "Face Stripe/Inner Stripes", 5 => "Eye Colour",
                            _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "UTAHR":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly/Fan Gradient/Feet", 1 => "Body",
                            2 => "Beak/Claws/Markings", 3 => "UNUSED REGION",
                            4 => "Details", 5 => "Eye Colour", _ => "Unknown",};}
                    if (patternNum == 2)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly/Feet", 1 => "Body",
                            2 => "Beak/Claws/Markings", 3 => "Details", 4 => "UNUSED REGION",
                            5 => "Eye Colour", _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "VELOC":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor2;
                        return idx switch {0 => "UNUSED REGION", 1 => "Head/Belly",
                            2 => "Body/Legs/Wings", 3 => "Beak Tip/Claws/Tail/Wing Tips",
                            4 => "Beak/Shoulders", 5 => "Eye Colour", _ => "Unknown",};}
                    if (patternNum == 2)
                    {state.Working.JournalColor = state.Working.PatternColor2;
                        return idx switch {0 => "UNUSED REGION", 1 => "Head/Belly",
                            2 => "Body/Wings", 3 => "Beak Tip/Claws/Tail/Wing Tips",
                            4 => "Beak/Feet/Shoulders", 5 => "Eye Colour", _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                case "WUERH":
                    if (patternNum == 1)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly/Claws/Thagomizer", 1 => "Body",
                            2 => "Plates/Spots", 3 => "UNUSED REGION", 4 => "UNUSED REGION",
                            5 => "Eye Colour", _ => "Unknown",};}
                    if (patternNum == 2)
                    {state.Working.JournalColor = state.Working.PatternColor1;
                        return idx switch {0 => "Belly/Claws/Thagomizer", 1 => "Body",
                            2 => "Plates", 3 => "Dots/Blush", 4 => "UNUSED REGION",
                            5 => "Eye Colour", _ => "Unknown",};}
                    else {state.Working.JournalColor = state.Working.BaseColor;
                        return ColorRegionDefault(idx);}
                default:
                    return ColorRegionDefault(idx);
            }
        }
    }
}
