﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public class TutorialBasicControlNeo : TutorialLogic
    {
        /*private bool LevelCompleted = false;
        private bool PlayerRequestedEnd = false;*/

        protected override void Update()
        {
            base.Update();
            if (ReadyToGo)
            {
                if (ActionIndex==3)
                {
                    LevelAsset.InputEnabled = true;
                    LevelAsset.CursorEnabled = true;
                    LevelAsset.RotateEnabled = true;
                }
            }
            if (ActionEnded)
            {
                LevelAsset.HintMaster.TutorialCheckList.MainGoalCompleted = LevelCompleted = AllUnitConnected();
            }

            if (LevelCompleted)
            {
                PlayerRequestedEnd = CtrlPack.HasFlag(ControllingCommand.NextButton);
            }
        }

        protected override string MainGoalEntryContent => "将所有单元链接起来";

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

            LevelAsset.DisableAllCoreFunctionAndFeature();
            LevelAsset.HintEnabled = true;
            LevelAsset.CurrencyEnabled = true;
            LevelAsset.GameOverEnabled = true;

            ReadyToGo = true;
        }

        /// <summary>
        /// 如果对通用动作没有重载的话，就是直接使用父类的。父类要是没有的话，会抛出NotImplementedException。
        /// </summary>
        /// <param name="data">输入的TutorialActionData</param>
        protected override void DealStep(TutorialActionData data)
        {
            try
            {
                base.DealStep(data);
            }
            catch (NotImplementedException)
            {
                switch (data.ActionType)
                {
                    case TutorialActionType.CreateCursor:
                        InitCursor(data.Pos);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}