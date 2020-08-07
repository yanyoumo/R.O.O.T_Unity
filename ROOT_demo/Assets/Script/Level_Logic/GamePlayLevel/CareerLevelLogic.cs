using System;
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
        //public override LevelType GetLevelType => LevelType.PlayLevel;

        public int RequirementSatisfiedCycleCount = 0;

        public override void InitLevel(ScoreSet scoreSet = null, PerMoveData perMoveData = new PerMoveData())
        {
            Debug.Assert(ReferenceOk);//意外的有确定Reference的……还行……
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));

            InitCurrencyIoMgr();
            LevelAsset.DeltaCurrency = 0.0f;

            LevelAsset.GameStateMgr = new StandardGameStateMgr();
            LevelAsset.GameStateMgr.InitGameMode(scoreSet ?? new ScoreSet(), perMoveData);

            InitShop();
            InitDestoryer();
            InitCursor(new Vector2Int(2, 3));
            LevelAsset.EnableAllCoreFunctionAndFeature();
            LevelAsset.GameBoard.InitBoardRealStart();
            LevelAsset.GameBoard.UpdateBoardAnimation();
            StartShop();

            ReadyToGo = true;

            LevelAsset.StartingScoreSet = scoreSet;
            LevelAsset.StartingPerMoveData = perMoveData;
            if (LevelAsset.ActionAsset.TimeLineTokens.Length>0)
            {
                LevelAsset.TimeLine.InitWithTokens(LevelAsset.ActionAsset.TimeLineTokens);
            }
        }

        protected void InitDestoryer()
        {
            LevelAsset.WarningDestoryer = new MeteoriteBomber();
            LevelAsset.WarningDestoryer.SetBoard(ref LevelAsset.GameBoard);
            LevelAsset.WarningDestoryer.Init(5, 2);
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
            //这个函数就很接近裁判要做的事儿了。
            int NormalRval=0;
            int NetworkRval=0;

            foreach (var actionAssetTimeLineToken in currentLevelAsset.ActionAsset.TimeLineTokens)
            {
                if (actionAssetTimeLineToken.InRange(currentLevelAsset._StepCount))
                {
                    if (actionAssetTimeLineToken.type == TimeLineTokenType.Ending)
                    {
                        //TODO 这里还要判断满足了多少周期。
                        PendingCleanUp = true;
                        LevelMasterManager.Instance.LevelFinished(LevelAsset);
                        return true;
                    }
                    else if(actionAssetTimeLineToken.type == TimeLineTokenType.RequireNormal)
                    {
                        NormalRval += actionAssetTimeLineToken.RequireAmount;
                    }
                    else if (actionAssetTimeLineToken.type == TimeLineTokenType.RequireNetwork)
                    {
                        NetworkRval += actionAssetTimeLineToken.RequireAmount;
                    }
                }
            }
            if (NormalRval==0&&NetworkRval==0)
            {
                currentLevelAsset.TimeLine.RequirementSatisfied = true;
            }
            else
            {
                bool valA=(Mathf.FloorToInt(currentLevelAsset.BoardDataCollector.CalculateProcessorScore())>=NormalRval);
                bool valB=(Mathf.FloorToInt(currentLevelAsset.BoardDataCollector.CalculateServerScore())>=NetworkRval);
                currentLevelAsset.TimeLine.RequirementSatisfied = valA && valB;
            }

            if (currentLevelAsset.TimeLine.RequirementSatisfied)
            {
                RequirementSatisfiedCycleCount++;
            }

            return false;
        }

        protected override void Update()
        {
            base.Update();

            LevelAsset.DestroyerEnabled = false;
            foreach (var actionAssetTimeLineToken in LevelAsset.ActionAsset.TimeLineTokens)
            {
                if (actionAssetTimeLineToken.type == TimeLineTokenType.DestoryerIncome)
                {
                    LevelAsset.DestroyerEnabled = actionAssetTimeLineToken.InRange(LevelAsset._StepCount);
                    if (LevelAsset.DestroyerEnabled)
                    {
                        break;
                    }
                }
            }
        }
    }
}