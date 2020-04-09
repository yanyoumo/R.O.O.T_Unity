﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public enum CoreType
    {
        PCB,
        NetworkCable,
        Server,
        Bridge,
        HardDrive,
        Processor,
        Cooler,
        BackPlate,
        CORETYPECOUNT
    }

    public enum SideType
    {
        NoConnection,
        Connection,
        //SerialConnector,
        //Firewall,
        SIDETYPECOUNT
    }

    public struct Core
    {
        public string Name;
    }

    public struct Side
    {
        public string Name;
    }

    public sealed partial class ShopMgr : MonoBehaviour
    {
        public void InitPrice()
        {
            _priceByCore = new Dictionary<CoreType, float>()
            {
                {CoreType.PCB, 10.0f},
                {CoreType.NetworkCable, 20.0f},
                {CoreType.Server, 30.0f},
                {CoreType.Bridge, 40.0f},
                {CoreType.HardDrive, 20.0f},
                {CoreType.Processor, 30.0f},
                {CoreType.Cooler, 30.0f},
                {CoreType.BackPlate, 10.0f},
            };
            _priceBySide = new Dictionary<SideType, float>()
            {
                {SideType.NoConnection,2.0f },
                //{SideType.Firewall,8.0f },
                {SideType.Connection,6.0f },
                //{SideType.SerialConnector,6.0f },
            };
        }

        public void InitSideCoreWeight()
        {
            _keySideLib = new Dictionary<CoreType, Tuple<SideType, int>>()
            {
                {CoreType.PCB,new Tuple<SideType, int>(SideType.NoConnection,4)},
                {CoreType.NetworkCable,new Tuple<SideType, int>(SideType.Connection,2)},
                {CoreType.Server,new Tuple<SideType, int>(SideType.Connection,1)},
                {CoreType.HardDrive,new Tuple<SideType, int>(SideType.Connection,1)},
                {CoreType.Processor,new Tuple<SideType, int>(SideType.Connection,1)},
            };

            #region SideSection
            //现在下面这个数据是除了关键接口去掉后，剩下的接口的概率。

            _defaultSideWeight = new Dictionary<SideType, float>()
            {
                {SideType.NoConnection, 0.75f},
                {SideType.Connection, 0.25f},
                //{SideType.SerialConnector, 0.25f},
            };

            _processorSideWeight = new Dictionary<SideType, float>()
            {
                {SideType.NoConnection, 0.5f},
                {SideType.Connection, 0.5f},
                //{SideType.SerialConnector, 0.2f},
            };

            _serverSideWeight = new Dictionary<SideType, float>()
            {
                {SideType.NoConnection, 0.5f},
                {SideType.Connection, 0.5f},
                //{SideType.SerialConnector, 0.5f},
            };

            _hddSideWeight = new Dictionary<SideType, float>()
            {
                {SideType.NoConnection, 0.8f},
                {SideType.Connection, 0.2f},
                //{SideType.SerialConnector, 0.3f},
            };

            _netCableSideWeight = new Dictionary<SideType, float>()
            {
                {SideType.NoConnection, 0.8f},
                {SideType.Connection, 0.2f},
                //{SideType.SerialConnector, 0.3f},
            };

            _sideWeightLib=new Dictionary<CoreType, Dictionary<SideType, float>>()
            {
                {CoreType.Server, _serverSideWeight},
                {CoreType.NetworkCable, _netCableSideWeight},
                {CoreType.HardDrive, _hddSideWeight},
                {CoreType.Processor, _processorSideWeight},
            };

            #endregion

            #region CoreSection

            _defaultCoreWeight = new Dictionary<CoreType, float>()
            {
                {CoreType.PCB, 0.00f},
                {CoreType.Server, 0.05f},
                {CoreType.NetworkCable, 0.45f},
                {CoreType.HardDrive, 0.45f},
                {CoreType.Processor, 0.05f},
            };

            _noServerCoreWeight = new Dictionary<CoreType, float>()
            {
                {CoreType.PCB, 0.00f},
                {CoreType.Server, 0.45f},
                {CoreType.NetworkCable, 0.25f},
                {CoreType.HardDrive, 0.25f},
                {CoreType.Processor, 0.05f},
            };

            _noProcessorCoreWeight = new Dictionary<CoreType, float>()
            {
                {CoreType.PCB, 0.00f},
                {CoreType.Server, 0.05f},
                {CoreType.NetworkCable, 0.25f},
                {CoreType.HardDrive, 0.25f},
                {CoreType.Processor, 0.45f},
            };

            _nandServerProcessorCoreWeight = new Dictionary<CoreType, float>()
            {
                {CoreType.PCB, 0.00f},
                {CoreType.Server, 0.45f},
                {CoreType.NetworkCable, 0.05f},
                {CoreType.HardDrive, 0.05f},
                {CoreType.Processor, 0.45f},
            };

            #endregion
        }
    }

    public partial class CurrencyIOCalculator : MonoBehaviour
    {
        private float perDriverIncome = 5.0f;
        private Dictionary<CoreType, float> costByCore;

        private void InitIncomeCost()
        {
            costByCore = new Dictionary<CoreType, float>()
            {
                {CoreType.PCB, 1.0f},
                {CoreType.NetworkCable, 2.0f},
                {CoreType.Server, 3.0f},
                {CoreType.Bridge, 4.0f},
                {CoreType.HardDrive, 2.0f},
                {CoreType.Processor, 3.0f},
            };
        }

        public float GetServerIncomeByLength(int length)
        {
            float[] incomeArray = { 4.0f,5.0f,7.0f,10.0f,14.0f,19.0f };
            if (length>=incomeArray.Length)
            {
                return incomeArray[incomeArray.Length - 1];
            }
            else
            {
                return incomeArray[length];
            }
        }

        public float GetPerDriverIncome()
        {
            return perDriverIncome;
        }

        public float GetCostByCore(CoreType key)
        {
            costByCore.TryGetValue(key, out float value);
            return value;
        }
    }
}