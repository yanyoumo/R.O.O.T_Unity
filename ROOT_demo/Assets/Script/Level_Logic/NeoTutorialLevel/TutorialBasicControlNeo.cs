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
                LevelAsset.HintMaster.TutorialCheckList.MainGoalCompleted = LevelCompleted = AllUnitConnected();
            }
        }

        protected override void AdditionalArtLevelReference(ref GameAssets LevelAsset)
        {
            LevelAsset.HintMaster.HideTutorialFrame = false;
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
            base.AddtionalMajorUpkeep();
        }
        
        protected override void AdditionalFSMActionsOperating(ref FSMActions actions)
        {
            if (actions.ContainsKey(RootFSMStatus.MajorUpKeep))
            {
                actions.Remove(RootFSMStatus.MajorUpKeep);
                actions.Add(RootFSMStatus.MajorUpKeep, MajorUpkeepAction_TutorialBasicControlNeo);
            }
        }

        public override void InitLevel()
        {
            //TODO 这个也要将Career和Boss尽量拆干净。
            Debug.Assert(ReferenceOk); //意外的有确定Reference的……还行……
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));

            LevelAsset.DeltaCurrency = 0.0f;
            LevelAsset.GameCurrencyMgr = new GameCurrencyMgr();
            LevelAsset.GameCurrencyMgr.InitGameMode(LevelAsset.ActionAsset.GameStartingData);
            LevelAsset.EnableAllCoreFunctionAndFeature();
            LevelAsset.GameBoard.InitBoardWAsset(LevelAsset.ActionAsset);
            LevelAsset.GameBoard.UpdateBoardAnimation();
            
            ReadyToGo = true;
            LevelAsset.HintMaster.ShouldShowCheckList = false;
        }
        
        protected override void AdditionalFSMTransitionOperating(ref FSMTransitions transitions)
        {
            //throw new NotImplementedException();
        }
    }
}