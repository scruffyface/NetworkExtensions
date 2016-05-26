﻿using System;
using System.Collections.Generic;
using System.Linq;
using Transit.Addon.RoadExtensions.Menus.Roads;
using Transit.Addon.RoadExtensions.Roads.Common;
using Transit.Framework;
using Transit.Framework.Network;

namespace Transit.Addon.RoadExtensions.Roads.PedestrianRoads.Common
{
    public static partial class ZonablePedestrianHelper
    {
        public static string BasedPrefabName
        {
            get { return NetInfos.Vanilla.ROAD_2L; }
        }

        public static string UICategory
        {
            get { return RExExtendedMenus.ROADS_PEDESTRIANS; }
        }

        public static void SetInfo(NetInfo info, NetInfoVersion version, bool useDefaultMeshes = true)
        {
            ///////////////////////////
            // Template              //
            ///////////////////////////
            var roadInfo = Prefabs.Find<NetInfo>(NetInfos.Vanilla.ROAD_2L_TREES);

            ///////////////////////////
            // 3DModeling            //
            ///////////////////////////
            if (useDefaultMeshes)
            {
                info.Setup8mNoSWMesh(version);
            }
            ///////////////////////////
            // Set up                //
            ///////////////////////////
            info.m_availableIn = ItemClass.Availability.All;
            info.m_surfaceLevel = 0;
            info.m_hasParkingSpaces = false;
            info.m_hasPedestrianLanes = true;
            info.m_halfWidth = 4;
            info.m_UnlockMilestone = roadInfo.m_UnlockMilestone;
            info.m_pavementWidth = 2;
            info.m_class = roadInfo.m_class.Clone($"NExt {info.name}");
            info.m_class.m_level = ItemClass.Level.Level5;

            // Setting up lanes
            info.SetRoadLanes(version, new LanesConfiguration
            {
                IsTwoWay = true,
                LaneWidth = 2.5f,
                SpeedLimit = 0.3f,
                HasBusStop = false,
                PedPropOffsetX = -2
            });

            var vehicleLanes = new List<NetInfo.Lane>();
            vehicleLanes.AddRange(info.m_lanes.Where(l => l.m_laneType == NetInfo.LaneType.Vehicle).ToList());
            var sVehicleLaneWidth = 2.5f;
            var sVehicleLanePosAbs = 2f;
            //var tempVLanes = new List<NetInfo.Lane>();
            for (int i = 0; i < vehicleLanes.Count; i++)
            {
                vehicleLanes[i].m_verticalOffset = 0f;
                vehicleLanes[i] = new ExtendedNetInfoLane(vehicleLanes[i], ExtendedVehicleType.ServiceVehicles | ExtendedVehicleType.CargoTruck | ExtendedVehicleType.SnowTruck)
                {
                    m_position = (Math.Abs(vehicleLanes[i].m_position) / vehicleLanes[i].m_position) * sVehicleLanePosAbs,
                    m_width = sVehicleLaneWidth,
                    m_verticalOffset = 0.05f
                };
            }
            var pedLane = new NetInfo.Lane();
            pedLane = info.m_lanes.FirstOrDefault(l => l.m_laneType == NetInfo.LaneType.Pedestrian);
            pedLane.m_position = 0;
            pedLane.m_width = 8;
            pedLane.m_verticalOffset = 0.05f;
            var tempProps = new List<NetLaneProps.Prop>();
            var tempProps2 = new List<NetLaneProps.Prop>();
            tempProps = pedLane.m_laneProps.m_props.ToList();
            for (int i = 0; i < vehicleLanes.Count; i++)
            {
                var temp = new List<NetLaneProps.Prop>();
                temp = vehicleLanes[i].m_laneProps.m_props.ToList();
                temp.RemoveProps(new string[] { "arrow", "manhole" });
                tempProps2.AddRange(temp);
                vehicleLanes[i].m_laneProps.m_props = tempProps2.ToArray();
            }
            tempProps.RemoveProps(new string[] { "random", "bus", "limit" });
            tempProps.ReplacePropInfo(new KeyValuePair<string, PropInfo>("street light", Prefabs.Find<PropInfo>("StreetLamp02")));
            tempProps.ReplacePropInfo(new KeyValuePair<string, PropInfo>("traffic light 01", Prefabs.Find<PropInfo>("Traffic Light 01 Mirror")));
            var pedLightProp = tempProps.FirstOrDefault(tp => tp.m_prop.name == "Traffic Light 01 Mirror").ShallowClone();
            var pedLightPropInfo = Prefabs.Find<PropInfo>("Traffic Light Pedestrian");
            pedLightProp.m_prop = pedLightPropInfo;
            pedLightProp.m_position.x = -3.5f;
            var lights = tempProps.Where(tp => tp.m_prop.name == "StreetLamp02").ToList();
            foreach (var light in lights)
            {
                light.m_position.x = 0;
            }
            var tLight = tempProps.LastOrDefault(tp => tp.m_prop.name == "Traffic Light 02");
            tLight.m_position.x = -3.5f;
            var pedLightProp2 = tempProps.FirstOrDefault(tp => tp.m_prop.name == "Traffic Light 02").ShallowClone();
            pedLightProp2.m_prop = pedLightPropInfo;
            pedLightProp2.m_position.x = 3.5f;
            tempProps.ReplacePropInfo(new KeyValuePair<string, PropInfo>("traffic light 02", Prefabs.Find<PropInfo>("Traffic Light 01 Mirror")));
            tempProps.Add(pedLightProp);
            tempProps.Add(pedLightProp2);
            pedLane.m_laneProps.m_props = tempProps.ToArray();

            var roadCollection = new List<NetInfo.Lane>();
            roadCollection.Add(pedLane);
            roadCollection.AddRange(vehicleLanes);
            info.m_lanes = roadCollection.ToArray();
            ///////////////////////////
            // AI                    //
            ///////////////////////////
            var hwPlayerNetAI = roadInfo.GetComponent<PlayerNetAI>();
            var playerNetAI = info.GetComponent<PlayerNetAI>();

            if (hwPlayerNetAI != null && playerNetAI != null)
            {
                playerNetAI.m_constructionCost = hwPlayerNetAI.m_constructionCost;
                playerNetAI.m_maintenanceCost = hwPlayerNetAI.m_maintenanceCost;
            }

            var roadBaseAI = info.GetComponent<RoadBaseAI>();

            if (roadBaseAI != null)
            {
                roadBaseAI.m_trafficLights = false;
                roadBaseAI.m_noiseAccumulation = 3;
                roadBaseAI.m_noiseRadius = 20;
            }
        }
    }
}