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
        public abstract SignalType SignalType { get; }

        [Obsolete]
        public Dictionary<SignalType, int> SignalStrength;
        //主要通过以下这个数据结构将整个棋盘中的数据网络记录下来。
        [ReadOnly]
        [ShowInInspector]
        public Dictionary<SignalType, (int, int)> SignalStrengthComplex;//Signal:信号 第一个int：物理深度 第二个int：（信号深度）只计算对应信号场单元的深度。

        protected (int, int) CorrespondingSignalData => SignalStrengthComplex[SignalType];

        public void ResetSignalStrengthComplex()
        {
            SignalStrengthComplex = new Dictionary<SignalType, (int, int)>();
            foreach (var signalType in SignalMasterMgr.Instance.SignalLib)
            {
                SignalStrengthComplex[signalType] = (0, 0);
            }
        }

        //为返回对应的在 遥测阶段 中获得的遥测范围。
        public abstract List<Vector2Int> SingleInfoCollectorZone { get; }

        [ReadOnly] public RotationDirection SignalFromDir;
        [ReadOnly] public bool Visited;//dequeue
        [ReadOnly] public bool Visiting;//enqueue

        [ShowInInspector]
        public bool IsActiveMatrixFieldUnit => (Owner.UnitSignal == SignalType.Matrix && Owner.UnitHardware == HardwareType.Field) && IsSignalUnitCoreActive;

        public bool HasCertainSignal(SignalType signalType)
        {
            return SignalStrengthComplex[signalType].Item1 > 0;
        }
        
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
            //IsMatrixFieldAndHasMatrixSignal = false;
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
        
        public virtual bool IsSignalUnitCoreActive => HasCertainSignal(SignalType);
        
        public abstract float SingleUnitScore { get; }
    }
}