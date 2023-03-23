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
        public SettingsWindow() : base("资源总览设置", 200, 160) // "Resource Overview Settings"
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
            KSPSettings.showTotalMass = GUILayout.Toggle(KSPSettings.showTotalMass, "显示总质量"); // "Show Total Mass"
            KSPSettings.showDryMass = GUILayout.Toggle(KSPSettings.showDryMass, "显示干质量"); // "Show Dry Mass"
            KSPSettings.showCrewCapacity = GUILayout.Toggle(KSPSettings.showCrewCapacity, "显示乘员容量"); // "Show Crew Capacity"
            KSPSettings.showPartCount = GUILayout.Toggle(KSPSettings.showPartCount, "显示部件数"); // "Show Part Count"
            KSPSettings.showTWR = GUILayout.Toggle(KSPSettings.showTWR, "显示推重比"); // "Show TWR"
            KSPSettings.useStockSkin = GUILayout.Toggle(KSPSettings.useStockSkin, "原版风格"); // "Use Stock Skin"
            KSPSettings.useBoldFont = GUILayout.Toggle(KSPSettings.useBoldFont, "粗字体"); // "Use Bold Font"
            KSPSettings.useCompactSpacing = GUILayout.Toggle(KSPSettings.useCompactSpacing, "紧凑排列"); // "Use Compact Spacing"
            GUILayout.BeginHorizontal();
            GUILayout.Label("字体大小"); // "Font Size:"
            KSPSettings.fontSize = GUILayout.HorizontalSlider(KSPSettings.fontSize, 9f, 15f);            
            GUILayout.EndHorizontal();
            // GUILayout.BeginHorizontal();
            // GUILayout.Label("标签/数据空格数"); // "Label/Data Space:"
            // KSPSettings.spaceBetween = GUILayout.HorizontalSlider(KSPSettings.spaceBetween, 40f, 150f);
            // GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("窗口透明度"); // "Window Transparency:"
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            KSPSettings.transparency = GUILayout.HorizontalSlider(KSPSettings.transparency, 0f, 255f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("保存")) // "Save"
            {
                saveSettings();
            }
            if (GUILayout.Button("关闭")) // "Close"
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
