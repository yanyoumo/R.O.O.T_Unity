using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
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
        
        [ReadOnly]
        [ShowInInspector]
        public Dictionary<SignalType,SignalData> SignalDataPackList;//Signal:信号 第一个int：物理深度 第二个int：（信号深度）只计算对应信号场单元的深度。

        public SignalData CorrespondingSignalData => SignalDataPackList[SignalType];

        public void ResetSignalStrengthComplex()
        {
            SignalDataPackList = new Dictionary<SignalType,SignalData>();
            foreach (var signalType in SignalMasterMgr.Instance.SignalLib)
            {
                SignalDataPackList[signalType] = new SignalData(0, 0, 0, null);
            }
        }

        //为返回对应的在 遥测阶段 中获得的遥测范围。
        public abstract List<Vector2Int> SingleInfoCollectorZone { get; }

        [ReadOnly] public RotationDirection SignalFromDir;
        [ReadOnly] public bool Visited;//dequeue
        [ReadOnly] public bool Visiting;//enqueue

        //这个偶尔会报个Excp、到时候要看看。
        [ShowInInspector]
        public bool IsActiveMatrixFieldUnit => (Owner.UnitSignal == SignalType.Matrix && Owner.UnitHardware == HardwareType.Field) && IsUnitActive;

        [ShowInInspector] public int Test => FindCertainSignalDiv_FlatSignal(SignalType.Matrix);
        
        public int FindCertainSignalDiv_FlatSignal(SignalType signalType)
        {
            var hw0 = CertainSignalData(signalType).FlatSignalDepth;
            var others = Owner.GetConnectedOtherUnit;
            if (others.Count==0) return 0;
            var dels = new int[others.Count];
            for (var i = 0; i < others.Count; i++)
            {
                dels[i] = hw0 - others[i].SignalCore.SignalDataPackList[signalType].FlatSignalDepth;
            }

            return dels.Sum();
        }
        
        public bool HasCertainSignal(SignalType signalType)
        {
            return SignalDataPackList[signalType].FlatSignalDepth > 0;
        }

        public SignalData CertainSignalData(SignalType signalType)
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
            SignalDataPackList = new Dictionary<SignalType, SignalData>();
            try
            {
                foreach (var signalType in SignalMasterMgr.Instance.SignalLib)
                {
                    SignalDataPackList.Add(signalType, new SignalData());
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

        public int GetActivationStatus
        {
            get
            {
                //TODO 现在认为扫描信号只有终端单元是激活的、这个还是要改。
                //尽量需要一个这个点亮系统的配置流程、这个可以搞。
                //这个激活还是要弄、要不然所有其他干线的networkSignal都不算的、这个还是得弄。
                if (Owner.UnitSignal == SignalType.Scan && Owner.UnitHardware == HardwareType.Field)
                {
                    //TODO 先写在这里、到时候估计还要整。
                    var core = Owner.SignalCore as ScanUnitSignalCore;
                    Debug.Assert(core != null);
                    if (core.IsUnitVeryActive)
                    {
                        return 3;
                    }
                }

                if (IsUnitActive)
                {
                    return 2;
                }

                if (SignalMasterMgr.Instance.WithinAnyPath(Owner))
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
