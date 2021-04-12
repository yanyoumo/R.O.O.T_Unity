using System;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Messages;
using I2.Loc;
using ROOT.Common;
using ROOT.Consts;
using ROOT.Message;
using ROOT.SetupAsset;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ROOT.FSMLevelLogic;
using static ROOT.TutorialActionType;

namespace ROOT.FSM
{
    using FSMActions = Dictionary<RootFSMStatus, Action>;
    using Trans = RootFSMTransition;
    using FSMTransitions = HashSet<RootFSMTransition>;
    using CheckingLib = Dictionary<TutorialCheckType, Func<FSMLevelLogic, Board, bool>>;
    
    public class TutorialFSMModule
    {
        private FSMLevelLogic owner;
        private GameAssets LevelAsset => owner.LevelAsset;

        private readonly CheckingLib CheckLib = new CheckingLib
        {
            {TutorialCheckType.MoveCursorToTarget55, TutorialCheckFunctionList.MoveCursorToTarget55},
            {TutorialCheckType.MoveMatrixUnitsToSameYIndex, TutorialCheckFunctionList.MoveMatrixUnitsToSameYIndex},
            {TutorialCheckType.MoveThreeMatrixUnitsToOneLink, TutorialCheckFunctionList.MoveThreeMatrixUnitsToOneLink},
            {TutorialCheckType.ConnectOneMatrixUnitWithMatrixCore, TutorialCheckFunctionList.ConnectOneMatrixUnitWithMatrixCore},
            {TutorialCheckType.ConnectAllMatrixUnitsWithMatrixCore, TutorialCheckFunctionList.ConnectAllMatrixUnitsWithMatrixCore},
            {TutorialCheckType.ConnectThermalUnitWithThermalCore, TutorialCheckFunctionList.ConnectThermalUnitWithThermalCore},
            //{TutorialCheckType.ConnectMatrixLinksWithThermalLinks, TutorialCheckFunctionList.ConnectMatrixLinksWithThermalLinks},
            //{TutorialCheckType.ConnectNewAddedThermalUnitsIntoLinks, TutorialCheckFunctionList.ConnectNewAddedThermalUnitsIntoLinks},
            {TutorialCheckType.Buy3UnitsOrNotEnoughMoney, TutorialCheckFunctionList.Buy3UnitsOrNotEnoughMoney},
            {TutorialCheckType.FourWarningGridOneHeatSink, TutorialCheckFunctionList.FourWarningGridOneHeatSink}
        };

        /*protected override string SucceedEndingTerm => ScriptTerms.EndingMessageTutorial;
        protected override string FailedEndingTerm => ScriptTerms.EndingMessageTutorialFailed;
        public override bool IsTutorial => true;
        public override bool CouldHandleSkill => false;
        public override bool CouldHandleBoss => false;
        public override bool CouldHandleShop => _couldHandleShopLocal;
        public override int LEVEL_ART_SCENE_ID => StaticName.SCENE_ID_ADDITIONAL_VISUAL_TUTORIAL;
        public override BossStageType HandleBossType => throw new ArgumentException("could not handle Boss");*/

        #region TutorialRelated

        private bool _couldHandleShopLocal;

        protected int CurrentActionIndex { get; private set; } = -1;

        private LevelActionAsset LevelActionAsset => LevelAsset.ActionAsset;
        private TutorialActionData[] tutActions => LevelActionAsset.Actions;

        /*private void ShowTextFunc(bool val)=>SendHintData(HintEventType.SetTutorialTextShow, val);
        private void ShowCheckListFunc(bool val)=>SendHintData(HintEventType.SetGoalCheckListShow, val);*/
        
        private bool? PendingEndTutorialData = null;//null不结束、true完成结束、false失败结束。
        //INFO 现在失败还没有需求、有了再补。

        private bool NotEnding => !PendingEndTutorialData.HasValue;
        private bool EndingWSuccess => PendingEndTutorialData.HasValue && PendingEndTutorialData.Value;
        public bool EndingWFailed => PendingEndTutorialData.HasValue && !PendingEndTutorialData.Value;

        internal bool TutorialCheckGameOver
        {
            get
            {
                if (!NotEnding)
                {
                    LevelAsset.TutorialCompleted = EndingWSuccess;
                    return true;
                }
                return false;
            }
        }

        private void StepForward() => CurrentActionIndex++;

        private void DisplayText(string text)
        {
            var hintData = new HintEventInfo
            {
                HintEventType = HintEventType.SetTutorialTextContent,
                StringData = text,
            };
            MessageDispatcher.SendMessage(hintData);
        }

        private bool ProcessdToTutorialCycle;

        internal void TutorialReactIO()
        {
            if (owner._ctrlPack.HasFlag(ControllingCommand.Confirm))
            {
                if (CurrentHandOnCheckMet)
                {
                    UnsetHandOn();
                    return;
                }

                ProcessdToTutorialCycle = true;
                //仔细想了一下、Driver和这个React的流程又要有、一个是吧额外的ActionID转义为ControllingCommand。
                //顺带、根据这个思路，可能可以把Driver再拆分一下。//现在先不这么做、有空给搞一下。
                //另一个是把ControllingCommand转义为实际的工作。
            }
        }

        private void ShowShop(TutorialActionData data)
        {
            //TODO 还是接一下Data的内容。//Hide 相关的流程放在里面。
            LevelAsset.Shop.OpenShop(true, 0);
            _couldHandleShopLocal = true;
        }

        private Func<FSMLevelLogic, Board, bool> PendingHandOnChecking = (a, b) => false;

        private Dictionary<TutorialActionType, Action<TutorialActionData>> StepActionLib;
        private Dictionary<TutorialActionType, Tuple<Action,Action>> ActionPrepLib;

        private void SetStationaryByTag(TutorialActionData data)
        {
            //TODO 现在还没做unset流程、实际上Unit就没有UnsetStationary流程。
            LevelAsset.GameBoard.FindUnitsByUnitTag(data.TargetTag).ForEach(u => u.SetupStationUnit());
        }

        private void DealStep(TutorialActionData data)
        {
            StepActionLib.Where(v => v.Key == data.ActionType).Select(v => v.Value).ForEach(v => v(data));
        }

        private void DealStepMgr()
        {
            //现在是执行也按照SubIdx升序执行。
            tutActions.Where(a => a.ActionIdx == CurrentActionIndex).OrderBy(d=>d.ActionSubIdx).ForEach(DealStep);
            if (tutActions.Any(a => a.ActionIdx == CurrentActionIndex + 1 && a.ActionType == End))
            {
                MessageDispatcher.SendMessage(new HintEventInfo { HintEventType = HintEventType.NextIsEnding });
            }
        }

        #endregion

        private bool shouldInitTutorial = true;
        private bool TutorialOnHand = false;

        private void SetHandOn(TutorialActionData data)
        {
            TutorialOnHand = true;
            PendingHandOnChecking = CheckLib[data.HandOnCheckType];
            MessageDispatcher.SendMessage(new HintEventInfo { HintEventType = HintEventType.SetGoalContent, StringData = data.HandOnMission });
            MessageDispatcher.SendMessage(new HintEventInfo { HintEventType = HintEventType.ToggleHandOnView, BoolData = true });
            ShowCheckListFunc(true);
            ShowTextFunc(false);
            CurrentHandOnCheckMet = PendingHandOnChecking(owner, LevelAsset.GameBoard);//这边就就地测一下
            MessageDispatcher.SendMessage(new HintEventInfo { HintEventType = HintEventType.GoalComplete, BoolData = CurrentHandOnCheckMet });
        }

        private void UnsetHandOn()
        {
            TutorialOnHand = false;
            PendingHandOnChecking = (a, b) => false;
            MessageDispatcher.SendMessage(new HintEventInfo { HintEventType = HintEventType.ToggleHandOnView, BoolData = false });
            ShowCheckListFunc(false);
            CurrentHandOnCheckMet = false;
            StepForward();
            DealStepMgr();
        }

        private bool CompletedAndRequestedEnd()
        {
            return PendingEndTutorialData.HasValue && PendingEndTutorialData.Value;
        }

        private bool CheckTutorialEnding() => EndingWSuccess;

        private bool CheckTutorialCycle() => ProcessdToTutorialCycle;

        private bool CheckNotOnHand() => !TutorialOnHand;

        private void TutorialCycle()
        {
            if (NotEnding && !TutorialOnHand)
            {
                StepForward();
                DealStepMgr();
            }
            ProcessdToTutorialCycle = false; //只要到了这儿这个流程就要改一下。
        }

        private bool CurrentHandOnCheckMet { get; set; }

        internal void TutorialInit()
        {
            if (!shouldInitTutorial) return;
            shouldInitTutorial = false;
            TutorialOnHand = false;
            //LastActionCount = 0;
            CurrentActionIndex = -1;
            StepForward();
            DealStepMgr();
            WorldExecutor.InitAndStartShop(LevelAsset);//shop这个东西还是要留给开关。
            LevelAsset.Shop.OpenShop(false, 0);
            MessageDispatcher.SendMessage(new HintEventInfo { HintEventType = HintEventType.ToggleHandOnView, BoolData = false });
        }

        /*public override void InitLevel()
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
        }*/

        private void ToggleAlternateText(TutorialActionData data)
        {
            Debug.Log("MessageDispatcher.SendMessage(new HintEventInfo {HintEventType = HintEventType.ToggleAlternateTextPos});");
            MessageDispatcher.SendMessage(new HintEventInfo {HintEventType = HintEventType.ToggleAlternateTextPos});
        }

        private void HighLightUIFunc(TutorialActionData data)
        {
            MessageDispatcher.SendMessage(new HighLightingUIChangedData {Toggle = data.HLSet,uiTag = data.UITag});
        }

        public TutorialFSMModule(FSMLevelLogic _fsm)
        {
            //base.Awake();
            owner = _fsm;
            StepActionLib = new Dictionary<TutorialActionType, Action<TutorialActionData>> {
                {Text, data => DisplayText(data.DoppelgangerToggle && StartGameMgr.UseTouchScreen ? data.DoppelgangerText : data.Text)},
                {CreateUnit, data =>CreateUnitOnBoard(data,LevelAsset)},
                {End, data => PendingEndTutorialData = true},
                {ShowText, data => ShowTextFunc(true)},
                {HideText, data =>  ShowTextFunc(false)},
                {ShowCheckList, data => ShowCheckListFunc(true)},
                {HideCheckList, data => ShowCheckListFunc(false)},
                {HandOn, SetHandOn},
                {CreateCursor, data => WorldExecutor.InitCursor(LevelAsset, data.Pos)},
                {SetUnitStationary, SetStationaryByTag},
                {ShowStorePanel, ShowShop},
                {ToggleAlternateTextPos, ToggleAlternateText},
                {HighLightUI, HighLightUIFunc},
            };
        }

        /*protected void InitTutorial()//TODO 这个应该不用接了、但是要处理一下。
        {

        }*/

        internal void TutorialMajorUpkeep()
        {
            TutorialInit();
            if (TutorialOnHand)
            {
                CurrentHandOnCheckMet = PendingHandOnChecking(owner, LevelAsset.GameBoard);//这边就就地测一下
                MessageDispatcher.SendMessage(new HintEventInfo { HintEventType = HintEventType.GoalComplete, BoolData = CurrentHandOnCheckMet });
            }
        }

        internal void TutorialMinorUpkeep()
        {
            if (TutorialOnHand)
            {
                //这个流程有个问题、需要再走一周才能判断，但是放在Minor里面的，应该能判明白的？//还得放到major里面用一下。
                //有可能是digong那个函数的具体实现的问题、但是和整体时序都有关系。
                CurrentHandOnCheckMet = PendingHandOnChecking(owner, LevelAsset.GameBoard);
                //根据现在能识别到需要再接收一下玩家的“回车”来“手动通过”这个判断。
                //那个判断现在具体的执行是：在系统判断到条件满足后、需要玩家手动按动一下确定键（回车）来继续。
                MessageDispatcher.SendMessage(new HintEventInfo { HintEventType = HintEventType.GoalComplete, BoolData = CurrentHandOnCheckMet });
            }
        }

        internal void InjectTutorialFSMActions(ref Dictionary<RootFSMStatus, Action> actions)
        {
            actions.Add(RootFSMStatus.Tutorial_Cycle, TutorialCycle);
        }

        internal void InjectTutorialFSMTransitions(ref HashSet<RootFSMTransition> RootFSMTransitions)
        {
            RootFSMTransitions.Add(new Trans(RootFSMStatus.Tutorial_Cycle, RootFSMStatus.F_Cycle, 1, CheckTutorialEnding));
            RootFSMTransitions.Add(new Trans(RootFSMStatus.Tutorial_Cycle, RootFSMStatus.MajorUpKeep, 0, true));
            var existingRIOMaxPriority = RootFSMTransitions.Where(t => t.StartingStatus == RootFSMStatus.R_IO).Select(t => t.priority).Max();
            RootFSMTransitions.Add(new Trans(RootFSMStatus.R_IO, RootFSMStatus.F_Cycle, existingRIOMaxPriority + 3, CompletedAndRequestedEnd));
            RootFSMTransitions.Add(new Trans(RootFSMStatus.R_IO, RootFSMStatus.Tutorial_Cycle, existingRIOMaxPriority + 2, CheckTutorialCycle));
            RootFSMTransitions.Add(new Trans(RootFSMStatus.R_IO, RootFSMStatus.MajorUpKeep, existingRIOMaxPriority + 1, CheckNotOnHand));
        }
    }
}