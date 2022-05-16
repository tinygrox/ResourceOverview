using UnityEngine;
using ToolbarControl_NS;
using KSP_Log;

namespace ResourceOverview
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        internal static Log Log = null;
        internal static GUIStyle boldFont;
        internal static GUIStyle boldStockFont;
        internal static GUIStyle font, stockFont;
        bool initted = false;

        internal static GUIStyle stockWindow;
        internal static GUIStyle kspWindow;
        internal static GUIStyle kspWindowStyle;

        void Awake()
        {
            if (Log == null)
#if DEBUG
                Log = new Log("ResourceOverview", Log.LEVEL.INFO);
#else
                Log = new Log("ResourceOverview", Log.LEVEL.ERROR);
#endif

            DontDestroyOnLoad(this);

        }

        void OnGUI()
        {
            if (!initted)
            {
                Log.Info("RegisterToolbar.OnGUI");
                initted = true;
                stockFont = new GUIStyle(GUI.skin.label);
                font = new GUIStyle(HighLogic.Skin.label);

                boldFont = new GUIStyle(HighLogic.Skin.label);
                boldFont.fontStyle = FontStyle.Bold;

                boldStockFont = new GUIStyle(GUI.skin.label);
                boldStockFont.fontStyle = FontStyle.Bold;

                stockWindow = new GUIStyle(GUI.skin.window);
                kspWindow = new GUIStyle(HighLogic.Skin.window);

                KSPSettings.load();

                if (KSPSettings.useStockSkin)
                    GUI.skin = HighLogic.Skin;

                
                ResourceOverview.UpdateActiveFont();
            }
        }
        void Start()
        {
            ToolbarControl.RegisterMod(ResourceOverview.MODID, ResourceOverview.MODNAME);
        }
    }
}