﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public class TutorialShopLogic : TutorialLogic
    {


        protected override void Update()
        {
            base.Update();
            if (ReadyToGo)
            {
                if (ActionIndex == 1)
                {
                    if (!LevelAsset.ShopEnabled)
                    {
                        InitShop();
                        StartShop();
                        LevelAsset.ShopEnabled = true;
                    }
                }
            }
        }

        protected override bool UpdateGameOverStatus(GameAssets currentLevelAsset)
        {
            //教程的结束一般都是在DealStep里面处理。
            //TODO 这里开启时间后，DealStep的结束和WorldLogic的结束逻辑就有可能冲突。
            return false;
        }

        //public override LevelType GetLevelType => LevelType.TutorialActionShop;

        public override void InitLevel(ScoreSet scoreSet = null, PerMoveData perMoveData = new PerMoveData())
        {
            Debug.Assert(ReferenceOk);//意外的有确定Reference的……还行……
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));

            InitCurrencyIoMgr();
            LevelAsset.DeltaCurrency = 0.0f;
            LevelAsset.GameStateMgr = new StandardGameStateMgr();
            LevelAsset.GameStateMgr.InitGameMode(new ScoreSet(), new PerMoveData());
            //LevelAsset.GameBoard.InitBoardRealStart();
            LevelAsset.GameBoard.UpdateBoardAnimation();
            LevelAsset.StartingScoreSet = new ScoreSet();
            LevelAsset.StartingPerMoveData = new PerMoveData();

            LevelAsset.DisableAllCoreFunctionAndFeature();
            LevelAsset.InputEnabled = true;
            LevelAsset.CursorEnabled = true;
            LevelAsset.RotateEnabled = true;
            LevelAsset.HintEnabled = true;
            LevelAsset.CurrencyEnabled = true;
            LevelAsset.LCDCurrencyEnabled = true;
            LevelAsset.CycleEnabled = true;

            InitCursor(new Vector2Int(2, 3));

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
                throw new ArgumentOutOfRangeException();
            }
        }

        protected void InitCurrencyIoMgr()
        {
            LevelAsset.BoardDataCollector = gameObject.AddComponent<BoardDataCollector>();
            LevelAsset.BoardDataCollector.m_Board = LevelAsset.GameBoard;
        }

        protected void InitCursor(Vector2Int pos)
        {
            LevelAsset.GameCursor = Instantiate(LevelAsset.CursorTemplate);
            Cursor cursor = LevelAsset.GameCursor.GetComponent<Cursor>();
            cursor.InitPosWithAnimation(pos);
            cursor.UpdateTransform(LevelAsset.GameBoard.GetFloatTransformAnimation(cursor.LerpingBoardPosition));
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