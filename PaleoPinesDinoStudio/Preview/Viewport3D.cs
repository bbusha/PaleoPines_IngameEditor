using UnityEngine;

namespace PaleoPinesDinoStudio.Preview
{
    /// <summary>
    /// A self-contained 3D preview that instantiates a DinoPawn prefab far away from the world,
    /// applies the working colour (with the ORIGINAL pattern) via the game's own SetColourData,
    /// and renders it with an orthographic camera to a RenderTexture.
    /// </summary>
    public class Viewport3D
    {
        private GameObject _root;
        private GameObject _dino;
        private Il2CppItalicPig.PaleoPines.Actors.DinoPawn _pawn;
        private Camera _cam;
        private RenderTexture _rt;

        public RenderTexture Output { get { return _rt; } }
        public bool IsReady { get; private set; }
        public string SpeciesId { get; private set; }

        /// <summary>False when the viewport GameObjects were destroyed (e.g. scene unload).</summary>
        public bool IsAlive { get { return _root != null; } }

        public int ViewWidth = 512;
        public int ViewHeight = 512;

        public void CreateFor(string speciesId)
        {
            if (string.IsNullOrEmpty(speciesId)) return;

            var herd = Core.GameCatalog.FindHerd(speciesId);
            Il2CppItalicPig.PaleoPines.Actors.DinoPawn prefab = null;
            var setups = herd != null ? herd.DinoSetups : null;
            if (setups != null && setups.Count > 0 && setups[0] != null)
            {
                prefab = setups[0]._Prefab;
            }

            if (prefab == null)
            {
                MelonLoader.MelonLogger.Error("No prefab found for species " + speciesId);
                return;
            }

            Create(speciesId, prefab);
        }

        public void Create(string speciesId, Il2CppItalicPig.PaleoPines.Actors.DinoPawn prefab)
        {
            Destroy();

            try
            {
                SpeciesId = speciesId;
                _root = new GameObject("DinoStudio_Viewport");
                _root.transform.position = new Vector3(99999f, 99999f, 99999f);

                if (prefab != null)
                {
                    _dino = Object.Instantiate(prefab.gameObject, _root.transform);
                    _dino.name = "DinoStudio_Dino";
                    _dino.transform.localPosition = Vector3.zero;
                    _dino.transform.localRotation = Quaternion.identity;

                    _pawn = _dino.GetComponent<Il2CppItalicPig.PaleoPines.Actors.DinoPawn>();

                    // Disable behaviours that would run gameplay logic.
                    foreach (var comp in _dino.GetComponents<Behaviour>())
                    {
                        if (comp == null) continue;
                        string t = comp.GetIl2CppType().FullName;
                        if (t.Contains("DinoAI") || t.Contains("DinoPawn") || t.Contains("Animator"))
                        {
                            try { comp.enabled = false; } catch { }
                        }
                    }
                }

                BuildCamera();

                _rt = new RenderTexture(ViewWidth, ViewHeight, 16, RenderTextureFormat.ARGB32);
                _rt.name = "DinoStudio_RT";
                _cam.targetTexture = _rt;

                IsReady = true;
                MelonLoader.MelonLogger.Msg("Viewport3D created for " + speciesId);
            }
            catch (System.Exception e)
            {
                MelonLoader.MelonLogger.Error("Viewport3D.Create failed: " + e);
                Destroy();
            }
        }

        private void BuildCamera()
        {
            _cam = new GameObject("DinoStudio_Cam").AddComponent<Camera>();
            _cam.transform.SetParent(_root.transform, false);
            _cam.transform.localPosition = new Vector3(0f, 0f, -12f);
            _cam.orthographic = true;
            _cam.orthographicSize = 6f;
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = new Color(0.72f, 0.72f, 0.78f, 1f);
            _cam.nearClipPlane = 0.1f;
            _cam.farClipPlane = 100f;
            _cam.allowHDR = false;
            _cam.enabled = true;
        }

        /// <summary>
        /// Applies the working colour through the game's own material pipeline
        /// (SetupDinoMaterials) plus a direct property fallback, and dumps the pawn's
        /// material properties once so we can confirm the shader property names.
        /// </summary>
        public void ApplyColours(Core.WorkingAssets assets)
        {
            if (assets == null || assets.WorkingPattern == null || assets.WorkingColor == null) return;
            if (_pawn == null) return;
            try
            {
                // The preview pawn has no gameplay context, so SetColourData would throw on it.
                // SetupDinoMaterials + the direct property writes are enough for the preview.
                Core.ColourApplier.Apply(_pawn, assets.WorkingPattern, assets.WorkingColor, setColourData: false);
                Core.ColourApplier.DumpMaterialInfo(_pawn, "preview");
            }
            catch (System.Exception e)
            {
                MelonLoader.MelonLogger.Error("Viewport ApplyColours failed: " + e);
            }
        }

        public void Tick()
        {
            if (_dino != null)
            {
                _dino.transform.Rotate(0f, 15f * Time.unscaledDeltaTime, 0f, Space.World);
            }
        }

        public void Destroy()
        {
            IsReady = false;
            if (_rt != null)
            {
                _rt.Release();
                Object.Destroy(_rt);
                _rt = null;
            }
            if (_root != null)
            {
                Object.Destroy(_root);
                _root = null;
            }
            _dino = null;
            _pawn = null;
            _cam = null;
        }
    }
}
