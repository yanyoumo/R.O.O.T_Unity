using System;
using System.Collections.Generic;
using ROOT.Common;

namespace ROOT
{
    using FSMActions = Dictionary<RootFSMStatus, Action>;
    using Trans = RootFSMTransition;
    using FSMTransitions = HashSet<RootFSMTransition>;
    using CheckingLib = Dictionary<TutorialCheckType, Func<FSMLevelLogic, Board, bool>>;

    //迪公说的想把Tutorial里面判断的逻辑从基于FSM变成这样基于函数代理的。
    //从技术上讲可以、使用Dict存储一个enum-Func对儿；这样在Action里面就可以通过enum配置实际的逻辑。
    //这么搞的确有很多优势、但是也有一些问题。
    //先说优势：
    //1、极端情况下、就都不用每个教程关卡都需要一个新的FSM了。
    //2、判断函数是可以对立拎出来了。
    //再说劣势：
    //1、实现框架需要仔细想；可能会十分复杂。
    //2、判断函数的参数定死了。（FSMLogic和Board可以提供数据源
    //但是一个重要的问题，是具体可配置的参数怎么办？例如判断已有的某个数据是否高于某个阈值、这个阈值怎么传进去？
    //理论上可以传一个Object、但是也有不少问题。

    //FSMLevelLogic这个流程要使用Module的流程、让它可以任意挂在Barebone、Career、这些FSM里面挂上的。
    //需要在FSMLevelLogic里面直接配置某个个派生类要不要生成自己的教程版本。
    //Tutorial需要不同的feature的话、就由不同等级的派生类生成版本。但是就出现一个问题：还是需要在各自的派生类里面有各种开关？
    //之前这么设计的时候、是为了将Tutorial相关逻辑彻底从Gameplay中拆开、但是这样的话Tutorial要运行不同派生类里面的逻辑又要捞出来。
    //所以说：【要么Tutorial要零散地弥散到全部基类和派生类中一部分】或者【Tutorial中要想办法移植Gameplay版中的所需逻辑】。
    [Obsolete]
    public sealed class FSMLevelLogic_Tutorial : FSMLevelLogic_Barebone//挪到TutorialFSMModule去了。
    {
        /*private readonly CheckingLib CheckLib = new CheckingLib
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

        //TODO 这里的相当于从抽象化的东西变成基本框架的一部分、主要是关于TutorialModule的Wrapper了。
        //还有一些东西需要直接内嵌了、这个内嵌是目前框架下的妥协。
        protected override string SucceedEndingTerm => ScriptTerms.EndingMessageTutorial;
        protected override string FailedEndingTerm => ScriptTerms.EndingMessageTutorialFailed;
        public override bool CouldHandleSkill => false;//这个的数据不受IsTutorial的限制、是宏观来看的。
        public override bool CouldHandleBoss => false;//这个的数据不受IsTutorial的限制、是宏观来看的。
        public override bool CouldHandleShop => _couldHandleShopLocal;//这个的数据不受IsTutorial的限制、是宏观来看的。（这个框架需要改）
        public override int LEVEL_ART_SCENE_ID => StaticName.SCENE_ID_ADDITIONAL_VISUAL_TUTORIAL;//这个直接内嵌吧…………
        public override BossStageType HandleBossType => throw new ArgumentException("could not handle Boss");

        #region TutorialRelated

        private bool _couldHandleShopLocal;

        protected int CurrentActionIndex { get; private set; } = -1;

        private LevelActionAsset LevelActionAsset => LevelAsset.ActionAsset;
        private TutorialActionData[] tutActions => LevelActionAsset.Actions;

        private bool? PendingEndTutorialData = null;//null不结束、true完成结束、false失败结束。
        //INFO 现在失败还没有需求、有了再补。

        public bool NotEnding => !PendingEndTutorialData.HasValue;
        public bool EndingWSuccess => PendingEndTutorialData.HasValue && PendingEndTutorialData.Value;
        public bool EndingWFailed => PendingEndTutorialData.HasValue && !PendingEndTutorialData.Value;

        protected override bool NormalCheckGameOver
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

        protected override void AdditionalReactIO()
        {
            if (_ctrlPack.HasFlag(ControllingCommand.Confirm))
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
            CurrentHandOnCheckMet = PendingHandOnChecking(this, LevelAsset.GameBoard);//这边就就地测一下
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
            //Debug.Log("TutorialCycle");
            //TODO 这个还有个事儿、就是要把Logic承接不了的输入屏蔽掉，CouldHandleSkill这些参量可以利用起来。
            if (NotEnding && !TutorialOnHand)
            {
                StepForward();
                DealStepMgr();
            }
            ProcessdToTutorialCycle = false; //只要到了这儿这个流程就要改一下。
        }

        private bool CurrentHandOnCheckMet { get; set; }

        private void TutorialInit()
        {
            if (!shouldInitTutorial) return;
            shouldInitTutorial = false;
            TutorialOnHand = false;
            //LastActionCount = 0;
            CurrentActionIndex = -1;
            StepForward();
            DealStepMgr();
            MessageDispatcher.SendMessage(new HintEventInfo { HintEventType = HintEventType.ToggleHandOnView, BoolData = false });
        }
        
        private void ToggleAlternateText(TutorialActionData data)
        {
            Debug.Log("MessageDispatcher.SendMessage(new HintEventInfo {HintEventType = HintEventType.ToggleAlternateTextPos});");
            MessageDispatcher.SendMessage(new HintEventInfo {HintEventType = HintEventType.ToggleAlternateTextPos});
        }

        private void HighLightUIFunc(TutorialActionData data)
        {
            MessageDispatcher.SendMessage(new HighLightingUIChangedData {Toggle = data.HLSet,uiTag = data.UITag});
        }

        protected override void AdditionalArtLevelReference(ref GameAssets LevelAsset)
        {
            LevelAsset.Shop = FindObjectOfType<ShopSelectableMgr>();
            LevelAsset.Shop._fsmLevelLogic = this;
        }

        protected override void Awake()
        {
            base.Awake();
            StepActionLib = new Dictionary<TutorialActionType, Action<TutorialActionData>> {
                {Text, data => DisplayText(data.DoppelgangerToggle && StartGameMgr.UseTouchScreen ? data.DoppelgangerText : data.Text)},
                {CreateUnit, data =>CreateUnitOnBoard(data,LevelAsset)},
                {End, data => PendingEndTutorialData = true},
                {ShowText, data => ShowTextFunc(true)},
                {HideText, data => ShowTextFunc(false)},
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

        protected override void AdditionalInitLevel()
        {
            WorldExecutor.InitAndStartShop(LevelAsset);//shop这个东西还是要留给开关。
            LevelAsset.Shop.OpenShop(false, 0);
        }

        protected override void AdditionalMajorUpkeep()
        {
            TutorialInit();
            if (TutorialOnHand)
            {
                CurrentHandOnCheckMet = PendingHandOnChecking(this, LevelAsset.GameBoard);//这边就就地测一下
                MessageDispatcher.SendMessage(new HintEventInfo { HintEventType = HintEventType.GoalComplete, BoolData = CurrentHandOnCheckMet });
            }
        }

        protected override void AdditionalMinorUpkeep()
        {
            if (TutorialOnHand)
            {
                //这个流程有个问题、需要再走一周才能判断，但是放在Minor里面的，应该能判明白的？//还得放到major里面用一下。
                //有可能是digong那个函数的具体实现的问题、但是和整体时序都有关系。
                CurrentHandOnCheckMet = PendingHandOnChecking(this, LevelAsset.GameBoard);
                //根据现在能识别到需要再接收一下玩家的“回车”来“手动通过”这个判断。
                //那个判断现在具体的执行是：在系统判断到条件满足后、需要玩家手动按动一下确定键（回车）来继续。
                MessageDispatcher.SendMessage(new HintEventInfo { HintEventType = HintEventType.GoalComplete, BoolData = CurrentHandOnCheckMet });
            }
        }

        protected override void ModifyFSMActions(ref Dictionary<RootFSMStatus, Action> actions)
        {
            base.ModifyFSMActions(ref actions);
            actions.Add(RootFSMStatus.Tutorial_Cycle, TutorialCycle);
        }

        protected override void ModifyRootFSMTransitions(ref HashSet<RootFSMTransition> RootFSMTransitions)
        {
            base.ModifyRootFSMTransitions(ref RootFSMTransitions);
            RootFSMTransitions.Add(new Trans(RootFSMStatus.Tutorial_Cycle, RootFSMStatus.F_Cycle, 1, CheckTutorialEnding));
            RootFSMTransitions.Add(new Trans(RootFSMStatus.Tutorial_Cycle, RootFSMStatus.MajorUpKeep, 0, true));
            RootFSMTransitions.Add(new Trans(RootFSMStatus.R_IO, RootFSMStatus.F_Cycle, 5, CompletedAndRequestedEnd));
            RootFSMTransitions.Add(new Trans(RootFSMStatus.R_IO, RootFSMStatus.Tutorial_Cycle, 4, CheckTutorialCycle));
            RootFSMTransitions.Add(new Trans(RootFSMStatus.R_IO, RootFSMStatus.MajorUpKeep, 3, CheckNotOnHand));
        }*/
    }
}