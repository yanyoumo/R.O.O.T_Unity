using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public class ShortEndingLevelLogic : BranchingLevelLogic
    {
        //public override LevelType GetLevelType => LevelType.SimpleLevel;

        public override void InitLevel()
        {
            Debug.Assert(ReferenceOk);//意外的有确定Reference的……还行……
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));

            LevelAsset.DeltaCurrency = 0.0f;
            LevelAsset.GameStateMgr = new GameStateMgr();
            LevelAsset.GameStateMgr.InitGameMode(LevelAsset.ActionAsset.GameModeAsset);
            LevelAsset.DisableAllCoreFunctionAndFeature();
            LevelAsset.InputEnabled = true;
            LevelAsset.CursorEnabled = true;
            LevelAsset.CycleEnabled = true;
            LevelAsset.GameOverEnabled = true;

            WorldExecutor.InitCursor(ref LevelAsset,new Vector2Int(2, 3));
            //LevelAsset.GameBoard.InitBoardRealStart();
            LevelAsset.GameBoard.UpdateBoardAnimation();

            //LevelAsset.StartingScoreSet = scoreSet;
//LevelAsset.StartingPerMoveData = new PerMoveData();

            ReadyToGo = true;
        }
    }
}