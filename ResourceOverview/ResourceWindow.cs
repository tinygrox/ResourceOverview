using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;
using KSP.Localization;
using PluginBaseFramework;
using static ResourceOverview.RegisterToolbar;

namespace ResourceOverview
{
    partial class ResourceOverview
    {
        const double g = 9.81f;
        static GUIStyle activeFont;

        private Dictionary<String, DisplayResource> resourceList = new Dictionary<String, DisplayResource>();
        public bool vesselDataFetched = false;
        private float vesselTotalMass;
        private float vesselDryMass;
        private float vesselTWR;
        private float vesselMaxThrust;
        private int vesselCrewCapacity;
        private int vesselPartCount;

        protected SettingsWindow settingsWindow;

        //public ResourceOverview() : base("Resource Overview", 250, 50)
        public ResourceOverview() : base("资源总览", 150, 50, true) // "Resource Overview"
        {

        }

        public void onSettingsChanged()
        {
            KSPSettings.load();
        }

        void SetUpUpdateCoroutine()
        {
            StartCoroutine(UpdateResourcesCoroutine());
        }

        IEnumerator UpdateResourcesCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                setFetchVesselData();
            }
        }

        private void onPartRemove(GameEvents.HostTargetAction<Part, Part> data)
        {
            setFetchVesselData();
        }


        void onEditorShipModified(ShipConstruct sc)
        {
            setFetchVesselData();
        }

        private void setFetchVesselData()
        {
            vesselDataFetched = false;
            vesselTotalMass = 0;
            vesselCrewCapacity = 0;
            if (resourceList.Count > 0)
            {
                resourceList.Clear();
            }
        }


        Vector3 SetDirection(int ctrlDir, Vessel vessel)
        {
            if (ctrlDir == 0)
            {
                return (vessel.rootPart.transform.up);
            }
            if (ctrlDir == 1)
            {
                return (vessel.rootPart.transform.forward);
            }
            if (ctrlDir == 2)
            {
                return (-vessel.rootPart.transform.up);
            }
            if (ctrlDir == 3)
            {
                return (-vessel.rootPart.transform.forward);
            }
            if (ctrlDir == 4)
            {
                return (vessel.rootPart.transform.right);
            }
            if (ctrlDir == 5)
            {
                return (-vessel.rootPart.transform.right);
            }
            else
            {
                return (vessel.rootPart.transform.up);
            }
        }
        int controlDirection = 0; //control direction         

        public double GetThrustInfo(double altitude, out double outMinThrust, out double outMaxThrust)
        {
            Vessel activeVessel = FlightGlobals.ActiveVessel;

            double maxThrust = 0;
            double maxThrustVertical = 0;
            double minThrust = 0;
            double minThrustVertical = 0;

            double actualThrustLastFrame = 0;
            var controlUp = SetDirection(controlDirection, activeVessel);
            for (int i = 0; i < activeVessel.Parts.Count; i++)
            {
                Part part = activeVessel.Parts[i];
                if (part.Modules.Contains<ModuleEngines>())
                {
                    //if (part.Modules.Contains("ModuleEngines") || part.Modules.Contains("ModuleEnginesFX") || part.Modules.Contains("ModuleEnginesRF")) //is part an engine?

                    float DavonThrottleID = 0;
                    if (part.Modules.Contains("DifferentialThrustEngineModule")) //Devon Throttle Control Installed?
                    {
                        for (int i1 = 0; i1 < part.Modules.Count; i1++)
                        {
                            PartModule pm = part.Modules[i1];

                            if (pm.moduleName == "DifferentialThrustEngineModule")
                            {
                                DavonThrottleID = (float)pm.Fields.GetValue("throttleFloatSelect"); //which throttle is engine assigned to?
                            }
                        }

                    }
                    if (DavonThrottleID == 0f)
                    {
                        foreach (var engineModule in part.Modules.OfType<ModuleEngines>())
                        {
                            double offsetMultiplier;
                            try
                            {
                                offsetMultiplier = Math.Max(0, Math.Cos(Mathf.Deg2Rad * Vector3.Angle(engineModule.thrustTransforms[0].forward, -controlUp)));
                            }
                            catch
                            {
                                offsetMultiplier = 1;
                            }

                            if ((bool)engineModule.Fields.GetValue("throttleLocked") && engineModule.isOperational)//if throttlelocked is true, this is solid rocket booster. then check engine is operational. if the engine is flamedout, disabled via-right click or not yet activated via stage control, isOperational returns false
                            {
                                maxThrust += ((engineModule.finalThrust) * offsetMultiplier); //add engine thrust to MaxThrust
                                maxThrustVertical += (double)(engineModule.finalThrust);
                                minThrust += ((engineModule.finalThrust) * offsetMultiplier); //add engine thrust to MinThrust since this is an SRB
                                minThrustVertical += (double)(engineModule.finalThrust);
                            }
                            else if (engineModule.isOperational)//we know it is an engine and not a solid rocket booster so:
                            {
                                maxThrust += ((engineModule.maxFuelFlow * engineModule.g * engineModule.atmosphereCurve.Evaluate((float)(engineModule.vessel.staticPressurekPa * PhysicsGlobals.KpaToAtmospheres)) * engineModule.thrustPercentage / 100F) * offsetMultiplier); //add engine thrust to MaxThrust
                                                                                                                                                                                                                                                                                // errLine = "16d1";
                                maxThrustVertical += ((engineModule.maxFuelFlow * engineModule.g * engineModule.atmosphereCurve.Evaluate((float)(engineModule.vessel.staticPressurekPa * PhysicsGlobals.KpaToAtmospheres)) * engineModule.thrustPercentage / 100F));
                            }
                            actualThrustLastFrame += engineModule.finalThrust * (float)offsetMultiplier;

                        }
                    }
                }
            }
            outMinThrust = minThrust;
            outMaxThrust = maxThrust;
            return actualThrustLastFrame;
        }

        void LoadFlightData()
        {
            Log.Info("LoadFlightData");
            var vessel = FlightGlobals.ActiveVessel;
            vesselTotalMass = 0;
            vesselDryMass = 0;
            vesselCrewCapacity = 0;
            for (int i = 0; i < vessel.Parts.Count; i++)
            {
                Part p = vessel.Parts[i];
                vesselDryMass += p.mass;
                vesselTotalMass += p.mass + p.GetResourceMass();
                vesselCrewCapacity += p.CrewCapacity;

                for (int i1 = 0; i1 < p.Resources.Count; i1++)
                {
                    PartResource res = p.Resources[i1];
                    if (resourceList.ContainsKey(res.info.displayName))
                    {
                        //res.info.density
                        resourceList[res.info.displayName].amount += res.amount;
                        resourceList[res.info.displayName].maxAmount += res.maxAmount;
                    }
                    else
                    {
                        if (res.info.name.StartsWith("_"))
                        {
                            // Log.Info($"now resource is {res.info.name}, which start with '_'!");
                            continue;
                        }
                        resourceList.Add(res.info.displayName, new DisplayResource(res.info.displayName, res.amount, res.maxAmount, res.info.density));
                    }
                }

            }
            vesselPartCount = vessel.Parts.Count;
            vesselMaxThrust = (float)GetThrustInfo(FlightGlobals.ActiveVessel.altitude, out double minThrust, out double maxThrust);

            vesselTotalMass = FlightGlobals.ActiveVessel.GetTotalMass();
            vesselTWR = (vesselMaxThrust / vesselTotalMass) / (float)9.81;

        }

        void LoadEditorData()
        {
            vesselTotalMass = EditorLogic.SortedShipList.Where(p => p.physicalSignificance == Part.PhysicalSignificance.FULL).Sum(p => p.mass + p.GetResourceMass());
            vesselDryMass = EditorLogic.SortedShipList.Where(p => p.physicalSignificance == Part.PhysicalSignificance.FULL).Sum(p => p.mass);
            vesselCrewCapacity = EditorLogic.SortedShipList.Sum(p => p.CrewCapacity);
            vesselPartCount = EditorLogic.SortedShipList.Count;

            // thanks to mechjeb for this part:
            var engines = (from part in EditorLogic.fetch.ship.parts
                           where part.inverseStage == StageManager.LastStage
                           from engine in part.Modules.OfType<ModuleEngines>()
                           select engine);
            vesselMaxThrust = engines.Sum(e => e.thrustPercentage / 100f * (e.maxThrust - e.minThrust) + e.minThrust);


            vesselTWR = (vesselMaxThrust / vesselTotalMass) / (float)g;

            for (int i = 0; i < EditorLogic.SortedShipList.Count; i++)
            {
                Part part = EditorLogic.SortedShipList[i];
                for (int i1 = 0; i1 < part.Resources.Count; i1++)
                {
                    PartResource res = part.Resources[i1];
                    if (resourceList.ContainsKey(res.info.displayName))
                    {
                        //res.info.density
                        resourceList[res.info.displayName].amount += res.amount;
                        resourceList[res.info.displayName].maxAmount += res.maxAmount;
                    }
                    else
                    {
                        if (res.info.name.StartsWith("_"))
                        {
                            continue;
                        }
                        resourceList.Add(res.info.displayName, new DisplayResource(res.info.displayName, res.amount, res.maxAmount, res.info.density));
                    }
                }
            }
        }

        void LoadTrackingData()
        {
            Log.Info("LoadTrackingData");
            vesselTotalMass = trackingStationVesselTotalMass;
            vesselDryMass = trackingStationVesselDryMass;
            vesselCrewCapacity = trackingStationCrewCapacity;
            vesselPartCount = trackingStationPartCount;

            foreach (ResourceData r in res.Values)
            {
                if (resourceList.ContainsKey(r.def.displayName))
                {
                    //res.info.density
                    resourceList[r.name].amount += r.current;
                    resourceList[r.name].maxAmount += r.max;
                }
                else
                {
                    if (r.def.name.StartsWith("_"))
                    {
                        continue;
                    }
                    resourceList.Add(r.def.displayName, new DisplayResource(r.def.displayName, r.current, r.max, r.def.density));
                }
            }
            string p = PodStatusText[(int)PodStatus];
            //windowPosition.width = 0;
            windowPosition.height = 0;
        }

        private void reloadVesselData()
        {
            if (vesselDataFetched)
            {
                return;
            }
            if (HighLogic.LoadedSceneIsEditor)
                LoadEditorData();
            if (HighLogic.LoadedSceneIsFlight)
                LoadFlightData();
            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
                LoadTrackingData();
            vesselDataFetched = true;
        }

        protected override void preDrawGui()
        {
            if (HighLogic.LoadedSceneIsEditor && EditorLogic.RootPart == null) // nothing to display, show only text
            {
                windowHeight = 50;
            }
            else // we got something, calculate size
            {
                windowHeight = 0;
                if (KSPSettings.showTotalMass
                    || KSPSettings.showDryMass
                    || KSPSettings.showCrewCapacity
                    || KSPSettings.showPartCount
                    || KSPSettings.showTWR)
                {
                    windowHeight += 10; // add some space before resources
                }

                if (KSPSettings.showTotalMass) windowHeight += 20;
                if (KSPSettings.showDryMass) windowHeight += 20;
                if (KSPSettings.showCrewCapacity) windowHeight += 20;
                if (KSPSettings.showPartCount) windowHeight += 20;
                if (KSPSettings.showTWR) windowHeight += 20;

                windowHeight += resourceList.Count * 20;
            }
        }

        public static void UpdateActiveFont()
        {
            Log.Info("UpdateActiveFont");
            if (KSPSettings.useStockSkin)
            {
                if (KSPSettings.useBoldFont)
                    activeFont = new GUIStyle(RegisterToolbar.boldStockFont);
                else
                    activeFont = new GUIStyle(RegisterToolbar.stockFont);
            }
            else
            {
                if (KSPSettings.useBoldFont)
                    activeFont = new GUIStyle(RegisterToolbar.boldFont);
                else
                    activeFont = new GUIStyle(RegisterToolbar.font);

            }
            activeFont.fontSize = (int)Math.Round(KSPSettings.fontSize);

            // width

            if (KSPSettings.useCompactSpacing)
                activeFont.padding = new RectOffset();

            activeFont.normal.textColor = Color.white;
            if (Instance != null)
                Instance.windowPosition.width -= 25;
            SetAlpha(KSPSettings.transparency);
        }

        internal static void SetAlpha(float Alpha)
        {
            Log.Info("SetALpha: " + Alpha);
            GUIStyle workingWindowStyle;

            // Not ideal, should really be able to delete the old kspWindowStyle first, but since this doesn't happen 
            // too often, it's ok to lose a few hundred bytes of storage here.

            if (KSPSettings.useStockSkin)
                RegisterToolbar.kspWindowStyle = new GUIStyle(RegisterToolbar.kspWindow);
            else
                RegisterToolbar.kspWindowStyle = new GUIStyle(RegisterToolbar.stockWindow);

            workingWindowStyle = RegisterToolbar.kspWindowStyle;

            if (GUI.skin == null)
                Log.Error("SetAlpha, GUI.skin is null");
            if (GUI.skin.window == null)
                Log.Error("SetAlpha, GUI.skin.window is null");
            if (GUI.skin.window.active == null)
                Log.Error("SetAlpha, GUI.skin.window.active is null");
            if (GUI.skin.window.active.background == null)
                Log.Error("SetAlpha, GUI.skin.window.active.background is null");
            else
                SetAlphaFor(Alpha, workingWindowStyle, kspWindowStyle.active.background, workingWindowStyle.active.textColor);
        }

        internal static Texture2D CopyTexture2D(Texture2D originalTexture)
        {
            Texture2D copyTexture = new Texture2D(originalTexture.width, originalTexture.height);
            copyTexture.SetPixels(originalTexture.GetPixels());
            copyTexture.Apply();
            return copyTexture;
        }

        static void SetAlphaFor(float Alpha, GUIStyle style, Texture2D backgroundTexture, Color color)
        {
            Texture2D copyTexture = CopyTexture2D(backgroundTexture);

            var pixels = copyTexture.GetPixels32();
            for (int i = 0; i < pixels.Length; ++i)
                pixels[i].a = (byte)Alpha;


            copyTexture.SetPixels32(pixels);
            copyTexture.Apply();

            style.active.background =
                style.normal.background =
                style.hover.background =
                style.onNormal.background =
                style.onHover.background =
                style.onActive.background =
                style.focused.background =
                style.onFocused.background =
                style.onNormal.background =
                style.normal.background = copyTexture;

            style.active.textColor =
                style.normal.textColor =
                style.hover.textColor =
                style.onNormal.textColor =
                style.onHover.textColor =
                style.onActive.textColor =
                style.focused.textColor =
                style.onFocused.textColor =
                style.onNormal.textColor =
                style.normal.textColor = color;
        }

        bool initted = false;

        private string getvesseltypedisplayName(VesselType v)
        {
            switch(v)
            {
                case VesselType.Base: 
                    return Localizer.Format("#autoLoc_6002178");
                case VesselType.Debris:
                    return Localizer.Format("#autoLOC_900676");
                case VesselType.DeployedGroundPart:
                    return Localizer.Format("#autoLOC_6009000");
                case VesselType.DeployedScienceController:
                    return Localizer.Format("#autoLOC_8002227");
                case VesselType.DeployedSciencePart:
                    return Localizer.Format("#autoLOC_8002223");
                case VesselType.DroppedPart:
                    return Localizer.Format("#autoLOC_6006094");
                case VesselType.EVA:
                    return "EVA";
                case VesselType.Flag:
                    return Localizer.Format("#autoLOC_8007221");
                case VesselType.Lander:
                    return Localizer.Format("#autoLOC_900686");
                case VesselType.Plane:
                    return Localizer.Format("#autoLOC_900685");
                case VesselType.Probe:
                    return Localizer.Format("#autoLOC_900681");
                case VesselType.Relay:
                    return Localizer.Format("#autoLOC_900687");
                case VesselType.Rover:
                    return Localizer.Format("#autoLOC_900683");
                case VesselType.Ship:
                    return Localizer.Format("#autoLOC_900684");
                case VesselType.SpaceObject:
                    return Localizer.Format("#autoLoc_6002177");
                case VesselType.Station:
                    return Localizer.Format("#autoLOC_900679");
                default:
                    return Localizer.Format("#autoLOC_6002223");
            }
        }
        protected override void drawGui(int windowID)
        {
            if (!initted)
            {
                UpdateActiveFont();
                initted = true;
            }

            if (GUI.Button(new Rect(windowPosition.width - 22, 2, 20, 20), "s"))
            {
                settingsWindow = gameObject.AddComponent<SettingsWindow>();
            }

            GUILayout.BeginVertical();

            if (!HighLogic.LoadedSceneIsEditor || EditorLogic.RootPart != null)
            {
                reloadVesselData();
                if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
                {
                    //GUILayout.Label("Vessel Type: <color=#32cd32>" + vesselType.ToString() + "</color>", activeFont);
                    // ShowLabel(Localizer.Format("载具类型："), "<color=#32cd32>" + vesselType.ToString() + "</color>"); // "Vessel Type:"
                    ShowLabel(Localizer.Format("载具类型："), "<color=#32cd32>" + getvesseltypedisplayName(vesselType) + "</color>"); // "Vessel Type:"
                    if (PodStatus != Statuses.pod)
                        GUILayout.Label(PodStatusText[(int)PodStatus], activeFont);
                }
                if (KSPSettings.showTotalMass)
                {
                    //GUILayout.Label("Total Mass: <color=#32cd32>" + String.Format("{0:,0.00}</color>", vesselTotalMass * 1000), activeFont);
                    ShowLabel(Localizer.Format("总质量："), "<color=#32cd32>" + String.Format("{0:,0.00}</color>", vesselTotalMass * 1000)); // "Total Mass:"
                }
                if (KSPSettings.showDryMass)
                {
                    //GUILayout.Label("Dry Mass: <color=#32cd32>" + String.Format("{0:,0.00}</color>", vesselDryMass * 1000), activeFont);
                    ShowLabel(Localizer.Format("干质量："), "<color=#32cd32>" + String.Format("{0:,0.00}</color>", vesselDryMass * 1000)); // "Dry Mass:"
                }
                if (KSPSettings.showCrewCapacity)
                {
                    //GUILayout.Label("Crew Capacity: <color=#32cd32>" + vesselCrewCapacity + "</color>", activeFont);
                    ShowLabel(Localizer.Format("乘员容量："), "<color=#32cd32>" + vesselCrewCapacity + "</color>"); // "Crew Capacity:"
                }
                if (KSPSettings.showPartCount)
                {
                    //GUILayout.Label("Part Count: <color=#32cd32>" + vesselPartCount + "</color>", activeFont);
                    ShowLabel(Localizer.Format("部件数量："), "<color=#32cd32>" + vesselPartCount + "</color>"); // "Part Count:"
                }
                if (KSPSettings.showTWR)
                {
                    //GUILayout.Label("TWR: <color=#32cd32>" + String.Format("{0:,0.00}</color>", vesselTWR), activeFont);
                    ShowLabel(Localizer.Format("推重比："), "<color=#32cd32>" + String.Format("{0:,0.00}</color>", vesselTWR)); // "TWR:"
                }
                GUILayout.Space(10);

                foreach (String key in resourceList.Keys)
                {
                    //GUILayout.Label(key + ": <color=#32cd32>" + String.Format("{0:,0.00}", resourceList[key].amount) + "</color>/<color=#7cfc00>" + String.Format("{0:,0.00}</color>", resourceList[key].maxAmount), activeFont, GUILayout.ExpandWidth(true));
                    ShowLabel(key + ":", ((resourceList[key].amount >= resourceList[key].maxAmount) ? 
                        String.Format("<color=#7cfc00>{0:,0.00}</color>", resourceList[key].amount) :
                        (resourceList[key].amount < 0.00001 ? 
                        String.Format("<color=#a9a12a>{0:,0.00}</color>", resourceList[key].amount) :
                        String.Format("<color=#32cd32>{0:,0.00}</color>", resourceList[key].amount))) + "/" + String.Format("<color=#7cfc00>{0:,0.00}</color>", resourceList[key].maxAmount));
                }

            }
            if (resourceList.Count == 0)
            {
                GUILayout.Label(Localizer.Format("无资源显示！"), GUILayout.ExpandWidth(true)); // "No resources to display!"
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        //float maxWidth = 0;
        void ShowLabel(string label, string data)
        {
            var width = Math.Min(windowPosition.width + 10, 400);

            GUIContent labelContent = new GUIContent(label);
            GUIContent dataContent = new GUIContent(data);

            Vector2 labelSize = activeFont.CalcSize(labelContent);
            Vector2 dataSize = activeFont.CalcSize(dataContent);

            float calcWidth = labelSize.x + dataSize.x + 2 * (activeFont.padding.left + activeFont.padding.right);
            //maxWidth = Math.Max(maxWidth, calcWidth);
            // float unusedWidth = Math.Max(KSPSettings.spaceBetween, width - calcWidth - 10);

            //Log.Info("ShowLabel, width: " + width + ", calcWidth: " + calcWidth + ", unusedWidth: " + unusedWidth);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(label, activeFont, GUILayout.Width(labelSize.x));
                // GUILayout.Space(unusedWidth - 30);
                GUILayout.FlexibleSpace();
                GUILayout.Label(data, activeFont, GUILayout.Width(dataSize.x));
                GUILayout.EndHorizontal();
            }

        }
        void OnDestroy()
        {
            Log.Info("window destroy");
            if (onPlanetariumTargetChangedLoaded)
            {
                GameEvents.onPlanetariumTargetChanged.Remove(activeShipChanged);
                onPlanetariumTargetChangedLoaded = false;
            }
            GameEvents.onGameSceneSwitchRequested.Remove(onGameSceneSwitchRequested);

            GameEvents.onPartRemove.Remove(onPartRemove);

            if (onEditorShipModifiedLoaded)
            {
                GameEvents.onEditorShipModified.Remove(onEditorShipModified);
                onEditorShipModifiedLoaded = false;
            }


            KSPSettings.SaveData();
        }
    }
}
