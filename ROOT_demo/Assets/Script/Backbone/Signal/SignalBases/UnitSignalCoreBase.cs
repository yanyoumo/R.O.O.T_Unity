using System;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Messages;
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
        public SignalDataPack SignalDataPackList;//Signal:信号 第一个int：物理深度 第二个int：（信号深度）只计算对应信号场单元的深度。

        public SignalData CorrespondingSignalData => SignalDataPackList[SignalType];

        public void ResetSignalStrengthComplex()
        {
            SignalDataPackList = new SignalDataPack();
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

        //标记扫描信号的路径的参数。
        [ReadOnly] public int ScanSignalPathDepth; //for scoring purpose

        [ReadOnly] [ShowInInspector] public int ServerSignalDepth;

        /// <summary>
        /// 标记一次计分后，本单元是否处于必要最长序列中。不处于的需要显式记为false。
        /// </summary>
        [ReadOnly]
        [ShowInInspector]
        public bool InServerGrid; //for scoring purpose

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
                MessageDispatcher.AddListener(WorldEvent.BoardSignalUpdatedEvent, BoardSignalUpdatedHandler);
            }
            catch (NullReferenceException)
            {
                RootDebug.Log("This is template Core, no need to set SignalStrength Dic.", NameID.YanYoumo_Log);
            }
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
        
                
        private bool ShowingNeighbouringLinkage = false;

        private void NeighbouringLinkageToggle(IMessage rMessage)
        {
            ShowingNeighbouringLinkage = !ShowingNeighbouringLinkage;
            NeighbouringLinkageDisplay();
        }

        /*private Color totalemptyColor = new Color(0.0f, 1.0f, 0.0f);
        private Color totalBlockedColor =new Color(1.0f, 0.0f, 0.0f);
        private float ColorLerpingIdx => (8 - GetEmptyExpellingPos().Count()) / 8.0f;*/
        
        //N/E/W/S/NE/NW/SE/SW

        private readonly Vector2Int[] neighbouringOffsetList =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.left,
            Vector2Int.down,
            Vector2Int.up + Vector2Int.right,
            Vector2Int.up + Vector2Int.left,
            Vector2Int.down + Vector2Int.right,
            Vector2Int.down + Vector2Int.left
        };

        private void NeighbouringLinkageDisplay()
        {
            //TODO 这个玩意儿互相的显示怎么弄？//可以“反查询”
            //TODO 有两个Icon的情况下怎么弄？//Icon可以45°角切半。
            var unitAsset = SignalMasterMgr.Instance.GetUnitAssetByUnitType(SignalType, Owner.UnitHardware);
            if (unitAsset.NeighbouringData.Length > 0)
            {
                foreach (var dataAsset in unitAsset.NeighbouringData)
                {
                    for (var i = 0; i < neighbouringOffsetList.Length; i++)
                    {
                        var inquiryBoardPos = Owner.CurrentBoardPosition + neighbouringOffsetList[i];
                        var displayIcon = false;
                        if (IsUnitActive)
                        {
                            if (i < 4 || !dataAsset.FourDirOrEightDir)
                            {
                                if (GameBoard != null)
                                {
                                    if (GameBoard.CheckBoardPosValidAndFilled(inquiryBoardPos))
                                    {
                                        displayIcon = true;
                                        var otherUnit = Owner.GameBoard.FindUnitByPos(inquiryBoardPos);
                                        if (dataAsset.FliteringSignalType && otherUnit.UnitSignal != dataAsset.TargetingSignalType)
                                        {
                                            displayIcon = false;
                                        }

                                        if (dataAsset.FliteringHardwareType && otherUnit.UnitHardware != dataAsset.TargetingHardwareType)
                                        {
                                            displayIcon = false;
                                        }
                                    }
                                }
                            }
                        }
                        Owner.UnitNeighbouringRendererRoot.LinkageIcons[i].material.mainTexture = dataAsset.NeighbouringSprite;
                        Owner.UnitNeighbouringRendererRoot.LinkageIcons[i].material.color = dataAsset.ColorTint;
                        Owner.UnitNeighbouringRendererRoot.LinkageIcons[i].gameObject.SetActive(displayIcon && ShowingNeighbouringLinkage);
                    }
                }
            }
        }

        private void BoardSignalUpdatedHandler(IMessage rMessage)
        {
            NeighbouringLinkageDisplay();
        }

        private void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.BoardSignalUpdatedEvent, BoardSignalUpdatedHandler);
            MessageDispatcher.RemoveListener(WorldEvent.InGameOverlayToggleEvent, NeighbouringLinkageToggle);
        }
    }
}
