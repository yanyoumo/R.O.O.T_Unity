using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    //TODO!!
    public class TutorialShopNeo : TutorialLogic
    {
        protected override void Update()
        {
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
                        LevelAsset.TimeLine.InitWithTokens(LevelAsset.ActionAsset.TimeLineTokens);
                    }
                }
                else if (ActionIndex == 3)
                {
                    LevelAsset.LCDCurrencyEnabled = true;
                }
            }

            if (ActionEnded)
            {
                LevelCompleted = false;
            }

            if (LevelCompleted)
            {
                PlayerRequestedEnd = CtrlPack.HasFlag(ControllingCommand.NextButton);
            }
        }

        protected override string MainGoalEntryContent => "成功购买5个单元";

        protected override bool UpdateGameOverStatus(GameAssets currentLevelAsset)
        {
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
            LevelAsset.GameBoard.UpdateBoardAnimation();
            //LevelAsset.StartingScoreSet = new ScoreSet(1000, 999);
            //LevelAsset.StartingPerMoveData = new PerMoveData();
            InitCursor(new Vector2Int(2, 3));

            //TODO 还要把成本去掉。（这个时候还不教学成本
            LevelAsset.DisableAllCoreFunctionAndFeature();
            LevelAsset.InputEnabled = true;
            LevelAsset.CursorEnabled = true;
            LevelAsset.RotateEnabled = true;
            LevelAsset.HintEnabled = true;
            LevelAsset.CurrencyEnabled = true;
            LevelAsset.GameOverEnabled = true;
            LevelAsset.CycleEnabled = true;

            ReadyToGo = true;
        }

        protected void InitShop()
        {
            LevelAsset.ShopMgr = gameObject.AddComponent<ShopMgr>();
            LevelAsset.ShopMgr.UnitTemplate = LevelAsset.GameBoard.UnitTemplate;
            LevelAsset.ShopMgr.ShopInit();
            LevelAsset.ShopMgr.ItemPriceTexts_TMP = new[] { LevelAsset.Item1PriceTmp, LevelAsset.Item2PriceTmp, LevelAsset.Item3PriceTmp, LevelAsset.Item4PriceTmp };
            LevelAsset.ShopMgr.CurrentGameStateMgr = LevelAsset.GameStateMgr;
            LevelAsset.ShopMgr.GameBoard = LevelAsset.GameBoard;
        }
    }
}