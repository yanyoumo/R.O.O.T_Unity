using System;
using System.Collections.Generic;

namespace ROOT
{
    using FSMActions = Dictionary<RootFSMStatus, Action>;
    using Trans= RootFSMTransition;
    using FSMTransitions = HashSet<RootFSMTransition>;
    public abstract class FSMTutorialLogic : FSMLevelLogic
    {
        protected override FSMActions fsmActions
        {
            get
            {
                //可能需要一个“整理节点（空节点）”这种概念的东西。
                var _fsmActions = new FSMActions
                {
                    {RootFSMStatus.PreInit, PreInit},
                    {RootFSMStatus.MajorUpKeep, MajorUpkeepAction},
                    {RootFSMStatus.MinorUpKeep, MinorUpKeepAction},
                    {RootFSMStatus.F_Cycle, ForwardCycle},
                    {RootFSMStatus.CleanUp, CleanUp},
                    {RootFSMStatus.Animate, AnimateAction},
                    {RootFSMStatus.R_IO, ReactIO},
                };
                return _fsmActions;
            }
        }
        protected override HashSet<RootFSMTransition> RootFSMTransitions {
            get
            {
                var transitions = new FSMTransitions
                {
                    new Trans(RootFSMStatus.PreInit, RootFSMStatus.F_Cycle, 1, CheckInited),
                    new Trans(RootFSMStatus.PreInit),
                    new Trans(RootFSMStatus.F_Cycle, RootFSMStatus.Animate, 1, CheckStartAnimate, TriggerAnimation),
                    new Trans(RootFSMStatus.F_Cycle, RootFSMStatus.MinorUpKeep),
                    new Trans(RootFSMStatus.Animate, RootFSMStatus.MinorUpKeep),
                    new Trans(RootFSMStatus.MajorUpKeep, RootFSMStatus.R_IO, 1, CheckCtrlPackAny),
                    new Trans(RootFSMStatus.MajorUpKeep),
                    new Trans(RootFSMStatus.MinorUpKeep, RootFSMStatus.Animate, 1, true, CheckLoopAnimate),
                    new Trans(RootFSMStatus.MinorUpKeep, RootFSMStatus.CleanUp),
                    new Trans(RootFSMStatus.R_IO, RootFSMStatus.F_Cycle, 2, CheckFCycle),
                    new Trans(RootFSMStatus.R_IO, RootFSMStatus.Animate, 1, CheckStartAnimate, TriggerAnimation),
                    new Trans(RootFSMStatus.R_IO, RootFSMStatus.MajorUpKeep, 0, true),
                    new Trans(RootFSMStatus.CleanUp, RootFSMStatus.MajorUpKeep, 0, true),
                };
                return transitions;
            }
        }
    }
}