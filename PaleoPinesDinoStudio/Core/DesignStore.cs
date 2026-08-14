using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Il2CppItalicPig.PaleoPines.Dinos;
using UnityEngine;

namespace PaleoPinesDinoStudio.Core
{
    /// <summary>
    /// A design the player saved to the library. It stores the editable colours plus the
    /// identity data needed to re-create the runtime DinoColor and re-inject it later.
    /// </summary>
    [System.Serializable]
    public class SavedDesign
    {
        public string speciesId = "";
        public string displayName = "";
        public string colorUid = "";
        public string patternUid = "";
        public int rarity = 0;
        public Color baseColor = Color.white;
        public Color p1 = Color.black;
        public Color p2 = Color.black;
        public Color p3 = Color.black;
        public Color p4 = Color.black;
        public Color journalColor = Color.gray;
        public Color eyeColor = Color.white;
        public string created = "";
    }

    [System.Serializable]
    public class DesignList
    {
        public List<SavedDesign> designs = new List<SavedDesign>();
    }

    /// <summary>
    /// Persists custom designs as JSON under the mod's UserData folder so they survive
    /// restarts and can be re-injected after the game (re)loads the world.
    /// </summary>
    public static class DesignStore
    {
        public static readonly List<SavedDesign> Designs = new List<SavedDesign>();
        private static string _filePath;
        private static bool _loaded;

        public static string FilePath
        {
            get
            {
                if (_filePath == null)
                {
                    try
                    {
                        _filePath = System.IO.Path.Combine(
                            MelonLoader.Utils.MelonEnvironment.UserDataDirectory,
                            "PaleoPinesDinoStudio", "designs.json");
                    }
                    catch
                    {
                        _filePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "PaleoPinesDinoStudio_designs.json");
                    }
                }
                return _filePath;
            }
        }

        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true
        };

        public static void Load()
        {
            if (_loaded) return;
            _loaded = true;
            try
            {
                if (!System.IO.File.Exists(FilePath)) return;
                string json = System.IO.File.ReadAllText(FilePath);
                var list = JsonSerializer.Deserialize<DesignList>(json, JsonOpts);
                Designs.Clear();
                if (list != null && list.designs != null)
                {
                    for (int i = 0; i < list.designs.Count; i++)
                    {
                        var d = list.designs[i];
                        if (d != null && !string.IsNullOrEmpty(d.speciesId)) Designs.Add(d);
                    }
                }
                MelonLoader.MelonLogger.Msg("Loaded " + Designs.Count + " saved design(s) from " + FilePath);
            }
            catch (System.Exception e)
            {
                MelonLoader.MelonLogger.Error("DesignStore.Load failed: " + e);
            }
        }

        public static void Save()
        {
            try
            {
                var dir = System.IO.Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrEmpty(dir)) System.IO.Directory.CreateDirectory(dir);
                var list = new DesignList();
                list.designs = Designs;
                System.IO.File.WriteAllText(FilePath, JsonSerializer.Serialize(list, JsonOpts));
            }
            catch (System.Exception e)
            {
                MelonLoader.MelonLogger.Error("DesignStore.Save failed: " + e);
            }
        }

        /// <summary>Adds (or replaces, keyed by colour UID) the current working look as a saved design.</summary>
        public static SavedDesign SaveCurrent(WorkingAssets w)
        {
            if (w == null || string.IsNullOrEmpty(w.SpeciesId)) return null;
            var d = new SavedDesign
            {
                speciesId = w.SpeciesId,
                displayName = string.IsNullOrEmpty(w.SetupDisplayName) ? ("My " + w.SpeciesId) : w.SetupDisplayName,
                colorUid = string.IsNullOrEmpty(w.ColorUid) ? ("DinoColor-" + WorkingAssets.Guid8()) : w.ColorUid,
                patternUid = w.PatternUid,
                rarity = (int)w.Rarity,
                baseColor = w.BaseColor,
                p1 = w.PatternColor1,
                p2 = w.PatternColor2,
                p3 = w.PatternColor3,
                p4 = w.PatternColor4,
                journalColor = w.JournalColor,
                eyeColor = w.EyeColor,
                created = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm")
            };
            for (int i = 0; i < Designs.Count; i++)
            {
                if (Designs[i].colorUid == d.colorUid)
                {
                    Designs[i] = d;
                    Save();
                    return d;
                }
            }
            Designs.Add(d);
            Save();
            return d;
        }

        public static bool Remove(string colorUid)
        {
            for (int i = 0; i < Designs.Count; i++)
            {
                if (Designs[i].colorUid == colorUid)
                {
                    Designs.RemoveAt(i);
                    Save();
                    return true;
                }
            }
            return false;
        }

        /// <summary>Re-creates a working set from a saved design, resolving the pattern from the live catalog.</summary>
        public static WorkingAssets Restore(SavedDesign d)
        {
            if (d == null) return null;
            DinoPattern pattern = null;
            if (!string.IsNullOrEmpty(d.patternUid))
            {
                GameCatalog.PatternsByName.TryGetValue(d.patternUid, out pattern);
            }
            if (pattern == null)
            {
                if (GameCatalog.SetupsBySpecies.TryGetValue(d.speciesId, out var setups) && setups.Count > 0)
                {
                    pattern = setups[0].Pattern;
                }
            }

            var w = new WorkingAssets();
            w.SpeciesId = d.speciesId;
            w.SetupDisplayName = d.displayName;
            w.ColorUid = string.IsNullOrEmpty(d.colorUid) ? ("DinoColor-" + WorkingAssets.Guid8()) : d.colorUid;
            w.PatternUid = pattern != null ? pattern.name : d.patternUid;
            w.Rarity = (DinoRarity)d.rarity;
            w.BaseColor = d.baseColor;
            w.PatternColor1 = d.p1;
            w.PatternColor2 = d.p2;
            w.PatternColor3 = d.p3;
            w.PatternColor4 = d.p4;
            w.JournalColor = d.journalColor;
            w.EyeColor = d.eyeColor;
            w.SourcePattern = pattern;
            w.WorkingPattern = pattern;
            w.WorkingColor = AssetFactory.CreateColor(w.ColorUid, w.ColorUid, w.BaseColor,
                w.PatternColor1, w.PatternColor2, w.PatternColor3, w.PatternColor4, w.JournalColor);
            return w;
        }

        /// <summary>Re-injects every saved design into its herd after a world load (deduped by UID).</summary>
        public static void ReinjectAll()
        {
            Load();
            if (Designs.Count == 0) return;
            if (!GameCatalog.Loaded) return;
            int injected = 0;
            for (int i = 0; i < Designs.Count; i++)
            {
                var w = Restore(Designs[i]);
                if (w == null || w.WorkingPattern == null) continue;
                if (Injector.InjectIntoHerd(w, refresh: false)) injected++;
            }
            if (injected > 0)
            {
                GameCatalog.Refresh();
                MelonLoader.MelonLogger.Msg("Re-injected " + injected + " saved design(s) into herds.");
            }
        }
    }
}
