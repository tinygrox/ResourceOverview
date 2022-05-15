using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ResourceOverview.RegisterToolbar;

namespace PluginBaseFramework
{
    abstract class BaseWindow : PluginBase
    {
        protected int windowID;
        protected string windowTitle;
        protected bool useTransparency;

        internal Rect windowPosition = new Rect();
        internal float windowHeight;
        internal float windowWidth;
        //internal static bool useKSPskin = false;

        protected bool _windowVisible = false;
        public bool windowVisible
        {
            get { return _windowVisible; }
            set
            {
                _windowVisible = value;
            }
        }

        protected bool _windowHover;
        public bool windowHover
        {
            get { return _windowHover; }
            set
            {
                _windowHover = value;
            }
        }

        public BaseWindow(string title, float width, float height, bool useTransparency = false)
        {
            windowTitle = title;
            windowWidth = width;
            windowHeight = height;
            this.useTransparency = useTransparency;
            windowID = UnityEngine.Random.Range(1000, 2000000) + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.GetHashCode(); // generate window ID

            if (windowPosition.x == 0 && windowPosition.y == 0)
            {
                // set initial size and position
                windowPosition = new Rect(Screen.width / 2 - windowWidth / 2, Screen.height / 2 - windowHeight / 2, windowWidth, windowHeight);
            }

        }


        void OnGUI()
        {
            if (windowVisible || windowHover)
            {
                if (ResourceOverview.KSPSettings.useStockSkin)
                    GUI.skin = HighLogic.Skin;
                preDrawGui();
                if (ResourceOverview.RegisterToolbar.kspWindowStyle == null || !useTransparency)
                    windowPosition = GUILayout.Window(windowID, windowPosition, drawGui, windowTitle);
                else
                    windowPosition = GUILayout.Window(windowID, windowPosition, drawGui, windowTitle, ResourceOverview.RegisterToolbar.kspWindowStyle);
            }
        }

        protected abstract void preDrawGui();
        protected abstract void drawGui(int windowID);

    }
}
