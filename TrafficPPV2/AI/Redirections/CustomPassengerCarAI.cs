﻿using ColossalFramework;
using System;
using System.Runtime.CompilerServices;
using ColossalFramework.Math;
using Transit.Framework.Network;
using UnityEngine;
using Transit.Framework.Redirection;

namespace CSL_Traffic
{
	public class CustomPassengerCarAI : CarAI
    {
        [RedirectFrom(typeof(PassengerCarAI))]
        public override void SimulationStep(ushort vehicleID, ref Vehicle data, Vector3 physicsLodRefPos)
        {
            if (((data.m_flags & Vehicle.Flags.Congestion) != 0) &&
                ((TrafficMod.Options & OptionsManager.ModOptions.NoDespawn) != OptionsManager.ModOptions.NoDespawn))
            {
                Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleID);
            }
            else
            {
                base.SimulationStep(vehicleID, ref data, physicsLodRefPos);
            }
        }
        
        [RedirectFrom(typeof(PassengerCarAI))]
        public override void SimulationStep(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            if ((TrafficMod.Options & OptionsManager.ModOptions.UseRealisticSpeeds) == OptionsManager.ModOptions.UseRealisticSpeeds)
            {
                var speedData = CarSpeedData.Of(vehicleID);

                if (speedData.SpeedMultiplier == 0 || speedData.CurrentPath != vehicleData.m_path)
                {
                    speedData.CurrentPath = vehicleData.m_path;
                    speedData.SetRandomSpeedMultiplier(0.6f, 1.4f);
                }
                m_info.ApplySpeedMultiplier(CarSpeedData.Of(vehicleID));
            }

            if ((vehicleData.m_flags & Vehicle.Flags.Stopped) != 0)
            {
                vehicleData.m_waitCounter += 1;
                if (this.CanLeave(vehicleID, ref vehicleData))
                {
                    vehicleData.m_flags &= ~Vehicle.Flags.Stopped;
                    vehicleData.m_waitCounter = 0;
                }
            }
            base.SimulationStep(vehicleID, ref vehicleData, ref frameData, leaderID, ref leaderData, lodPhysics);

            if ((TrafficMod.Options & OptionsManager.ModOptions.UseRealisticSpeeds) == OptionsManager.ModOptions.UseRealisticSpeeds)
            {
                m_info.RestoreVehicleSpeed(CarSpeedData.Of(vehicleID));
            }
        }
        
        [RedirectFrom(typeof(PassengerCarAI))]
        protected override bool StartPathFind(ushort vehicleID, ref Vehicle vehicleData, Vector3 startPos, Vector3 endPos, bool startBothWays, bool endBothWays, bool undergroundTarget)
        {
            VehicleInfo info = this.m_info;
            ushort driverInstance = this.GetDriverInstance(vehicleID, ref vehicleData);
            if (driverInstance == 0)
            {
                return false;
            }
            CitizenManager instance = Singleton<CitizenManager>.instance;
            CitizenInfo info2 = instance.m_instances.m_buffer[(int)driverInstance].Info;
            NetInfo.LaneType laneTypes = NetInfo.LaneType.Vehicle | NetInfo.LaneType.Pedestrian;
            VehicleInfo.VehicleType vehicleType = this.m_info.m_vehicleType;
            bool allowUnderground = (vehicleData.m_flags & (Vehicle.Flags.Underground | Vehicle.Flags.Transition)) != (Vehicle.Flags)0;
            bool randomParking = false;
            ushort targetBuilding = instance.m_instances.m_buffer[(int)driverInstance].m_targetBuilding;
            if (targetBuilding != 0 && Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)targetBuilding].Info.m_class.m_service > ItemClass.Service.Office)
            {
                randomParking = true;
            }
            PathUnit.Position startPosA;
            PathUnit.Position startPosB;
            float num;
            float num2;
            PathUnit.Position endPosA;
            if (PathManager.FindPathPosition(startPos, ItemClass.Service.Road, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, info.m_vehicleType, allowUnderground, false, 32f, out startPosA, out startPosB, out num, out num2) && info2.m_citizenAI.FindPathPosition(driverInstance, ref instance.m_instances.m_buffer[(int)driverInstance], endPos, laneTypes, vehicleType, undergroundTarget, out endPosA))
            {
                if (!startBothWays || num < 10f)
                {
                    startPosB = default(PathUnit.Position);
                }
                PathUnit.Position endPosB = default(PathUnit.Position);
                SimulationManager instance2 = Singleton<SimulationManager>.instance;
                uint path;
                if (Singleton<PathManager>.instance.CreatePath(ExtendedVehicleType.PassengerCar, out path, ref instance2.m_randomizer, instance2.m_currentBuildIndex, startPosA, startPosB, endPosA, endPosB, default(PathUnit.Position), laneTypes, vehicleType, 20000f, false, false, false, false, randomParking))
                {
                    if (vehicleData.m_path != 0u)
                    {
                        Singleton<PathManager>.instance.ReleasePath(vehicleData.m_path);
                    }
                    vehicleData.m_path = path;
                    vehicleData.m_flags |= Vehicle.Flags.WaitingPath;
                    return true;
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [RedirectTo(typeof(PassengerCarAI))]
        private ushort GetDriverInstance(ushort vehicleID, ref Vehicle data)
        {
            throw new NotImplementedException("GetDriverInstance is target of redirection and is not implemented.");
        }

	    [MethodImpl(MethodImplOptions.NoInlining)]
        [RedirectTo(typeof(PassengerCarAI))]
        private static bool CheckOverlap(ushort ignoreParked, ref Bezier3 bezier, float offset, float length, out float minPos, out float maxPos)
        {
            throw new NotImplementedException("CheckOverlap is target of redirection and is not implemented.");
        }
    }
}
