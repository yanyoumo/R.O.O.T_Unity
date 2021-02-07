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
    using SignalDataPack = Tuple<int, int, int,bool>;
    
    public abstract class UnitSignalCoreBase : MonoBehaviour
    {
        public Unit Owner;
        protected Board GameBoard => Owner.GameBoard;
        //返回对应的信号的enum
        public abstract SignalType SignalType { get; }
        
        [ReadOnly]
        [ShowInInspector]
        public Dictionary<SignalType,SignalDataPack> SignalDataPackList;//Signal:信号 第一个int：物理深度 第二个int：（信号深度）只计算对应信号场单元的深度。

        public SignalDataPack CorrespondingSignalData => SignalDataPackList[SignalType];

        public void ResetSignalStrengthComplex()
        {
            SignalDataPackList = new Dictionary<SignalType,SignalDataPack>();
            foreach (var signalType in SignalMasterMgr.Instance.SignalLib)
            {
                SignalDataPackList[signalType] = new SignalDataPack(0, 0, 0, false);
            }
        }

        //为返回对应的在 遥测阶段 中获得的遥测范围。
        public abstract List<Vector2Int> SingleInfoCollectorZone { get; }

        [ReadOnly] public RotationDirection SignalFromDir;
        [ReadOnly] public bool Visited;//dequeue
        [ReadOnly] public bool Visiting;//enqueue

        [ShowInInspector]
        public bool IsActiveMatrixFieldUnit => (Owner.UnitSignal == SignalType.Matrix && Owner.UnitHardware == HardwareType.Field) && IsUnitActive;

        private int RectifyInt(int a)
        {
            return a==int.MaxValue ? 0 : a;
        }
        
        public bool HasCertainSignal(SignalType signalType)
        {
            return RectifyInt(SignalDataPackList[signalType].Item1) > 0;
        }

        public SignalDataPack CertainSignalData(SignalType signalType)
        {
            return SignalDataPackList[signalType];
        }
        
        public bool IsEndingScanFieldUnit => InServerGrid && (Owner.UnitSignal == SignalType.Scan && Owner.UnitHardware == HardwareType.Field) && ScanSignalPathDepth == 1;


        //标记扫描信号的路径的参数。
        [ReadOnly] public int ScanSignalPathDepth; //for scoring purpose

        [ReadOnly] [ShowInInspector] public int ServerSignalDepth;

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
            //SignalStrength = new Dictionary<SignalType, int>();
            SignalDataPackList = new Dictionary<SignalType, SignalDataPack>();
            try
            {
                foreach (var signalType in SignalMasterMgr.Instance.SignalLib)
                {
                    //SignalStrength.Add(signalType, 0);
                    SignalDataPackList.Add(signalType, new SignalDataPack(0, 0, 0, false));
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
        //public virtual bool GetActivationStatusPerSignal => CorrespondingSignalData.Item1 > 0;//默认看硬件深度。

        public int GetActivationStatus
        {
            get
            {
                if (IsUnitActive)
                {
                    return 2;
                }

                if (SignalDataPackList.Values.Any(valueTuple => valueTuple.Item1 > 0))
                {
                    return 1;
                }

                return 0;
            }
        }

        public virtual bool IsUnitActive => HasCertainSignal(SignalType) || Owner.UnitHardware == HardwareType.Core;

        public abstract float SingleUnitScore { get; }
    }
}