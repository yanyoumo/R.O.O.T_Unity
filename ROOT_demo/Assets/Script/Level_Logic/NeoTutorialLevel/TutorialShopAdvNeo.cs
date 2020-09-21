using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public class TutorialShopAdvNeo : TutorialLogic
    {
        protected override void Update()
        {
            base.Update();

            if (ReadyToGo)
            {
                if (ActionIndex == 8)
                {
                    if (!LevelAsset.InputEnabled)
                    {
                        LevelAsset.InputEnabled = true;
                    }
                }
            }

            if (ActionEnded)
            {
                LevelAsset.HintMaster.TutorialCheckList.MainGoalCompleted = (LevelAsset.DeltaCurrency>0);
                LevelAsset.HintMaster.TutorialCheckList.SecondaryGoalCompleted = !OnceFlagB;
                LevelCompleted = (!OnceFlagB) && (LevelAsset.DeltaCurrency > 0);
            }
            
            if (LevelCompleted)
            {
                PlayerRequestedEnd = CtrlPack.HasFlag(ControllingCommand.NextButton);
            }

            if (LevelActionAsset.HasEnded(LevelAsset.StepCount))
            {
                LevelFailed = true;
                OnceFlagB = true;
            }

            if (LevelFailed)
            {
                PlayerRequestedQuit = CtrlPack.HasFlag(ControllingCommand.NextButton);
                LevelAsset.HintMaster.TutorialCheckList.TutorialFailed = true;
            }
        }

        protected override string MainGoalEntryContent => "将这个棋盘扭亏为盈";
        protected override string SecondaryGoalEntryContent => "在时间线结束之前完成任务";

        public override void InitLevel()
        {
            Debug.Assert(ReferenceOk);//意外的有确定Reference的……还行……
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));

            InitCurrencyIoMgr();
            LevelAsset.DeltaCurrency = 0.0f;

            LevelAsset.DeltaCurrency = 0.0f;
            LevelAsset.GameStateMgr = new GameStateMgr();
            LevelAsset.GameStateMgr.InitGameMode(LevelAsset.ActionAsset.GameModeAsset);
            LevelAsset.GameBoard.InitBoardWAsset(LevelActionAsset);
            LevelAsset.GameBoard.UpdateBoardAnimation();
            InitCursor(new Vector2Int(2, 3));

            LevelAsset.DisableAllCoreFunctionAndFeature();
            //LevelAsset.InputEnabled = true;
            LevelAsset.CursorEnabled = true;
            LevelAsset.RotateEnabled = true;
            LevelAsset.HintEnabled = true;
            LevelAsset.CurrencyEnabled = true;
            LevelAsset.GameOverEnabled = true;
            LevelAsset.CycleEnabled = true;
            LevelAsset.LCDCurrencyEnabled=true;
            LevelAsset.LCDDeltaCurrencyEnabled = true;
            
            LevelAsset.StepCount = 0;
            LevelAsset.TimeLine.InitWithAssets(LevelAsset);
            LevelAsset.TimeLine.SetNoCount();

            ReadyToGo = true;
        }
    }
}