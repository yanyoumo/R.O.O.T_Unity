﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public class TutorialTimeLineAdvNeo : TutorialLogic
    {
        /*private bool LevelCompleted = false;
        private bool PlayerRequestedEnd = false;*/
        private bool OnceFlagA = false;
        private bool OnceFlagB = false;

        protected override void Update()
        {
            base.Update();


            if (ReadyToGo)
            {
                if (ActionIndex == 2)
                {
                    //Debug.Log("HERE");
                    if (!OnceFlagA)
                    {
                        OnceFlagA = true;
                        LevelAsset.TimeLine.InitWithTokens(LevelAsset.ActionAsset.TimeLineTokens);
                        LevelAsset.TimeLine.SetGoalCount = LevelAsset.ActionAsset.TargetCount;
                    }
                }
            }

            if (ActionEnded)
            {
                bool timeLineGood = RequirementSatisfiedCycleCount >= LevelAsset.ActionAsset.TargetCount;
                LevelAsset.HintMaster.TutorialCheckList.MainGoalCompleted = timeLineGood;
                LevelAsset.HintMaster.TutorialCheckList.SecondaryGoalCompleted = !OnceFlagB;
                LevelCompleted = (!OnceFlagB) && timeLineGood;
            }

            if (LevelCompleted)
            {
                PlayerRequestedEnd = CtrlPack.HasFlag(ControllingCommand.NextButton);
            }

            foreach (var actionAssetTimeLineToken in LevelAsset.ActionAsset.TimeLineTokens)
            {
                if (actionAssetTimeLineToken.type == TimeLineTokenType.Ending)
                {
                    if (actionAssetTimeLineToken.InRange(LevelAsset._StepCount))
                    {
                        OnceFlagB = true;
                    }
                }
            }
        }

        protected override string MainGoalEntryContent => "满足时间线上要求计数";
        protected override string SecondaryGoalEntryContent => "在时间线结束之前完成任务";

        protected override bool UpdateGameOverStatus(GameAssets currentLevelAsset)
        {
            base.UpdateCareerGameOverStatus(currentLevelAsset);
            LevelAsset.TimeLine.SetCurrentCount = RequirementSatisfiedCycleCount;
            if (LevelCompleted && PlayerRequestedEnd)
            {
                PendingCleanUp = true;
                LevelMasterManager.Instance.LevelFinished(LevelAsset);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool AllUnitConnected()
        {
            return LevelAsset.GameBoard.Units.All(gameBoardUnit => gameBoardUnit.Value.GetComponentInChildren<Unit>().AnyConnection);
        }

        protected void InitCurrencyIoMgr()
        {
            LevelAsset.BoardDataCollector = gameObject.AddComponent<BoardDataCollector>();
            LevelAsset.BoardDataCollector.m_Board = LevelAsset.GameBoard;
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
            LevelAsset.GameBoard.InitBoardWAsset(LevelAsset.ActionAsset);
            LevelAsset.GameBoard.UpdateBoardAnimation();
            InitCursor(new Vector2Int(2, 3));
            InitShop();
            StartShop();

            LevelAsset.DisableAllCoreFunctionAndFeature();
            LevelAsset.InputEnabled = true;
            LevelAsset.CursorEnabled = true;
            LevelAsset.RotateEnabled = true;
            LevelAsset.HintEnabled = true;
            LevelAsset.CurrencyEnabled = true;
            LevelAsset.GameOverEnabled = true;
            LevelAsset.CycleEnabled = true;
            LevelAsset.ShopEnabled = true;
            LevelAsset.LCDCurrencyEnabled = true;


            ReadyToGo = true;
        }
    }
}