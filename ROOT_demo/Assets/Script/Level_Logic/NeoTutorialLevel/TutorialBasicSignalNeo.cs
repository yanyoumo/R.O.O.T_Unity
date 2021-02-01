using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public class TutorialBasicSignalNeo : TutorialBasicControlNeo
    {
        protected override void AddtionalDealStep(TutorialActionData data) { }

        protected override string MainGoalEntryContent => "将所有接收端单元都链接至发送端";

        public override void InitLevel()
        {
            Debug.Assert(ReferenceOk);//意外的有确定Reference的……还行……
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));
            LevelAsset.DeltaCurrency = 0.0f;
            LevelAsset.GameStateMgr = new GameStateMgr();
            LevelAsset.GameStateMgr.InitGameMode(LevelAsset.ActionAsset.GameModeAsset);
            LevelAsset.GameBoard.UpdateBoardAnimation();
            WorldExecutor.InitCursor(ref LevelAsset,new Vector2Int(2, 3));
            
            ReadyToGo = true;
        }
        
        /*protected override void TutorialMinorUpkeep()
        {
            LevelCompleted = AllUnitConnected();
            LevelFailed = !LevelCompleted;
            if (ActionEnded)
            {
                LevelAsset.HintMaster.TutorialCheckList.MainGoalCompleted = LevelCompleted = AllUnitConnected();
            }
        }
        
        protected override string MainGoalEntryContent => "将所有接收端单元都链接至发送端";
        protected override void AddtionalDealStep(TutorialActionData data)
        {
            //throw new NotImplementedException();
        }

        protected override void AdditionalFSMActionsOperating(ref Dictionary<RootFSMStatus, Action> actions)
        {
            //throw new NotImplementedException();
        }

        protected override void AdditionalFSMTransitionOperating(ref HashSet<RootFSMTransition> transitions)
        {
            //throw new NotImplementedException();
        }

        public override void InitLevel()
        {
            Debug.Assert(ReferenceOk);//意外的有确定Reference的……还行……
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));
            LevelAsset.DeltaCurrency = 0.0f;
            LevelAsset.GameStateMgr = new GameStateMgr();
            LevelAsset.GameStateMgr.InitGameMode(LevelAsset.ActionAsset.GameModeAsset);
            LevelAsset.GameBoard.UpdateBoardAnimation();
            WorldExecutor.InitCursor(ref LevelAsset,new Vector2Int(2, 3));
            
            ReadyToGo = true;
        }*/

        /*private bool LevelCompleted = false;
        private bool PlayerRequestedEnd = false;*/

        /*protected override void Update()
        {
            base.Update();
            if (ActionEnded)
            {
                LevelAsset.HintMaster.TutorialCheckList.MainGoalCompleted = LevelCompleted = AllUnitConnected();
            }

            if (LevelCompleted)
            {
                PlayerRequestedEnd = CtrlPack.HasFlag(ControllingCommand.NextButton);
            }
        }


        public override void InitLevel()
        {
            Debug.Assert(ReferenceOk);//意外的有确定Reference的……还行……
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));

            InitCurrencyIoMgr();
            LevelAsset.DeltaCurrency = 0.0f;

            LevelAsset.DeltaCurrency = 0.0f;
            LevelAsset.GameStateMgr = new GameStateMgr();
            LevelAsset.GameStateMgr.InitGameMode(LevelAsset.ActionAsset.GameModeAsset);
            LevelAsset.GameBoard.UpdateBoardAnimation();
            //LevelAsset.StartingScoreSet = new ScoreSet();
            //LevelAsset.StartingPerMoveData = new PerMoveData();
            WorldExecutor.InitCursor(ref LevelAsset,new Vector2Int(2, 3));


            LevelAsset.DisableAllCoreFunctionAndFeature();
            LevelAsset.InputEnabled = true;
            LevelAsset.CursorEnabled = true;
            LevelAsset.RotateEnabled = true;
            LevelAsset.HintEnabled = true;
            LevelAsset.CurrencyEnabled = true;
            LevelAsset.GameOverEnabled = true;

            ReadyToGo = true;
        }*/
    }
}