using System;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Messages;
using ROOT.Message;
using ROOT.Message.Inquiry;
using Sirenix.OdinInspector;
using UnityEngine;

//这个函数的优化点就是在于，在得到全局信号深度后；将非0值作为一个个“岛”、之后选择“岛”中最小的即可。

namespace ROOT.Signal
{
    public abstract class UnitSignalCoreBase : MonoBehaviour
    {
        [ReadOnly]public Unit Owner;
        [ReadOnly] [ShowInInspector] public SignalDataPack SignalDataPackList;//Signal:信号 第一个int：物理深度 第二个int：（信号深度）只计算对应信号场单元的深度。
        //返回对应的信号的enum
        //为返回对应的在 遥测阶段 中获得的遥测范围。
        [ReadOnly] public RotationDirection SignalFromDir;
        [ReadOnly] public bool Visited;//dequeue
        [ReadOnly] public bool Visiting;//enqueue
        
        //标记扫描信号的路径的参数。
        [ReadOnly] public int ScanSignalPathDepth; //for scoring purpose
        [ReadOnly] [ShowInInspector] public int ServerSignalDepth;
        [ReadOnly] [ShowInInspector] public bool InServerGrid; //for scoring purpose
        
        public abstract SignalType SignalType { get; }
        public abstract List<Vector2Int> SingleInfoCollectorZone { get; }
        public SignalData CorrespondingSignalData => SignalDataPackList[SignalType];

        protected Board GameBoard => Owner.GameBoard;
        protected bool IsActiveFieldUnitThisSignal(Unit u) => u.SignalCore.IsUnitActive && u.UnitSignal == SignalType && u.UnitHardware == HardwareType.Field;
        protected bool IsActiveUnitThisSignal(Unit u) => u.SignalCore.IsUnitActive && u.UnitSignal == SignalType;
        
        public void ResetSignalStrengthComplex()
        {
            SignalDataPackList = new SignalDataPack();
            foreach (var signalType in SignalMasterMgr.Instance.SignalLib)
            {
                SignalDataPackList[signalType] = new SignalData(0, 0, 0, null);
            }
        }

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

        //0:no signal.
        //1:has signal but no active.
        //2:signal and active.

        public virtual UnitActivationLEDColor GetLEDLightingStatus
        {
            get
            {
                if (IsUnitActive) return UnitActivationLEDColor.Activated;
                if (SignalMasterMgr.Instance.Paths.WithinAnyPath(Owner)) return UnitActivationLEDColor.Dormant;
                return UnitActivationLEDColor.Deactivated;
            }
        }

        //现在Unit的几层逻辑框架分层拆开：
        //激活层：简单计算单元是否有对应信号对应+所有的核心单元。(扫描信号分开)
        //分数层：每个信号均不同。
        //LED 层:每个信号均不同。
        public virtual bool IsUnitActive => HasCertainSignal(SignalType) || Owner.UnitHardware == HardwareType.Core;

        public abstract float SingleUnitScore { get; }

        public virtual void SignalCoreInit()
        {
            InitNeighbouringLinkageDisplay();
        }
                
        protected bool ShowingNeighbouringLinkage = false;
        
        protected Vector2Int cachedCursorPos { private set; get; } = -Vector2Int.one;

        protected virtual void NeighbouringLinkageDisplay() {}

        protected virtual void InitNeighbouringLinkageDisplay() { }

        private void BoardDataUpdatedHandler(IMessage rMessage)
        {
            if (rMessage is CursorMovedEventData data)
            {
                cachedCursorPos = data.CurrentPosition;
            }
            NeighbouringLinkageDisplay();
        }
        
        private void NeighbouringLinkageToggle(IMessage rMessage)
        {
            ShowingNeighbouringLinkage = !ShowingNeighbouringLinkage;
            NeighbouringLinkageDisplay();
        }
        
        protected virtual void Awake()
        {
            Visited = false;
            InServerGrid = false;
            SignalDataPackList = new SignalDataPack();
            try
            {
                foreach (var signalType in SignalMasterMgr.Instance.SignalLib)
                {
                    SignalDataPackList.Add(signalType, new SignalData());
                }
                MessageDispatcher.AddListener(WorldEvent.InGameOverlayToggleEvent, NeighbouringLinkageToggle);
                MessageDispatcher.AddListener(WorldEvent.BoardSignalUpdatedEvent, BoardDataUpdatedHandler);
                MessageDispatcher.AddListener(WorldEvent.CursorMovedEvent, BoardDataUpdatedHandler);
            }
            catch (NullReferenceException)
            {
                RootDebug.Log("This is template Core, no need to set SignalStrength Dic.", NameID.YanYoumo_Log);
            }
        }

        private void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.CursorMovedEvent, BoardDataUpdatedHandler);
            MessageDispatcher.RemoveListener(WorldEvent.BoardSignalUpdatedEvent, BoardDataUpdatedHandler);
            MessageDispatcher.RemoveListener(WorldEvent.InGameOverlayToggleEvent, NeighbouringLinkageToggle);
        }
    }
}
