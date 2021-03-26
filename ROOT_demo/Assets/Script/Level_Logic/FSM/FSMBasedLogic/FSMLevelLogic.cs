using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Messages;
using ROOT.SetupAsset;
using ROOT.Signal;
using ROOT.UI;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ROOT.WorldEvent;

namespace ROOT
{
    using FSMActions = Dictionary<RootFSMStatus, Action>;
    using FSMTransitions = HashSet<RootFSMTransition>;

    public enum RootFSMStatus
    {
        //这里写全部的、Root系列中、全部可以使用的潜在状态。
        PreInit,//FSM逻辑在初始化完成之前“阻塞性”逻辑、原则上里面不写实际逻辑。
        MajorUpKeep,//查询玩家的输入事件、并且进行基础的清理、更新逻辑。
        MinorUpKeep,//即使在Animate流程也会执行的逻辑部分、主要是查询是否有打断输入。
        R_Cycle,//倒行逻辑的部分。
        F_Cycle,//整形逻辑的核心逻辑、主要是执行具体的主干更新、数据更新等等。
        Career_Cycle,//现有“职业”模式需要的逻辑、包含但不限于对时间轴数据的更新、等等。
        Tutorial_Cycle,//教程相关流程的演进。
        R_IO,//ReactToIO、对从Driver获得的CtrlPack转换成实际执行的逻辑。
        Skill,//这个是在使用某些技能的时候需要进行Upkeep的代码。
        TelemetryPause,//在Boss暂停的时候执行的代码。
        Animate,//将动画向前执行一帧、但是Root的动画流程时绑定时间而不是绑定帧数的。
        CleanUp,//将所有FSM的类数据重置、并且是FSM流程等待一帧的充分条件。
        COUNT,//搁在最后、计数的。
    }

    public abstract class FSMLevelLogic:MonoBehaviour   //LEVEL-LOGIC/每一关都有一个这个类。
    {
        public bool Playing { get; set; }
        [HideInInspector] public bool Animating = false;
        [HideInInspector] public bool ReadyToGo = false;
        [HideInInspector] public bool ReferenceOk = false;
        [HideInInspector] public bool PendingCleanUp;
        [ShowInInspector] public bool IsTutorialLevel => IsTutorial;
        [HideInInspector] public bool movedTile = false;

        public abstract bool IsTutorial { get; }
        public abstract bool CouldHandleSkill { get; }
        public abstract bool CouldHandleBoss { get; }
        public abstract BossStageType HandleBossType { get; }
        
        public abstract int LEVEL_ART_SCENE_ID { get; }
        private bool movedCursor = false;
        
        protected internal GameAssets LevelAsset;
        private Cursor Cursor => LevelAsset.Cursor;
        protected ControllingPack _ctrlPack;
        protected ControllingPack CtrlPack => _ctrlPack;

        private float AnimationTimerOrigin = 0.0f; //都是秒
        public static float AnimationDuration => WorldCycler.AnimationTimeLongSwitch ? AutoAnimationDuration : DefaultAnimationDuration;
        private static readonly float DefaultAnimationDuration = 0.15f; //都是秒
        private static readonly float AutoAnimationDuration = 1.5f; //都是秒

        protected abstract string SucceedEndingTerm { get; }
        protected abstract string FailedEndingTerm { get; }

        #region 类属性

        protected bool? AutoDrive => WorldCycler.NeedAutoDriveStep;
        private bool ShouldCycle => (AutoDrive.HasValue) || ShouldCycleFunc(in _ctrlPack, true, in movedTile, in movedCursor);
        private bool ShouldStartAnimate => ShouldCycle;
        protected virtual bool IsForwardCycle => movedTile;
        #endregion

        #region 元初始化相关函数

        protected void SendHintData(HintEventType type, bool boolData) => MessageDispatcher.SendMessage(new HintEventInfo {BoolData = boolData, HintEventType = type});

        [Obsolete]
        public bool CheckReference() => true;

        protected void PopulateArtLevelReference()
        {
            ReferenceOk = CheckReference();
        }

        protected virtual void AdditionalArtLevelReference(ref GameAssets LevelAsset)
        {
            //BaseVerison,DoNothing.
        }

        //这个肯定也要改成Virtual的、并且要听两个的aOP。
        public abstract IEnumerator UpdateArtLevelReference(AsyncOperation baseVisualScene,AsyncOperation addtionalVisualScene);

        #endregion

        #region FSM参数
        protected RootFSM _mainFSM;
        protected ControlActionDriver _actionDriver;
        protected abstract FSMActions fsmActions { get; }
        protected abstract FSMTransitions RootFSMTransitions { get; }
        protected virtual Dictionary<BreakingCommand, Action> RootFSMBreakings => new Dictionary<BreakingCommand, Action>();
        
        protected float TypeASignalScore = 0;
        protected float TypeBSignalScore = 0;
        protected int TypeASignalCount = 0;
        protected int TypeBSignalCount = 0;
        #endregion
        
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
            LevelAsset.MovedTileAni = movedTile;
            LevelAsset.MovedCursorAni = movedCursor;
            animate_Co = StartCoroutine(Animate()); //这里完成后会把Animating设回来。
        }
        
        private bool ShouldCycleFunc(in ControllingPack ctrlPack, in bool pressedAny, in bool movedTile, in bool movedCursor)
        {
            var shouldCycleTMP = false;
            var hasCycleNext = ctrlPack.HasFlag(ControllingCommand.CycleNext);
            if (StartGameMgr.UseTouchScreen)
            {
                shouldCycleTMP = movedTile | hasCycleNext;
            }
            else if (StartGameMgr.UseMouse)
            {
                shouldCycleTMP = ((movedTile | movedCursor)) | hasCycleNext;
            }
            else
            {
                shouldCycleTMP = (pressedAny & (movedTile | movedCursor)) | hasCycleNext;
            }

            return shouldCycleTMP;
        }

        #endregion

        #region Init

        public abstract void InitLevel();
        
        private void UpdateLogicLevelReference()
        {
            LevelAsset.CursorTemplate = Resources.Load<GameObject>("Cursor/Prefab/Cursor");
            LevelAsset.GameBoard = FindObjectOfType<Board>();
            LevelAsset.AirDrop = LevelAsset.GameBoard.AirDrop;
            LevelAsset.AirDrop.GameAsset = LevelAsset;
            LevelAsset.Owner = this;
        }

        #endregion

        #region Animate
        private float animationTimer => Time.timeSinceLevelLoad - AnimationTimerOrigin;
        private float AnimationLerper
        {
            get
            {
                float res = animationTimer / AnimationDuration;
                return Mathf.Min(res, 1.0f);
            }
        }

        private Coroutine animate_Co;
        private void AnimatingUpdate(MoveableBase moveableBase)
        {
            if (moveableBase.NextBoardPosition == moveableBase.CurrentBoardPosition)
            {
                moveableBase.SetPosWithAnimation(moveableBase.NextBoardPosition, PosSetFlag.CurrentAndLerping);
            }
            else
            {
                moveableBase.LerpingBoardPosition = moveableBase.LerpBoardPos(AnimationLerper);
            }
        }

        private void PostAnimationUpdate(MoveableBase moveableBase)
        {
            moveableBase.SetPosWithAnimation(moveableBase.NextBoardPosition, PosSetFlag.All);
        }

        private IEnumerator Animate()
        {
            while (AnimationLerper < 1.0f)
            {
                yield return 0;
                LevelAsset.AnimationPendingObj.ForEach(AnimatingUpdate);
                
                //加上允许手动步进后，这个逻辑就应该独立出来了。
                if (LevelAsset.MovedTileAni && LevelAsset.Shop && LevelAsset.Shop is IAnimatableShop shop)
                {
                    shop.ShopUpdateAnimation(AnimationLerper);
                }

                LevelAsset.GameBoard.UpdateBoardAnimation();
                Cursor.UpdateTransform(LevelAsset.GameBoard.GetFloatTransformAnimation(Cursor.LerpingBoardPosition));
            }

            LevelAsset.AnimationPendingObj.ForEach(PostAnimationUpdate);

            if (LevelAsset.MovedTileAni)
            {
                if (LevelAsset.GameBoard != null)
                {
                    LevelAsset.GameBoard.UpdateBoardPostAnimation();
                }

                if (LevelAsset.Shop && LevelAsset.Shop is IAnimatableShop shop)
                {
                    shop.ShopPostAnimationUpdate();
                }
            }

            Animating = false;
        }

        #endregion

        #region AdditionalActionInjection

        protected virtual void AdditionalReactIO()
        {
            //BaseVerison Do-nothing.
        }

        protected virtual void AdditionalMajorUpkeep()
        {
            //BaseVerison Do-nothing.
        }

        protected virtual void AdditionalMinorUpkeep()
        {
            //BaseVerison Do-nothing.
        }

        #endregion

        #region Status

        protected void PreInit()
        {
            //NOP
        }
        
        protected void MajorUpkeepAction()
        {
            _ctrlPack = _actionDriver.CtrlQueueHeader;
            UpdateBoardData_Stepped(ref LevelAsset);//RISK 放在这儿能解决一些问题，但是太费了。一个可以靠谱地检测这个需要更新的逻辑。
            AdditionalMajorUpkeep();
            //WorldExecutor.LightUpBoard(ref LevelAsset, _ctrlPack);
        }
        
        //现在在MinorUpkeep流程中、会将队列的break命令一口气全处理完。
        protected void MinorUpKeepAction()
        {
            AdditionalMinorUpkeep();
            while (_actionDriver.PendingRequestedBreak)
            {
                //这个东西也要改成可配置的。 DONE
                _mainFSM.Breaking(_actionDriver.RequestedBreakType);
            }
            if (CheckGameOver) GameEnding();
        }

        protected void SendCurrencyMessage()
        {
            var message = new CurrencyUpdatedInfo()
            {
                CurrencyVal = Mathf.RoundToInt(LevelAsset.GameCurrencyMgr.Currency),
                TotalIncomesVal = Mathf.RoundToInt(LevelAsset.DeltaCurrency),
                BaseIncomesVal = Mathf.RoundToInt(LevelAsset.BaseDeltaCurrency),
                BonusIncomesVal = Mathf.RoundToInt(LevelAsset.BonusDeltaCurrency),
            };
            MessageDispatcher.SendMessage(message);
        }

        //考虑吧ForwardCycle再拆碎、就是movedTile与否的两种状态。
        protected void ForwardCycle()
        {
            WorldCycler.StepUp();
            if (LevelAsset.TimeLine != null)
            {
                LevelAsset.TimeLine.Step();
            }

            LevelAsset.GameCurrencyMgr.PerMove(LevelAsset.DeltaCurrency);
            SendCurrencyMessage();
        }

        protected void CleanUp()
        {
            movedTile = false;
            movedCursor = false;
            animate_Co = null;
            LevelAsset.BoughtOnce = false;
            LevelAsset.AnimationPendingObj = new List<MoveableBase>();
            LevelAsset.LevelProgress = LevelAsset.StepCount / (float)LevelAsset.ActionAsset.PlayableCount;
        }

        protected void AnimateAction()
        {
            //目前这里基本空的，到时候可能把Animate的CoRoutine里面的东西弄出来。
            Debug.Assert(animate_Co != null);
            UpdateBoardData_Stepped(ref LevelAsset);
        }
        
        protected void ReactIO()
        {
            //这整个React to IO框架有可能都要模块化。
            WorldExecutor.UpdateCursor_Unit(ref LevelAsset, in _ctrlPack, out movedTile, out movedCursor);
            WorldExecutor.UpdateRotate(ref LevelAsset, in _ctrlPack);
            LevelAsset.GameBoard.UpdateBoardRotate(); //TODO 旋转现在还是闪现的。这个不用着急做。
            var Res = WorldExecutor.UpdateShopBuy(ref LevelAsset, in _ctrlPack);

            movedTile |= Res;
            movedTile |= _ctrlPack.HasFlag(ControllingCommand.CycleNext); //这个flag的实际含义和名称有冲突。

            AdditionalReactIO();
        }

        protected void SkillMajorUpkeep()
        {
            LevelAsset.SkillMgr.SwapTick_FSM(LevelAsset, _ctrlPack);
            movedTile = false;
        }

        #endregion

        private void GameEnding()
        {
            PendingCleanUp = true;
            LevelMasterManager.Instance.LevelFinished(LevelAsset);
            LevelAsset.GameOverAsset.SuccessTerm = SucceedEndingTerm;
            LevelAsset.GameOverAsset.FailedTerm = FailedEndingTerm;
        }
        
        protected virtual bool CheckGameOver => LevelAsset.GameCurrencyMgr.EndGameCheck();

        private void UpdateBoardData_Instantly()
        {
            var currentLevelAsset = LevelAsset;
            TypeASignalScore = SignalMasterMgr.Instance.CalAllScoreBySignal(
                currentLevelAsset.ActionAsset.AdditionalGameSetup.PlayingSignalTypeA, currentLevelAsset.GameBoard,
                out var hardwareACount, out TypeASignalCount);
            TypeBSignalScore = SignalMasterMgr.Instance.CalAllScoreBySignal(
                currentLevelAsset.ActionAsset.AdditionalGameSetup.PlayingSignalTypeB, currentLevelAsset.GameBoard,
                out var hardwareBCount, out TypeBSignalCount);
            if (LevelAsset.ActionAsset.AdditionalGameSetup.IsPlayingCertainSignal(SignalType.Thermo))
            {
                var thermoFieldUnits=LevelAsset.GameBoard.FindUnitWithCoreType(SignalType.Thermo, HardwareType.Field);
                //var res = new List<Vector2Int>();
                //thermoFieldUnits.Select(u => u.SignalCore as ThermoUnitSignalCore).Where(s => s.IsUnitActive).ForEach(s => res.AddRange(s.ExpellingPatternList));
                var res = thermoFieldUnits.Where(u=>u.SignalCore.IsUnitActive).Select(u => u.CurrentBoardPosition);
                currentLevelAsset.ThermoZone = res.Where(LevelAsset.GameBoard.CheckBoardPosValid).Distinct().ToList();
            }
        }

        protected virtual void UpdateBoardData_Stepped(ref GameAssets currentLevelAsset)
        {
            //BaseVerison Do-nothing.
        }


        //这个函数只有在Board被更新的时候才会走、但是里面有和轮次相关的数据。
        //现在的解决方法是变轮次的的时候，发一个"Board已更新"的事件.
        protected virtual void BoardUpdatedHandler(IMessage rMessage)
        {
            UpdateBoardData_Instantly();
            var signalInfo = new BoardSignalUpdatedInfo
            {
                SignalData = new BoardSignalUpdatedData()
                {
                    CrtTypeASignal = TypeASignalCount,
                    CrtTypeBSignal = TypeBSignalCount,
                    TypeATier = LevelAsset.GameBoard.GetTotalTierCountByCoreType(LevelAsset.ActionAsset.AdditionalGameSetup.PlayingSignalTypeA, HardwareType.Field),
                    TypeBTier = LevelAsset.GameBoard.GetTotalTierCountByCoreType(LevelAsset.ActionAsset.AdditionalGameSetup.PlayingSignalTypeB, HardwareType.Field),
                },
            };
            MessageDispatcher.SendMessage(signalInfo);
        }
        
        private void BoardGridThermoZoneInquiryHandler(IMessage rMessage)
        {
            if (rMessage is BoardGridThermoZoneInquiry info)
            {
                info.BoardGridThermoZoneInquiryCallBack(LevelAsset.ThermoZone);
            }
        }
        
        private void Update()
        {
            do
            {
                //现在这里是“动态一帧多态”设计、在一帧内现在会无限制地转移状态；
                //只不过在有的状态转移时进行了标记（即：waitForNextFrame）
                //进行标记后、就会强制等待新的一帧。
                _mainFSM.Execute();
                _mainFSM.Transit();
                RootDebug.Log("FSM:" + _mainFSM.currentStatus, NameID.YanYoumo_Log);
                //RootDebug.Watch("FSM:" + _mainFSM.currentStatus, WatchID.YanYoumo_WatchA);
            } while (!_mainFSM.waitForNextFrame);
            _mainFSM.waitForNextFrame = false;//等待之后就把这个关了。
        }
        
        protected virtual void Awake()
        {
            LevelAsset = new GameAssets();
            _mainFSM = new RootFSM {owner = this};
            //_inquiryResponder = new FSMEventInquiryResponder(this);
            
            UpdateLogicLevelReference();

            _mainFSM.ReplaceActions(fsmActions);
            _mainFSM.ReplaceTransition(RootFSMTransitions);
            _mainFSM.ReplaceBreaking(RootFSMBreakings);

            LevelAsset.AnimationPendingObj = new List<MoveableBase>();
            _actionDriver = new BaseControlActionDriver(this, _mainFSM);
            
            MessageDispatcher.AddListener(BoardUpdatedEvent, BoardUpdatedHandler);
            MessageDispatcher.AddListener(WorldEvent.BoardGridThermoZoneInquiry,BoardGridThermoZoneInquiryHandler);
        }
        protected virtual void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.BoardGridThermoZoneInquiry,BoardGridThermoZoneInquiryHandler);
            MessageDispatcher.RemoveListener(BoardUpdatedEvent, BoardUpdatedHandler);

            _actionDriver.unsubscribe();
            _actionDriver = null;
        }
    }
}