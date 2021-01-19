using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        R_IO,//ReactToIO、对从Driver获得的CtrlPack转换成实际执行的逻辑。
        Skill,//这个是在使用某些技能的时候需要进行Upkeep的代码。
        BossInit,//进入boss阶段的时候、初始化调用一次的代码。
        BossMajorUpKeep,//Boss阶段时和MajorUpKeep相同时刻调用的代码。
        BossMinorUpKeep,//Boss阶段时和MinorUpKeep相同时刻调用的代码。
        BossPause,//在Boss暂停的时候执行的代码。
        Animate,//将动画向前执行一帧、但是Root的动画流程时绑定时间而不是绑定帧数的。
        CleanUp,//将所有FSM的类数据重置、并且是FSM流程等待一帧的充分条件。
    }
    
    //里面不同的类型可以使用partial关键字拆开管理。
    public abstract class FSMLevelLogic : LevelLogic //LEVEL-LOGIC/每一关都有一个这个类。
    {
        //[ReadOnly] bool shouldCycle = false;
        [ReadOnly] private bool movedTile = false;
        [ReadOnly] private bool movedCursor = false;

        protected virtual void AdditionalArtLevelReference(ref GameAssets LevelAsset)
        {
            //BaseVerison,DoNothing.
        }

        public override IEnumerator UpdateArtLevelReference(AsyncOperation aOP)
        {
            while (!aOP.isDone)
            {
                yield return 0;
            }
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVEVISUAL));
            LevelAsset.ItemPriceRoot = GameObject.Find("PlayUI");
            LevelAsset.Shop = FindObjectOfType<ShopBase>();
            LevelAsset.DataScreen = FindObjectOfType<DataScreen>();
            LevelAsset.HintMaster = FindObjectOfType<HintMaster>();
            AdditionalArtLevelReference(ref LevelAsset);
            LevelAsset.HintMaster.HideTutorialFrame = false;
            PopulateArtLevelReference();
        }

        #region FSM参数
        private RootFSM _mainFSM;
        private ControlActionDriver _actionDriver;
        protected abstract FSMActions fsmActions { get; }
        protected abstract FSMTransitions RootFSMTransitions { get; }
        protected virtual Dictionary<BreakingCommand, Action> RootFSMBreakings => new Dictionary<BreakingCommand, Action>();
        #endregion
        
        #region TransitionReq

        protected bool CheckBossStageInit()
        {
            return (Stage == StageType.Boss)&&(!WorldCycler.BossStage);
        }

        protected bool CheckBossStage()
        {
            return (Stage == StageType.Boss);
        }

        protected bool CheckBossAndPaused()
        {
            return WorldCycler.BossStage && WorldCycler.BossStagePause;
        }

        protected bool CheckBossAndNotPaused()
        {
            return WorldCycler.BossStage && !WorldCycler.BossStagePause;
        }

        protected bool CheckIsSkill()
        {
            return LevelAsset.SkillMgr.CurrentSkillType.HasValue && LevelAsset.SkillMgr.CurrentSkillType.Value == SkillType.Swap;
        }

        protected bool CheckInited()
        {
            return (ReadyToGo) && (!PendingCleanUp);
        }

        protected bool CheckAutoF()
        {
            return AutoDrive.HasValue && AutoDrive.Value;
        }
        protected bool CheckAutoR()
        {
            return IsReverseCycle;
        }

        protected bool CheckFCycle()
        {
            return IsForwardCycle;
        }
        
        protected bool CheckCtrlPackAny()
        {
            return CtrlPack.AnyFlag();
        }

        protected bool CheckStartAnimate()
        {
            return ShouldStartAnimate;
        }

        protected bool CheckLoopAnimate()
        {
            return Animating;
        }

        protected bool CheckNotAnimating()
        {
            return !Animating;
        }
        
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
                cursor.UpdateTransform(LevelAsset.GameBoard.GetFloatTransformAnimation(cursor.LerpingBoardPosition));
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

        #region 类属性

        private RoundGist? RoundGist=> LevelAsset.ActionAsset.GetRoundGistByStep(LevelAsset.StepCount);
        private StageType Stage => RoundGist?.Type ?? StageType.Shop;
        bool IsBossRound => Stage == StageType.Boss;
        bool IsShopRound => Stage == StageType.Shop;
        bool IsRequireRound => Stage == StageType.Require;
        bool IsDestoryerRound => Stage == StageType.Destoryer;
        bool IsSkillAllowed => !IsShopRound;
        bool ShouldCurrencyIo => (IsRequireRound || IsDestoryerRound);
        private bool? AutoDrive => WorldCycler.NeedAutoDriveStep;
        private bool ShouldCycle => (AutoDrive.HasValue) || WorldExecutor.ShouldCycle(in _ctrlPack, true, in movedTile, in movedCursor);
        private bool ShouldStartAnimate => ShouldCycle;
        private bool IsForwardCycle => (AutoDrive.HasValue && AutoDrive.Value) || movedTile;
        private bool IsReverseCycle => (AutoDrive.HasValue && !AutoDrive.Value);
        #endregion
        
        protected void PreInit()
        {
            //NOP
        }

        protected void MajorUpkeepAction()
        {
            _ctrlPack = _actionDriver.CtrlQueueHeader;
            WorldExecutor.UpkeepLogic(LevelAsset, Stage, false); //RISK 这个也要弄。
            WorldExecutor.LightUpBoard(ref LevelAsset, _ctrlPack);
        }

        protected void DealBossPauseBreaking()
        {
            if (CheckBossAndNotPaused())
            {
                WorldExecutor.BossStagePauseTriggered(ref LevelAsset);
            }
        }
        
        //现在在MinorUpkeep流程中、会将队列的break命令一口气全处理完。
        protected void MinorUpKeepAction()
        {
            while (_actionDriver.PendingRequestedBreak)
            {
                //这个东西也要改成可配置的。 DONE
                _mainFSM.Breaking(_actionDriver.RequestedBreakType);
            }
        }

        protected void ReactIO()
        {
            WorldExecutor.UpdateCursor_Unit(ref LevelAsset, in _ctrlPack, out movedTile, out movedCursor);
            WorldExecutor.UpdateRotate(ref LevelAsset, in _ctrlPack);
            LevelAsset.GameBoard.UpdateBoardRotate(); //TODO 旋转现在还是闪现的。这个不用着急做。
            var Res = WorldExecutor.UpdateShopBuy(ref LevelAsset, in _ctrlPack);

            movedTile |= Res;
            movedTile |= _ctrlPack.HasFlag(ControllingCommand.CycleNext); //这个flag的实际含义和名称有冲突。

            if (LevelAsset.SkillEnabled && LevelAsset.SkillMgr != null)
            {
                //SkillMgr的分支怎么处理？
                LevelAsset.SkillMgr.TriggerSkill(LevelAsset, _ctrlPack);
            }

            if (_ctrlPack.HasFlag(ControllingCommand.BossResume))
            {
                if (CheckBossAndPaused())
                {
                    WorldExecutor.BossStagePauseTriggered(ref LevelAsset);
                }
            }

            //意外地优先级不高。
            //TODO LED的时刻不只是这个函数的问题，还是积分函数的调用；后面的的时序还是要比较大幅度的调整的；
            WorldExecutor.UpdateBoardData(ref LevelAsset);
        }

        //考虑吧ForwardCycle再拆碎、就是movedTile与否的两种状态。
        protected void ForwardCycle()
        {
            if (IsForwardCycle)
            {
                //现在的框架下，在一个Loop的中段StepUp还凑活，但是感觉有隐患。
                WorldCycler.StepUp();
                if (LevelAsset.TimeLine != null)
                {
                    LevelAsset.TimeLine.Step();
                }
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

        protected void ReverseCycle()
        {
            WorldCycler.StepDown();
            LevelAsset.TimeLine.Reverse();
        }
        
        protected void SkillMajorUpkeep()
        {
            LevelAsset.SkillMgr.SwapTick_FSM(LevelAsset, _ctrlPack);
            movedTile = false;
        }

        private void UpdateRoundStatus_FSM()
        {
            // ReSharper disable once PossibleInvalidOperationException
            var roundGist = RoundGist.Value;
            var normalRval = 0;
            var networkRval = 0;
            var tCount = LevelAsset.ActionAsset.GetTruncatedCount(LevelAsset.StepCount, out var count);

            if (IsRequireRound && movedTile)
            {
                LevelAsset.GameBoard.UpdatePatternDiminishing();
            }

            if (IsRequireRound || IsShopRound)
            {
                normalRval = roundGist.normalReq;
                networkRval = roundGist.networkReq;
            }
            
            if ((lastDestoryBool && !IsDestoryerRound) && !WorldCycler.NeedAutoDriveStep.HasValue)
            {
                //Debug.Log("LevelAsset.GameBoard.DestoryHeatsinkOverlappedUnit()");
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
            
            if ((LevelAsset.DestroyerEnabled && !IsDestoryerRound) && !WorldCycler.BossStage)
            {
                LevelAsset.WarningDestoryer.ForceReset();
            }

            lastDestoryBool = IsDestoryerRound;

            LevelAsset.DestroyerEnabled = WorldCycler.BossStage;
            LevelAsset.CurrencyIncomeEnabled = IsRequireRound;
            LevelAsset.CurrencyIOEnabled = ShouldCurrencyIo;

            int harDriverCountInt = 0, networkCountInt = 0;
            _noRequirement = (normalRval == 0 && networkRval == 0);

            if (_noRequirement)
            {
                LevelAsset.TimeLine.RequirementSatisfied = true;
            }
            else
            {
                SignalMasterMgr.Instance.CalAllScoreBySignal(SignalType.Matrix, LevelAsset.GameBoard, out harDriverCountInt);
                SignalMasterMgr.Instance.CalAllScoreBySignal(SignalType.Scan, LevelAsset.GameBoard, out networkCountInt);
                LevelAsset.TimeLine.RequirementSatisfied = (harDriverCountInt >= normalRval) && (networkCountInt >= networkRval);
            }

            var discount = 0;
            if (!LevelAsset.Shop.ShopOpening && IsShopRound)
            {
                discount = LevelAsset.SkillMgr.CheckDiscount();
            }
            
            LevelAsset.Shop.OpenShop(IsShopRound, discount);
            LevelAsset.SkillMgr.SkillEnabled = LevelAsset.SkillEnabled = IsSkillAllowed;
            LevelAsset.SignalPanel.TgtNormalSignal = normalRval;
            LevelAsset.SignalPanel.TgtNetworkSignal = networkRval;
            LevelAsset.SignalPanel.CrtNormalSignal = harDriverCountInt;
            LevelAsset.SignalPanel.CrtNetworkSignal = networkCountInt;
            LevelAsset.SignalPanel.NetworkTier = LevelAsset.GameBoard.GetTotalTierCountByCoreType(CoreType.NetworkCable);
            LevelAsset.SignalPanel.NormalTier = LevelAsset.GameBoard.GetTotalTierCountByCoreType(CoreType.HardDrive);
        }

        protected void CareerCycle()
        {
            if (RoundGist.HasValue)
            {
                UpdateRoundStatus_FSM();
                if (LevelAsset.GameOverEnabled)
                {
                    UpdateGameOverStatus(LevelAsset);
                }
            }

            if (((AutoDrive.HasValue && AutoDrive.Value || ShouldCycle && movedTile)) && (!_noRequirement))
            {
                if (LevelAsset.TimeLine.RequirementSatisfied)
                {
                    LevelAsset.ReqOkCount++;
                }
            }
        }

        protected void AnimateAction()
        {
            //目前这里基本空的，到时候可能把Animate的CoRoutine里面的东西弄出来。
            Debug.Assert(animate_Co != null);
            WorldExecutor.UpkeepLogic(LevelAsset, Stage, Animating);
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
        
        protected virtual void AdditionalInitLevel()
        {
            //BaseVerison,DoNothing.
        }

        public sealed override void InitLevel()
        {
            //TODO 这个也要将Career和Boss尽量拆干净。
            Debug.Assert(ReferenceOk); //意外的有确定Reference的……还行……
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));

            LevelAsset.DeltaCurrency = 0.0f;
            LevelAsset.GameStateMgr = new GameStateMgr();
            LevelAsset.GameStateMgr.InitGameMode(LevelAsset.ActionAsset.GameModeAsset);

            InitShop();
            InitDestoryer();
            WorldExecutor.InitCursor(ref LevelAsset,new Vector2Int(2, 3));
            LevelAsset.EnableAllCoreFunctionAndFeature();
            LevelAsset.GameBoard.InitBoardWAsset(LevelAsset.ActionAsset);
            LevelAsset.GameBoard.UpdateBoardAnimation();
            StartShop();
            AdditionalInitLevel();
            
            ReadyToGo = true;
            LevelAsset.HintMaster.ShouldShowCheckList = false;
        }
        protected sealed override void StartShop()
        {
            base.StartShop();
        }

        private bool UpdateGameOverStatus(GameAssets currentLevelAsset)
        {
            return LevelAsset.ActionAsset.HasEnded(LevelAsset.StepCount);
        }
        protected sealed override void UpdateLogicLevelReference()
        {
            base.UpdateLogicLevelReference();
        }
        
        private void Awake()
        {
            LevelAsset = new GameAssets();
            _mainFSM = new RootFSM {owner = this};

            UpdateLogicLevelReference();

            _mainFSM.ReplaceActions(fsmActions);
            _mainFSM.ReplaceTransition(RootFSMTransitions);
            _mainFSM.ReplaceBreaking(RootFSMBreakings);

            LevelAsset.AnimationPendingObj = new List<MoveableBase>();
            _actionDriver = new CareerControlActionDriver(this, _mainFSM);
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
                //RootDebug.Log("FSM:" + _mainFSM.currentStatus, NameID.YanYoumo_Log);
                RootDebug.Watch("FSM:" + _mainFSM.currentStatus, WatchID.YanYoumo_WatchA);
            } while (!_mainFSM.waitForNextFrame);
            _mainFSM.waitForNextFrame = false;//等待之后就把这个关了。
        }
    }
}