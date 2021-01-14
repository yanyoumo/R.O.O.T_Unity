using System;
using System.Collections.Generic;

namespace ROOT
{
    using FSMActions = Dictionary<RootFSMStatus, Action>;
    using Trans= RootFSMTransition;
    using FSMTransitions = HashSet<RootFSMTransition>;
    using Status = RootFSMStatus;

    public class CareerFSMLevelLogic : FSMLevelLogic //LEVEL-LOGIC/每一关都有一个这个类。
    {
        protected override FSMTransitions RootFSMTransitions
        {
            get
            {
                var transitions = new FSMTransitions
                {
                    new Trans(Status.PreInit, Status.F_Cycle, 1, CheckInited),
                    new Trans(Status.PreInit),
                    new Trans(Status.MajorUpKeep, Status.BossInit, 5, CheckBossStageInit),
                    new Trans(Status.MajorUpKeep, Status.BossMajorUpKeep, 4, CheckBossAndNotPaused),
                    new Trans(Status.MajorUpKeep, Status.R_Cycle, 3, CheckAutoR),
                    new Trans(Status.MajorUpKeep, Status.F_Cycle, 2, CheckAutoF),
                    new Trans(Status.MajorUpKeep, Status.R_IO, 1, CheckCtrlPackAny),
                    new Trans(Status.MajorUpKeep),
                    new Trans(Status.R_IO,Status.BossPause,4,CheckBossAndPaused),
                    new Trans(Status.R_IO, Status.Skill, 3, CheckIsSkill),
                    new Trans(Status.R_IO, Status.F_Cycle, 2, CheckFCycle),
                    new Trans(Status.R_IO, Status.Animate, 1, CheckStartAnimate, TriggerAnimation),
                    new Trans(Status.R_IO, Status.MajorUpKeep, 0, true),
                    new Trans(Status.BossInit, Status.BossMajorUpKeep),
                    new Trans(Status.BossMajorUpKeep, Status.F_Cycle),
                    new Trans(Status.BossPause,Status.Career_Cycle),
                    new Trans(Status.F_Cycle, Status.Career_Cycle),
                    new Trans(Status.R_Cycle, Status.Career_Cycle),
                    new Trans(Status.Skill, Status.Career_Cycle),
                    new Trans(Status.Career_Cycle, Status.Animate, 2, CheckStartAnimate, TriggerAnimation),
                    new Trans(Status.Career_Cycle, Status.BossMinorUpKeep,1,CheckBossStage),
                    new Trans(Status.Career_Cycle, Status.MinorUpKeep),
                    new Trans(Status.MinorUpKeep, Status.Animate, 1, true, CheckLoopAnimate),
                    new Trans(Status.MinorUpKeep, Status.CleanUp),
                    new Trans(Status.BossMinorUpKeep, Status.MinorUpKeep),
                    new Trans(Status.Animate, Status.BossMinorUpKeep, 1, CheckBossStage),
                    new Trans(Status.Animate, Status.MinorUpKeep),
                    new Trans(Status.CleanUp, Status.MajorUpKeep, 0, true),
                };
                return transitions;
            }
        }

        //现在Upkeep分为Major和Minor两种；
        //Major可以写消耗较高的地方、在Animate期间不会被调用；
        //Minor原则上应该只写一些消耗较小的代码、这个在Animate期间会被调用。
        //即——常规Loop：Major先调用；Minor后调用。
        //     动画Loop：只有Minor会被调用。
        protected override FSMActions fsmActions
        {
            get
            {
                //可能需要一个“整理节点（空节点）”这种概念的东西。
                var _fsmActions = new FSMActions
                {
                    {Status.PreInit, PreInit},
                    {Status.MajorUpKeep, MajorUpkeepAction},
                    {Status.MinorUpKeep, MinorUpKeepAction},
                    {Status.F_Cycle, ForwardCycle},
                    {Status.R_Cycle, ReverseCycle},
                    {Status.Career_Cycle, CareerCycle},
                    {Status.CleanUp, CleanUp},
                    {Status.BossInit, BossInit},
                    {Status.BossMajorUpKeep, BossMajorUpdate},
                    {Status.BossMinorUpKeep, BossMinorUpdate},
                    {Status.BossPause, BossPauseAction},
                    {Status.Animate, AnimateAction},
                    {Status.R_IO, ReactIO},
                    {Status.Skill, SkillMajorSkill},
                };
                return _fsmActions;
            }
        }
    }
}