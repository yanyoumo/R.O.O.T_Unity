using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public class TutorialSignalBasicMgr : BaseTutorialMgr
    {
        protected override void Update()
        {
            base.Update();
            if (ReadyToGo)
            {
                if (ActionIndex == 3)
                {
                    LevelAsset.CurrencyEnabled = true;
                    LevelAsset.ForceHddConnectionHint = true;
                }
                if (ActionIndex==5)
                {
                    LevelAsset.ForceServerConnectionHint = true;
                }
                if (ActionIndex == 6)
                {
                    LevelAsset.ForceHddConnectionHint = false;
                    LevelAsset.ForceServerConnectionHint = false;
                }
            }
        }

        protected override bool UpdateGameOverStatus(GameAssets currentLevelAsset)
        {
            //教程的结束一般都是在DealStep里面处理。
            return false;
        }

        public override void InitLevel(ScoreSet scoreSet = null, PerMoveData perMoveData = new PerMoveData())
        {
            Debug.Assert(ReferenceOk);//意外的有确定Reference的……还行……
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));

            InitCurrencyIoMgr();
            LevelAsset.DeltaCurrency = 0.0f;
            LevelAsset.GameStateMgr = new StandardGameStateMgr();
            LevelAsset.GameStateMgr.InitGameMode(scoreSet ?? new ScoreSet(), perMoveData);
            //LevelAsset.GameBoard.InitBoardRealStart();
            LevelAsset.GameBoard.UpdateBoardAnimation();
            LevelAsset.StartingScoreSet = new ScoreSet();
            LevelAsset.StartingPerMoveData = new PerMoveData();

            LevelAsset.DisableAllCoreFunctionAndFeature();
            LevelAsset.InputEnabled = true;
            LevelAsset.HintEnabled = true;
            LevelAsset.CursorEnabled = true;
            LevelAsset.RotateEnabled = true;

            InitCursor(new Vector2Int(2, 3));
            TutorialActionAsset = Resources.Load<TutorialActionAsset>("TutorialActionAsset/TutorialActionSignalBasic");

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