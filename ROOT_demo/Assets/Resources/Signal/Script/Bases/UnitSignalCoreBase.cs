using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

//这个函数的优化点就是在于，在得到全局信号深度后；将非0值作为一个个“岛”、之后选择“岛”中最小的即可。

namespace ROOT.Signal
{
    public abstract class UnitSignalCoreBase : MonoBehaviour
    {
        public Unit Owner;
        protected Board GameBoard => Owner.GameBoard;
        //返回对应的信号的enum
        public abstract SignalType Type { get; }

        [Obsolete]
        public Dictionary<SignalType, int> SignalStrength;
        //主要通过以下这个数据结构将整个棋盘中的数据网络记录下来。
        [ReadOnly]
        [ShowInInspector]
        public Dictionary<SignalType, (int, int)> SignalStrengthComplex;//Signal:信号 第一个int：物理深度 第二个int：（信号深度）只计算对应信号场单元的深度。

        protected (int, int) CorrespondingSignalData => SignalStrengthComplex[Type];

        public void ResetSignalStrengthComplex()
        {
            foreach (var signalType in SignalStrengthComplex.Keys)
            {
                GameBoard.Units.ForEach(unit =>
                    unit.SignalCore.SignalStrengthComplex[signalType] = (int.MaxValue, int.MaxValue));
                foreach (var coreUnit in GameBoard.FindUnitWithCoreType(Type, HardwareType.Core))
                {
                    GameBoard.Units.ForEach(unit => unit.SignalCore.Visited = false);
                    var queue = new Queue<Unit>();
                    coreUnit.SignalCore.SignalStrengthComplex[signalType] = (0, 0);
                    coreUnit.SignalCore.Visited = true;
                    queue.Enqueue(coreUnit);
                    while (queue.Count != 0)
                    {
                        var now = queue.Dequeue();
                        var physicalDepth = now.SignalCore.SignalStrengthComplex[signalType].Item1;
                        var scoringDepth = now.SignalCore.SignalStrengthComplex[signalType].Item2;
                        foreach (var unit in now.GetConnectedOtherUnit.Where(unit => unit.SignalCore.Visited == false))
                        {
                            unit.SignalCore.Visited = true;
                            if (unit.UnitSignal == signalType && unit.UnitHardware == HardwareType.Core)
                                continue;
                            var item1 = unit.SignalCore.SignalStrengthComplex[signalType].Item1;
                            var item2 = unit.SignalCore.SignalStrengthComplex[signalType].Item2;
                            var renew = false;
                            if (physicalDepth + 1 < item1)
                            {
                                item1 = physicalDepth + 1;
                                renew = true;
                            }
                            if (unit.UnitSignal == signalType && unit.UnitHardware == HardwareType.Field)
                            {
                                if (scoringDepth + 1 < item2)
                                {
                                    item2 = scoringDepth + 1;
                                    renew = true;
                                }
                                unit.SignalCore.SignalStrengthComplex[signalType] = (item1, item2);
                            }
                            else
                            {
                                if (scoringDepth < item2)
                                {
                                    item2 = scoringDepth;
                                    renew = true;
                                }
                                unit.SignalCore.SignalStrengthComplex[signalType] = (item1, item2);
                            }
                            if (renew)
                                queue.Enqueue(unit);
                        }
                    }
                }
            }
        }

        //为返回对应的在 遥测阶段 中获得的遥测范围。
        public abstract List<Vector2Int> SingleInfoCollectorZone { get; }

        [ReadOnly] public RotationDirection SignalFromDir;
        [ReadOnly] public bool Visited;//dequeue
        [ReadOnly] public bool Visiting;//enqueue
        [ReadOnly] public bool InMatrix;
        [ReadOnly] public bool InMatrixSignal;

        public bool IsActiveMatrixFieldUnit => InMatrix && (Owner.UnitSignal == SignalType.Matrix && Owner.UnitHardware == HardwareType.Field);
        public bool IsEndingScanFieldUnit => InServerGrid && (Owner.UnitSignal == SignalType.Scan && Owner.UnitHardware == HardwareType.Field) && ScanSignalPathDepth == 1;


        //标记扫描信号的路径的参数。
        [ReadOnly] public int ScanSignalPathDepth; //for scoring purpose

        [ReadOnly]
        [ShowInInspector]
        public int ServerSignalDepth
        {
            get => SignalStrength[SignalType.Scan];
            set => SignalStrength[SignalType.Scan] = value;
        }
        /// <summary>
        /// 标记一次计分后，本单元是否处于必要最长序列中。不处于的需要显式记为false。
        /// </summary>
        [ReadOnly]
        [ShowInInspector]
        public bool InServerGrid; //for scoring purpose

        void Awake()
        {
            Visited = false;
            InServerGrid = false;
            InMatrix = false;
            SignalStrength = new Dictionary<SignalType, int>();
            SignalStrengthComplex = new Dictionary<SignalType, (int, int)>();
            try
            {
                foreach (var signalType in SignalMasterMgr.Instance.SignalLib)
                {
                    SignalStrength.Add(signalType, 0);
                    SignalStrengthComplex.Add(signalType, (0, 0));
                }
            }
            catch (NullReferenceException)
            {
                Debug.Log("This is template Core, no need to set SignalStrength Dic.");
            }
        }

        //0:no signal.
        //1:has signal but no active.
        //2:signal and active.
        public virtual bool GetActivationStatusPerSignal => CorrespondingSignalData.Item1 > 0;//默认看硬件深度。

        public int GetActivationStatus
        {
            get
            {
                if (GetActivationStatusPerSignal)
                {
                    return 2;
                }

                if (SignalStrengthComplex.Values.Any(valueTuple => valueTuple.Item1 > 0))
                {
                    return 1;
                }

                return 0;
            }
        }

        public abstract float CalSingleUnitScore();

        //从某个独立单元计分的逻辑、主要是为SignalAsset中总体计算的BackUp函数。 
        public abstract float CalScore(out int hardwareCount);
    }
}