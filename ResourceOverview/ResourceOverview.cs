using PluginBaseFramework;
using KSP.UI.Screens;
using ToolbarControl_NS;
using static ResourceOverview.RegisterToolbar;

namespace ResourceOverview
{

    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    partial class ResourceOverview : BaseWindow
    {
        internal static ResourceOverview Instance;

        private static ToolbarControl toolbarControl = null;

        internal const string MODID = "ResourceOveriew";
        internal const string MODNAME = "Resource Overview";


        public void Start()
        {
            Log.Info("Start");
            Instance = this;
            if (toolbarControl == null)
            {
                Log.Info("Initting toolbarControl");
                toolbarControl = gameObject.AddComponent<ToolbarControl>();
                toolbarControl.AddToAllToolbars(
                    onAppLaunchToggle, onAppLaunchToggle,
                    onAppLaunchHoverOn, onAppLaunchHoverOff,
                    null, null,
                    ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.FLIGHT| ApplicationLauncher.AppScenes.TRACKSTATION,
                    MODID,
                    "RO_Btn",
                    "ResourceOverview/PluginData/ro_app_button_active",
                    "ResourceOverview/PluginData/ro_app_button",
                    "ResourceOverview/PluginData/ro_toolbar_button_active",
                    "ResourceOverview/PluginData/ro_toolbar_button",
                    MODNAME);
            }

            KSPSettings.load();


            if (HighLogic.LoadedSceneIsFlight)
            {
                windowPosition.x = KSPSettings.flightWinX;
                windowPosition.y = KSPSettings.flightWinY;
            }
            if (HighLogic.LoadedSceneIsEditor)
            {
                windowPosition.x = KSPSettings.editorWinX;
                windowPosition.y = KSPSettings.editorWinY;
            }
            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                GameEvents.onPlanetariumTargetChanged.Add(activeShipChanged);
            }

            GameEvents.onGameSceneSwitchRequested.Add(onGameSceneSwitchRequested);

            GameEvents.onPartRemove.Add(onPartRemove);
            if (HighLogic.LoadedSceneIsEditor)
            {
                GameEvents.onEditorShipModified.Add(onEditorShipModified);
            }
            if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene == GameScenes.TRACKSTATION)
                SetUpUpdateCoroutine();
            saved = false;
        }

        bool saved = false;
        public void onGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> eData)
        {
            if (saved) return;
            saved = true;

            switch (eData.from)
            {
                case GameScenes.FLIGHT:
                    KSPSettings.flightWinX = windowPosition.x;
                    KSPSettings.flightWinY = windowPosition.y;
                    break;
                case GameScenes.EDITOR:
                    KSPSettings.editorWinX = windowPosition.x;
                    KSPSettings.editorWinY = windowPosition.y;
                    break;
            }

            KSPSettings.save();
        }


        private void onAppLaunchHoverOn()
        {
            windowHover = true;
        }

        private void onAppLaunchHoverOff()
        {
            windowHover = false;
        }

        private void onAppLaunchToggle()
        {
            windowVisible = !windowVisible;
        }
    }
}