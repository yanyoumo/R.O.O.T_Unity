using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
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
        HQ,
        CORETYPECOUNT
    }

    public enum CoreGenre
    {
        Source,
        Destination,
        Support,
        Other
    }

    public enum SideType
    {
        NoConnection,
        Connection,
        SIDETYPECOUNT
    }

    public enum ConnectionMeshType
    {
        NoConnectionMesh,
        DtoDConnectedMesh,
        StDConnectedMesh,
        DtSConnectedMesh,
        StSConnectedMesh,
        NoChange
    }

    public partial class Unit : MoveableBase
    {
        protected readonly CoreType[] SourceCoreTypeLib =
        {
            CoreType.Server,
            CoreType.Processor,
            CoreType.HQ
        };

        public CoreGenre GetCoreGenreByCoreType(CoreType coreType)
        {
            foreach (var type in SourceCoreTypeLib)
            {
                if (coreType == type)
                {
                    return CoreGenre.Source;
                }
            }

            return CoreGenre.Destination;
        }
    }

    public sealed partial class ShopMgr : MonoBehaviour
    {
        private float StationaryRate => (1 - (GameBoard.GetUnitCount / 36.0f))*0.8f;

        private float StationaryDiscount(SideType[] sides)
        {
            //静态单元的价格就是端口的数量。
            return sides.Count(side => side == SideType.Connection);
        }

        private readonly float[] _priceShopDiscount = {0.5f, 0.67f, 0.8f, 1.0f};

        public float PriceMultiplier(int unitCount)
        {
            float multiplierDelPerUnit = 0.7f / 36.0f;
            return multiplierDelPerUnit * unitCount + 1.0f;
        }

        public void InitPrice()
        {

            _priceByCore = new Dictionary<CoreType, float>()
            {
                {CoreType.PCB, 1.0f},
                {CoreType.NetworkCable, 2.0f},
                {CoreType.Server, 3.0f},
                {CoreType.Bridge, 4.0f},
                {CoreType.HardDrive, 2.0f},
                {CoreType.Processor, 3.0f},
                {CoreType.Cooler, 3.0f},
                {CoreType.BackPlate, 1.0f},
            };
            _priceBySide = new Dictionary<SideType, float>()
            {
                {SideType.NoConnection, 0.0f},
                {SideType.Connection, 1.0f},
            };
        }

        public void InitSideCoreWeight()
        {
            _keySideLib = new Dictionary<CoreType, Tuple<SideType, int>>()
            {
                {CoreType.PCB, new Tuple<SideType, int>(SideType.NoConnection, 4)},
                {CoreType.NetworkCable, new Tuple<SideType, int>(SideType.Connection, 2)},
                {CoreType.Server, new Tuple<SideType, int>(SideType.Connection, 1)},
                {CoreType.HardDrive, new Tuple<SideType, int>(SideType.Connection, 1)},
                {CoreType.Processor, new Tuple<SideType, int>(SideType.Connection, 1)},
            };

            #region SideSection

            //现在下面这个数据是除了关键接口去掉后，剩下的接口的概率。

            _defaultSideWeight = new Dictionary<SideType, float>()
            {
                {SideType.NoConnection, 0.75f},
                {SideType.Connection, 0.25f},
            };

            _processorSideWeight = new Dictionary<SideType, float>()
            {
                {SideType.NoConnection, 0.5f},
                {SideType.Connection, 0.5f},
            };

            _serverSideWeight = new Dictionary<SideType, float>()
            {
                {SideType.NoConnection, 0.5f},
                {SideType.Connection, 0.5f},
            };

            _hddSideWeight = new Dictionary<SideType, float>()
            {
                {SideType.NoConnection, 0.6f},
                {SideType.Connection, 0.4f},
            };

            _netCableSideWeight = new Dictionary<SideType, float>()
            {
                {SideType.NoConnection, 0.8f},
                {SideType.Connection, 0.2f},
            };

            _sideWeightLib = new Dictionary<CoreType, Dictionary<SideType, float>>()
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

        /// <summary>
        /// this function would calculate the total price
        /// </summary>
        /// <param name="unitPrice">the unit price of product</param>
        /// <returns>total price</returns>
        public float CalculateTotalPrice(float unitPrice, out float postalPrice)
        {
            postalPrice = 0.0f;
            return unitPrice;
        }
    }

    public partial class BoardDataCollector : MonoBehaviour
    {
        private float perDriverIncome = 1.5f;
        private Dictionary<CoreType, float> costByCore;

        private float TokenizedCostList(int count)
        {
            //这个不太对，就是这个Cost的价格除了线性增长外，应该只有轻微的调整，单元价格应该在购买的时候越来越贵。
            //TODO 感觉这种量化的数据后，具体花销是多少一定要打出来。
            //0-1-3-5-7-09-11-13-15-17-18-19-20-22-25-27-30-32-34-36+
            //0-1-3-6-8-10-13-16-18-22-23-25-26-29-35-38-45-48-51-54
            Dictionary<int, int> tokenizedVal = new Dictionary<int, int>()
            {
                {0, 0},
                {1, 1},
                {3, 3},
                {5, 6},
                {7, 8},
                {9, 10},
                {11, 13},
                {13, 16},
                {15, 18},
                {17, 22},
                {18, 23},
                {19, 25},
                {20, 26},
                {22, 29},
                {25, 35},
                {27, 38},
                {30, 45},
                {32, 48},
                {34, 51},
                {36, 54},
            };
            Debug.Assert(count >= 0);
            var maxKey = tokenizedVal.Keys.Where(val => (val >= count)).Min();
            tokenizedVal.TryGetValue(maxKey, out int maxVal);
            if (tokenizedVal.All(val => (val.Value < count)))
            {
                return tokenizedVal.Values.Max();
            }

            return maxVal;
        }

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
            float[] incomeArrayDel = {1.0f, 2.0f, 3.0f, 4.0f, 5.0f};
            float incomeArrayBase = 1.0f;
            float income = incomeArrayBase;
            for (int i = 0; i < length; i++)
            {
                int idx = Mathf.Min(i, incomeArrayDel.Length - 1);
                income += incomeArrayDel[idx];
            }

            return Mathf.Floor(income);
        }

        public float GetPerDriverIncome => perDriverIncome;

        public float GetCostByCore(CoreType key)
        {
            costByCore.TryGetValue(key, out float value);
            return value;
        }
    }
}