using Il2CppItalicPig.PaleoPines.Dinos;
using Il2CppItalicPig.PaleoPines.Actors;
using UnityEngine;

namespace PaleoPinesDinoStudio.Core
{
    public static class Injector
    {
        public static bool IsInjected(DinoHerd herd, DinoPattern pattern, DinoColor color)
        {
            if (herd == null) return false;
            var setups = herd.DinoSetups;
            if (setups == null) return false;
            for (int i = 0; i < setups.Count; i++)
            {
                var s = setups[i];
                if (s == null) continue;
                if (s.Pattern == pattern && s.Color == color) return true;
            }
            return false;
        }

        public static bool AddSetupToHerd(DinoHerd herd, DinoHerdSetup setup)
        {
            if (herd == null || setup == null) return false;
            if (IsInjected(herd, setup.Pattern, setup.Color)) return true;

            var setups = herd.DinoSetups;
            if (setups == null)
            {
                MelonLoader.MelonLogger.Error("Herd " + herd.name + " has null _DinoSetups; cannot inject.");
                return false;
            }
            setups.Add(setup);
            return true;
        }

        public static bool InjectIntoHerd(WorkingAssets w)
        {
            if (w == null || w.WorkingPattern == null || w.WorkingColor == null) return false;
            var herd = GameCatalog.FindHerd(w.SpeciesId);
            if (herd == null) return false;

            var prefab = herd.DinoSetups != null && herd.DinoSetups.Count > 0 && herd.DinoSetups[0] != null
                ? herd.DinoSetups[0]._Prefab
                : null;

            var setup = AssetFactory.CreateSetup(prefab, w.WorkingPattern, w.WorkingColor, w.Rarity);
            bool ok = AddSetupToHerd(herd, setup);
            if (ok)
            {
                GameCatalog.Refresh();
            }
            return ok;
        }

        public static int RecolorTamedPawns(WorkingAssets w)
        {
            if (w == null || w.WorkingPattern == null || w.WorkingColor == null) return 0;
            var pawns = Object.FindObjectsOfType<DinoPawn>(true);
            int n = 0;
            for (int i = 0; i < pawns.Length; i++)
            {
                var p = pawns[i];
                if (p == null) continue;
                if (!string.IsNullOrEmpty(w.SpeciesId) && p.DefaultSpeciesID != w.SpeciesId) continue;
                if (!IsTamed(p)) continue;
                if (RecolorPawn(p, w.WorkingPattern, w.WorkingColor)) n++;
            }
            return n;
        }

        public static DinoPawn FindPawnToRecolor(string speciesId, int index)
        {
            var pawns = Object.FindObjectsOfType<DinoPawn>(true);
            int seen = 0;
            for (int i = 0; i < pawns.Length; i++)
            {
                var p = pawns[i];
                if (p == null) continue;
                if (!string.IsNullOrEmpty(speciesId) && p.DefaultSpeciesID != speciesId) continue;
                if (seen == index) return p;
                seen++;
            }
            return null;
        }

        public static int CountPawns(string speciesId)
        {
            var pawns = Object.FindObjectsOfType<DinoPawn>(true);
            int n = 0;
            for (int i = 0; i < pawns.Length; i++)
            {
                var p = pawns[i];
                if (p == null) continue;
                if (!string.IsNullOrEmpty(speciesId) && p.DefaultSpeciesID != speciesId) continue;
                n++;
            }
            return n;
        }

        public static bool IsTamed(DinoPawn pawn)
        {
            if (pawn == null) return false;
            try
            {
                if (DinoPawn.TamedDinoPawns != null && DinoPawn.TamedDinoPawns.Contains(pawn)) return true;
                return pawn._WasPreviouslyTamed;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// The dino's actual name: the player-given name or the one the game generated.
        /// Returns "" for dinos without one (e.g. untamed), so callers can fall back to
        /// the species id. Tamed dinos carry the name on DinoStats (and its SaveData).
        /// </summary>
        public static string DinoDisplayName(DinoPawn pawn)
        {
            if (pawn == null) return "";
            try
            {
                var stats = pawn.DinoStats;
                if (stats != null)
                {
                    string n = stats.DinoName;
                    if (!string.IsNullOrEmpty(n)) return n;
                    try
                    {
                        var sd = stats.SaveData;
                        if (sd != null)
                        {
                            string n2 = sd.Name;
                            if (!string.IsNullOrEmpty(n2)) return n2;
                        }
                    }
                    catch { }
                }
            }
            catch { }
            return "";
        }

        public static bool RecolorPawn(DinoPawn pawn, DinoPattern pattern, DinoColor color)
        {
            if (pawn == null) return false;
            try
            {
                ColourApplier.Apply(pawn, pattern, color);
                return true;
            }
            catch (System.Exception e)
            {
                MelonLoader.MelonLogger.Error("RecolorPawn failed: " + e);
                return false;
            }
        }
    }
}
