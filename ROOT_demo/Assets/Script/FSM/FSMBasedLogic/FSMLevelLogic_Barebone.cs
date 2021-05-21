using System;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Messages;
using I2.Loc;
using ROOT.Common;
using ROOT.Consts;
using ROOT.Message;
using ROOT.Message.Inquiry;
using ROOT.Signal;
using ROOT.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    using FSMActions = Dictionary<RootFSMStatus, Action>;
    using Trans= RootFSMTransition;
    using Status = RootFSMStatus;
    public class FSMLevelLogic_Barebone : FSMLevelLogic
    {
        protected override float LevelProgress => 0.0f;
        public override int LEVEL_ART_SCENE_ID => -1;
        
        #region TransitionReq

        protected bool CheckInited() => (ReadyToGo) && (!PendingCleanUp);
        protected bool CheckFCycle() => IsForwardCycle;
        protected bool CheckCtrlPackAny() => CtrlPack.AnyFlag();
        protected bool CheckStartAnimate() => ShouldStartAnimate;
        protected bool CheckLoopAnimate() => Animating;
        protected bool CheckNotAnimating() => !Animating;

        protected void TriggerAnimation()
        {
            _mainFSM.currentStatus = RootFSMStatus.Animate;
            Animating = true;
            //这里的流程和多态机还不是特别兼容，差不多了还是要整理一下。
            //RISK Skill那个状态并不是FF技能好使的原因；是因为那个时候，关了输入，但是也跑了对应事件长度的动画。
            //FF前进N个时刻，就跑N个空主动画阻塞；只是恰好主动画时长和时间轴动画时长匹配；
            //就造成了时间轴动画“匹配阻塞”的“假象”。
            //在FSM流程中，不去跑错误的空动画了；就匹配不上了。
            //（也不是说时序的问题；只是Animating的计算逻辑原本计算了AutoDrive，之前为了简化删了；按照原始的逻辑补回来就好了）
            //上面是个治标不治本的方法，感觉还是有比“空动画”的“意外”阻塞更加高明的算法。
            //SOLVED-还是先把“空动画”这个设计弄回来了；先从新整理一下再弄。
            AnimationTimerOrigin = Time.timeSinceLevelLoad;
            LevelAsset.MovedTileAni = MovedTile;
            LevelAsset.MovedCursorAni = MovedCursor;
            //animate_Co = StartCoroutine(Animate()); //这里完成后会把Animating设回来。
            Animate_DOTween();//DOTween版的Animate。
        }
        
        #endregion
        
        protected virtual void ModifyFSMActions(ref FSMActions actions)
        {
            //Base version, DoNothing.
        }
        
        protected virtual void ModifyRootFSMTransitions(ref RootFSMTranstionLib RootFSMTransitions)
        {
            //Base version, DoNothing.
        }

        public override bool CouldHandleBoss => false;
        public override bool CouldHandleTimeLine => false;
        public override BossStageType HandleBossType => throw new ArgumentException("could not handle Boss");

        private bool HandlingCurrency => !UseTutorialVer || FeatureManager.GetExternalToggleVal(FSMFeatures.Currency);
        private bool HandlingShop => !UseTutorialVer || FeatureManager.GetExternalToggleVal(FSMFeatures.Shop);
        
        protected int GetBaseInCome() => Mathf.RoundToInt((TypeASignalScore + TypeBSignalScore));

        protected virtual void AdditionalInitLevel()
        {
            MessageDispatcher.SendMessage(new HintPageChangedData {Toggle = false, TutorialOrGameplay = false});
            MessageDispatcher.SendMessage(new HintPageChangedData {Toggle = false, TutorialOrGameplay = true});
            if (UseTutorialVer)
            {
                //教程不自动切，完全交予Action。
                TutorialModule.TutorialInit();
            }
            else
            {
                SetUpHandlingCurrency();
                //正式游戏自动切第一页先。
                MessageDispatcher.SendMessage(new HintPageChangedData {Toggle = true, TutorialOrGameplay = false, PageNum = 0});
                WorldExecutor.InitCursor(LevelAsset, new Vector2Int(2, 3));
            }
        }

        protected virtual void SetUpHandlingCurrency()
        {
            MessageDispatcher.SendMessage(new ToggleGameplayUIData {Set = true, SelectAll = false, UITag = UITag.Currency_BareBone});
            MessageDispatcher.SendMessage(new TimingEventInfo
            {
                Type = WorldEvent.CurrencyIOStatusChangedEvent,
                BoardCouldIOCurrencyData = true,
                UnitCouldGenerateIncomeData = true,
            });
            UpdateBoardData_Instantly();
            LevelAsset.BaseDeltaCurrency = GetBaseInCome();
            LevelAsset.BonusDeltaCurrency = 0;
            SendCurrencyMessage(LevelAsset);
        }

        private void SetUpShop()
        {
            LevelAsset.Shop.OpenShop(true, 0);
        }
        
        protected override void FeaturesChangedHandler()
        {
            base.FeaturesChangedHandler();
            if (HandlingCurrency)
            {
                SetUpHandlingCurrency();
            }

            if (HandlingShop)
            {
                SetUpShop();
            }
        }

        public sealed override void InitLevel()
        {
            //就先这么Sealed、急了的话、所有需要"关掉"的可以在AdditionalInit里面再关掉。
            Debug.Assert(ReferenceOk); //意外的有确定Reference的……还行……
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));
            SendHintData(HintEventType.SetGoalCheckListShow, false);

            LevelAsset.BaseDeltaCurrency = 0.0f;
            LevelAsset.GameCurrencyMgr = new GameCurrencyMgr();
            LevelAsset.GameCurrencyMgr.InitGameMode(LevelAsset.ActionAsset.GameStartingData);
            
            LevelAsset.EnableAllCoreFunctionAndFeature();
            LevelAsset.GameBoard.InitBoardWAsset(LevelAsset.ActionAsset);
            LevelAsset.GameBoard.UpdateBoardUnit();

            if (!UseTutorialVer)
            {
                MessageDispatcher.SendMessage(new HintEventInfo {HintEventType = HintEventType.ToggleHandOnView, BoolData = true});
            }

            AdditionalInitLevel();
            ReadyToGo = true;
        }

        protected override void createDriver()
        {
            if (UseTutorialVer)
            {
                _actionDriver = new TutorialControlActionDriver(this, _mainFSM);
            }
            else
            {
                _actionDriver = new BaseControlActionDriver(this, _mainFSM);
            }
        }

        protected override void AdditionalArtLevelReference(ref GameAssets LevelAsset)
        {
            LevelAsset.Shop = FindObjectOfType<ShopSelectableMgr>();
            LevelAsset.Shop._fsmLevelLogic = this;
        }

        protected override bool NormalCheckGameOver => LevelAsset.GameCurrencyMgr.EndGameCheck();

        #region Status

        private void PreInit()
        {
            //NOP
        }

        private void MajorUpkeepAction()
        {
            _ctrlPack = _actionDriver.CtrlQueueHeader;
            UpdateBoardData_Stepped(ref LevelAsset); //RISK 放在这儿能解决一些问题，但是太费了。一个可以靠谱地检测这个需要更新的逻辑。
            AdditionalMajorUpkeep();
            //WorldExecutor.LightUpBoard(ref LevelAsset, _ctrlPack);
        }

        //现在在MinorUpkeep流程中、会将队列的break命令一口气全处理完。
        private void MinorUpKeepAction()
        {
            AdditionalMinorUpkeep();
            while (_actionDriver.PendingRequestedBreak)
            {
                //这个东西也要改成可配置的。 DONE
                _mainFSM.Breaking(_actionDriver.RequestedBreakType);
            }

            if (CheckGameOver) GameEnding();
        }
        
        //考虑吧ForwardCycle再拆碎、就是movedTile与否的两种状态。
        private void ForwardCycle()
        {
            WorldCycler.StepUp();
            if (LevelAsset.TimeLine != null)
            {
                LevelAsset.TimeLine.Step();
            }

            if (HandlingCurrency)
            {
                //Debug.Log("LevelAsset.GameCurrencyMgr.PerMove=" + LevelAsset.GameCurrencyMgr.Currency);
                LevelAsset.GameCurrencyMgr.PerMove(LevelAsset.DeltaCurrency);
                SendCurrencyMessage(LevelAsset);
            }
        }

        private void CleanUp()
        {
            MovedTile = false;
            MovedCursor = false;
            //animate_Co = null;
            LevelAsset.BoughtOnce = false;
            LevelAsset.AnimationPendingObj = new List<MoveableBase>();
            LevelAsset.LevelProgress = LevelProgress;
        }

        private void AnimateAction()
        {
            //目前这里基本空的，到时候可能把Animate的CoRoutine里面的东西弄出来。
            //Debug.Assert(animate_Co != null);
            UpdateBoardData_Stepped(ref LevelAsset);
        }

        private void ReactIO()
        {
            //这整个React to IO框架有可能都要模块化。
            WorldExecutor.UpdateCursor_Unit(ref LevelAsset, in _ctrlPack, out MovedTile, out MovedCursor);
            WorldExecutor.UpdateRotate(ref LevelAsset, in _ctrlPack, out RotatedTile, out RotatedCursor);
            //LevelAsset.GameBoard.UpdateBoardRotate(); //TODO 旋转现在还是闪现的。这个不用着急做。终于要做了！！！
            MovedTile |= _ctrlPack.HasFlag(ControllingCommand.CycleNext); //这个flag的实际含义和名称有冲突。
            if (HandlingShop)
            {
                MovedTile |= WorldExecutor.UpdateShopBuy(ref LevelAsset, in _ctrlPack);
            }
            AdditionalReactIO();
        }

        #endregion
        
        protected virtual void GameEnding()
        {
            //实质上Barebone模式下其实不能结束。//Tutorial版可以。
            if (UseTutorialVer)
            {
                PendingCleanUp = true;
                LevelMasterManager.Instance.LevelFinished(LevelAsset);
                LevelAsset.GameOverAsset = new GameOverAsset
                {
                    SuccessTerm = ScriptTerms.EndingMessageTutorial,
                    FailedTerm = ScriptTerms.EndingMessageTutorialFailed
                };
                return;
            }
            throw new Exception("This game mode could not end.");
        }
        
        protected virtual void UpdateBoardData_Instantly()
        {
            TypeASignalScore = SignalMasterMgr.Instance.CalAllScoreBySignal(
                LevelAsset.ActionAsset.AdditionalGameSetup.PlayingSignalTypeA, LevelAsset.GameBoard,
                out var hardwareACount, out TypeASignalCount);
            TypeBSignalScore = SignalMasterMgr.Instance.CalAllScoreBySignal(
                LevelAsset.ActionAsset.AdditionalGameSetup.PlayingSignalTypeB, LevelAsset.GameBoard,
                out var hardwareBCount, out TypeBSignalCount);
            if (LevelAsset.ActionAsset.AdditionalGameSetup.IsPlayingCertainSignal(SignalType.Thermo))
            {
                var thermoFieldUnits = LevelAsset.GameBoard.FindUnitWithCoreType(SignalType.Thermo, HardwareType.Field);
                var res = thermoFieldUnits.Where(u => u.SignalCore.IsUnitActive).Select(u => u.CurrentBoardPosition);
                LevelAsset.ThermoZone = res.Where(LevelAsset.GameBoard.CheckBoardPosValid).Distinct().ToList();
            }
        }

        protected virtual void UpdateBoardData_Stepped(ref GameAssets currentLevelAsset)
        {
            //BaseVerison Do-nothing.
        }
        
        //这个函数只有在Board被更新的时候才会走、但是里面有和轮次相关的数据。
        //现在的解决方法是变轮次的的时候，发一个"Board已更新"的事件.
        protected override void BoardUpdatedHandler(IMessage rMessage)
        {
            UpdateBoardData_Instantly();
            var signalInfo = new BoardSignalUpdatedInfo
            {
                SignalData = new BoardSignalUpdatedData()
                {
                    CrtTypeASignal = TypeASignalCount,
                    CrtTypeBSignal = TypeBSignalCount,
                    TypeATier = LevelAsset.GameBoard.GetTotalTierCountByType(LevelAsset.ActionAsset.AdditionalGameSetup.PlayingSignalTypeA, HardwareType.Field),
                    TypeBTier = LevelAsset.GameBoard.GetTotalTierCountByType(LevelAsset.ActionAsset.AdditionalGameSetup.PlayingSignalTypeB, HardwareType.Field),
                },
            };
            MessageDispatcher.SendMessage(signalInfo);
            if (HandlingCurrency)
            {
                //在Barebone部分收入就是每一步都计算内容；并且不考虑HeatSink的情况。
                LevelAsset.BaseDeltaCurrency = GetBaseInCome();
                LevelAsset.BonusDeltaCurrency = 0;
                SendCurrencyMessage(LevelAsset);
            }
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
                if (UseTutorialVer) TutorialModule.InjectTutorialFSMActions(ref _fsmActions);
                return _fsmActions;
            }
        }
        protected sealed override RootFSMTranstionLib RootFSMTransitions {
            get
            {
                var transitions = new RootFSMTranstionLib
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
                if (UseTutorialVer) TutorialModule.InjectTutorialFSMTransitions(ref transitions);
                return transitions;
            }
        }
    }
}