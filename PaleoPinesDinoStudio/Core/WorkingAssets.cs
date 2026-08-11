using Il2CppItalicPig.PaleoPines.Dinos;
using UnityEngine;

namespace PaleoPinesDinoStudio.Core
{
    /// <summary>
    /// The colour we are editing. The pattern is always the ORIGINAL game DinoPattern
    /// (reused by reference - the GPU reads its textures directly, so no CPU-readable
    /// copies are needed). Only a new DinoColor with a unique UID is created.
    /// </summary>
    public class WorkingAssets
    {
        public string SpeciesId = "";
        public DinoPattern SourcePattern;
        public DinoColor WorkingColor;
        public DinoPattern WorkingPattern;

        public Color BaseColor = Color.white;
        public Color PatternColor1 = Color.black;
        public Color PatternColor2 = Color.black;
        public Color PatternColor3 = Color.black;
        public Color PatternColor4 = Color.black;
        public Color JournalColor = Color.gray;
        public string ColorUid = "DinoColor-Custom";
        public string PatternUid = "DinoPattern-Custom";
        public string SetupDisplayName = "";
        public DinoRarity Rarity = DinoRarity.Common;

        public bool HasContent { get { return WorkingPattern != null && WorkingColor != null; } }

        public void LoadFromSpeciesAndSetup(string speciesId, DinoHerdSetup sourceSetup)
        {
            SpeciesId = speciesId;
            ColorUid = "DinoColor-" + Guid8();

            var pattern = sourceSetup != null ? sourceSetup.Pattern : null;
            var color = sourceSetup != null ? sourceSetup.Color : null;

            if (color != null)
            {
                BaseColor = color._BaseColor;
                PatternColor1 = color._PatternColor1;
                PatternColor2 = color._PatternColor2;
                PatternColor3 = color._PatternColor3;
                PatternColor4 = color._PatternColor4;
                JournalColor = color._JournalDisplayColor;
            }

            SourcePattern = pattern;
            WorkingPattern = pattern;
            PatternUid = pattern != null ? pattern.name : "DinoPattern-None";

            WorkingColor = AssetFactory.CreateColor(ColorUid, ColorUid, BaseColor,
                PatternColor1, PatternColor2, PatternColor3, PatternColor4, JournalColor);
        }

        public void LoadFromPawn(string speciesId, DinoPattern pattern, Color baseCol,
            Color p1, Color p2, Color p3, Color p4, Color journal)
        {
            SpeciesId = speciesId;
            ColorUid = "DinoColor-" + Guid8();
            BaseColor = baseCol;
            PatternColor1 = p1;
            PatternColor2 = p2;
            PatternColor3 = p3;
            PatternColor4 = p4;
            JournalColor = journal;

            SourcePattern = pattern;
            WorkingPattern = pattern;
            PatternUid = pattern != null ? pattern.name : "DinoPattern-None";
            SetupDisplayName = "From dino";

            WorkingColor = AssetFactory.CreateColor(ColorUid, ColorUid, BaseColor,
                PatternColor1, PatternColor2, PatternColor3, PatternColor4, JournalColor);
        }

        public void SyncWorkingObjects()
        {
            if (WorkingColor == null)
            {
                WorkingColor = AssetFactory.CreateColor(ColorUid, ColorUid, BaseColor,
                    PatternColor1, PatternColor2, PatternColor3, PatternColor4, JournalColor);
            }
            else
            {
                WorkingColor._BaseColor = BaseColor;
                WorkingColor._PatternColor1 = PatternColor1;
                WorkingColor._PatternColor2 = PatternColor2;
                WorkingColor._PatternColor3 = PatternColor3;
                WorkingColor._PatternColor4 = PatternColor4;
                WorkingColor._JournalDisplayColor = JournalColor;
            }
        }

        public static string Guid8()
        {
            return System.Guid.NewGuid().ToString("N").Substring(0, 8);
        }
    }
}
