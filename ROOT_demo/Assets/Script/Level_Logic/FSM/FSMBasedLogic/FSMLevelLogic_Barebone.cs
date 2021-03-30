using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using ROOT.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    using FSMActions = Dictionary<RootFSMStatus, Action>;
    using Trans= RootFSMTransition;
    using FSMTransitions = HashSet<RootFSMTransition>;
    using Status = RootFSMStatus;
    public class FSMLevelLogic_Barebone : FSMLevelLogic
    {
        protected override string SucceedEndingTerm => ScriptTerms.EndingMessageNoBoss_EarnedMoney;
        protected override string FailedEndingTerm => ScriptTerms.EndingMessageNoBoss_NoEarnedMoney;
        public override int LEVEL_ART_SCENE_ID => -1;
        
        protected virtual void ModifyFSMActions(ref FSMActions actions)
        {
            //Base version, DoNothing.
        }
        
        protected virtual void ModifyRootFSMTransitions(ref HashSet<RootFSMTransition> RootFSMTransitions)
        {
            //Base version, DoNothing.
        }

        public override bool IsTutorial => false;
        public override bool CouldHandleSkill => false;
        public override bool CouldHandleBoss => false;
        public override BossStageType HandleBossType => throw new ArgumentException("could not handle Boss");

        protected virtual void AdditionalInitLevel()
        {
            WorldExecutor.InitCursor(ref LevelAsset,new Vector2Int(2, 3));
        }

        public sealed override void InitLevel()
        {
            //就先这么Sealed、急了的话、所有需要"关掉"的可以在AdditionalInit里面再关掉。
            Debug.Assert(ReferenceOk); //意外的有确定Reference的……还行……
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));

            LevelAsset.BaseDeltaCurrency = 0.0f;
            LevelAsset.GameCurrencyMgr = new GameCurrencyMgr();
            LevelAsset.GameCurrencyMgr.InitGameMode(LevelAsset.ActionAsset.GameStartingData);
            
            LevelAsset.EnableAllCoreFunctionAndFeature();
            LevelAsset.GameBoard.InitBoardWAsset(LevelAsset.ActionAsset);
            LevelAsset.GameBoard.UpdateBoardAnimation();
            AdditionalInitLevel();
            
            ReadyToGo = true;

            SendHintData(HintEventType.SetGoalCheckListShow, false);
        }
        
        public override IEnumerator UpdateArtLevelReference(AsyncOperation baseVisualScene,AsyncOperation addtionalVisualScene)
        {
            while (!baseVisualScene.isDone)
            {
                yield return 0;
            }
            AdditionalArtLevelReference(ref LevelAsset);
            SendHintData(HintEventType.SetTutorialTextShow, false);
            PopulateArtLevelReference();
        }
        
        protected sealed override FSMActions fsmActions
        {
            get
            {
                //可能需要一个“整理节点（空节点）”这种概念的东西。
                var _fsmActions = new FSMActions
                {
                    {Status.PreInit, PreInit},
                    {Status.MajorUpKeep, MajorUpkeepAction},
                    {Status.MinorUpKeep, MinorUpKeepAction},
                    {Status.F_Cycle, ForwardCycle},
                    {Status.CleanUp, CleanUp},
                    {Status.Animate, AnimateAction},
                    {Status.R_IO, ReactIO},
                };
                ModifyFSMActions(ref _fsmActions);
                return _fsmActions;
            }
        }
        protected sealed override HashSet<RootFSMTransition> RootFSMTransitions {
            get
            {
                var transitions = new FSMTransitions
                {
                    new Trans(Status.PreInit, Status.MajorUpKeep, 1, CheckInited),
                    new Trans(Status.PreInit),
                    new Trans(Status.F_Cycle, Status.Animate, 1, CheckStartAnimate, TriggerAnimation),
                    new Trans(Status.F_Cycle, Status.MinorUpKeep),
                    new Trans(RootFSMStatus.Animate, RootFSMStatus.MinorUpKeep),
                    new Trans(RootFSMStatus.MajorUpKeep, RootFSMStatus.R_IO, 1, CheckCtrlPackAny),
                    new Trans(RootFSMStatus.MajorUpKeep),
                    new Trans(RootFSMStatus.MinorUpKeep, RootFSMStatus.Animate, 1, true, CheckLoopAnimate),
                    new Trans(RootFSMStatus.MinorUpKeep, RootFSMStatus.CleanUp),
                    new Trans(RootFSMStatus.R_IO, RootFSMStatus.F_Cycle, 2, CheckFCycle),
                    new Trans(RootFSMStatus.R_IO, RootFSMStatus.Animate, 1, CheckStartAnimate, TriggerAnimation),
                    new Trans(RootFSMStatus.R_IO, RootFSMStatus.MajorUpKeep, 0, true),
                    new Trans(RootFSMStatus.CleanUp, RootFSMStatus.MajorUpKeep, 0, true),
                };
                ModifyRootFSMTransitions(ref transitions);
                return transitions;
            }
        }
    }
}