using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ROOT.SetupAsset;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    using FSMActions = Dictionary<RootFSMStatus, Action>;
    using Trans = RootFSMTransition;
    using FSMTransitions = HashSet<RootFSMTransition>;

    public class TutorialBasicControlNeo : FSMTutorialLogic
    {
        protected override string MainGoalEntryContent => "将所有单元链接起来";

        protected override void TutorialMinorUpkeep()
        {
            LevelCompleted = AllUnitConnected();
            LevelFailed = !LevelCompleted;
            if (ActionEnded)
            {
                LevelCompleted = AllUnitConnected();
                SendHintData(HintEventType.ShowMainGoalComplete, LevelCompleted);
            }
        }

        protected override void AdditionalArtLevelReference(ref GameAssets LevelAsset)
        {
            SendHintData(HintEventType.ShowTutorialTextFrame, false);
        }

        protected override void AddtionalDealStep(TutorialActionData data)
        {
            switch (data.ActionType)
            {
                case TutorialActionType.CreateCursor:
                    WorldExecutor.InitCursor(ref LevelAsset,data.Pos);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void MajorUpkeepAction_TutorialBasicControlNeo()
        {
            _ctrlPack = _actionDriver.CtrlQueueHeader;
            UpdateBoardData_Stepped(ref LevelAsset);
            base.AdditionalMajorUpkeep();
        }
        
        protected override void AdditionalFSMActionsOperating(ref FSMActions actions)
        {
            if (actions.ContainsKey(RootFSMStatus.MajorUpKeep))
            {
                actions.Remove(RootFSMStatus.MajorUpKeep);
                actions.Add(RootFSMStatus.MajorUpKeep, MajorUpkeepAction_TutorialBasicControlNeo);
            }
        }

        protected override void AdditionalInitLevel()
        {
            //WorldExecutor.InitCursor(ref LevelAsset,new Vector2Int(2, 3));
        }
        
        protected override void AdditionalFSMTransitionOperating(ref FSMTransitions transitions)
        {
            //throw new NotImplementedException();
        }
    }
}