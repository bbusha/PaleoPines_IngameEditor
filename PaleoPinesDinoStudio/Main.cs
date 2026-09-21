using MelonLoader;
using UnityEngine;
using PaleoPinesDinoStudio.UI;

namespace PaleoPinesDinoStudio
{
    public class Main : MelonMod
    {
        internal static Main Instance;
        internal static StudioState State;

        public override void OnInitializeMelon()
        {
            Instance = this;
            State = new StudioState();
            LoggerInstance.Msg("Paleo Pines Dino Studio initialized.");
        }

        public override void OnUpdate()
        {
            if (State == null) return;

            if (Input.GetKeyDown(KeyCode.F2))
            {
                State.ToggleEditor();
            }

            if (State.EditorOpen)
            {
                // The game hides/locks the cursor when it receives mouse-wheel input
                // (camera zoom, item cycling, etc). Force our editor state every frame
                // so the cursor can't vanish while the overlay is up.
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                GameUI.Tick(State);
                Core.ColourApplier.TickLive(State);
                UI.Tabs.ViewportTab.Maintain(State);
                if (UI.Tabs.ViewportTab.Viewport != null)
                {
                    UI.Tabs.ViewportTab.Viewport.Tick();
                }
            }
        }

        public override void OnGUI()
        {
            // Calculate position dynamically
            int buttonWidth = 200;
            int buttonHeight = 30;

            float x = Screen.width - buttonWidth - 10; // 10px margin from right
            float y = 10;                              // 10px margin from top

            GUILayout.BeginArea(new Rect(x, y, buttonWidth, buttonHeight));

            if (GUILayout.Button("Open Editor", GUILayout.Width(buttonWidth)))
            {
                State.ToggleEditor();
            }

            GUILayout.EndArea();
        }


        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (State == null) return;
            State.OnSceneChanged(sceneName);

            // Re-catalog the world's herds and re-inject any saved designs so custom
            // variants survive scene changes / world reloads.
            Core.DesignStore.Load();
            Core.GameCatalog.Refresh();
            Core.DesignStore.ReinjectAll();
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            if (State == null) return;
            State.OnSceneUnloaded(sceneName);
        }
    }
}
