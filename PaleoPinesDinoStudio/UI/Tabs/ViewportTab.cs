using UnityEngine;

namespace PaleoPinesDinoStudio.UI.Tabs
{
    public static class ViewportTab
    {
        public const int PreviewWidth = 680;
        public const int PreviewHeight = 680;

        public static Preview.Viewport3D Viewport;

        private static float _lastAutoApply;
        private static bool _builtWithContent;
        private static Color[] _lastColors;

        public static void Build(StudioState state, RectTransform parent)
        {
            var w = state.Working;
            bool has = w != null && w.HasContent;
            _builtWithContent = has;
            _lastColors = null;

            if (!has)
            {
                UiFactory.Label(parent, "NoContent", "Pick a species + setup in the Catalog tab first.",
                    0f, 50f, 900f, 40f, 24f, UiPalette.Warn, UiPalette.LeftMid);
                return;
            }

            EnsureViewport(w.SpeciesId);

            UiFactory.Label(parent, "ViewTitle", "Preview - your colours on the real dino model. Drag to orbit, scroll to zoom.",
                0f, 0f, 1200f, 26f, 22f, UiPalette.Text, UiPalette.LeftMid);

            if (Viewport != null && Viewport.IsReady && Viewport.Output != null)
            {
                UiFactory.Raw(parent, "ViewportImage", 0f, 40f, PreviewWidth, PreviewHeight, Viewport.Output);

                // Drag on the preview to orbit the model; pause auto-rotate while doing so.
                GameUI.Drags.Add(new UiDrag
                {
                    r = new Rect(0f, 40f, PreviewWidth, PreviewHeight),
                    Origin = () => GameUI.ContentOrigin(),
                    OnDragDelta = d => { if (Viewport != null) Viewport.DragOrbit(d); }
                });

                UiFactory.Toggle(parent, "AutoRotate", "Auto rotate", 0f, 40f + PreviewHeight + 8f, 150f, 26f,
                    () => Viewport != null && Viewport.AutoRotate,
                    () => { if (Viewport != null) Viewport.SetAutoRotate(!Viewport.AutoRotate); });
                UiFactory.Label(parent, "ViewHint2",
                    "Drag the preview to orbit. Scroll to zoom. Auto-rotate resumes after a short pause.",
                    160f, 40f + PreviewHeight + 8f, 500f, 26f, 16f, UiPalette.Dim, UiPalette.LeftMid);
            }

            UiFactory.Label(parent, "ViewHint",
                "How it works:\n" +
                "1. Dino tab: pick a dino in the area to copy its look, OR Catalog: pick a species and setup.\n" +
                "2. Color: change Base Colour and Pattern Colours 1-4 (markings keep the setup's pattern).\n" +
                "3. Apply: adds your colour as a NEW wild variant of the species and recolours tamed dinos in the area.\n" +
                "Nothing existing is ever replaced. The loaded dino updates in real time.",
                720f, 40f, 700f, 220f, 18f, UiPalette.Text, UiPalette.LeftMid);
        }

        private static void EnsureViewport(string speciesId)
        {
            if (Viewport != null && !Viewport.IsAlive) Viewport = null;
            if (Viewport == null) Viewport = new Preview.Viewport3D();
            if (!Viewport.IsReady || Viewport.SpeciesId != speciesId)
            {
                Viewport.CreateFor(speciesId);
            }
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

            EnsureViewport(w.SpeciesId);

            // Scroll wheel over the preview zooms the camera.
            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.001f && Viewport != null && Viewport.IsReady)
            {
                Vector2 mp = UiFactory.DesignPoint(Input.mousePosition);
                Rect view = new Rect(0f, 40f, PreviewWidth, PreviewHeight);
                Vector2 origin = GameUI.ContentOrigin();
                view.x += origin.x;
                view.y += origin.y;
                if (UiFactory.InRect(view, mp))
                {
                    Viewport.Zoom(wheel);
                }
            }

            if (Time.unscaledTime - _lastAutoApply > 0.3f)
            {
                _lastAutoApply = Time.unscaledTime;
                ApplyColours(w);
            }
        }

        /// <summary>
        /// Runs every frame while the editor is open (regardless of the active tab) so the
        /// preview follows the working colours/species in real time, e.g. after the Dino tab
        /// loads a different species as the base.
        /// </summary>
        public static void Maintain(StudioState state)
        {
            var w = state.Working;
            if (w == null || !w.HasContent) { _lastColors = null; return; }

            EnsureViewport(w.SpeciesId);

            if (Time.unscaledTime - _lastAutoApply > 0.3f)
            {
                _lastAutoApply = Time.unscaledTime;
                ApplyColours(w);
            }
        }

        private static void ApplyColours(Core.WorkingAssets w)
        {
            Color[] cur = Core.ColorUtil.WorkingColors(w);
            if (Core.ColorUtil.ColorsEqual(_lastColors, cur)) return;
            _lastColors = cur;
            if (Viewport != null && Viewport.IsReady)
            {
                Viewport.ApplyColours(w);
            }
        }
    }
}
