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
                GameUI.Tick(State);
                Core.ColourApplier.TickLive(State);
                UI.Tabs.ViewportTab.Maintain(State);
                if (UI.Tabs.ViewportTab.Viewport != null)
                {
                    UI.Tabs.ViewportTab.Viewport.Tick();
                }
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (State == null) return;
            State.OnSceneChanged(sceneName);
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            if (State == null) return;
            State.OnSceneUnloaded(sceneName);
        }
    }
}
