using UnityEngine;
using Il2CppItalicPig.PaleoPines.Dinos;

namespace PaleoPinesDinoStudio.UI.Tabs
{
    public static class ApplyTab
    {
        private static string _customName = "";
        private static int _rarityIndex = 0;
        private static bool _injectSpawns = true;
        private static bool _recolorTamed = true;

        private static readonly string[] RarityNames = { "Common", "Uncommon", "Rare", "UltraRare" };

        private static UiButton _applyBtn;
        private static UiScroll _pawnScroll;
        private static bool _builtWithContent;
        private static float _lastPawnRefresh;
        private static int _lastPawnCount = -1;

        public static void Build(StudioState state, RectTransform parent)
        {
            _applyBtn = null;
            _pawnScroll = null;
            var w = state.Working;
            bool has = w != null && w.HasContent;
            _builtWithContent = has;

            UiFactory.Label(parent, "ApplyTitle", "Apply your custom colour to the game.", 0f, 0f, 700f, 26f, 22f, UiPalette.Text, UiPalette.LeftMid);
            UiFactory.Label(parent, "ApplySub", "This ADDS a new colour variant of the species; nothing existing is replaced.", 0f, 26f, 760f, 26f, 18f, UiPalette.Dim, UiPalette.LeftMid);

            // Name
            UiFactory.Label(parent, "NameLabel", "Custom setup name:", 0f, 64f, 300f, 24f, 18f, UiPalette.Text, UiPalette.LeftMid);
            UiFactory.TextField(parent, "NameField", 0f, 92f, 420f, 34f,
                () => _customName, v => _customName = v);

            // Rarity
            UiFactory.Label(parent, "RarityLabel", "Rarity:", 0f, 138f, 200f, 24f, 18f, UiPalette.Text, UiPalette.LeftMid);
            for (int i = 0; i < RarityNames.Length; i++)
            {
                int idx = i;
                UiFactory.Toggle(parent, "Rarity_" + idx, RarityNames[i], 0f + i * 112f, 166f, 106f, 30f,
                    () => _rarityIndex == idx, () => _rarityIndex = idx);
            }

            // Options
            UiFactory.Toggle(parent, "OptInject", "Add to herd (new wild spawns can appear in this colour)", 0f, 212f, 620f, 30f,
                () => _injectSpawns, () => _injectSpawns = !_injectSpawns);
            UiFactory.Toggle(parent, "OptRecolor", "Recolour tamed dinos of this species in the current area", 0f, 248f, 720f, 30f,
                () => _recolorTamed, () => _recolorTamed = !_recolorTamed);

            // Apply
            _applyBtn = UiFactory.Button(parent, "ApplyBtn", "Apply", 0f, 300f, 200f, 42f, () => Apply(state, w));
            _applyBtn.Disabled = !has;

            if (!has)
            {
                UiFactory.Label(parent, "NoContent", "Pick a species + setup in the Catalog tab first.",
                    0f, 360f, 700f, 40f, 22f, UiPalette.Warn, UiPalette.LeftMid);
            }
            else
            {
                UiFactory.Label(parent, "IdTitle", "Your custom colour identifier:", 0f, 360f, 400f, 24f, 18f, UiPalette.Text, UiPalette.LeftMid);
                UiFactory.Label(parent, "ColorUid", "Colour:  " + w.ColorUid, 0f, 390f, 700f, 22f, 17f, UiPalette.Dim, UiPalette.LeftMid);
                UiFactory.Label(parent, "PatternUid", "Pattern: " + w.PatternUid + "  (kept from the source setup)", 0f, 414f, 760f, 22f, 17f, UiPalette.Dim, UiPalette.LeftMid);

                UiFactory.Label(parent, "ApplyHint",
                    "Tip: 'Recolour tamed' works on dinos already in the current area.\n" +
                    "Wild spawns only use the new colour after the area has reloaded (e.g. sleeping or leaving and returning).",
                    0f, 444f, 760f, 60f, 16f, UiPalette.Warn, UiPalette.LeftMid);
            }

            // Pawn list
            UiFactory.Label(parent, "PawnTitle", "Tamed dinos in scene of this species:", 720f, 0f, 700f, 26f, 20f, UiPalette.Text, UiPalette.LeftMid);
            _pawnScroll = UiFactory.Scroll(parent, "PawnList", 720f, 30f, 796f, 650f);
            _lastPawnCount = -1;
        }

        public static void Tick(StudioState state)
        {
            var w = state.Working;
            bool has = w != null && w.HasContent;
            if (has != _builtWithContent)
            {
                GameUI.RefreshTab();
                return;
            }

            if (!has) return;

            if (_applyBtn != null) _applyBtn.Disabled = !has;

            if (_pawnScroll != null && Time.unscaledTime - _lastPawnRefresh > 0.5f)
            {
                _lastPawnRefresh = Time.unscaledTime;
                var pawns = Core.GameCatalog.FindPawnsOfSpecies(w.SpeciesId);
                if (pawns.Count != _lastPawnCount)
                {
                    RebuildPawnList(pawns);
                }
            }
        }

        private static void RebuildPawnList(System.Collections.Generic.List<Il2CppItalicPig.PaleoPines.Actors.DinoPawn> pawns)
        {
            if (_pawnScroll == null) return;
            _lastPawnCount = pawns.Count;
            UiFactory.ClearChildren(_pawnScroll.Content);
            _pawnScroll.ContentHeight = 0f;
            _pawnScroll.Scroll = 0f;

            if (pawns.Count == 0)
            {
                var rt = UiFactory.ScrollItem(_pawnScroll, "None", 0f, 400f, 26f);
                UiFactory.Label(rt, "NoneLabel", "None found in the current scene.", 0f, 0f, 400f, 26f, 18f, UiPalette.Dim, UiPalette.LeftMid);
                return;
            }

            for (int i = 0; i < pawns.Count; i++)
            {
                var pawn = pawns[i];
                if (pawn == null) continue;
                bool tamed = Core.Injector.IsTamed(pawn);
                string text = "DinoPawn uid=" + pawn.Uid + "   [" + (tamed ? "tamed" : "wild") + "]";
                var rt = UiFactory.ScrollItem(_pawnScroll, "Pawn_" + i, i * 30f, 400f, 28f);
                UiFactory.Label(rt, "Pawn_" + i + "_Label", text, 4f, 0f, 780f, 28f, 17f, tamed ? UiPalette.Text : UiPalette.Dim, UiPalette.LeftMid);
            }
        }

        private static void Apply(StudioState state, Core.WorkingAssets w)
        {
            if (w == null) return;
            string name = string.IsNullOrEmpty(_customName) ? ("My " + w.SpeciesId) : _customName.Trim();
            w.SetupDisplayName = name;
            w.Rarity = (DinoRarity)_rarityIndex;

            w.SyncWorkingObjects();

            Core.AssetFactory.EnsureAssets(w);
            bool any = false;

            if (_injectSpawns)
            {
                any |= Core.Injector.InjectIntoHerd(w);
            }

            if (_recolorTamed)
            {
                int recolored = Core.Injector.RecolorTamedPawns(w);
                any |= recolored > 0;
            }

            if (any)
            {
                state.SetStatus("Applied \"" + name + "\" to species " + w.SpeciesId + ".");
            }
            else
            {
                state.SetStatus("Nothing to apply - check toggles, or no wild spawns / tamed dinos matched.");
            }
        }
    }
}
