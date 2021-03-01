using System;
using System.Collections;
using System.Collections.Generic;
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
        public override int LEVEL_ART_SCENE_ID => -1;
        
        protected virtual void ModifyFSMActions(ref FSMActions actions)
        {
            //Base version, DoNothing.
        }
        
        protected virtual void ModifiyRootFSMTransitions(ref HashSet<RootFSMTransition> RootFSMTransitions)
        {
            //Base version, DoNothing.
        }

        public override bool IsTutorial => false;
        public override bool CouldHandleSkill => false;
        public override bool CouldHandleBoss => false;
        public override BossStageType HandleBossType => throw new ArgumentException("could not handle Boss");

        public override void InitLevel()
        {
            Debug.Assert(ReferenceOk); //意外的有确定Reference的……还行……
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));

            LevelAsset.DeltaCurrency = 0.0f;
            LevelAsset.GameCurrencyMgr = new GameCurrencyMgr();
            LevelAsset.GameCurrencyMgr.InitGameMode(LevelAsset.ActionAsset.GameStartingData);
            
            WorldExecutor.InitCursor(ref LevelAsset,new Vector2Int(2, 3));
            LevelAsset.EnableAllCoreFunctionAndFeature();
            LevelAsset.GameBoard.InitBoardWAsset(LevelAsset.ActionAsset);
            LevelAsset.GameBoard.UpdateBoardAnimation();
            AdditionalInitLevel();
            
            ReadyToGo = true;
            LevelAsset.HintMaster.ShouldShowCheckList = false;
        }
        
        public override IEnumerator UpdateArtLevelReference(AsyncOperation aOP,AsyncOperation aOP2)
        {
            while (!aOP.isDone)
            {
                yield return 0;
            }
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVEVISUAL));
            AdditionalArtLevelReference(ref LevelAsset);
            LevelAsset.HintMaster.HideTutorialFrame = false;
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
                ModifiyRootFSMTransitions(ref transitions);
                return transitions;
            }
        }
    }
}