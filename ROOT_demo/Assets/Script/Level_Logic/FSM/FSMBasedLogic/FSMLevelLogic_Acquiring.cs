using System;
using System.Collections.Generic;
using com.ootii.Messages;
using ROOT.Message;
using UnityEngine;

namespace ROOT
{
   
    public class FSMLevelLogic_Acquiring : FSMLevelLogic_Career
    {
        public override bool IsTutorial => false;
        public override bool CouldHandleSkill => true;
        public override bool CouldHandleBoss => true;
        public override BossStageType HandleBossType => BossStageType.Acquiring;
        public override int LEVEL_ART_SCENE_ID => StaticName.SCENE_ID_ADDITIONAL_VISUAL_ACQUIRING;

        private readonly float[] _balancingChart = {2.0f, 1.75f, 1.5f, 1.25f, 1.0f, 0.75f};
        private Func<int, int, int> DelSignalFunc => (a, b) => Math.Abs(a - b);

        private float BalancingSignal(int ASignalCount, int BSignalCount)
        {
            var del = DelSignalFunc(ASignalCount, BSignalCount);
            del = Math.Min(del, _balancingChart.Length - 1);
            return _balancingChart[del];
        }

        private float _multiplier = 1.0f;

        protected override int GetInCome() => Mathf.RoundToInt((TypeASignalScore + TypeBSignalScore) * LevelAsset.CurrencyRebate * _multiplier);

        private void UpdateRoundData_Instantly_Acquiring()
        {
            var levelAsset = LevelAsset;
            var lvlLogic = this;

            var aSignalCount = levelAsset.GameBoard.GetTotalTierCountByCoreType(levelAsset.ActionAsset.AdditionalGameSetup.PlayingSignalTypeA, HardwareType.Field);
            var bSignalCount = levelAsset.GameBoard.GetTotalTierCountByCoreType(levelAsset.ActionAsset.AdditionalGameSetup.PlayingSignalTypeB, HardwareType.Field);

            if (RoundLibDriver.IsRequireRound)
            {
                _multiplier = BalancingSignal(aSignalCount, bSignalCount);
            }

            var signalInfo = new BoardSignalUpdatedInfo
            {
                SignalData = new BoardSignalUpdatedData()
                {
                    CrtTypeASignal = TypeASignalCount,
                    CrtTypeBSignal = TypeBSignalCount,
                    TypeATier = aSignalCount,
                    TypeBTier = bSignalCount,
                },
            };
            MessageDispatcher.SendMessage(signalInfo);
        }

        protected override void AdditionalInitLevel()
        {
            base.AdditionalInitLevel();
            LevelAsset.DestroyerEnabled = false;
        }
        
        protected override void BoardUpdatedHandler(IMessage rMessage)
        {
            base.BoardUpdatedHandler(rMessage);
            if (RoundLibDriver.CurrentRoundGist.HasValue)
            {
                UpdateRoundData_Instantly_Acquiring();
            }
        }

        private void BalancingSignalSetupInquiryHandler(IMessage rMessage)
        {
            if (rMessage is BalancingSignalSetupInquiry inquiry)
            {
                inquiry.BalancingSignalFuncCallBack(BalancingSignal);
            }
        }
        
        //TODO 这个还要正经接入。
        private int TargetCurrency = 10000;
        
        private void AcquiringCostTargetHandler(IMessage rMessage)
        {
            if (rMessage is AcquiringCostTargetInquiry inquiry)
            {
                inquiry.AcquiringCostTargetCallBack(TargetCurrency);
            }
        }
        
        protected override void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.AcquiringCostTargetInquiry,AcquiringCostTargetHandler);
            MessageDispatcher.RemoveListener(WorldEvent.BalancingSignalSetupInquiry,BalancingSignalSetupInquiryHandler);
            base.OnDestroy();
        }

        protected override void Awake()
        {
            base.Awake();
            MessageDispatcher.AddListener(WorldEvent.BalancingSignalSetupInquiry,BalancingSignalSetupInquiryHandler);
            MessageDispatcher.AddListener(WorldEvent.AcquiringCostTargetInquiry,AcquiringCostTargetHandler);
        }
    }
}