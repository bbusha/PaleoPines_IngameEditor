using System.Collections.Generic;
using Il2CppItalicPig.PaleoPines.Dinos;

namespace PaleoPinesDinoStudio.Core
{
    public static class GameCatalog
    {
        public static List<string> SpeciesIds = new List<string>();
        public static List<string> PatternNames = new List<string>();
        public static List<string> ColorNames = new List<string>();
        public static bool Loaded;
        public static bool LoadFailed;

        public static readonly System.Collections.Generic.Dictionary<string, DinoHerd> HerdsBySpecies =
            new System.Collections.Generic.Dictionary<string, DinoHerd>();

        public static readonly System.Collections.Generic.Dictionary<string, DinoPattern> PatternsByName =
            new System.Collections.Generic.Dictionary<string, DinoPattern>();

        public static readonly System.Collections.Generic.Dictionary<string, DinoColor> ColorsByName =
            new System.Collections.Generic.Dictionary<string, DinoColor>();

        public static readonly System.Collections.Generic.Dictionary<string, List<DinoHerdSetup>> SetupsBySpecies =
            new System.Collections.Generic.Dictionary<string, List<DinoHerdSetup>>();

        public static void Refresh()
        {
            HerdsBySpecies.Clear();
            PatternsByName.Clear();
            ColorsByName.Clear();
            SetupsBySpecies.Clear();
            SpeciesIds.Clear();
            PatternNames.Clear();
            ColorNames.Clear();

            try
            {
                var allHerds = DinoHerd.AllHerds;
                if (allHerds == null)
                {
                    LoadFailed = true;
                    Loaded = false;
                    return;
                }

                for (int h = 0; h < allHerds.Count; h++)
                {
                    DinoHerd herd = allHerds[h];
                    if (herd == null) continue;

                    string speciesId = herd.SpeciesId;
                    if (string.IsNullOrEmpty(speciesId)) continue;
                    if (HerdsBySpecies.ContainsKey(speciesId)) continue;

                    HerdsBySpecies[speciesId] = herd;
                    SpeciesIds.Add(speciesId);

                    var setups = herd.DinoSetups;
                    var list = new List<DinoHerdSetup>();
                    if (setups != null)
                    {
                        for (int s = 0; s < setups.Count; s++)
                        {
                            var setup = setups[s];
                            if (setup == null) continue;
                            list.Add(setup);

                            var pattern = setup.Pattern;
                            if (pattern != null && !PatternsByName.ContainsKey(pattern.name))
                            {
                                PatternsByName[pattern.name] = pattern;
                                PatternNames.Add(pattern.name);
                            }

                            var color = setup.Color;
                            if (color != null && !ColorsByName.ContainsKey(color.name))
                            {
                                ColorsByName[color.name] = color;
                                ColorNames.Add(color.name);
                            }
                        }
                    }
                    SetupsBySpecies[speciesId] = list;
                }

                SpeciesIds.Sort();
                PatternNames.Sort();
                ColorNames.Sort();
                Loaded = true;
                LoadFailed = false;
            }
            catch (System.Exception e)
            {
                LoadFailed = true;
                MelonLoader.MelonLogger.Error("GameCatalog.Refresh failed: " + e);
            }
        }

        public static DinoHerd FindHerd(string speciesId)
        {
            if (string.IsNullOrEmpty(speciesId)) return null;
            if (HerdsBySpecies.TryGetValue(speciesId, out var herd)) return herd;
            return null;
        }

        public static DinoHerdSetup FindSetup(string speciesId, string patternName, string colorName)
        {
            if (SetupsBySpecies.TryGetValue(speciesId, out var list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var s = list[i];
                    if (s == null) continue;
                    var p = s.Pattern;
                    var c = s.Color;
                    if (p == null || c == null) continue;
                    if (p.name == patternName && c.name == colorName) return s;
                }
            }
            return null;
        }

        public static System.Collections.Generic.List<Il2CppItalicPig.PaleoPines.Actors.DinoPawn> FindPawnsOfSpecies(string speciesId)
        {
            var result = new System.Collections.Generic.List<Il2CppItalicPig.PaleoPines.Actors.DinoPawn>();
            try
            {
                var pawns = UnityEngine.Object.FindObjectsOfType<Il2CppItalicPig.PaleoPines.Actors.DinoPawn>(true);
                for (int i = 0; i < pawns.Length; i++)
                {
                    var p = pawns[i];
                    if (p == null) continue;
                    if (string.IsNullOrEmpty(speciesId) || p.DefaultSpeciesID == speciesId)
                    {
                        result.Add(p);
                    }
                }
            }
            catch (System.Exception e)
            {
                MelonLoader.MelonLogger.Error("FindPawnsOfSpecies failed: " + e);
            }
            return result;
        }
    }
}
