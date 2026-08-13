using System;
using UnityEngine;
using UnityEngine.UI;

namespace PaleoPinesDinoStudio.UI.Tabs
{
    public static class ColorTab
    {
        private class Entry
        {
            public string Name;
            public Func<Core.WorkingAssets, Color> Get;
            public Action<Core.WorkingAssets, Color> Set;
        }

        private static readonly Entry[] Entries =
        {
            new Entry { Name = SetColorRegions(0) + " (Base)",       Get = w => w.BaseColor,     Set = (w, c) => w.BaseColor = c },
            new Entry { Name = "Pattern Colour 1",  Get = w => w.PatternColor1, Set = (w, c) => w.PatternColor1 = c },
            new Entry { Name = "Pattern Colour 2",  Get = w => w.PatternColor2, Set = (w, c) => w.PatternColor2 = c },
            new Entry { Name = "Pattern Colour 3",  Get = w => w.PatternColor3, Set = (w, c) => w.PatternColor3 = c },
            new Entry { Name = "Pattern Colour 4",  Get = w => w.PatternColor4, Set = (w, c) => w.PatternColor4 = c },
            new Entry { Name = "Journal Display",   Get = w => w.JournalColor,  Set = (w, c) => w.JournalColor = c },
        };

        private static RawImage[] _swatches;
        private static bool _builtWithContent;
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
                "Colour Studio - drag sliders or type hex to change the 5 dino colours.\n" +
                "Base Colour paints the body; Pattern Colours 1-4 tint the markings, which keep the setup's own pattern.",
                0f, 0f, 1300f, 46f, 20f, UiPalette.Text, UiPalette.LeftMid);

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

                UiFactory.Label(parent, "ColorName_" + idx, e.Name, 0f, y, 210f, 30f, 21f, UiPalette.Text, UiPalette.LeftMid);

                var rVal = UiFactory.Label(parent, "ColorR_" + idx + "_Val", "", 482f, y, 40f, 30f, 17f, UiPalette.Dim, UiPalette.LeftMid);
                UiFactory.Slider(parent, "Color_" + idx + "_R", 220f, y + 6f, 260f, 16f,
                    () => Clamp01(e.Get(w).r), v => ApplyChannel(idx, 0, v), rVal, "{0:0}");

                var gVal = UiFactory.Label(parent, "ColorG_" + idx + "_Val", "", 792f, y, 40f, 30f, 17f, UiPalette.Dim, UiPalette.LeftMid);
                UiFactory.Slider(parent, "Color_" + idx + "_G", 530f, y + 6f, 260f, 16f,
                    () => Clamp01(e.Get(w).g), v => ApplyChannel(idx, 1, v), gVal, "{0:0}");

                var bVal = UiFactory.Label(parent, "ColorB_" + idx + "_Val", "", 1102f, y, 40f, 30f, 17f, UiPalette.Dim, UiPalette.LeftMid);
                UiFactory.Slider(parent, "Color_" + idx + "_B", 840f, y + 6f, 260f, 16f,
                    () => Clamp01(e.Get(w).b), v => ApplyChannel(idx, 2, v), bVal, "{0:0}");

                UiFactory.TextField(parent, "Color_" + idx + "_Hex", 1150f, y, 96f, 32f,
                    () => HexOf(Entries[idx].Get(w)),
                    hex => OnHex(idx, hex),
                    "0123456789abcdefABCDEF");

                _swatches[idx] = UiFactory.Raw(parent, "Color_" + idx + "_Swatch", 1260f, y, 96f, 32f, WhiteTex());
                _swatches[idx].color = Entries[idx].Get(w);
            }
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

        private static string SetColorRegions(int idx)
        {
            var w = Main.State.Working;

            switch (w.SpeciesId)
            {
                case "ALLOS":
                    MelonLoader.MelonLogger.Msg("Setting color regions for Allosaurus");
                    switch (idx)
                    {
                        case 0: return "Belly";
                        case 1: return "Pattern Colour 1";
                        case 2: return "Pattern Colour 2";
                        case 3: return "Pattern Colour 3";
                        case 4: return "Pattern Colour 4";
                        default: return "Unknown";
                    }
                default:
                    MelonLoader.MelonLogger.Msg("Setting color regions for default species");
                    return "Default";
            }
        }
    }
}
