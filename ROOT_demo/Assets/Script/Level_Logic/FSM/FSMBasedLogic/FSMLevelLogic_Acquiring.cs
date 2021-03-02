using System;
using System.Collections.Generic;
using com.ootii.Messages;

namespace ROOT
{
    public class FSMLevelLogic_Acquiring : FSMLevelLogic_Career
    {
        public override bool IsTutorial => false;
        public override bool CouldHandleSkill => true;
        public override bool CouldHandleBoss => true;
        public override BossStageType HandleBossType => BossStageType.Acquiring;
        public override int LEVEL_ART_SCENE_ID => StaticName.SCENE_ID_ADDITIONAL_VISUAL_ACQUIRING;

        private void BalancingSignal()
        {
            //TODO 因为Boss逻辑比较简单、这里尝试添加一些复杂的玩法。
            //throw new NotImplementedException();
        }

        private void UpdateRoundData_Instantly_Acquiring()
        {
            var levelAsset = LevelAsset;
            var lvlLogic = this;
            
            var signalInfo = new BoardSignalUpdatedInfo
            {
                SignalData = new BoardSignalUpdatedData()
                {
                    CrtTypeASignal = TypeASignalCount,
                    CrtTypeBSignal = TypeBSignalCount,
                    TypeATier = levelAsset.GameBoard.GetTotalTierCountByCoreType(levelAsset.ActionAsset.AdditionalGameSetup.PlayingSignalTypeA, HardwareType.Field),
                    TypeBTier = levelAsset.GameBoard.GetTotalTierCountByCoreType(levelAsset.ActionAsset.AdditionalGameSetup.PlayingSignalTypeB, HardwareType.Field),
                },
            };
            MessageDispatcher.SendMessage(signalInfo);
        }

        protected override void BoardUpdatedHandler(IMessage rMessage)
        {
            base.BoardUpdatedHandler(rMessage);
            if (RoundLibDriver.CurrentRoundGist.HasValue)
            {
                UpdateRoundData_Instantly_Acquiring();
            }
        }
        
        protected override void UpdateRoundData_Stepped()
        {
            base.UpdateRoundData_Stepped();
            if (RoundLibDriver.IsRequireRound || RoundLibDriver.IsShopRound) BalancingSignal();
        }
    }
}