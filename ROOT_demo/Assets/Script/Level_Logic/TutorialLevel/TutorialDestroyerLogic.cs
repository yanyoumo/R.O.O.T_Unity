using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public partial class TutorialDestroyerLogic : TutorialLogic
    {
        private bool windingOnce = false;

        IEnumerator ForceWindingDestroyer()
        {
            while (LevelAsset.WarningDestoryer.GetStatus()!=WarningDestoryerStatus.Striking)
            {
                yield return 0;
                LevelAsset.WarningDestoryer.Step();
            }
        }

        protected override void Update()
        {
            base.Update();
            if (ReadyToGo)
            {
                if (ActionIndex == 0)
                {
                    if (!windingOnce)
                    {
                        windingOnce = true;
                        //这里有个问题，就是这个有可能出现在说明框后面。
                        //现在把框改小了，凑活这样吧。
                        ForceSetWarningDestoryer(new Vector2Int(4, 1));
                        StartCoroutine(ForceWindingDestroyer());
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

        public override void InitLevel()
        {
            Debug.Assert(ReferenceOk); //意外的有确定Reference的……还行……
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));

            LevelAsset.GameBoard.UpdateBoardAnimation();
            //LevelAsset.StartingScoreSet = new ScoreSet();
            //LevelAsset.StartingPerMoveData = new PerMoveData();

            LevelAsset.GameStateMgr = new GameStateMgr();
            LevelAsset.GameStateMgr.InitGameMode(LevelAsset.ActionAsset.GameModeAsset);

            LevelAsset.DisableAllCoreFunctionAndFeature();
            LevelAsset.InputEnabled = true;
            LevelAsset.CursorEnabled = true;
            LevelAsset.RotateEnabled = true;
            LevelAsset.HintEnabled = true;
            LevelAsset.DestroyerEnabled = true;
            LevelAsset.CycleEnabled = true;

            InitCursor(new Vector2Int(2, 3));
            InitDestoryer();

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

        protected void InitDestoryer()
        {
            LevelAsset.WarningDestoryer = new MeteoriteBomber();
            LevelAsset.WarningDestoryer.SetBoard(ref LevelAsset.GameBoard);
            LevelAsset.WarningDestoryer.Init(5, 2);
        }
    }
}