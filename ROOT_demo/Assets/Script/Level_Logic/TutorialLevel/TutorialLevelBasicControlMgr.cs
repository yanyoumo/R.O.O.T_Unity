using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public class TutorialLevelBasicControlMgr : BaseTutorialMgr
    {
        protected override void Update()
        {
            base.Update();
            if (ReadyToGo)
            {
                if (ActionIndex==2)
                {
                    LevelAsset.InputEnabled = true;
                    LevelAsset.CursorEnabled = true;
                    LevelAsset.RotateEnabled = true;
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

            LevelAsset.DeltaCurrency = 0.0f;
            LevelAsset.GameStateMgr = new StandardGameStateMgr();
            LevelAsset.GameStateMgr.InitGameMode(scoreSet ?? new ScoreSet(), perMoveData);
            //LevelAsset.GameBoard.InitBoardRealStart();
            LevelAsset.GameBoard.UpdateBoardAnimation();
            LevelAsset.StartingScoreSet = new ScoreSet();
            LevelAsset.StartingPerMoveData = new PerMoveData();

            LevelAsset.DisableAllFeature();

            TutorialAction =new TutorialActionBasicControl();

            ReadyToGo = true;
            InvokeGameStartedEvent();
        }

        protected override void DealStep(TutorialActionData data)
        {
            switch (data.ActionType)
            {
                case TutorialActionType.Text:
                    Debug.Log(data.Text);
                    break;
                case TutorialActionType.CreateCursor:
                    InitCursor(new Vector2Int(2, 3));
                    break;
                case TutorialActionType.CreateUnit:
                    CreateUnitOnBoard(data);
                    break;
                case TutorialActionType.End:
                    Debug.Log("Current TutorialCompleteReached");
                    //TODO 这里的tutorialFinish还可以优化一下。
                    LevelMasterManager.Instance.LevelFinished(LevelAsset);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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