using System.Collections.Generic;
using UnityEngine;

namespace PaleoPinesDinoStudio
{
    public class StudioState
    {
        public bool EditorOpen;
        public string LastScene;
        public string StatusMessage = "";
        public float StatusMessageTime;
        public int ActiveTab;

        public Core.WorkingAssets Working;
        public Il2CppItalicPig.PaleoPines.Actors.DinoPawn LivePawn;

        public List<string> SceneNames = new List<string>();

        private CursorLockMode _prevLock;
        private bool _prevCursorVisible;

        public void ToggleEditor()
        {
            EditorOpen = !EditorOpen;
            if (EditorOpen)
            {
                // The editor is a mouse-driven overlay, so the cursor must be free.
                _prevLock = Cursor.lockState;
                _prevCursorVisible = Cursor.visible;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                SceneNames.Clear();
                for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
                {
                    SceneNames.Add(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).name);
                }
                Core.GameCatalog.Refresh();
                Time.timeScale = 0f;
                UI.UiFactory.EnsureCanvas();
                UI.UiFactory.CanvasGO.SetActive(true);
                MelonLoader.MelonLogger.Msg("Editor opened.");
            }
            else
            {
                Cursor.lockState = _prevLock;
                Cursor.visible = _prevCursorVisible;

                Time.timeScale = 1f;
                var viewport = UI.Tabs.ViewportTab.Viewport;
                if (viewport != null) viewport.Destroy();
                if (UI.UiFactory.CanvasGO != null) UI.UiFactory.CanvasGO.SetActive(false);
                MelonLoader.MelonLogger.Msg("Editor closed.");
            }
        }

        public void OnSceneChanged(string sceneName)
        {
            LastScene = sceneName;
            if (EditorOpen) Core.GameCatalog.Refresh();
        }

        public void OnSceneUnloaded(string sceneName)
        {
            // nothing yet
        }

        public void SetStatus(string msg)
        {
            StatusMessage = msg;
            StatusMessageTime = Time.unscaledTime;
        }
    }
}
