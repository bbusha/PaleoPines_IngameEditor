using Il2CppItalicPig.PaleoPines.Dinos;
using Il2CppItalicPig.PaleoPines.Actors;
using UnityEngine;

namespace PaleoPinesDinoStudio.Core
{
    public static class AssetFactory
    {
        public static DinoColor CreateColor(string uid, string displayName, Color baseColor,
            Color p1, Color p2, Color p3, Color p4, Color journalColor)
        {
            var color = ScriptableObject.CreateInstance<DinoColor>();
            color.name = uid;
            color._StringUID = uid;
            color._BaseColor = baseColor;
            color._PatternColor1 = p1;
            color._PatternColor2 = p2;
            color._PatternColor3 = p3;
            color._PatternColor4 = p4;
            color._JournalDisplayColor = journalColor;
            color._BaseOverrideTexture = null;
            color._EyeOverrideTexture = null;
            return color;
        }

        public static DinoHerdSetup CreateSetup(DinoPawn prefab, DinoPattern pattern, DinoColor color, DinoRarity rarity)
        {
            var setup = new DinoHerdSetup(prefab, pattern, color, rarity);
            setup._Name = (pattern != null ? pattern.name : "pattern")
                + " " + (color != null ? color.name : "color");
            setup._RarityModifiers = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<DinoRarityModifier>(0);
            return setup;
        }

        public static void EnsureAssets(WorkingAssets w)
        {
            if (w == null) return;

            if (w.WorkingColor == null)
            {
                w.WorkingColor = CreateColor(w.ColorUid, w.ColorUid, w.BaseColor,
                    w.PatternColor1, w.PatternColor2, w.PatternColor3, w.PatternColor4, w.JournalColor);
            }

            // WorkingPattern is intentionally the ORIGINAL game pattern. Never replace it:
            // the game's shaders read its textures on the GPU, so we must not copy them.
            if (w.WorkingPattern == null && w.SourcePattern != null)
            {
                w.WorkingPattern = w.SourcePattern;
            }
        }
    }
}
