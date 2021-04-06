using System;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Messages;
using I2.Loc;
using ROOT.Consts;
using ROOT.Message;
using ROOT.SetupAsset;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ROOT.TextProcessHelper;
using static ROOT.TutorialActionType;


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
   
    public static class TutorialCheckFunctionList
    {
        public static bool MoveCursorToTarget55(FSMLevelLogic fsm,Board board)
        {
            return fsm.LevelAsset.Cursor.CurrentBoardPosition.Equals(new Vector2Int(5,5));
        }

        public static bool MoveMatrixUnitsToSameYIndex(FSMLevelLogic fsm, Board board)
        {
            var y = -1;
            foreach (var unit in board.Units)
            {
                if (y == -1)
                    y = unit.CurrentBoardPosition.y;
                else if (y != unit.CurrentBoardPosition.y)
                    return false;
            }

            return true;
        }

        public static bool MoveThreeMatrixUnitsToOneLink(FSMLevelLogic fsm, Board board)
        {
            while (!board.IsDataReady) { }
            return board.GetConnectComponent()==1;
        }
    }

    public enum TutorialCheckType
    {
        MoveCursorToTarget55,
        MoveMatrixUnitsToSameYIndex,
        MoveThreeMatrixUnitsToOneLink
    }

    public sealed class FSMLevelLogic_Tutorial : FSMLevelLogic_Barebone
    {
        private readonly CheckingLib CheckLib = new CheckingLib
        {
            {TutorialCheckType.MoveCursorToTarget55, TutorialCheckFunctionList.MoveCursorToTarget55},
            {TutorialCheckType.MoveMatrixUnitsToSameYIndex, TutorialCheckFunctionList.MoveMatrixUnitsToSameYIndex},
            {TutorialCheckType.MoveThreeMatrixUnitsToOneLink, TutorialCheckFunctionList.MoveThreeMatrixUnitsToOneLink}
        };
        
        protected override string SucceedEndingTerm => ScriptTerms.EndingMessageTutorial;
        protected override string FailedEndingTerm => ScriptTerms.EndingMessageTutorialFailed;
        public override bool IsTutorial => true;
        public override bool CouldHandleSkill => false;
        public override bool CouldHandleBoss => false;
        public override bool CouldHandleShop => _couldHandleShopLocal;
        public override BossStageType HandleBossType => throw new ArgumentException("could not handle Boss");

        #region TutorialRelated

        private bool _couldHandleShopLocal;
        
        protected int CurrentActionIndex { get; private set; } = -1;
        //private int LastActionCount { get; set; } = 0;

        /*protected abstract string MainGoalEntryContent { get; }
        protected virtual string SecondaryGoalEntryContent { get; } = "";*/

        private LevelActionAsset LevelActionAsset => LevelAsset.ActionAsset;
        
        private void ShowTextFunc(bool val)=>SendHintData(HintEventType.SetTutorialTextShow, val);
        private void ShowCheckListFunc(bool val)=>SendHintData(HintEventType.SetGoalCheckListShow, val);
        

        private bool? PendingEndTutorialData = null;//null不结束、true完成结束、false失败结束。
        //INFO 现在失败还没有需求、有了再补。

        public bool NotEnding => !PendingEndTutorialData.HasValue;
        public bool EndingWSuccess => PendingEndTutorialData.HasValue && PendingEndTutorialData.Value;
        public bool EndingWFailed => PendingEndTutorialData.HasValue && !PendingEndTutorialData.Value;
        
        protected override bool CheckGameOver
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

        private void CreateUnitOnBoard(TutorialActionData data)
        {
            var pos = data.Pos;
            if (pos.x < 0 || pos.y < 0) pos = LevelAsset.GameBoard.FindRandomEmptyPlace();
            LevelAsset.GameBoard.CreateUnit(pos, data.Core, data.HardwareType, data.Sides, data.Tier, data.IsStationary, data.Tag);
            LevelAsset.GameBoard.UpdateBoardUnit();
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

        //protected abstract void AdditionalDealStep(TutorialActionData data);

        private void ShowShop()
        {
            if (LevelAsset.Shop == null)
            {
                LevelAsset.Shop = FindObjectOfType<ShopSelectableMgr>();
                if (LevelAsset.Shop == null) throw new ArgumentException("Could not find shop in scene.");
            }
            WorldExecutor.InitShop(ref LevelAsset);

            _couldHandleShopLocal = true;
        }
        
        private Func<FSMLevelLogic, Board, bool> PendingHandOnChecking = (a, b) => false;

        private Dictionary<TutorialActionType, Action<TutorialActionData>> StepActionLib;

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
            LevelActionAsset.Actions.Where(a => a.ActionIdx == CurrentActionIndex).OrderBy(d=>d.ActionSubIdx).ForEach(DealStep);
            if (LevelActionAsset.Actions.Any(a => a.ActionIdx == CurrentActionIndex + 1 && a.ActionType == End))
            {
                MessageDispatcher.SendMessage(new HintEventInfo {HintEventType = HintEventType.NextIsEnding});
            }
        }

        #endregion

        private bool shouldInitTutorial = true;
        private bool TutorialOnHand = false;

        private void SetHandOn(TutorialActionData data)
        {
            TutorialOnHand = true;
            PendingHandOnChecking = CheckLib[data.HandOnCheckType];
            MessageDispatcher.SendMessage(new HintEventInfo {HintEventType = HintEventType.SetGoalContent, StringData = data.HandOnMission});
            MessageDispatcher.SendMessage(new HintEventInfo {HintEventType = HintEventType.ToggleHandOnView, BoolData = true});
            ShowCheckListFunc(true);
            ShowTextFunc(false);
            CurrentHandOnCheckMet = PendingHandOnChecking(this, LevelAsset.GameBoard);//这边就就地测一下
            MessageDispatcher.SendMessage(new HintEventInfo {HintEventType = HintEventType.GoalComplete, BoolData = CurrentHandOnCheckMet});
        }

        private void UnsetHandOn()
        {
            TutorialOnHand = false;
            PendingHandOnChecking = (a, b) => false;
            MessageDispatcher.SendMessage(new HintEventInfo {HintEventType = HintEventType.ToggleHandOnView, BoolData = false});
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
            MessageDispatcher.SendMessage(new HintEventInfo {HintEventType = HintEventType.ToggleHandOnView, BoolData = false});
        }

        public override void InitLevel()
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

        protected override void AdditionalInitLevel()
        {
            StepActionLib = new Dictionary<TutorialActionType, Action<TutorialActionData>>
            {
                {Text,data=>DisplayText(data.DoppelgangerToggle && StartGameMgr.UseTouchScreen ? data.DoppelgangerText : data.Text)},
                {CreateUnit,CreateUnitOnBoard},
                {End,data=>PendingEndTutorialData = true},
                {ShowText,data=>ShowTextFunc(true)},
                {HideText,data=>ShowTextFunc(false)},
                {ShowCheckList,data=>ShowCheckListFunc(true)},
                {HideCheckList,data=>ShowCheckListFunc(false)},
                {HandOn,SetHandOn},
                {CreateCursor,data=>WorldExecutor.InitCursor(ref LevelAsset,data.Pos)},
                {SetUnitStationary,SetStationaryByTag},
                {ShowStorePanel,data=>ShowShop()},
            };
        }

        protected override void AdditionalMajorUpkeep()
        {
            TutorialInit();
            if (TutorialOnHand)
            {
                CurrentHandOnCheckMet = PendingHandOnChecking(this, LevelAsset.GameBoard);//这边就就地测一下
                MessageDispatcher.SendMessage(new HintEventInfo {HintEventType = HintEventType.GoalComplete, BoolData = CurrentHandOnCheckMet});
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
                MessageDispatcher.SendMessage(new HintEventInfo {HintEventType = HintEventType.GoalComplete, BoolData = CurrentHandOnCheckMet});
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
        }
    }
}