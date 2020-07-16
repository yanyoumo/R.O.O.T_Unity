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
            LevelAsset.DisableAllCoreFunctionAndFeature();
            LevelAsset.InputEnabled = true;
            LevelAsset.CursorEnabled = true;
            LevelAsset.CycleEnabled = true;
            LevelAsset.GameOverEnabled = true;

            InitCursor(new Vector2Int(2, 3));
            LevelAsset.GameBoard.InitBoardRealStart();
            LevelAsset.GameBoard.UpdateBoardAnimation();

            LevelAsset.StartingScoreSet = scoreSet;
            LevelAsset.StartingPerMoveData = perMoveData;

            ReadyToGo = true;
            InvokeGameStartedEvent();
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