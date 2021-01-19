using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public class TutorialShopNeo : TutorialLogic
    {
        private int BoughtCount = 0;
        private readonly int BoughtCountTarget = 5;

        protected override void Update()
        {
            bool haventBought = LevelAsset.BoughtOnce;

            base.Update();

            if (ReadyToGo)
            {
                if (ActionIndex == 2)
                {
                    if (!LevelAsset.ShopEnabled)
                    {
                        InitShop();
                        StartShop();
                        LevelAsset.ShopEnabled = true;
                        WorldCycler.InitCycler();
                        LevelAsset.TimeLine.InitWithAssets(LevelAsset);
                    }
                }
                else if (ActionIndex == 3)
                {
                }
            }

            if (!haventBought&&LevelAsset.BoughtOnce)
            {
                BoughtCount++;
            }

            if (ActionEnded)
            {
                LevelAsset.HintMaster.TutorialCheckList.MainGoalCompleted = LevelCompleted = (BoughtCount >= BoughtCountTarget);
            }
            
            if (LevelCompleted)
            {
                PlayerRequestedEnd = CtrlPack.HasFlag(ControllingCommand.NextButton);
            }
        }

        protected override string MainGoalEntryContent => "购买5个单元";

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
            WorldExecutor.InitCursor(ref LevelAsset,new Vector2Int(2, 3));


            LevelAsset.DisableAllCoreFunctionAndFeature();
            LevelAsset.InputEnabled = true;
            LevelAsset.CursorEnabled = true;
            LevelAsset.RotateEnabled = true;
            LevelAsset.HintEnabled = true;
            LevelAsset.CurrencyEnabled = true;
            LevelAsset.GameOverEnabled = true;

            LevelAsset.TimeLine.SetNoCount();

            ReadyToGo = true;
        }
    }
}