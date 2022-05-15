using PluginBaseFramework;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using SpaceTuxUtility;
using static ResourceOverview.RegisterToolbar;


namespace ResourceOverview
{
    class SettingsWindow : BaseWindow
    {
        internal static SettingsWindow Instance;
        public SettingsWindow() : base("Resource Overview Settings", 200, 160)
        {

        }

        public void Start()
        {
            Log.Debug("SettingsWindow start");
            Instance = this;
            _windowVisible = true;
            KSPSettings.load();
            windowPosition.x = KSPSettings.settingsWinX;
            windowPosition.y = KSPSettings.settingsWinY;

        }

        protected override void preDrawGui()
        {

        }

        protected override void drawGui(int windowID)
        {
             bool
                oshowDryMass = KSPSettings.showDryMass,
                oshowTotalMass = KSPSettings.showTotalMass,
                oshowCrewCapacity = KSPSettings.showCrewCapacity,
                oshowPartCount = KSPSettings.showPartCount,
                oshowTWR = KSPSettings.showTWR,
                ouseStockSkin = KSPSettings.useStockSkin,
                ouseBoldFont = KSPSettings.useBoldFont,
                ouseCompactSpacing = KSPSettings.useCompactSpacing;

             float 
                    ofontSize = KSPSettings.fontSize,
                    ospaceBetween = KSPSettings.spaceBetween,
                    otransparency = KSPSettings.transparency                    ;


            GUILayout.BeginVertical();
            KSPSettings.showTotalMass = GUILayout.Toggle(KSPSettings.showTotalMass, "Show Total Mass");
            KSPSettings.showDryMass = GUILayout.Toggle(KSPSettings.showDryMass, "Show Dry Mass");
            KSPSettings.showCrewCapacity = GUILayout.Toggle(KSPSettings.showCrewCapacity, "Show Crew Capacity");
            KSPSettings.showPartCount = GUILayout.Toggle(KSPSettings.showPartCount, "Show Part Count");
            KSPSettings.showTWR = GUILayout.Toggle(KSPSettings.showTWR, "Show TWR");
            KSPSettings.useStockSkin = GUILayout.Toggle(KSPSettings.useStockSkin, "Use Stock Skin");
            KSPSettings.useBoldFont = GUILayout.Toggle(KSPSettings.useBoldFont, "Use Bold Font");
            KSPSettings.useCompactSpacing = GUILayout.Toggle(KSPSettings.useCompactSpacing, "Use Compact Spacing");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Font Size:");
            KSPSettings.fontSize = GUILayout.HorizontalSlider(KSPSettings.fontSize, 9f, 15f);            
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Label/Data Space:");
            KSPSettings.spaceBetween = GUILayout.HorizontalSlider(KSPSettings.spaceBetween, 40f, 150f);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Window Transparency:");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            KSPSettings.transparency = GUILayout.HorizontalSlider(KSPSettings.transparency, 0f, 255f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                saveSettings();
            }
            if (GUILayout.Button("Close"))
            {
                windowVisible = false;
                Destroy(this);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow();

            if (
                oshowDryMass != KSPSettings.showDryMass||
                oshowTotalMass != KSPSettings.showTotalMass||
                oshowCrewCapacity != KSPSettings.showCrewCapacity||
                oshowPartCount != KSPSettings.showPartCount ||
                oshowTWR != KSPSettings.showTWR ||
                ouseStockSkin != KSPSettings.useStockSkin ||
                ouseBoldFont != KSPSettings.useBoldFont ||
                ouseCompactSpacing != KSPSettings.useCompactSpacing ||
                ofontSize != KSPSettings.fontSize ||
                ospaceBetween != KSPSettings.spaceBetween||
                otransparency != KSPSettings.transparency)
                ResourceOverview.UpdateActiveFont();
        }

        public void OnDestroy()
        {
            Log.Debug("SettingsWindow destroy");
            KSPSettings.settingsWinX = windowPosition.x;
            KSPSettings.settingsWinY = windowPosition.y;

            saveSettings();
        }

        protected void saveSettings()
        {
            KSPSettings.save();
        }
    }
}
