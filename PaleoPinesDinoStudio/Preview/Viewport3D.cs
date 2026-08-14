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
        public bool AutoRotate { get { return _autoRotate; } }

        /// <summary>False when the viewport GameObjects were destroyed (e.g. scene unload).</summary>
        public bool IsAlive { get { return _root != null; } }

        public int ViewWidth = 512;
        public int ViewHeight = 512;

        private float _yaw;
        private float _pitch;
        private float _zoom = 6f;
        private bool _autoRotate;
        private float _lastInteraction;
        private const float OrbitDist = 12f;

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
                _yaw = 0f;
                _pitch = 0f;
                _zoom = 6f;
                _autoRotate = false;
                _lastInteraction = 0f;
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
                Core.ColourApplier.Apply(_pawn, assets.WorkingPattern, assets.WorkingColor, setColourData: false, eyeColor: assets.EyeColor);
                Core.ColourApplier.DumpMaterialInfo(_pawn, "preview");
            }
            catch (System.Exception e)
            {
                MelonLoader.MelonLogger.Error("Viewport ApplyColours failed: " + e);
            }
        }

        public void Tick()
        {
            if (_dino == null || _root == null) return;
            if (_autoRotate && Time.unscaledTime - _lastInteraction > 1.5f)
            {
                _yaw += 15f * Time.unscaledDeltaTime;
            }
            ApplyTransform();
        }

        /// <summary>Drag to orbit. Delta is in design units (x = yaw, y = pitch).</summary>
        public void DragOrbit(Vector2 designDelta)
        {
            _yaw = Wrap(_yaw - designDelta.x * 0.3f, 360f);
            _pitch = Mathf.Clamp(_pitch - designDelta.y * 0.3f, -70f, 70f);
            _autoRotate = false;
            _lastInteraction = Time.unscaledTime;
            ApplyTransform();
        }

        /// <summary>Positive zooms in (smaller orthographic size).</summary>
        public void Zoom(float delta)
        {
            _zoom = Mathf.Clamp(_zoom - delta * 0.5f, 2.5f, 12f);
            _lastInteraction = Time.unscaledTime;
            ApplyTransform();
        }

        public void SetAutoRotate(bool on)
        {
            _autoRotate = on;
            if (on) _lastInteraction = 0f;
        }

        private void ApplyTransform()
        {
            if (_root == null || _cam == null) return;

            // The model stays fixed; the camera orbits it. Keeping the camera looking at
            // the model centre keeps rotation smooth and centred regardless of the model's
            // pivot (rotating the model root itself made it swing and shake).
            _root.transform.localRotation = Quaternion.identity;

            float radYaw = _yaw * Mathf.Deg2Rad;
            float radPitch = _pitch * Mathf.Deg2Rad;
            float cp = Mathf.Cos(radPitch);
            Vector3 offset = new Vector3(
                OrbitDist * Mathf.Sin(radYaw) * cp,
                OrbitDist * Mathf.Sin(radPitch),
                OrbitDist * Mathf.Cos(radYaw) * cp);
            _cam.transform.position = _root.transform.position + offset;
            _cam.transform.rotation = Quaternion.LookRotation(-offset, Vector3.up);
            _cam.orthographicSize = _zoom;
        }

        private static float Wrap(float a, float m)
        {
            a %= m;
            if (a < 0f) a += m;
            return a;
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
