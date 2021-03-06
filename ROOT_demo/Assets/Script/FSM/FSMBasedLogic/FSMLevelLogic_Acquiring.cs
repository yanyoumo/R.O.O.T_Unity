using System;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Messages;
using I2.Loc;
using ROOT.Common;
using ROOT.Consts;
using ROOT.Message;
using ROOT.Message.Inquiry;
using ROOT.UI;
using UnityEngine;

namespace ROOT
{
   
    public class FSMLevelLogic_Acquiring : FSMLevelLogic_Career
    {
        protected override string SucceedEndingTerm => ScriptTerms.EndingMessageAcquiring_Successed;
        protected override string FailedEndingTerm => ScriptTerms.EndingMessageAcquiring_Failed;
        public override bool CouldHandleBoss => true;
        public override BossStageType HandleBossType => BossStageType.Acquiring;
        public override int LEVEL_ART_SCENE_ID => StaticName.SCENE_ID_ADDITIONAL_VISUAL_ACQUIRING;

        protected override IEnumerable<int> GamePlayHintPagesByLevelType
        {
            get
            {
                var res = base.GamePlayHintPagesByLevelType.ToList();
                return res.Append(7);
            }
        }
        
        private Func<int, float> BalancingFunc = (i) => 2.0f - i * 0.25f;
        private IEnumerable<int> offset=> new[]{0, 1, 2, 3, 4, 5};
        private float[] _balancingChart => new[]{1.3f, 1.2f, 1.0f, 0.8f, 0.5f, 0.1f};
        
        private Func<int, int, int> DelSignalFunc => (a, b) => Math.Abs(a - b);

        #region FeatureSet_TutorialOnly.
        public bool HandlingAcquiring => (!UseTutorialVer || (_acquiringEnabled && HandlingRound));
        private bool _acquiringEnabled = true;

        #endregion

        private float BalancingSignal(int ASignalCount, int BSignalCount)
        {
            var del = DelSignalFunc(ASignalCount, BSignalCount);
            del = Math.Min(del, _balancingChart.Length - 1);
            return _balancingChart[del];
        }

        private float _multiplier = 1.0f;

        protected override float GetBonusInCome() => (GetBaseInCome() + base.GetBonusInCome()) * (_multiplier - 1.0f);

        private void UpdateRoundData_Instantly_Acquiring()
        {
            var levelAsset = LevelAsset;
            var lvlLogic = this;

            var aSignalCount = levelAsset.GameBoard.GetTotalTierCountByType(levelAsset.ActionAsset.AdditionalGameSetup.PlayingSignalTypeA, HardwareType.Field);
            var bSignalCount = levelAsset.GameBoard.GetTotalTierCountByType(levelAsset.ActionAsset.AdditionalGameSetup.PlayingSignalTypeB, HardwareType.Field);

            if (RoundLibDriver.IsRequireRound)
            {
                _multiplier = BalancingSignal(aSignalCount, bSignalCount);
            }

            var signalInfo = new BoardSignalUpdatedInfo
            {
                SignalData = new BoardSignalUpdatedData()
                {
                    CurrentActivatedTierSumA = TypeASignalCount,
                    CurrentActivatedTierSumB = TypeBSignalCount,
                    CurrentTotalTierSumA = aSignalCount,
                    CurrentTotalTierSumB = bSignalCount,
                },
            };
            MessageDispatcher.SendMessage(signalInfo);
        }

        protected override void BoardUpdatedCallBack()
        {
            base.BoardUpdatedCallBack();
            if (RoundLibDriver.CurrentRoundGist.HasValue)
            {
                UpdateRoundData_Instantly_Acquiring();
            }
        }

        private void BalancingSignalSetupInquiryHandler(IMessage rMessage)
        {
            if (rMessage is BalancingSignalSetupInquiryData inquiry)
            {
                inquiry.BalancingSignalFuncCallBack(BalancingSignal);
            }
        }
        
        private int TargetCurrency => LevelAsset.ActionAsset.BossSetup.AcquiringTarget;
        
        private void AcquiringCostTargetHandler(IMessage rMessage)
        {
            if (rMessage is AcquiringCostTargetInquiryData inquiry)
            {
                inquiry.AcquiringCostTargetCallBack(TargetCurrency);
            }
        }

        protected override void AdditionalInitLevel()
        {
            base.AdditionalInitLevel();
            MessageDispatcher.SendMessage(new ToggleGameplayUIData {Set = false, SelectAll = false, UITag = UITag.Currency_Career});
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