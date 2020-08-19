using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    //TODO TutorialDestoryerNeo 还没做。
    public class TutorialDestoryerNeo : TutorialLogic
    {
        IEnumerator ForceWindingDestroyer()
        {
            while (LevelAsset.WarningDestoryer.GetStatus() != WarningDestoryerStatus.Striking)
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
                if (ActionIndex == 1)
                {
                    if (!OnceFlagB)
                    {
                        OnceFlagB = true;
                        ForceSetWarningDestoryer(new Vector2Int(4, 1));
                        StartCoroutine(ForceWindingDestroyer());
                    }
                }
            }

            if (ActionEnded)
            {
                if (LevelAsset.DestoryedCoreType.HasValue)
                {
                    if (LevelAsset.DestoryedCoreType != CoreType.PCB)
                    {
                        LevelFailed = true;
                        OnceFlagA = true;
                    }
                }
                bool bComplete = RidOfPCBUnit();
                LevelAsset.HintMaster.TutorialCheckList.MainGoalCompleted = !OnceFlagA;
                LevelAsset.HintMaster.TutorialCheckList.SecondaryGoalCompleted = bComplete;
                LevelCompleted = (!OnceFlagA) && bComplete;
            }

            if (LevelCompleted)
            {
                PlayerRequestedEnd = CtrlPack.HasFlag(ControllingCommand.NextButton);
            }

            if (LevelFailed)
            {
                PlayerRequestedQuit = CtrlPack.HasFlag(ControllingCommand.NextButton);
                LevelAsset.HintMaster.TutorialCheckList.TutorialFailed = true;
            }
        }

        protected override string MainGoalEntryContent => "保护所有非垃圾单元";
        protected override string SecondaryGoalEntryContent => "摧毁全部垃圾单元";

        private bool RidOfPCBUnit()
        {
            return LevelAsset.GameBoard.Units.All(gameBoardUnit => gameBoardUnit.Value.GetComponentInChildren<Unit>().UnitCore != CoreType.PCB);
        }

        protected override bool UpdateGameOverStatus(GameAssets currentLevelAsset)
        {
            base.UpdateCareerGameOverStatus(currentLevelAsset);
            return base.UpdateGameOverStatus(currentLevelAsset);
        }
        
        protected void InitDestoryer()
        {
            LevelAsset.WarningDestoryer = new MeteoriteBomber();
            LevelAsset.WarningDestoryer.SetBoard(ref LevelAsset.GameBoard);
            LevelAsset.WarningDestoryer.Init(5, 2);
        }

        public override void InitLevel()
        {
            Debug.Assert(ReferenceOk);//意外的有确定Reference的……还行……
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));

            InitCurrencyIoMgr();
            LevelAsset.DeltaCurrency = 0.0f;
            LevelAsset.GameStateMgr = new GameStateMgr();
            LevelAsset.GameStateMgr.InitGameMode(LevelAsset.ActionAsset.GameModeAsset);
            LevelAsset.GameBoard.InitBoardWAsset(LevelAsset.ActionAsset);
            LevelAsset.GameBoard.UpdateBoardAnimation();
            InitCursor(new Vector2Int(2, 3));
            InitDestoryer();

            LevelAsset.DisableAllCoreFunctionAndFeature();
            LevelAsset.InputEnabled = true;
            LevelAsset.CursorEnabled = true;
            LevelAsset.RotateEnabled = true;
            LevelAsset.HintEnabled = true;
            LevelAsset.CurrencyEnabled = true;
            LevelAsset.GameOverEnabled = true;
            LevelAsset.DestroyerEnabled = true;
            LevelAsset.CycleEnabled = true;

            LevelAsset.TimeLine.InitWithTokens(LevelAsset.ActionAsset.TimeLineTokens);

            ReadyToGo = true;
        }
    }
}