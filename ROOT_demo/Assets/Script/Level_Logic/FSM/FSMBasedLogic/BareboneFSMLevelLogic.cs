using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    using FSMActions = Dictionary<RootFSMStatus, Action>;
    using Trans= RootFSMTransition;
    using FSMTransitions = HashSet<RootFSMTransition>;
    using Status = RootFSMStatus;
    public class BareboneFSMLevelLogic : LevelLogic
    {
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
                    {Status.CleanUp, CleanUp},
                    {Status.Animate, AnimateAction},
                    {Status.R_IO, ReactIO},
                };
                return _fsmActions;
            }
        }
        protected override HashSet<RootFSMTransition> RootFSMTransitions {
            get
            {
                var transitions = new FSMTransitions
                {
                    new Trans(Status.PreInit, Status.MajorUpKeep, 1, CheckInited),
                    new Trans(Status.PreInit),
                    new Trans(Status.F_Cycle, Status.Animate, 1, CheckStartAnimate, TriggerAnimation),
                    new Trans(Status.F_Cycle, Status.MinorUpKeep),
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