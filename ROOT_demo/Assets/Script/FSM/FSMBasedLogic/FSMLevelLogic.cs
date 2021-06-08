using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Messages;
using DG.Tweening;
using ROOT.Common;
using ROOT.Consts;
using ROOT.FSM;
using ROOT.Message;
using ROOT.Message.Inquiry;
using ROOT.SetupAsset;
using ROOT.Signal;
using ROOT.UI;
using UnityEngine;
using static ROOT.WorldEvent;

namespace ROOT
{
    using FSMActions = Dictionary<RootFSMStatus, Action>;
    using FSMTransitions = RootFSMTranstionLib;

    public abstract class FSMLevelLogic : MonoBehaviour //LEVEL-LOGIC/每一关都有一个这个类。
    {
        public bool Playing { get; set; }
        [HideInInspector] public bool Animating = false;
        [HideInInspector] public bool ReadyToGo = false;
        [HideInInspector] public bool ReferenceOk = false;
        [HideInInspector] public bool PendingCleanUp;
        public bool UseTutorialVer = false;

        public abstract bool CouldHandleTimeLine { get; }
        public abstract bool CouldHandleBoss { get; }
        public abstract BossStageType HandleBossType { get; }

        public abstract int LEVEL_ART_SCENE_ID { get; }
        protected bool MovedTile = false;
        protected bool MovedCursor = false;
        protected bool RotatedTile = false;
        protected bool RotatedCursor = false;

        protected bool ExternalQuit { get; private set; } = false;

        protected internal GameAssets LevelAsset;
        private Cursor Cursor => LevelAsset.Cursor;
        internal ControllingPack _ctrlPack;
        protected ControllingPack CtrlPack => _ctrlPack;
        protected TutorialFSMModule TutorialModule;
        public FSMFeatureManager FeatureManager;
        
        protected float AnimationTimerOrigin = 0.0f; //都是秒

        public static float AnimationDuration => WorldCycler.AnimationTimeLongSwitch ? StaticNumericData.AutoAnimationDuration : StaticNumericData.DefaultAnimationDuration;
        
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
        
        #region 类属性

        protected bool? AutoDrive => WorldCycler.NeedAutoDriveStep;

        private bool ShouldCycle => (AutoDrive.HasValue) || ShouldCycleFunc(in _ctrlPack, true, in MovedTile, in MovedCursor);

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

        protected bool ShouldStartAnimate => ShouldCycle || (RotatedCursor || RotatedTile);
        protected virtual bool IsForwardCycle => MovedTile;
        protected abstract float LevelProgress { get; }

        #endregion

        #region 元初始化相关函数

        protected static void SendHintData(HintEventType type, bool boolData) => MessageDispatcher.SendMessage(new HintEventInfo {BoolData = boolData, HintEventType = type});

        [Obsolete]
        public bool CheckReference() => true;

        protected void PopulateArtLevelReference()
        {
            ReferenceOk = CheckReference();
        }

        protected abstract void AdditionalArtLevelReference(ref GameAssets LevelAsset);

        //这个肯定也要改成Virtual的、并且要听两个的aOP。
        public IEnumerator UpdateArtLevelReference(
            AsyncOperation baseVisualScene, AsyncOperation tutorialScene,
            AsyncOperation addtionalVisualScene)
        {
            var couldProceed = false;
            do
            {
                couldProceed = baseVisualScene.isDone
                               && (addtionalVisualScene == null || addtionalVisualScene.isDone)
                               && (!UseTutorialVer || tutorialScene.isDone);
                yield return 0;
            } while (!couldProceed);

            AdditionalArtLevelReference(ref LevelAsset);
            SendHintData(HintEventType.SetTutorialTextShow, false);
            PopulateArtLevelReference();
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

        private void PostAnimationUpdate(MoveableBase moveableBase)
        {
            moveableBase.SetCurrentAndNextPos(moveableBase.NextBoardPosition);
            moveableBase.PingPongRotationDirection();
            if (moveableBase is Unit u)
            {
                u.UpdateWorldRotationTransform();
            }
        }

        protected void Animate_DOTween()
        {
            var animatingSeq = DOTween.Sequence();
            animatingSeq.PrependInterval(AnimationDuration);//是为了干挪时间轴的时候也等一个AnimationDuration的时长。
            foreach (var moveableBase in LevelAsset.AnimationPendingObj)
            {
                if (moveableBase.NextBoardPosition != moveableBase.CurrentBoardPosition)
                {
                    var actualNextPos = LevelAsset.GameBoard.GetFloatTransformAnimation(moveableBase.NextBoardPosition);
                    actualNextPos.y = moveableBase.AnimatingRoot.transform.position.y; //保证所有物体移动时对于棋盘的垂直高度不变。
                    animatingSeq.Insert(0, moveableBase.AnimatingRoot.DOMove(actualNextPos, AnimationDuration));
                }
                if (moveableBase.NextRotationDirection != moveableBase.CurrentRotationDirection)
                {
                    var actualNextRotEuler = Common.Utils.RotationToEuler(moveableBase.NextRotationDirection);
                    animatingSeq.Insert(0, moveableBase.AnimatingRoot.DORotate(actualNextRotEuler, AnimationDuration));
                }
            }
            animatingSeq.OnComplete(PostAnimateUpdate);
        }

        private void PostAnimateUpdate()
        {
            LevelAsset.AnimationPendingObj.ForEach(PostAnimationUpdate);

            if (LevelAsset.MovedTileAni)
            {
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
            if (UseTutorialVer)
            {
                TutorialModule.TutorialReactIO();
            }
        }

        protected virtual void AdditionalMajorUpkeep()
        {
            //这个东西之前没有base链式调用、改了后可能有问题；好像没有
            if (UseTutorialVer)
            {
                TutorialModule.TutorialMajorUpkeep();
            }
        }

        protected virtual void AdditionalMinorUpkeep()
        {
            if (UseTutorialVer)
            {
                TutorialModule.TutorialMinorUpkeep();
            }
        }

        #endregion

        protected abstract bool NormalCheckGameOver { get; }

        protected bool CheckGameOver => ExternalQuit || (UseTutorialVer ? TutorialModule.TutorialCheckGameOver : NormalCheckGameOver);

        protected abstract void BoardUpdatedHandler(IMessage rMessage);

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
                if (WorldCycler.GamePausedStatus) return;
                //现在这里是“动态一帧多态”设计、在一帧内现在会无限制地转移状态；
                //只不过在有的状态转移时进行了标记（即：waitForNextFrame）
                //进行标记后、就会强制等待新的一帧。
                _mainFSM.Execute();
                _mainFSM.Transit();
                RootDebug.Log("FSM:" + _mainFSM.currentStatus, NameID.YanYoumo_Log);
                //RootDebug.Watch("FSM:" + _mainFSM.currentStatus, WatchID.YanYoumo_WatchA);
            } while (!_mainFSM.waitForNextFrame);

            _mainFSM.waitForNextFrame = false; //等待之后就把这个关了。
        }

        protected abstract void createDriver();

        protected virtual void FeaturesChangedHandler()
        {
            //baseVersion Do Nothing.
        }

        private void RequestLevelQuitHandler(IMessage rMessage)
        {
            Debug.Log("ExternalQuit = true");
            ExternalQuit = true;
        }
        
        protected virtual void Awake()
        {
            LevelAsset = new GameAssets();
            _mainFSM = new RootFSM {owner = this};
            if (UseTutorialVer)
            {
                TutorialModule = new TutorialFSMModule(this);
                FeatureManager = new FSMFeatureManager {FeaturesChanged = FeaturesChangedHandler};
                FeatureManager.RegistFSMFeature(FSMFeatures.Currency,new FSMFeatures[0], false);
                FeatureManager.RegistFSMFeature(FSMFeatures.Shop,new [] {FSMFeatures.Currency}, false);
            }

            UpdateLogicLevelReference();

            _mainFSM.ReplaceActions(fsmActions);
            _mainFSM.ReplaceTransition(RootFSMTransitions);
            _mainFSM.ReplaceBreaking(RootFSMBreakings);

            LevelAsset.AnimationPendingObj = new List<MoveableBase>();
            createDriver();
            Debug.Assert(_actionDriver != null, "have to implement controller driver in 'createDriver' func");

            MessageDispatcher.AddListener(BoardUpdatedEvent, BoardUpdatedHandler);
            MessageDispatcher.AddListener(WorldEvent.BoardGridThermoZoneInquiry, BoardGridThermoZoneInquiryHandler);
            MessageDispatcher.AddListener(RequestLevelQuitEvent, RequestLevelQuitHandler);
        }

        protected virtual void OnDestroy()
        {
            MessageDispatcher.RemoveListener(RequestLevelQuitEvent, RequestLevelQuitHandler);
            MessageDispatcher.RemoveListener(WorldEvent.BoardGridThermoZoneInquiry, BoardGridThermoZoneInquiryHandler);
            MessageDispatcher.RemoveListener(BoardUpdatedEvent, BoardUpdatedHandler);

            _actionDriver.Unsubscribe();
            _actionDriver = null;
        }

        #region static Func

        protected static void SendCurrencyMessage(GameAssets LevelAsset)
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
        public static void CreateUnitOnBoard(TutorialActionData data, GameAssets LevelAsset)
        {
            var pos = data.Pos;
            if (pos.x < 0 || pos.y < 0) pos = LevelAsset.GameBoard.FindRandomEmptyPlace();
            LevelAsset.GameBoard.CreateUnit(pos, data.Core, data.HardwareType, data.Sides, data.Tier, data.IsStationary, data.Tag);
            LevelAsset.GameBoard.UpdateBoardUnit();
        }
        public static void ShowTextFunc(bool val) => SendHintData(HintEventType.SetTutorialTextShow, val);
        public static void ShowCheckListFunc(bool val) => SendHintData(HintEventType.SetGoalCheckListShow, val);

        #endregion
    }
}