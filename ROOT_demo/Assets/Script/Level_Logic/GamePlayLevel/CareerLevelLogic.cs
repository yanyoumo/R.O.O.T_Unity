﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace ROOT
{
    public class CareerLevelLogic : LevelLogic //LEVEL-LOGIC/每一关都有一个这个类。
    {
        public override void InitLevel()
        {
            Debug.Assert(ReferenceOk);//意外的有确定Reference的……还行……
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));

            InitCurrencyIoMgr();
            LevelAsset.DeltaCurrency = 0.0f;

            LevelAsset.GameStateMgr = new GameStateMgr();
            LevelAsset.GameStateMgr.InitGameMode(LevelAsset.ActionAsset.GameModeAsset);

            InitShop();
            InitDestoryer();
            InitCursor(new Vector2Int(2, 3));
            LevelAsset.EnableAllCoreFunctionAndFeature();
            LevelAsset.GameBoard.InitBoardWAsset(LevelAsset.ActionAsset);
            LevelAsset.GameBoard.UpdateBoardAnimation();
            StartShop();

            ReadyToGo = true;
            if (LevelAsset.ActionAsset.RoundDatas.Length>0)
            {
                LevelAsset.StepCount = 0;
                LevelAsset.TimeLine.InitWithAssets(LevelAsset);
            }
            LevelAsset.TimeLine.SetGoalCount = LevelAsset.ActionAsset.TargetCount;
        }

        protected void InitDestoryer()
        {
            LevelAsset.WarningDestoryer = new MeteoriteBomber();
            LevelAsset.WarningDestoryer.SetBoard(ref LevelAsset.GameBoard);
            LevelAsset.WarningDestoryer.Init(4, 1);
        }
        protected void InitShop()
        {
            LevelAsset.ShopMgr.ShopInit(LevelAsset);
            LevelAsset.ShopMgr.CurrentGameStateMgr = LevelAsset.GameStateMgr;
            LevelAsset.ShopMgr.GameBoard = LevelAsset.GameBoard;
            if (LevelAsset.ActionAsset.ExcludedShop)
            {
                LevelAsset.ShopMgr.excludedTypes = LevelAsset.ActionAsset.ShopExcludedType;
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

        protected override bool UpdateGameOverStatus(GameAssets currentLevelAsset)
        {
            bool res= UpdateCareerGameOverStatus(currentLevelAsset);
            LevelAsset.TimeLine.SetCurrentCount = RequirementSatisfiedCycleCount;
            return res;
        }

        private int obsoletedID = -1;
        protected override void Update()
        {
            base.Update();

            LevelAsset.DestroyerEnabled = false;
            var roundGist = LevelAsset.ActionAsset.GetRoundGistByStep(LevelAsset.StepCount);
            if (!roundGist.HasValue) return;
            var gist = roundGist.Value;

            LevelAsset.DestroyerEnabled = (gist.Type == StageType.Destoryer);
            //TODO 目前这个还没有HeatSinkSwitch的处理。
            LevelAsset.LevelProgress = LevelAsset.StepCount / (float) LevelAsset.ActionAsset.PlayableCount;
        }
    }
}