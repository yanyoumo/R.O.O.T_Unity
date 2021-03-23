using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ROOT.SetupAsset;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    
    public class TutorialTimeLineNeo : FSMTutorialLogic
    {
        protected override string MainGoalEntryContent => "将接收端单元都链至发送端";
        protected override string SecondaryGoalEntryContent => "在时间线结束之前完成任务";
        protected override void AddtionalDealStep(TutorialActionData data)
        {
            //throw new NotImplementedException();
        }

        /*protected override void AdditionalFSMActionsOperating(ref Dictionary<RootFSMStatus, Action> actions)
        {
            //throw new NotImplementedException();
        }

        protected override void AdditionalFSMTransitionOperating(ref HashSet<RootFSMTransition> transitions)
        {
            //throw new NotImplementedException();
        }*/

        protected override void TutorialMinorUpkeep()
        {
            if (ReadyToGo)
            {
                if (ActionIndex == 2)
                {
                    if (!OnceFlagA)
                    {
                        OnceFlagA = true;
                        WorldCycler.InitCycler();
                        LevelAsset.TimeLine.InitWithAssets(LevelAsset);
                    }
                }
            }

            if (ActionEnded)
            {
                SendHintData(HintEventType.ShowMainGoalComplete,AllUnitConnected());
                SendHintData(HintEventType.ShowSecondaryGoalComplete,!OnceFlagB);
                LevelCompleted = (!OnceFlagB) && AllUnitConnected();
            }

            if (LevelCompleted)
            {
                PlayerRequestedEnd = CtrlPack.HasFlag(ControllingCommand.Confirm);
            }

            if (LevelAsset.ActionAsset.HasEnded(LevelAsset.StepCount))
            {
                LevelFailed = true;
                OnceFlagB = true;
            }

            if (LevelFailed)
            {
                PlayerRequestedQuit = CtrlPack.HasFlag(ControllingCommand.Confirm);
                SendHintData(HintEventType.ShowTutorialFailed,true);
            }
            
        }

        protected override void AdditionalArtLevelReference(ref GameAssets LevelAsset)
        {
            LevelAsset.TimeLine = FindObjectOfType<TimeLine>();
        }

        protected override void AdditionalInitLevel()
        {
            LevelAsset.TimeLine.SetNoCount();
        }
    }
}