using System;
using System.Collections;
using System.Collections.Generic;
using com.ootii.Messages;
using UnityEngine;

namespace ROOT
{
    //TODO 从Barebone里面尽量挪东西下来。
    public class FSMLevelLogic_Career : FSMLevelLogic_Barebone
    {
        private bool CheckIsSkill() => LevelAsset.SkillMgr.CurrentSkillType.HasValue && LevelAsset.SkillMgr.CurrentSkillType.Value == SkillType.Swap;
        private bool CheckAutoF() => AutoDrive.HasValue && AutoDrive.Value;
        private bool CheckAutoR() => IsReverseCycle;

        private void AddtionalRecatIO_Skill()
        {
            if (LevelAsset.SkillEnabled)
            {
                LevelAsset.SkillMgr.TriggerSkill(LevelAsset, _ctrlPack);
            }
        }

        protected override void AddtionalRecatIO() => AddtionalRecatIO_Skill();

        private void CareerCycle()
        {
            if (LevelAsset.DestroyerEnabled)
            {
                WorldExecutor.UpdateDestoryer(LevelAsset);
                if (LevelAsset.WarningDestoryer != null)
                {
                    LevelAsset.WarningDestoryer.Step(out var outCore);
                    LevelAsset.DestoryedCoreType = outCore;
                }
            }

            if (RoundLibDriver.CurrentRoundGist.HasValue)
            {
                WorldExecutor.UpdateRoundData_Stepped(ref LevelAsset);
                var timingEvent = new TimingEventInfo
                {
                    Type = WorldEvent.InGameStatusChangedEvent,
                    CurrentStageType = RoundLibDriver.CurrentRoundGist.Value.Type,
                };
                var timingEvent2 = new TimingEventInfo
                {
                    Type = WorldEvent.CurrencyIOStatusChangedEvent,
                    BoardCouldIOCurrencyData = BoardCouldIOCurrency,
                    UnitCouldGenerateIncomeData = RoundLibDriver.IsRequireRound,
                };
                MessageDispatcher.SendMessage(timingEvent);
                MessageDispatcher.SendMessage(timingEvent2);
            }
        }

        private void ReverseCycle()
        {
            WorldCycler.StepDown();
            LevelAsset.TimeLine.Reverse();
        }
        
        protected override void ModifyFSMActions(ref Dictionary<RootFSMStatus, Action> actions)
        {
            base.ModifyFSMActions(ref actions);
            actions.Add(RootFSMStatus.R_Cycle, ReverseCycle);
            actions.Add(RootFSMStatus.Career_Cycle, CareerCycle);
            actions.Add(RootFSMStatus.Skill, SkillMajorUpkeep);
        }

        private void InitCareer()
        {
            CareerCycle();
            _mainFSM.currentStatus = RootFSMStatus.MajorUpKeep;
            _mainFSM.waitForNextFrame = false;
        }

        protected override void ModifiyRootFSMTransitions(ref HashSet<RootFSMTransition> RootFSMTransitions)
        {
            base.ModifiyRootFSMTransitions(ref RootFSMTransitions);
            //RISK Remove好使吗？
            //RISK 转移后的Consequence也一定要在设计中体现出来。
            RootFSMTransitions.Remove(new RootFSMTransition(RootFSMStatus.F_Cycle, RootFSMStatus.Animate, 1, CheckStartAnimate, TriggerAnimation));
            #region ADD Consequence
            RootFSMTransitions.Remove(new RootFSMTransition(RootFSMStatus.PreInit, RootFSMStatus.MajorUpKeep, 1, CheckInited));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.PreInit, RootFSMStatus.MajorUpKeep, 1, CheckInited, InitCareer));
            #endregion
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.Skill, RootFSMStatus.Career_Cycle));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.F_Cycle, RootFSMStatus.Career_Cycle));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.R_IO, RootFSMStatus.Skill, 3, CheckIsSkill));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.Career_Cycle, RootFSMStatus.Animate, 1, CheckStartAnimate, TriggerAnimation));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.MajorUpKeep, RootFSMStatus.R_Cycle, 3, CheckAutoR));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.Career_Cycle, RootFSMStatus.MinorUpKeep));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.R_Cycle, RootFSMStatus.Career_Cycle));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.MajorUpKeep, RootFSMStatus.F_Cycle, 2, CheckAutoF));
        }
    }
}