﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public class TutorialShopMgr : BaseTutorialMgr
    {
        protected override void Update()
        {
            base.Update();
            if (ReadyToGo)
            {
                if (ActionIndex==5)
                {
                    LevelAsset.LCDEnabled = true;
                    LevelAsset.PlayerDataUiEnabled = true;
                }
            }
        }

        protected override bool UpdateGameOverStatus(GameAssets currentLevelAsset)
        {
            //教程的结束一般都是在DealStep里面处理。
            //TODO 这里开启时间后，DealStep的结束和WorldLogic的结束逻辑就有可能冲突。
            return false;
        }

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

            LevelAsset.DisableAllFeature();
            LevelAsset.InputEnabled = true;
            LevelAsset.CursorEnabled = true;
            LevelAsset.RotateEnabled = true;
            LevelAsset.HintEnabled = true;
            LevelAsset.UpdateDeltaCurrencyEnabled = true;

            InitCursor(new Vector2Int(2, 3));
            
            TutorialAction = new TutorialActionShop();

            ReadyToGo = true;
            InvokeGameStartedEvent();
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
            LevelAsset.CurrencyIoCalculator = gameObject.AddComponent<CurrencyIOCalculator>();
            LevelAsset.CurrencyIoCalculator.m_Board = LevelAsset.GameBoard;
        }

        protected void InitCursor(Vector2Int pos)
        {
            LevelAsset.GameCursor = Instantiate(LevelAsset.CursorTemplate);
            Cursor cursor = LevelAsset.GameCursor.GetComponent<Cursor>();
            cursor.InitPosWithAnimation(pos);
            cursor.UpdateTransform(LevelAsset.GameBoard.GetFloatTransformAnimation(cursor.LerpingBoardPosition));
        }
    }
}