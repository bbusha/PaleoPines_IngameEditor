using UnityEngine;

namespace PaleoPinesDinoStudio.Core
{
    /// <summary>
    /// Shared colour helpers so the live-apply path (ColourApplier) and the preview path
    /// (ViewportTab) don't duplicate the same logic.
    /// </summary>
    public static class ColorUtil
    {
        /// <summary>Reads the six editable channels out of a working set as an array.</summary>
        public static Color[] WorkingColors(WorkingAssets w)
        {
            return new Color[]
            {
                w.BaseColor, w.PatternColor1, w.PatternColor2, w.PatternColor3, w.PatternColor4, w.JournalColor, w.EyeColor
            };
        }

        public static bool ColorsEqual(Color[] a, Color[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i].r != b[i].r || a[i].g != b[i].g || a[i].b != b[i].b) return false;
            }
            return true;
        }

        public static Color[] Snapshot(params Color[] colors) { return (Color[])colors.Clone(); }
    }
}
