using UnityEngine;
using System.IO;
using SpaceTuxUtility;
using static ResourceOverview.RegisterToolbar;

namespace ResourceOverview
{
    class KSPSettings
    {
        static internal bool
            showDryMass = true,
            showTotalMass = true,
            showCrewCapacity = true,
            showPartCount = true,
            showTWR = false,
            useStockSkin = false,
            useBoldFont = false,
            useCompactSpacing = false;

        static internal float fontSize = 12f;
        static internal float spaceBetween = 40f;
        static internal float transparency = 255f;
        internal static float editorWinX, editorWinY, flightWinX, flightWinY;
        internal static float settingsWinX, settingsWinY;


        public delegate void SettingsChangedEventHandler();
        //public static event SettingsChangedEventHandler SettingsChanged;

        internal static readonly string CFG_PATH = "/GameData/ResourceOverview/PluginData/";
        static readonly string CFG_FILE = CFG_PATH + "ResourceOverview.cfg";
        internal static readonly string DISPLAYINFO_NODENAME = "ResourceOverview";

        static public void SaveData()
        {
            string fullPath = KSPUtil.ApplicationRootPath + CFG_FILE;
            var configFile = new ConfigNode();
            var configFileNode = new ConfigNode(DISPLAYINFO_NODENAME);

            configFileNode.AddValue("showTotalMass", showTotalMass);
            configFileNode.AddValue("showDryMass", showDryMass);
            configFileNode.AddValue("showCrewCapacity", showCrewCapacity);
            configFileNode.AddValue("showPartCount", showPartCount);
            configFileNode.AddValue("showTWR", showTWR);
            configFileNode.AddValue("useStockSkin", useStockSkin);
            configFileNode.AddValue("useBoldFont", useBoldFont);
            configFileNode.AddValue("useCompactSpacing", useCompactSpacing);
            configFileNode.AddValue("fontSize", fontSize);
            configFileNode.AddValue("spaceBetween", spaceBetween);
            configFileNode.AddValue("transparency", transparency);

            configFileNode.AddValue("settingsWinX", settingsWinX);
            configFileNode.AddValue("settingsWinY", settingsWinY);

            configFileNode.AddValue("editorWinX", editorWinX);
            configFileNode.AddValue("editorWinY", editorWinY);

            configFileNode.AddValue("flightWinX", flightWinX);
            configFileNode.AddValue("flightWinY", flightWinY);

            configFile.AddNode(configFileNode);
            configFile.Save(fullPath);
        }

        static public void LoadData()
        {
            string fullPath = KSPUtil.ApplicationRootPath + CFG_FILE;
            Log.Info("LoadData, fullpath: " + fullPath);
            if (File.Exists(fullPath))
            {
                Log.Info("file exists");
                var configFile = ConfigNode.Load(fullPath);
                if (configFile != null)
                {
                    Log.Info("configFile loaded");
                    var configFileNode = configFile.GetNode(DISPLAYINFO_NODENAME);
                    if (configFileNode != null)
                    {
                        Log.Info("configFileNode loaded");
                        showTotalMass = configFileNode.SafeLoad("showTotalMass", showTotalMass);
                        showDryMass = configFileNode.SafeLoad("showDryMass", showDryMass);
                        showCrewCapacity = configFileNode.SafeLoad("showCrewCapacity", showCrewCapacity);
                        showPartCount = configFileNode.SafeLoad("showPartCount", showPartCount);
                        showTWR = configFileNode.SafeLoad("showTWR", showTWR);
                        useStockSkin = configFileNode.SafeLoad("useStockSkin", useStockSkin);
                        useBoldFont = configFileNode.SafeLoad("useBoldFont", useBoldFont);
                        useCompactSpacing = configFileNode.SafeLoad("useCompactSpacing", useCompactSpacing);
                        fontSize = configFileNode.SafeLoad("fontSize", fontSize);
                        spaceBetween = configFileNode.SafeLoad("spaceBetween", spaceBetween);
                        transparency = configFileNode.SafeLoad("transparency", transparency);

                        editorWinX = configFileNode.SafeLoad("editorWinX", editorWinX);
                        editorWinY = configFileNode.SafeLoad("editorWinY", editorWinY);

                        flightWinX = configFileNode.SafeLoad("flightWinX", flightWinX);
                        flightWinY = configFileNode.SafeLoad("flightWinY", flightWinY);

                        settingsWinX = configFileNode.SafeLoad("settingsWinX", settingsWinX);
                        settingsWinY = configFileNode.SafeLoad("settingsWinY", settingsWinY);                        
                    }
                }
            }
        }

        public static void load()
        {
            LoadData();
        }

        public static void save()
        {
            SaveData();
               // SettingsChanged();
        }
    }
}
