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

        public static void Build(StudioState state, RectTransform parent)
        {
            UiFactory.Label(parent, "SpeciesTitle", "Species", 0f, 0f, 320f, 26f, 22f, UiPalette.Text, UiPalette.LeftMid);
            _speciesScroll = UiFactory.Scroll(parent, "SpeciesList", 0f, 30f, 320f, 650f);

            UiFactory.Label(parent, "SetupTitle", "Colour/Pattern setups", 340f, 0f, 1000f, 26f, 22f, UiPalette.Text, UiPalette.LeftMid);
            _setupTitle = UiFactory.Label(parent, "SetupTitle2", "", 340f, 0f, 1000f, 26f, 22f, UiPalette.Dim, UiPalette.LeftMid);
            _setupScroll = UiFactory.Scroll(parent, "SetupList", 340f, 30f, 1176f, 650f);

            _lastSpeciesCount = -1;
            _lastSetupSpecies = "";
            _lastSetupCount = -1;
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
            if (_speciesIndex < 0 || _speciesIndex >= Core.GameCatalog.SpeciesIds.Count) return null;
            return Core.GameCatalog.SpeciesIds[_speciesIndex];
        }

        private static void RebuildSpecies()
        {
            _lastSpeciesCount = Core.GameCatalog.SpeciesIds.Count;
            if (_speciesScroll == null) return;
            GameUI.RemoveScrollButtons(_speciesScroll);
            UiFactory.ClearChildren(_speciesScroll.Content);
            _speciesScroll.ContentHeight = 0f;
            _speciesScroll.Scroll = 0f;

            for (int i = 0; i < Core.GameCatalog.SpeciesIds.Count; i++)
            {
                int idx = i;
                string id = Core.GameCatalog.SpeciesIds[i];
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
                _setupTitle.text = species != null ? "Colour/Pattern setups for " + species : "Select a species on the left to browse its setups.";
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

                int idx = i;
                var b = UiFactory.ScrollButton(_setupScroll, "Setup_" + idx, label, i * 32f, 1172f, 30f,
                    () => BeginEdit(Main.State, species, setup));
                b.IsActive = () => _setupIndex == idx;
                b.Label.fontSize = 18f;
                b.Label.horizontalAlignment = Il2CppTMPro.HorizontalAlignmentOptions.Left;
                b.Label.verticalAlignment = Il2CppTMPro.VerticalAlignmentOptions.Middle;
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
