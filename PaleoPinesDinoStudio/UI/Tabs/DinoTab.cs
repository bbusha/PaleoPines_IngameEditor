using System.Collections.Generic;
using UnityEngine;
using Il2CppTMPro;
using Il2CppItalicPig.PaleoPines.Actors;

namespace PaleoPinesDinoStudio.UI.Tabs
{
    /// <summary>
    /// Lists DinoPawns in the current scene and lets the user load one as the editing base.
    /// The pawn's live material colours are captured (via ColourApplier.LoadPawnAsBase) and
    /// become the working base; from then on, colour changes apply to that pawn in real time.
    /// </summary>
    public static class DinoTab
    {
        private static UiScroll _scroll;
        private static TextMeshProUGUI _info;
        private static int _lastCount = -1;
        private static bool _rebuilding;
        private static string _filter = "";
        private static string _lastFilter = "";
        private static System.Collections.Generic.List<DinoPawn> _pawns = new System.Collections.Generic.List<DinoPawn>();

        public static void Build(StudioState state, RectTransform parent)
        {
            UiFactory.Label(parent, "DinoTitle", "Nearby dinos", 0f, 0f, 600f, 26f, 24f, UiPalette.Text, UiPalette.LeftMid);
            UiFactory.Label(parent, "DinoHint",
                "Pick a dino in the current area to use its look as your editing base.\n" +
                "Its colours are copied in; from then on your changes update it in real time.",
                0f, 28f, 900f, 44f, 16f, UiPalette.Dim, UiPalette.LeftMid);

            UiFactory.Label(parent, "DinoFilterLabel", "Filter:", 0f, 76f, 80f, 26f, 18f, UiPalette.Text, UiPalette.LeftMid);
            UiFactory.TextField(parent, "DinoFilter", 84f, 76f, 420f, 28f,
                () => _filter, v => _filter = v);

            _info = UiFactory.Label(parent, "DinoInfo", "", 0f, 110f, 1200f, 22f, 18f, UiPalette.Warn, UiPalette.LeftMid);

            var clear = UiFactory.Button(parent, "ClearLive", "Stop editing (clear)", 0f, 136f, 220f, 30f,
                () => ClearLive(Main.State));

            _scroll = UiFactory.Scroll(parent, "DinoList", 0f, 174f, 1200f, 560f);

            _lastCount = -1;
            _lastFilter = "";
            Rebuild(state);
            UpdateInfo(state);
        }

        private static void ClearLive(StudioState state)
        {
            if (state == null) return;
            state.LivePawn = null;
            state.SetStatus("Stopped editing the live dino.");
            UpdateInfo(state);
        }

        public static void Tick(StudioState state)
        {
            int count = Core.GameCatalog.PawnCount();
            if (count != _lastCount && !_rebuilding)
            {
                _rebuilding = true;
                Rebuild(state);
                _rebuilding = false;
            }
            else if (_filter != _lastFilter && !_rebuilding)
            {
                _rebuilding = true;
                _lastFilter = _filter;
                Rebuild(state);
                _rebuilding = false;
            }
            UpdateInfo(state);
        }

        private static void UpdateInfo(StudioState state)
        {
            if (_info == null) return;
            string loaded = "";
            if (state != null && state.LivePawn != null && state.Working != null)
            {
                string uid = "";
                try { uid = state.LivePawn.Uid; } catch { }
                string name = Core.Injector.DinoDisplayName(state.LivePawn);
                loaded = !string.IsNullOrEmpty(name)
                    ? "Editing: " + name + " (" + state.Working.SpeciesId + ", uid " + uid + ") - changes apply live."
                    : "Editing: " + state.Working.SpeciesId + " (uid " + uid + ") - changes apply live.";
            }
            _info.text = loaded;
        }

        private static void Rebuild(StudioState state)
        {
            var all = Core.GameCatalog.PawnCache();
            _pawns.Clear();
            string f = _filter != null ? _filter.Trim().ToLowerInvariant() : "";
            for (int i = 0; i < all.Count; i++)
            {
                var p = all[i];
                if (p == null) continue;
                if (f.Length > 0)
                {
                    string name = Core.Injector.DinoDisplayName(p);
                    string hay = name + " " + p.DefaultSpeciesID;
                    if (hay.IndexOf(f, System.StringComparison.OrdinalIgnoreCase) < 0) continue;
                }
                _pawns.Add(p);
            }
            _lastCount = all.Count;

            if (_scroll == null) return;
            GameUI.RemoveScrollButtons(_scroll);
            UiFactory.ClearChildren(_scroll.Content);
            _scroll.ContentHeight = 0f;
            _scroll.Scroll = 0f;

            if (_pawns.Count == 0)
            {
                UiFactory.Label(_scroll.Content, "Empty", "No dinos found nearby. Make sure you are in a world with dinos around.",
                    0f, 0f, _scroll.view.width, 40f, 18f, UiPalette.Dim, UiPalette.LeftMid);
                return;
            }

            for (int i = 0; i < _pawns.Count; i++)
            {
                var p = _pawns[i];
                if (p == null) continue;

                string uid = "";
                try { uid = p.Uid; } catch { }
                string tamed = Core.Injector.IsTamed(p) ? "tamed" : "wild";
                string name = Core.Injector.DinoDisplayName(p);
                string label = !string.IsNullOrEmpty(name)
                    ? name + "  (" + p.DefaultSpeciesID + ")  [" + tamed + "]"
                    : p.DefaultSpeciesID + "  [" + tamed + "]  " + uid;

                int idx = i;
                var b = UiFactory.ScrollButton(_scroll, "Dino_" + idx, label, i * 32f, 1196f, 30f,
                    () => LoadPawn(Main.State, _pawns[idx]));
                b.IsActive = () => Main.State != null && Main.State.LivePawn == _pawns[idx];
                b.Label.fontSize = 19f;
                b.Label.horizontalAlignment = Il2CppTMPro.HorizontalAlignmentOptions.Left;
                b.Label.verticalAlignment = Il2CppTMPro.VerticalAlignmentOptions.Middle;
            }
        }

        private static void LoadPawn(StudioState state, DinoPawn pawn)
        {
            if (state == null || pawn == null) return;
            if (state.Working == null) state.Working = new Core.WorkingAssets();
            Core.ColourApplier.LoadPawnAsBase(state, pawn);
            UpdateInfo(state);
        }
    }
}
