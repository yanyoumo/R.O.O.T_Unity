using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public class TutorialBasicSignalNeo : TutorialLogic
    {
        /*private bool LevelCompleted = false;
        private bool PlayerRequestedEnd = false;*/

        protected override void Update()
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

        protected override string MainGoalEntryContent => "将所有接收端单元都链接至发送端";

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
        }
    }
}