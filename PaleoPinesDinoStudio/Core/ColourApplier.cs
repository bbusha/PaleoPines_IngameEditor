using System.Collections.Generic;
using Il2CppInterop.Runtime;
using Il2CppItalicPig.PaleoPines.Actors;
using Il2CppItalicPig.PaleoPines.Dinos;
using UnityEngine;
using UnityEngine.Rendering;

namespace PaleoPinesDinoStudio.Core
{
    /// <summary>
    /// Applies DinoPattern + DinoColor to a DinoPawn using the game's own material pipeline
    /// (DinoHelpers.SetupDinoMaterials) plus a direct fallback that writes the shader colour
    /// properties on the renderer materials. Also handles runtime diagnostics and capturing a
    /// nearby dino's current look to use as an editing base.
    /// </summary>
    public static class ColourApplier
    {
        private static bool _dumped;
        private static float _lastLiveApply;
        private static Color[] _lastLiveColors;

        /// <param name="setColourData">
        /// Also call the pawn's SetColourData. The viewport pawn is instantiated raw and has no
        /// gameplay context, so SetColourData throws on it - the preview only needs the material
        /// path, so we skip it there.
        /// </param>
        /// <param name="eyeColor">
        /// Optional eye tint written to the eye renderers. The game keeps eyes texture-driven,
        /// so this only takes effect when the eye shader exposes a colour property.
        /// </param>
        public static void Apply(DinoPawn pawn, DinoPattern pattern, DinoColor color, bool setColourData = true, Color? eyeColor = null)
        {
            if (pawn == null || pattern == null || color == null) return;

            try
            {
                DinoHelpers.SetupDinoMaterials(pawn.bodyRenderers, pawn.eyelidRenderers, pawn.eyeRenderers, pattern, color);
            }
            catch (System.Exception e) { MelonLoader.MelonLogger.Error("SetupDinoMaterials failed: " + e); }

            try { ApplyBodyColoursDirect(pawn, pattern, color, eyeColor); }
            catch (System.Exception e) { MelonLoader.MelonLogger.Error("ApplyBodyColoursDirect failed: " + e); }

            if (setColourData)
            {
                try { pawn.SetColourData(pattern, color); }
                catch (System.Exception e) { MelonLoader.MelonLogger.Error("SetColourData failed: " + e); }
            }
        }

        /// <summary>
        /// Writes the base/pattern shader colours directly onto the body renderer materials
        /// (per-dino material instances, so other dinos are unaffected). Property names match
        /// the game's "Shader Graphs/Dino" shader: base tint is _Color, pattern fills are
        /// _PatternColor1..4, textures are _BaseMap and _PatternMask. Common aliases are also
        /// handled in case another model uses them.
        /// </summary>
        public static void ApplyBodyColoursDirect(DinoPawn pawn, DinoPattern pattern, DinoColor color, Color? eyeColor = null)
        {
            if (pawn == null) return;
            Texture2D maskTex = pattern != null ? pattern._PatternMask : null;

            var renderers = pawn.bodyRenderers;
            if (renderers != null)
            {
                for (int i = 0; i < renderers.Count; i++)
                {
                    var r = renderers[i];
                    if (r == null) continue;
                    var m = r.material;
                    if (m == null) continue;

                    SetColorIf(m, "_Color", color._BaseColor);
                    SetColorIf(m, "_BaseColor", color._BaseColor);
                    SetColorIf(m, "_PatternColor1", color._PatternColor1);
                    SetColorIf(m, "_PatternColor2", color._PatternColor2);
                    SetColorIf(m, "_PatternColor3", color._PatternColor3);
                    SetColorIf(m, "_PatternColor4", color._PatternColor4);
                    if (maskTex != null)
                    {
                        SetTextureIf(m, "_PatternMask", maskTex);
                        SetTextureIf(m, "_MaskTexture", maskTex);
                    }
                }
            }

            // Fallback: any renderer that still exposes the pattern colours (e.g. unparented
            // extras) gets the same values.
            var children = pawn.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < children.Length; i++)
            {
                var r = children[i];
                if (r == null) continue;
                var m = r.material;
                if (m == null) continue;
                if (!m.HasProperty("_PatternColor1")) continue;

                SetColorIf(m, "_Color", color._BaseColor);
                SetColorIf(m, "_BaseColor", color._BaseColor);
                SetColorIf(m, "_PatternColor1", color._PatternColor1);
                SetColorIf(m, "_PatternColor2", color._PatternColor2);
                SetColorIf(m, "_PatternColor3", color._PatternColor3);
                SetColorIf(m, "_PatternColor4", color._PatternColor4);
            }

            if (eyeColor.HasValue) ApplyEyeColour(pawn, eyeColor.Value);
        }

        /// <summary>
        /// Writes the eye tint onto the eye renderer materials. The game bakes eye colour into
        /// textures, so this is best-effort: it only affects shaders that expose a colour
        /// property (common candidates are tried and guarded with HasProperty).
        /// </summary>
        public static void ApplyEyeColour(DinoPawn pawn, Color eyeColor)
        {
            if (pawn == null) return;

            void SetOn(Material m)
            {
                if (m == null) return;
                SetColorIf(m, "_EyeColor", eyeColor);
                SetColorIf(m, "_EyeTint", eyeColor);
                SetColorIf(m, "_EyeColour", eyeColor);
                SetColorIf(m, "_EmissionColor", eyeColor);
                SetColorIf(m, "_Color", eyeColor);
            }

            var eyes = pawn.eyeRenderers;
            if (eyes != null)
            {
                for (int i = 0; i < eyes.Count; i++)
                {
                    if (eyes[i] == null) continue;
                    SetOn(eyes[i].material);
                }
            }

            var children = pawn.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < children.Length; i++)
            {
                var r = children[i];
                if (r == null) continue;
                var m = r.material;
                if (m == null) continue;
                if (m.HasProperty("_EyeColor") || m.HasProperty("_EyeTint") || m.HasProperty("_EyeColour"))
                {
                    SetOn(m);
                }
            }
        }

        private static void SetColorIf(Material m, string prop, Color c)
        {
            try { if (m.HasProperty(prop)) m.SetColor(prop, c); }
            catch { }
        }

        private static void SetTextureIf(Material m, string prop, Texture2D t)
        {
            try { if (m.HasProperty(prop)) m.SetTexture(prop, t); }
            catch { }
        }

        // ---- Real-time editing: apply the working colours to the loaded live dino ----

        public static void TickLive(StudioState state)
        {
            if (state == null || state.LivePawn == null) { _lastLiveColors = null; return; }
            var w = state.Working;
            if (w == null || !w.HasContent) { _lastLiveColors = null; return; }

            if (!IsPawnAlive(state.LivePawn)) { state.LivePawn = null; _lastLiveColors = null; return; }

            if (Time.unscaledTime - _lastLiveApply < 0.12f) return;
            _lastLiveApply = Time.unscaledTime;

            Color[] cur = ColorUtil.WorkingColors(w);
            if (ColorUtil.ColorsEqual(_lastLiveColors, cur)) return;
            _lastLiveColors = cur;

            Apply(state.LivePawn, w.WorkingPattern, w.WorkingColor, setColourData: true, eyeColor: w.EyeColor);
        }

        private static bool IsPawnAlive(DinoPawn pawn)
        {
            try
            {
                if (pawn == null || pawn.gameObject == null) return false;
                return pawn.gameObject.activeInHierarchy || pawn.gameObject.activeSelf;
            }
            catch { return false; }
        }

        // ---- Diagnostics: dump a dino's actual material properties once ----

        public static void DumpMaterialInfo(DinoPawn pawn, string tag)
        {
            if (_dumped || pawn == null) return;
            _dumped = true;
            try
            {
                Log("=== DinoStudio material dump (" + tag + ") species=" + pawn.DefaultSpeciesID + " ===");
                DumpList("body", pawn.bodyRenderers);
                DumpList("eyelid", pawn.eyelidRenderers);
                DumpList("eye", pawn.eyeRenderers);
                Log("=== end dump ===");
            }
            catch (System.Exception e)
            {
                Log("dump error: " + e);
            }
        }

        private static void DumpList(string label, Il2CppSystem.Collections.Generic.List<Renderer> list)
        {
            if (list == null) { Log(label + ": null list"); return; }
            Log(label + " renderers: " + list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                var r = list[i];
                if (r == null) continue;
                var m = r.sharedMaterial;
                if (m == null) { Log("  [" + i + "] " + r.gameObject.name + " (no material)"); continue; }
                var sh = m.shader;
                Log("  [" + i + "] " + r.gameObject.name + " shader=" + (sh != null ? sh.name : "null"));
                if (sh == null) continue;
                int n = sh.GetPropertyCount();
                for (int p = 0; p < n; p++)
                {
                    string pn = sh.GetPropertyName(p);
                    var pt = sh.GetPropertyType(p);
                    string val = "?";
                    try
                    {
                        if (pt == ShaderPropertyType.Color) val = m.GetColor(pn).ToString();
                        else if (pt == ShaderPropertyType.Texture) { var t = m.GetTexture(pn); val = t != null ? t.name : "null"; }
                        else if (pt == ShaderPropertyType.Float || pt == ShaderPropertyType.Range) val = m.GetFloat(pn).ToString();
                        else if (pt == ShaderPropertyType.Vector) val = m.GetVector(pn).ToString();
                    }
                    catch { val = "(read fail)"; }
                    Log("      " + pn + " = " + val);
                }
            }
        }

        private static void Log(string msg) { MelonLoader.MelonLogger.Msg(msg); }

        // ---- Load a nearby dino's current look as the editing base ----

        public static bool LoadPawnAsBase(StudioState state, DinoPawn pawn)
        {
            if (state == null || pawn == null) return false;
            try
            {
                string speciesId = pawn.DefaultSpeciesID;
                if (string.IsNullOrEmpty(speciesId))
                {
                    state.SetStatus("That dino has no species id.");
                    return false;
                }

                var setups = new List<DinoHerdSetup>();
                if (GameCatalog.SetupsBySpecies.TryGetValue(speciesId, out var list)) setups = list;

                // Read the pawn's current rendered look from its body material.
                // The game's colour variants are written to per-renderer material
                // instances (renderer.material); sharedMaterial is the untouched
                // base-skin asset, which is exactly what was making selected dinos
                // snap back to their base colours.
                Renderer r0 = pawn.bodyRenderers != null && pawn.bodyRenderers.Count > 0 ? pawn.bodyRenderers[0] : null;
                Material m = null;
                if (r0 != null)
                {
                    try { m = r0.material; } catch { }
                    if (m == null) { try { m = r0.sharedMaterial; } catch { } }
                }

                Color baseCol = Color.white, p1 = Color.black, p2 = Color.black, p3 = Color.black, p4 = Color.black, journal = Color.gray;
                // Base tint lives in _Color on the game's Dino shader (aliased _BaseColor elsewhere).
                bool haveBase = TryColor(m, "_Color", out baseCol);
                if (!haveBase) haveBase = TryColor(m, "_BaseColor", out baseCol);
                bool h1 = TryColor(m, "_PatternColor1", out p1);
                bool h2 = TryColor(m, "_PatternColor2", out p2);
                bool h3 = TryColor(m, "_PatternColor3", out p3);
                bool h4 = TryColor(m, "_PatternColor4", out p4);

                Texture2D maskTex = null;
                if (!TryTexture(m, "_PatternMask", out maskTex)) TryTexture(m, "_MaskTexture", out maskTex);

                DinoPattern pattern = MatchPattern(setups, maskTex);
                DinoColor fallbackColor = null;
                if (pattern == null && setups.Count > 0 && setups[0] != null)
                {
                    pattern = setups[0].Pattern;
                    fallbackColor = setups[0].Color;
                }
                else if (pattern != null)
                {
                    fallbackColor = ColorForPattern(setups, pattern);
                }

                if (pattern == null)
                {
                    state.SetStatus("No pattern found for species " + speciesId + ".");
                    return false;
                }

                // Read the eye tint from the eye renderer materials (best-effort: only when
                // the eye shader exposes a colour property). Falls back to the pattern's
                // eyelid colour so the channel still starts at a sensible value.
                Color eyeCol = pattern._EyelidDefaultColour;
                var eyeR = pawn.eyeRenderers;
                if (eyeR != null && eyeR.Count > 0)
                {
                    Material em = null;
                    try { em = eyeR[0].material; } catch { }
                    if (em == null) { try { em = eyeR[0].sharedMaterial; } catch { } }
                    Color tmp;
                    if (TryColor(em, "_EyeColor", out tmp) || TryColor(em, "_EyeTint", out tmp)
                        || TryColor(em, "_EyeColour", out tmp) || TryColor(em, "_EmissionColor", out tmp))
                    {
                        eyeCol = tmp;
                    }
                }

                // A white _Color means "use the baked base texture as-is", so for a useful
                // editing base we prefer the game's real colour for this pattern instead.
                if (!haveBase || (baseCol.r > 0.99f && baseCol.g > 0.99f && baseCol.b > 0.99f))
                {
                    baseCol = fallbackColor != null ? fallbackColor._BaseColor : baseCol;
                }
                if (!h1) p1 = fallbackColor != null ? fallbackColor._PatternColor1 : p1;
                if (!h2) p2 = fallbackColor != null ? fallbackColor._PatternColor2 : p2;
                if (!h3) p3 = fallbackColor != null ? fallbackColor._PatternColor3 : p3;
                if (!h4) p4 = fallbackColor != null ? fallbackColor._PatternColor4 : p4;
                journal = fallbackColor != null ? fallbackColor._JournalDisplayColor : journal;

                var w = new WorkingAssets();
                w.LoadFromPawn(speciesId, pattern, baseCol, p1, p2, p3, p4, journal, eyeCol);
                state.Working = w;
                state.LivePawn = pawn;

                DumpMaterialInfo(pawn, "load");
                Apply(pawn, pattern, w.WorkingColor, setColourData: true, eyeColor: eyeCol);

                string uid = "?";
                try { uid = pawn.Uid; } catch { }
                string who = speciesId;
                string dn = Injector.DinoDisplayName(pawn);
                if (!string.IsNullOrEmpty(dn)) who = dn + " (" + speciesId + ")";
                state.SetStatus("Loaded " + who + " (uid " + uid + ") as base - colours now update live.");
                return true;
            }
            catch (System.Exception e)
            {
                MelonLoader.MelonLogger.Error("LoadPawnAsBase failed: " + e);
                state.SetStatus("Could not load dino as base.");
                return false;
            }
        }

        private static DinoColor ColorForPattern(List<DinoHerdSetup> setups, DinoPattern pattern)
        {
            if (setups == null || pattern == null) return null;
            for (int i = 0; i < setups.Count; i++)
            {
                var s = setups[i];
                if (s != null && s.Pattern == pattern && s.Color != null) return s.Color;
            }
            return setups.Count > 0 ? setups[0].Color : null;
        }

        private static DinoPattern MatchPattern(List<DinoHerdSetup> setups, Texture2D maskTex)
        {
            if (setups == null || maskTex == null) return null;
            for (int i = 0; i < setups.Count; i++)
            {
                var s = setups[i];
                if (s == null || s.Pattern == null) continue;
                if (s.Pattern._PatternMask == maskTex) return s.Pattern;
            }
            return null;
        }

        private static bool TryColor(Material m, string name, out Color c)
        {
            c = Color.white;
            if (m == null || !m.HasProperty(name)) return false;
            try { c = m.GetColor(name); return true; }
            catch { return false; }
        }

        private static bool TryTexture(Material m, string name, out Texture2D t)
        {
            t = null;
            if (m == null || !m.HasProperty(name)) return false;
            try
            {
                var tex = m.GetTexture(name);
                t = tex != null ? tex.TryCast<Texture2D>() : null;
                return t != null;
            }
            catch { t = null; return false; }
        }
    }
}
