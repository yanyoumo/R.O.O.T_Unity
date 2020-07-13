using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public class ShortEndingLevelMgr : BaseLevelMgr
    {
        public override void InitLevel(ScoreSet scoreSet = null, PerMoveData perMoveData = new PerMoveData())
        {
            Debug.Assert(ReferenceOk);//意外的有确定Reference的……还行……
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));

            LevelAsset.DeltaCurrency = 0.0f;
            LevelAsset.GameStateMgr = new StandardGameStateMgr();
            LevelAsset.GameStateMgr.InitGameMode(new ScoreSet(10, 3), new PerMoveData());
            //LevelAsset.EnableAllFeature();
            LevelAsset.DisableAllFeature();
            LevelAsset.InputEnabled = true;
            LevelAsset.CursorEnabled = true;
            LevelAsset.PlayerDataUiEnabled = true;
            LevelAsset.GameOverEnabled = true;

            InitCursor(new Vector2Int(2, 3));
            LevelAsset.GameBoard.InitBoardRealStart();
            LevelAsset.GameBoard.UpdateBoardAnimation();

            ReadyToGo = true;
            InvokeGameStartedEvent();
        }

        protected override void InitDestoryer(){}
        protected override void InitShop(){}

        protected override void InitGameStateMgr(){}
        protected override void InitCurrencyIoMgr(){}

        protected override void InitCursor(Vector2Int pos)
        {
            LevelAsset.GameCursor = Instantiate(LevelAsset.CursorTemplate);
            Cursor cursor = LevelAsset.GameCursor.GetComponent<Cursor>();
            cursor.InitPosWithAnimation(pos);
            cursor.UpdateTransform(LevelAsset.GameBoard.GetFloatTransformAnimation(cursor.LerpingBoardPosition));
        }

        protected override bool UpdateGameOverStatus(GameAssets currentLevelAsset)
        {
            //这个函数就很接近裁判要做的事儿了。
            if (currentLevelAsset.GameStateMgr.EndGameCheck(new ScoreSet(10,3), new PerMoveData()))
            {
                GameMasterManager.UpdateGameGlobalStatuslastEndingIncome(0);
                GameMasterManager.UpdateGameGlobalStatuslastEndingTime(0);
                //此时要把GameOverScene所需要的内容填好。
                PendingCleanUp = true;
                GameMasterManager.Instance.LevelFinished();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}