
//
// This file originated from the public domain mod Resource Details in Tracking Center
// Original source was lost, this source was obtained from:  https://github.com/Real-Gecko/KSP-TCShipInfo
//
// The source has been extensively modified to work with this mod
//
// This file can be considered Public Domain, and doesn't affect the license for the rest of the mod
//
using System;
using System.Linq;
using System.Collections.Generic;
using static ResourceOverview.RegisterToolbar;

namespace ResourceOverview
{
	partial class ResourceOverview
	{
		VesselType vesselType;

		float trackingStationVesselTotalMass;
		float trackingStationVesselDryMass;
		int trackingStationCrewCapacity;
		int trackingStationPartCount;

		List<string> PodStatusText = new List<string> { "No command pod", "Has command seat", "ProbeCore" };
		public enum Statuses { none, seat, pod }

		public Statuses PodStatus = Statuses.none;

		private void activeShipChanged(MapObject target)
		{
			Log.Info("TCShipInfo.activeShipChanged");
			if (target == null)
				return;

			updateVesselInfo(target.vessel);

		}

		SortedDictionary<string, ResourceData> res = new SortedDictionary<string, ResourceData>();
		private void updateVesselInfo(Vessel vessel)
		{
			if (vessel == null ||
				vessel.DiscoveryInfo.Level != DiscoveryLevels.Owned)
			{
				Log.Info("TCShipInfo., vessel == null ||vessel.DiscoveryInfo.Level != DiscoveryLevels.Owned");
				return;
			}

			ProtoVessel proto = vessel.protoVessel;
			vesselType = vessel.vesselType;

			res.Clear();
			float mass = 0, dryMass = 0;
			ResetVesselStatus();
			for (int i = 0; i < proto.protoPartSnapshots.Count; i++)
			{
				ProtoPartSnapshot p = proto.protoPartSnapshots[i];
				CheckVesselPodType(p);
				for (int i1 = 0; i1 < p.resources.Count; i1++)
                {
					ProtoPartResourceSnapshot r = p.resources[i1];
					ResourceData d;
					if (res.ContainsKey(r.resourceName))
						d = res[r.resourceName];
					else
					{
						d = new ResourceData(r.resourceName);
					}
					d.current += r.amount;
					d.max += r.maxAmount;
					res[r.resourceName] = d;
				}

				mass += p.mass;
				dryMass += p.mass;
			}

			if (!vessel.isEVA)
			{
			int crew = proto.GetVesselCrew().Count;
				mass += (float)res.Values.Sum(d => d.GetMass());
				var parts = proto.protoPartSnapshots.Count;

				trackingStationVesselTotalMass = mass;
				trackingStationVesselDryMass = dryMass;
				trackingStationCrewCapacity = crew;
				trackingStationPartCount = parts;
			}
			else
			{
				trackingStationVesselTotalMass = 0;
				trackingStationVesselDryMass = 0;
				trackingStationCrewCapacity = 0;
				trackingStationPartCount = 0;
			}
			
			setFetchVesselData();
		}

		public void ResetVesselStatus()
		{
			PodStatus = Statuses.none;
		}

		public void CheckVesselPodType(ProtoPartSnapshot part)
		{
			if (PodStatus == Statuses.pod) return;
			Log.Info("CheckVesselPodType, part: " + part.partInfo.title);
			for (int i = 0; i < part.modules.Count; i++)
			{
				var m = part.modules[i];

				if (m.moduleName == "ModuleCommand")
				{
					Log.Info("CheckVesselPodType, part.module is ModuleCommand");
					PodStatus = Statuses.pod;
					return;
				}
				if (m.moduleName == "KerbalSeat")
				{
					Log.Info("CheckVesselPodType, part.module is KerbalSeat");
					PodStatus = Statuses.seat;
				}
			}
		}

	}

	internal class ResourceData
	{
		public double current, max;

		public readonly string name;
		readonly internal PartResourceDefinition def;

		public ResourceData(string name)
		{
			this.name = name;
			this.def = PartResourceLibrary.Instance.GetDefinition(name);
		}

		public double GetMass()
		{
			return def == null ? 0 : def.density * current;
		}

		public override string ToString()
		{
			return string.Format("{0}: {1} / {2}", name, s(current), s(max));
		}

		private static string s(double d)
		{
			return d.ToString(d < 100 ? "f2" : "f0");
		}
	}
}
