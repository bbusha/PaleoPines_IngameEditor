using System.Collections.Generic;
using UnityEngine;
using Il2CppTMPro;
using Il2CppItalicPig.PaleoPines.Dinos;

namespace PaleoPinesDinoStudio.UI.Tabs
{
    public static class CatalogTab
    {
        private static int _speciesIndex = -1;
        private static int _setupIndex = -1;
        private static UiScroll _speciesScroll;
        private static UiScroll _setupScroll;
        private static TextMeshProUGUI _setupTitle;
        private static string _lastSetupSpecies = "";
        private static int _lastSetupCount = -1;
        private static int _lastSpeciesCount = -1;
        private static string _speciesFilter = "";
        private static string _lastSpeciesFilter = "";
        private static string _setupFilter = "";
        private static string _lastSetupFilter = "";
        private static readonly System.Collections.Generic.List<string> _filteredSpecies =
            new System.Collections.Generic.List<string>();

        public static void Build(StudioState state, RectTransform parent)
        {
            UiFactory.Label(parent, "SpeciesTitle", "Species", 0f, 0f, 320f, 26f, 22f, UiPalette.Text, UiPalette.LeftMid);
            UiFactory.Label(parent, "SpeciesFilterLabel", "Filter:", 0f, 28f, 60f, 24f, 16f, UiPalette.Dim, UiPalette.LeftMid);
            UiFactory.TextField(parent, "SpeciesFilter", 60f, 28f, 260f, 28f,
                () => _speciesFilter, v => _speciesFilter = v);
            _speciesScroll = UiFactory.Scroll(parent, "SpeciesList", 0f, 60f, 320f, 620f);

            UiFactory.Label(parent, "SetupTitle", "Colour/Pattern setups", 340f, 0f, 1000f, 26f, 22f, UiPalette.Text, UiPalette.LeftMid);
            UiFactory.Label(parent, "SetupFilterLabel", "Filter:", 340f, 28f, 60f, 24f, 16f, UiPalette.Dim, UiPalette.LeftMid);
            UiFactory.TextField(parent, "SetupFilter", 400f, 28f, 600f, 28f,
                () => _setupFilter, v => _setupFilter = v);
            _setupTitle = UiFactory.Label(parent, "SetupTitle2", "", 1000f, 28f, 520f, 24f, 16f, UiPalette.Dim, UiPalette.LeftMid);
            _setupScroll = UiFactory.Scroll(parent, "SetupList", 340f, 60f, 1176f, 620f);

            _lastSpeciesCount = -1;
            _lastSetupSpecies = "";
            _lastSetupCount = -1;
            _lastSpeciesFilter = "";
            _lastSetupFilter = "";
            RebuildSpecies();
            RebuildSetups();
        }

        public static void Tick(StudioState state)
        {
            if (Core.GameCatalog.SpeciesIds.Count != _lastSpeciesCount)
            {
                _speciesIndex = -1;
                _setupIndex = -1;
                RebuildSpecies();
                RebuildSetups();
                return;
            }

            if (_speciesFilter != _lastSpeciesFilter)
            {
                _lastSpeciesFilter = _speciesFilter;
                RebuildSpecies();
                RebuildSetups();
                return;
            }

            if (_setupFilter != _lastSetupFilter)
            {
                _lastSetupFilter = _setupFilter;
                RebuildSetups();
                return;
            }

            string species = SelectedSpecies();
            if (species != _lastSetupSpecies)
            {
                _setupIndex = -1;
                RebuildSetups();
                return;
            }

            if (species != null && Core.GameCatalog.SetupsBySpecies.TryGetValue(species, out var list))
            {
                if (list.Count != _lastSetupCount) RebuildSetups();
            }
        }

        private static string SelectedSpecies()
        {
            if (_speciesIndex < 0 || _speciesIndex >= _filteredSpecies.Count) return null;
            return _filteredSpecies[_speciesIndex];
        }

        private static void RebuildSpecies()
        {
            _lastSpeciesCount = Core.GameCatalog.SpeciesIds.Count;
            _filteredSpecies.Clear();
            string f = _speciesFilter != null ? _speciesFilter.Trim().ToLowerInvariant() : "";
            for (int i = 0; i < Core.GameCatalog.SpeciesIds.Count; i++)
            {
                string id = Core.GameCatalog.SpeciesIds[i];
                if (f.Length == 0 || id.IndexOf(f, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _filteredSpecies.Add(id);
                }
            }

            if (_speciesIndex >= _filteredSpecies.Count) _speciesIndex = -1;

            if (_speciesScroll == null) return;
            GameUI.RemoveScrollButtons(_speciesScroll);
            UiFactory.ClearChildren(_speciesScroll.Content);
            _speciesScroll.ContentHeight = 0f;
            _speciesScroll.Scroll = 0f;

            if (_filteredSpecies.Count == 0)
            {
                UiFactory.Label(_speciesScroll.Content, "SpeciesEmpty", "No species match the filter.",
                    0f, 0f, _speciesScroll.view.width, 40f, 17f, UiPalette.Dim, UiPalette.LeftMid);
                return;
            }

            for (int i = 0; i < _filteredSpecies.Count; i++)
            {
                int idx = i;
                string id = _filteredSpecies[i];
                var b = UiFactory.ScrollButton(_speciesScroll, "Species_" + idx, id, i * 32f, 316f, 30f,
                    () => SelectSpecies(idx));
                b.IsActive = () => _speciesIndex == idx;
                b.Label.fontSize = 19f;
                b.Label.horizontalAlignment = Il2CppTMPro.HorizontalAlignmentOptions.Left;
                b.Label.verticalAlignment = Il2CppTMPro.VerticalAlignmentOptions.Middle;
            }
        }

        private static void SelectSpecies(int index)
        {
            _speciesIndex = index;
            _setupIndex = -1;
            RebuildSetups();
        }

        private static void RebuildSetups()
        {
            string species = SelectedSpecies();
            if (_setupTitle != null)
            {
                _setupTitle.text = species != null ? "Setups for " + species : "Select a species on the left to browse its setups.";
            }

            _lastSetupSpecies = species ?? "";
            if (_setupScroll == null) return;
            GameUI.RemoveScrollButtons(_setupScroll);
            UiFactory.ClearChildren(_setupScroll.Content);
            _setupScroll.ContentHeight = 0f;
            _setupScroll.Scroll = 0f;

            if (species == null || !Core.GameCatalog.SetupsBySpecies.TryGetValue(species, out var setups))
            {
                _lastSetupCount = -1;
                return;
            }
            _lastSetupCount = setups.Count;

            string f = _setupFilter != null ? _setupFilter.Trim().ToLowerInvariant() : "";
            int shown = 0;
            for (int i = 0; i < setups.Count; i++)
            {
                var setup = setups[i];
                if (setup == null) continue;

                var pattern = setup.Pattern;
                var color = setup.Color;
                string pName = pattern != null ? pattern.name : "(null pattern)";
                string cName = color != null ? color.name : "(null color)";
                string rarity = setup.Rarity.ToString();
                string label = "[" + rarity + "]  " + pName + "  /  " + cName;

                if (f.Length > 0)
                {
                    string hay = label.ToLowerInvariant();
                    if (hay.IndexOf(f, System.StringComparison.OrdinalIgnoreCase) < 0) continue;
                }

                string lbl = label;
                int row = shown;
                var b = UiFactory.ScrollButton(_setupScroll, "Setup_" + row, lbl, row * 32f, 1172f, 30f,
                    () => BeginEdit(Main.State, species, setup));
                b.IsActive = () => _setupIndex == row;
                b.Label.fontSize = 18f;
                b.Label.horizontalAlignment = Il2CppTMPro.HorizontalAlignmentOptions.Left;
                b.Label.verticalAlignment = Il2CppTMPro.VerticalAlignmentOptions.Middle;
                shown++;
            }

            if (shown == 0)
            {
                UiFactory.Label(_setupScroll.Content, "SetupsEmpty", "No setups match the filter.",
                    0f, 0f, _setupScroll.view.width, 40f, 17f, UiPalette.Dim, UiPalette.LeftMid);
            }
        }

        private static void BeginEdit(StudioState state, string species, DinoHerdSetup setup)
        {
            if (state == null) return;
            if (state.Working == null) state.Working = new Core.WorkingAssets();
            state.Working.LoadFromSpeciesAndSetup(species, setup);
            state.SetStatus("Loaded base: " + (setup.Pattern != null ? setup.Pattern.name : "") + " + " + (setup.Color != null ? setup.Color.name : ""));
        }
    }
}
