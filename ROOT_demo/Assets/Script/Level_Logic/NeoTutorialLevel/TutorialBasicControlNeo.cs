using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    using FSMActions = Dictionary<RootFSMStatus, Action>;
    using Trans = RootFSMTransition;
    using FSMTransitions = HashSet<RootFSMTransition>;

    public class TutorialBasicControlNeo : FSMTutorialLogic
    {
        protected bool AllUnitConnected()
        {
            return LevelAsset.GameBoard.Units.All(u => u.AnyConnection);
        }

        protected override void TutorialMinorUpkeep()
        {
            LevelCompleted = AllUnitConnected();
            LevelFailed = !LevelCompleted;
            
            
            if (ActionEnded)
            {
                LevelAsset.HintMaster.TutorialCheckList.MainGoalCompleted = LevelCompleted = AllUnitConnected();
            }
            if (LevelCompleted)
            {
                PlayerRequestedEnd = CtrlPack.HasFlag(ControllingCommand.NextButton);
            }
        }

        protected override void AdditionalArtLevelReference(ref GameAssets LevelAsset)
        {
            //LevelAsset.ItemPriceRoot = GameObject.Find("PlayUI");
            //LevelAsset.DataScreen = FindObjectOfType<DataScreen>();
            //LevelAsset.HintMaster = FindObjectOfType<HintMaster>();
            LevelAsset.HintMaster.HideTutorialFrame = false;
        }

        protected override void AddtionalDealStep(TutorialActionData data)
        {
            switch (data.ActionType)
            {
                case TutorialActionType.CreateCursor:
                    //WorldExecutor.InitCursor(ref LevelAsset,data.Pos);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void AdditionalFSMActionsOperating(ref FSMActions actions)
        {
            //throw new NotImplementedException();
        }

        protected override void AdditionalFSMTransitionOperating(ref FSMTransitions transitions)
        {
            //throw new NotImplementedException();
        }

        protected override string MainGoalEntryContent => "将所有单元链接起来";
        
        /*private bool LevelCompleted = false;
        private bool PlayerRequestedEnd = false;*/

        /*protected override void Update()
        {
            base.Update();
            if (ReadyToGo)
            {
                if (ActionIndex==3)
                {
                    LevelAsset.InputEnabled = true;
                    LevelAsset.CursorEnabled = true;
                    LevelAsset.RotateEnabled = true;
                }
            }
            if (ActionEnded)
            {
                LevelAsset.HintMaster.TutorialCheckList.MainGoalCompleted = LevelCompleted = AllUnitConnected();
            }

            if (LevelCompleted)
            {
                PlayerRequestedEnd = CtrlPack.HasFlag(ControllingCommand.NextButton);
            }
        }

        protected override string MainGoalEntryContent => "将所有单元链接起来";

        public override void InitLevel()
        {
            Debug.Assert(ReferenceOk);//意外的有确定Reference的……还行……
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));

            InitCurrencyIoMgr();
            LevelAsset.DeltaCurrency = 0.0f;

            LevelAsset.DeltaCurrency = 0.0f;
            LevelAsset.GameStateMgr = new GameStateMgr();
            LevelAsset.GameStateMgr.InitGameMode(LevelAsset.ActionAsset.GameModeAsset);
            LevelAsset.GameBoard.UpdateBoardAnimation();
            //LevelAsset.StartingScoreSet = new ScoreSet();
            //LevelAsset.StartingPerMoveData = new PerMoveData();

            LevelAsset.DisableAllCoreFunctionAndFeature();
            LevelAsset.HintEnabled = true;
            LevelAsset.CurrencyEnabled = true;
            LevelAsset.GameOverEnabled = true;

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
                switch (data.ActionType)
                {
                    case TutorialActionType.CreateCursor:
                        WorldExecutor.InitCursor(ref LevelAsset,data.Pos);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }*/
    }
}