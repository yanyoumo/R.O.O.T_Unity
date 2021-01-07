using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace ROOT
{
    using FSMActions = Dictionary<RootFSMStatus, Action>;
    using FSMTransitions = List<RootFSMTransition>;

    //里面不同的类型可以使用partial关键字拆开管理。
    public abstract class FSMLevelLogic : LevelLogic //LEVEL-LOGIC/每一关都有一个这个类。
    {
        //[ReadOnly] bool shouldCycle = false;
        [ReadOnly] bool movedTile = false;
        [ReadOnly] bool movedCursor = false;

        #region TransitionReq

        protected bool CheckInited()
        {
            return (ReadyToGo) && (!PendingCleanUp);
        }

        protected bool CheckAutoF()
        {
            return AutoDrive.HasValue && AutoDrive.Value;
        }

        protected bool CheckFCycle()
        {
            return forwardCycle;
        }

        protected bool CheckCtrlPackAny()
        {
            return CtrlPack.AnyFlag();
        }

        protected bool CheckAnimating()
        {
            if (MainFSM.currentStatus == RootFSMStatus.Animate)
            {
                return Animating;
            }
            else
            {
                return shouldStartAnimate;
            }
        }

        protected bool CheckNotAnimating()
        {
            return !Animating;
        }

        protected void TriggerAnimation()
        {
            MainFSM.currentStatus = RootFSMStatus.Animate;
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

        #endregion

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
        
        IEnumerator Animate()
        {
            while (AnimationLerper < 1.0f)
            {
                yield return 0;
                if (LevelAsset.AnimationPendingObj.Count > 0)
                {
                    foreach (var moveableBase in LevelAsset.AnimationPendingObj)
                    {
                        if (moveableBase.NextBoardPosition == moveableBase.CurrentBoardPosition)
                        {
                            moveableBase.SetPosWithAnimation(moveableBase.NextBoardPosition,
                                PosSetFlag.CurrentAndLerping);
                        }
                        else
                        {
                            moveableBase.LerpingBoardPosition = moveableBase.LerpBoardPos(AnimationLerper);
                        }
                    }
                }

                //加上允许手动步进后，这个逻辑就应该独立出来了。
                if (LevelAsset.MovedTileAni)
                {
                    if (LevelAsset.Shop)
                    {
                        if (LevelAsset.Shop is IAnimatableShop shop)
                        {
                            shop.ShopUpdateAnimation(AnimationLerper);
                        }
                    }
                }

                LevelAsset.GameBoard.UpdateBoardAnimation();
                cursor.UpdateTransform(LevelAsset.GameBoard.GetFloatTransformAnimation(cursor.LerpingBoardPosition));
            }

            foreach (var moveableBase in LevelAsset.AnimationPendingObj)
            {
                //完成后的pingpong
                moveableBase.SetPosWithAnimation(moveableBase.NextBoardPosition, PosSetFlag.All);
            }

            if (LevelAsset.MovedTileAni)
            {
                if (LevelAsset.GameBoard != null)
                {
                    LevelAsset.GameBoard.UpdateBoardPostAnimation();
                }

                if (LevelAsset.Shop)
                {
                    if (LevelAsset.Shop is IAnimatableShop shop)
                    {
                        shop.ShopPostAnimationUpdate();
                    }
                }
            }

            Animating = false;
        }

        //现在一共提供Info的计数是：Boss阶段*BossInfoSprayCount*SprayCountPerAnimateInterval;
        private const int SprayCountPerAnimateInterval = 4;
        private const float BossInfoSprayTimerIntervalOffsetRange = 0.5f;

        private float _bossInfoSprayTimerIntervalBase => AnimationDuration / SprayCountPerAnimateInterval;
        private float _bossInfoSprayTimerInterval => _bossInfoSprayTimerIntervalBase + _bossInfoSprayTimerIntervalOffset; 

        private float _bossInfoSprayTimerIntervalOffset = 0.0f;
        private float _bossInfoSprayTimer = 0.0f;
        private Coroutine ManualListenBossPauseKeyCoroutine;

        private int[] SprayCountArray;
        private int SprayCounter = 0;

        protected void BossInit()
        {
            var bossStageCount = LevelAsset.ActionAsset.BossStageCount;
            var totalSprayCount = bossStageCount * SprayCountPerAnimateInterval;
            //这个数据还得传过去。
            var targetInfoCount =
                Mathf.RoundToInt(LevelAsset.ActionAsset.InfoCount * LevelAsset.ActionAsset.InfoTargetRatio);
            LevelAsset.SignalPanel.SignalTarget = targetInfoCount;

            SprayCountArray = Utils.SpreadOutLayingWRandomization(totalSprayCount, LevelAsset.ActionAsset.InfoCount,
                LevelAsset.ActionAsset.InfoVariantRatio);

            LevelAsset.DestroyerEnabled = true;
            LevelAsset.SignalPanel.IsBossStage = true;
            ManualListenBossPauseKeyCoroutine = StartCoroutine(ManualPollingBossPauseKey());
            WorldCycler.BossStage = true;
        }
        protected void BossUpdate()
        {
            //Spray的逻辑可以再做一些花活。
            if (!WorldCycler.BossStagePause)
            {
                _bossInfoSprayTimer += Time.deltaTime;
                if (_bossInfoSprayTimer >= _bossInfoSprayTimerInterval)
                {
                    try
                    {
                        LevelAsset.AirDrop.SprayInfo(SprayCountArray[SprayCounter]);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        LevelAsset.AirDrop.SprayInfo(3);
                    }

                    _bossInfoSprayTimerIntervalOffset = Random.Range(
                        -BossInfoSprayTimerIntervalOffsetRange,
                        BossInfoSprayTimerIntervalOffsetRange);
                    _bossInfoSprayTimer = 0.0f;
                    SprayCounter++;
                }
            }
        }

        protected void PreInit()
        {

        }

        protected void UpKeepAction()
        {
            _ctrlPack = WorldController.UpdateInputScheme(LevelAsset, out movedTile, out movedCursor, ref LevelAsset._boughtOnce);
            RootDebug.Watch(stage.ToString(), WatchID.YanYoumo_ExampleA);
            WorldLogic.UpkeepLogic(LevelAsset, stage, false); //RISK 这个也要弄。
            LightUpBoard();
            //RISK 临时在这里试一下相关代码
            if (Input.GetKeyDown(KeyCode.K))
            {
                //BUG 这块儿炸了，还得再看看。
                //这里还是要不用Animation的流程做一个单纯的阻塞。
                _ctrlPack.SetFlag(ControllingCommand.Skill);
                WorldCycler.ExpectedStepIncrement(5);
            }
        }

        protected void ReactIO()
        {
            //CycleKeepUp();
            WorldExecutor.UpdateCursor_Unit(ref LevelAsset, in _ctrlPack, out movedTile, out movedCursor);
            WorldExecutor.UpdateRotate(ref LevelAsset, in _ctrlPack);
            LevelAsset.GameBoard.UpdateBoardRotate(); //TODO 旋转现在还是闪现的。这个不用着急做。
            var Res = WorldExecutor.UpdateShopBuy(ref LevelAsset, in _ctrlPack);

            movedTile |= Res;
            movedTile |= _ctrlPack.HasFlag(ControllingCommand.CycleNext); //这个flag的实际含义和名称有冲突。

            LevelAsset.SkillMgr.SkillEnabled = LevelAsset.SkillEnabled;
            LevelAsset.SkillMgr.TriggerSkill(LevelAsset, _ctrlPack);

            //TODO LED的时刻不只是这个函数的问题，还是积分函数的调用；后面的的时序还是要比较大幅度的调整的；
            //意外地优先级不高。
            WorldExecutor.UpdateBoardData(ref LevelAsset);
        }

        public RoundGist? roundGist=> LevelAsset.ActionAsset.GetRoundGistByStep(LevelAsset.StepCount);
        public StageType stage => roundGist?.Type ?? StageType.Shop;
        bool? AutoDrive => WorldCycler.NeedAutoDriveStep;
        //这个Anykeydown不是同一帧了；所以不能用了。
        bool shouldCycle => (AutoDrive.HasValue) || WorldLogic.ShouldCycle(in _ctrlPack, true, in movedTile, in movedCursor);
        bool shouldStartAnimate => shouldCycle;
        bool forwardCycle => (AutoDrive.HasValue && AutoDrive.Value) || movedTile;

        //考虑吧ForwardCycle再拆碎、就是movedTile与否的两种状态。
        protected void ForwardCycle()
        {
            if (forwardCycle)
            {
                //现在的框架下，在一个Loop的中段StepUp还凑活，但是感觉有隐患。
                WorldCycler.StepUp();
                LevelAsset.TimeLine.Step();
                LevelAsset.BoughtOnce = false;

                if (LevelAsset.DestroyerEnabled) WorldExecutor.UpdateDestoryer(LevelAsset);
                LevelAsset.GameStateMgr.PerMove(LevelAsset.DeltaCurrency);

                if (LevelAsset.WarningDestoryer != null && LevelAsset.DestroyerEnabled)
                {
                    LevelAsset.WarningDestoryer.Step(out var outCore);
                    LevelAsset.DestoryedCoreType = outCore;
                }

                WorldExecutor.UpdateBoardData(ref LevelAsset);
            }
        }

        protected void UpdateRoundStatus_FSM(GameAssets currentLevelAsset, RoundGist roundGist)
        {
            int normalRval = 0;
            int networkRval = 0;
            var tCount = LevelAsset.ActionAsset.GetTruncatedCount(LevelAsset.StepCount, out var count);
            var isBossRound = roundGist.Type == StageType.Boss;
            var isShopRound = roundGist.Type == StageType.Shop;
            var isRequireRound = roundGist.Type == StageType.Require;
            var isDestoryerRound = roundGist.Type == StageType.Destoryer;
            var isSkillAllowed = !isShopRound;
            var shouldCurrencyIo = (isRequireRound || isDestoryerRound);

            if (isRequireRound && movedTile)
            {
                LevelAsset.GameBoard.UpdatePatternDiminishing();
            }

            if (isRequireRound || isShopRound)
            {
                normalRval = roundGist.normalReq;
                networkRval = roundGist.networkReq;
            }
            
            if ((lastDestoryBool && !isDestoryerRound) && !WorldCycler.NeedAutoDriveStep.HasValue)
            {
                Debug.Log("LevelAsset.GameBoard.DestoryHeatsinkOverlappedUnit()");
                LevelAsset.GameBoard.DestoryHeatsinkOverlappedUnit();
            }

            if (roundGist.SwitchHeatsink(tCount))
            {
                if (_obselateStepID == -1 || _obselateStepID != LevelAsset.StepCount)
                {
                    LevelAsset.GameBoard.UpdatePatternID();
                }
                _obselateStepID = LevelAsset.StepCount;
            }
            
            if ((LevelAsset.DestroyerEnabled && !isDestoryerRound) && !WorldCycler.BossStage)
            {
                LevelAsset.WarningDestoryer.ForceReset();
            }

            lastDestoryBool = isDestoryerRound;

            LevelAsset.DestroyerEnabled = WorldCycler.BossStage;
            LevelAsset.CurrencyIncomeEnabled = isRequireRound;
            LevelAsset.CurrencyIOEnabled = shouldCurrencyIo;

            int harDriverCountInt = 0, networkCountInt = 0;
            _noRequirement = (normalRval == 0 && networkRval == 0);

            if (_noRequirement)
            {
                currentLevelAsset.TimeLine.RequirementSatisfied = true;
            }
            else
            {
                SignalMasterMgr.Instance.CalAllScoreBySignal(SignalType.Matrix, currentLevelAsset.GameBoard, out harDriverCountInt);
                SignalMasterMgr.Instance.CalAllScoreBySignal(SignalType.Scan, currentLevelAsset.GameBoard, out networkCountInt);
                currentLevelAsset.TimeLine.RequirementSatisfied = (harDriverCountInt >= normalRval) && (networkCountInt >= networkRval);
            }

            var discount = 0;
            if (!LevelAsset.Shop.ShopOpening && isShopRound)
            {
                discount = LevelAsset.SkillMgr.CheckDiscount();
            }
            LevelAsset.Shop.OpenShop(isShopRound, discount);
            LevelAsset.SkillEnabled = isSkillAllowed;

            LevelAsset.SignalPanel.TgtNormalSignal = normalRval;
            LevelAsset.SignalPanel.TgtNetworkSignal = networkRval;
            LevelAsset.SignalPanel.CrtNormalSignal = harDriverCountInt;
            LevelAsset.SignalPanel.CrtNetworkSignal = networkCountInt;
            LevelAsset.SignalPanel.NetworkTier = LevelAsset.GameBoard.GetTotalTierCountByCoreType(CoreType.NetworkCable);
            LevelAsset.SignalPanel.NormalTier = LevelAsset.GameBoard.GetTotalTierCountByCoreType(CoreType.HardDrive);
        }

        protected void CareerCycle()
        {
            if (roundGist.HasValue)
            {
                UpdateRoundStatus_FSM(LevelAsset, roundGist.Value);
                if (LevelAsset.GameOverEnabled)
                {
                    UpdateGameOverStatus(LevelAsset);
                }
            }

            if (((AutoDrive.HasValue && AutoDrive.Value || shouldCycle && movedTile)) && (!_noRequirement))
            {
                if (LevelAsset.TimeLine.RequirementSatisfied)
                {
                    LevelAsset.ReqOkCount++;
                }
            }
        }

        protected void ReverseCycle()
        {
            //CycleKeepUp();
        }

        protected void AnimateAction()
        {
            //目前这里基本空的，到时候可能把Animate的CoRoutine里面的东西弄出来。
            Debug.Assert(animate_Co != null);
            WorldLogic.UpkeepLogic(LevelAsset, stage, Animating);
        }

        protected void CleanUp()
        {
            //shouldCycle = false;
            movedTile = false;
            movedCursor = false;
            animate_Co = null;
            LevelAsset.AnimationPendingObj = new List<MoveableBase>();
            LevelAsset.DeltaCurrency = 0.0f;
            LevelAsset.LevelProgress = LevelAsset.StepCount / (float)LevelAsset.ActionAsset.PlayableCount;
        }

        private RootFSM MainFSM;

        private void InitDestoryer()
        {
            LevelAsset.WarningDestoryer = new MeteoriteBomber {GameBoard = LevelAsset.GameBoard};
            LevelAsset.WarningDestoryer.Init(4, 1);
        }

        private void InitShop()
        {
            LevelAsset.Shop.ShopInit(LevelAsset);
            LevelAsset.Shop.CurrentGameStateMgr = LevelAsset.GameStateMgr;
            LevelAsset.Shop.GameBoard = LevelAsset.GameBoard;
            if (LevelAsset.ActionAsset.ExcludedShop)
            {
                LevelAsset.Shop.excludedTypes = LevelAsset.ActionAsset.ShopExcludedType;
            }
        }

        private void InitCursor(Vector2Int pos)
        {
            LevelAsset.GameCursor = Instantiate(LevelAsset.CursorTemplate);
            Cursor cursor = LevelAsset.GameCursor.GetComponent<Cursor>();
            cursor.InitPosWithAnimation(pos);
            cursor.UpdateTransform(LevelAsset.GameBoard.GetFloatTransformAnimation(cursor.LerpingBoardPosition));
        }

        public sealed override void InitLevel()
        {
            Debug.Assert(ReferenceOk); //意外的有确定Reference的……还行……
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));

            LevelAsset.DeltaCurrency = 0.0f;

            LevelAsset.GameStateMgr = new GameStateMgr();
            LevelAsset.GameStateMgr.InitGameMode(LevelAsset.ActionAsset.GameModeAsset);
            LevelAsset.CostChart.CurrencyVal = Mathf.RoundToInt(LevelAsset.GameStateMgr.GetCurrency());

            InitShop();
            InitDestoryer();
            InitCursor(new Vector2Int(2, 3));
            LevelAsset.EnableAllCoreFunctionAndFeature();
            LevelAsset.GameBoard.InitBoardWAsset(LevelAsset.ActionAsset);
            LevelAsset.GameBoard.UpdateBoardAnimation();
            StartShop();

            ReadyToGo = true;
            if (LevelAsset.ActionAsset.RoundDatas.Length > 0)
            {
                //这个东西放在这里还是怎么着？就先这样吧。
                WorldCycler.InitCycler();
                LevelAsset.TimeLine.InitWithAssets(LevelAsset);
            }

            LevelAsset.HintMaster.ShouldShowCheckList = false;
        }

        private void LightUpBoard()
        {
            if (_ctrlPack.HasFlag(ControllingCommand.FloatingOnGrid) ||
                _ctrlPack.HasFlag(ControllingCommand.ClickOnGrid))
            {
                if (_ctrlPack.HasFlag(ControllingCommand.FloatingOnGrid))
                {
                    LevelAsset.GameBoard.LightUpBoardGird(_ctrlPack.CurrentPos);
                }

                if (_ctrlPack.HasFlag(ControllingCommand.ClickOnGrid))
                {
                    LevelAsset.GameBoard.LightUpBoardGird(_ctrlPack.CurrentPos,
                        LightUpBoardGirdMode.REPLACE,
                        LightUpBoardColor.Clicked);
                }
            }
            else
            {
                LevelAsset.GameBoard.LightUpBoardGird(Vector2Int.zero, LightUpBoardGirdMode.CLEAR);
            }
        }

        protected abstract FSMActions fsmActions { get; }
        protected abstract FSMTransitions RootFSMTransitions { get; }

        protected sealed override void StartShop()
        {
            base.StartShop();
        }
        protected sealed override bool UpdateGameOverStatus(GameAssets currentLevelAsset)
        {
            return LevelAsset.ActionAsset.HasEnded(LevelAsset.StepCount);
        }
        protected sealed override void UpdateLogicLevelReference()
        {
            base.UpdateLogicLevelReference();
        }

        void Awake()
        {
            LevelAsset = new GameAssets();
            MainFSM = new RootFSM {owner = this};

            UpdateLogicLevelReference();

            MainFSM.ReplaceActions(fsmActions);
            MainFSM.ReplaceTransition(RootFSMTransitions);

            LevelAsset.AnimationPendingObj = new List<MoveableBase>();
        }
        //现在就是要将还有大型分叉的逻辑以状态的形式拆出来、如果简单的一个if Guard就忍了；
        //然后原本设计的基于事件的逻辑；就需要变成，发出事件是：请求逻辑状态机立刻（或者延迟）切换状态或者改变参数什么的。
        //WorldLogic就在FSM的语境下拆散的world_executor里面去了。

        //现在操纵有微妙的延迟，是因为IO的控制状态（Idle）到实际的动画（开启）（Cycle）之间还隔了一帧。
        //大体上还是要把IO变成事件、可以将FSM跳到某个状态上；要不然还得弄。
        void Update()
        {
            //这里有个很好的地方，状态转移到是和Update完全解耦了。
            //这里可以让各个状态的物理事件间隔减少、但是绝对不是靠谱的解法；还是要用事件。
            var transitPerFrame = 3;
            for (var i = 0; i < transitPerFrame; i++)
            {
                MainFSM.Execute();
                MainFSM.Transit();
                RootDebug.Watch("FSM:" + MainFSM.currentStatus, WatchID.YanYoumo_ExampleA);
                //RootDebug.Log("FSM:" + MainFSM.currentStatus, NameID.YanYoumo_Log);
            }
        }
    }
}

