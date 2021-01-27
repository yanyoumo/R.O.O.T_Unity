using System;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    using FSMActions = Dictionary<RootFSMStatus, Action>;
    using Trans= RootFSMTransition;
    using FSMTransitions = HashSet<RootFSMTransition>;
    public abstract partial class FSMTutorialLogic : FSMLevelLogic
    {
        private bool shouldInitTutorial = true;
        private bool TutorialOnHand = false;
        private bool CheckTutorialCycle()
        {
            //Debug.Log("CtrlPack.HasFlag(ControllingCommand.Confirm):" + CtrlPack.HasFlag(ControllingCommand.Confirm));
            return CtrlPack.HasFlag(ControllingCommand.Confirm);
        }

        private bool CheckNotOnHand()
        {
            return !TutorialOnHand;
        }

        private void TutorialCycle()
        {
            if (!ActionEnded)
            {
                StepForward();
                DealStepMgr();
            }
        }

        private void TutorialMinorUpkeep()
        {
            
        }

        private void TutorialInit()
        {
            if (!shouldInitTutorial) return;
            shouldInitTutorial = false;
            LevelAsset.HintMaster.HideTutorialFrame = true;
        }
        
        protected override void AddtionalMajorUpkeep()
        {
            TutorialInit();
        }

        protected override void AddtionalMinorUpkeep()
        {
            TutorialMinorUpkeep();
        }

        protected abstract void AdditionalFSMActionsOperating(ref FSMActions actions);
        protected abstract void AdditionalFSMTransitionOperating(ref FSMTransitions transitions);
        
        protected sealed override FSMActions fsmActions
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
                    {RootFSMStatus.Tutorial_Cycle, TutorialCycle},
                };
                AdditionalFSMActionsOperating(ref _fsmActions);
                return _fsmActions;
            }
        }
        protected sealed override HashSet<RootFSMTransition> RootFSMTransitions {
            get
            {
                var transitions = new FSMTransitions
                {
                    new Trans(RootFSMStatus.PreInit, RootFSMStatus.MajorUpKeep, 1, CheckInited),
                    new Trans(RootFSMStatus.PreInit),
                    new Trans(RootFSMStatus.F_Cycle, RootFSMStatus.Animate, 1, CheckStartAnimate, TriggerAnimation),
                    new Trans(RootFSMStatus.F_Cycle, RootFSMStatus.MinorUpKeep),
                    new Trans(RootFSMStatus.Tutorial_Cycle, RootFSMStatus.MajorUpKeep, 0, true),
                    new Trans(RootFSMStatus.Animate, RootFSMStatus.MinorUpKeep),
                    new Trans(RootFSMStatus.MajorUpKeep, RootFSMStatus.R_IO, 1, CheckCtrlPackAny),
                    new Trans(RootFSMStatus.MajorUpKeep),
                    new Trans(RootFSMStatus.MinorUpKeep, RootFSMStatus.Animate, 1, true, CheckLoopAnimate),
                    new Trans(RootFSMStatus.MinorUpKeep, RootFSMStatus.CleanUp),
                    new Trans(RootFSMStatus.R_IO, RootFSMStatus.Tutorial_Cycle, 4, CheckTutorialCycle),
                    new Trans(RootFSMStatus.R_IO, RootFSMStatus.MajorUpKeep, 3, CheckNotOnHand),
                    new Trans(RootFSMStatus.R_IO, RootFSMStatus.F_Cycle, 2, CheckFCycle),
                    new Trans(RootFSMStatus.R_IO, RootFSMStatus.Animate, 1, CheckStartAnimate, TriggerAnimation),
                    new Trans(RootFSMStatus.R_IO, RootFSMStatus.MajorUpKeep, 0, true),
                    new Trans(RootFSMStatus.CleanUp, RootFSMStatus.MajorUpKeep, 0, true),
                };
                AdditionalFSMTransitionOperating(ref transitions);
                return transitions;
            }
        }
    }
}