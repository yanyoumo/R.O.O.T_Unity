using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace ROOT
{
    public class CareerFSMLevelLogic : LevelLogic //LEVEL-LOGIC/每一关都有一个这个类。
    {
        [ReadOnly]bool shouldCycle = false;
        [ReadOnly] bool movedTile = false;
        [ReadOnly] bool movedCursor = false;

        #region BossStage

        private static float BossStagePauseCostTimer = 0.0f;
        private const float BossStagePauseCostInterval = 1.0f;
        private const int BossStagePricePerInterval = 1;
        private IEnumerator BossStagePauseCost()
        {
            yield return 0;
            while (true)
            {
                yield return 0;
                BossStagePauseCostTimer += Time.deltaTime;

                if (!(BossStagePauseCostTimer >= BossStagePauseCostInterval)) continue;
                BossStagePauseCostTimer = 0.0f;
                LevelAsset.ReqOkCount -= BossStagePricePerInterval;

                if (LevelAsset.ReqOkCount > 0) continue;
                WorldExecutor_Dispatcher.Root_Executor_void_PUBLIC(LogicCommand.BossUnpaused, ref LevelAsset);
                yield break;
            }
        }
        private bool _noRequirement;

        #endregion

        //ASSET
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

        protected override void UpdateLogicLevelReference()
        {
            LevelAsset.CursorTemplate = Resources.Load<GameObject>("Cursor/Prefab/Cursor");
            LevelAsset.GameBoard = FindObjectOfType<Board>();
            LevelAsset.AirDrop = LevelAsset.GameBoard.AirDrop;
            LevelAsset.AirDrop.GameAsset = LevelAsset;
            LevelAsset.Owner = this;
        }

        protected override void Awake()
        {
            Debug.Log("FSM Awake");
            LevelAsset = new GameAssets();
            UpdateLogicLevelReference();
            MainFSM = gameObject.AddComponent<CareerDefaultFSM>();
            MainFSM.owner = this;
        }

        /// <summary>
        /// 需要允许各个Level去自定义如何Link。
        /// </summary>
        /// <param name="aOP">上一个Loading核心逻辑场景的异步操作实例</param>
        /// <returns></returns>
        public override IEnumerator UpdateArtLevelReference(AsyncOperation aOP)
        {
            while (!aOP.isDone)
            {
                yield return 0;
            }

            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVEVISUAL));
            LevelAsset.ItemPriceRoot = GameObject.Find("PlayUI");
            LevelAsset.DataScreen = FindObjectOfType<DataScreen>();
            LevelAsset.HintMaster = FindObjectOfType<HintMaster>();
            LevelAsset.TimeLine = FindObjectOfType<TimeLine>();
            LevelAsset.CostLine = FindObjectOfType<CostLine>();
            LevelAsset.Shop = FindObjectOfType<ShopBase>();
            LevelAsset.SkillMgr = FindObjectOfType<SkillMgr>();
            LevelAsset.CostChart = FindObjectOfType<CostChart>();
            LevelAsset.SignalPanel = FindObjectOfType<SignalPanel>();
            LevelAsset.CineCam = FindObjectOfType<CinemachineFreeLook>();
            LevelAsset.HintMaster.HideTutorialFrame = false;
            PopulateArtLevelReference();
        }

        public override bool CheckReference()
        {
            bool res = true;
            res &= (LevelAsset.DataScreen != null);
            return res;
        }

        public override void PopulateArtLevelReference()
        {
            ReferenceOk = CheckReference();
        }

        protected override void StartShop()
        {
            LevelAsset.Shop.ShopStart();
        }

        protected override bool UpdateGameOverStatus(GameAssets currentLevelAsset)
        {
            //这个函数就很接近裁判要做的事儿了。
            if (!currentLevelAsset.GameStateMgr.EndGameCheck()) return false;
            PendingCleanUp = true;
            LevelMasterManager.Instance.LevelFinished(LevelAsset);
            return true;
        }

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
            yield break;
        }

        //现在一共提供Info的计数是：Boss阶段*BossInfoSprayCount*SprayCountPerAnimateInterval;
        private const int SprayCountPerAnimateInterval = 4;
        //private const int BossInfoSprayCount = 3;
        private const float BossInfoSprayTimerIntervalOffsetRange = 0.5f;

        private float _bossInfoSprayTimerIntervalBase => AnimationDuration / SprayCountPerAnimateInterval;//TODO 这个可能要做成和Anime时长相关的随机数。
        private float _bossInfoSprayTimerInterval => _bossInfoSprayTimerIntervalBase + _bossInfoSprayTimerIntervalOffset;//TODO 这个可能要做成和Anime时长相关的随机数。
        private float _bossInfoSprayTimerIntervalOffset = 0.0f;
        private float _bossInfoSprayTimer = 0.0f;
        private Coroutine ManualListenBossPauseKeyCoroutine;

        private int[] SprayCountArray;
        private int SprayCounter = 0;

        private void BossInit()
        {
            var bossStageCount = LevelAsset.ActionAsset.BossStageCount;
            var totalSprayCount = bossStageCount * SprayCountPerAnimateInterval;
            //这个数据还得传过去。
            var targetInfoCount = Mathf.RoundToInt(LevelAsset.ActionAsset.InfoCount * LevelAsset.ActionAsset.InfoTargetRatio);
            LevelAsset.SignalPanel.SignalTarget = targetInfoCount;

            SprayCountArray = Utils.SpreadOutLayingWRandomization(totalSprayCount, LevelAsset.ActionAsset.InfoCount,
                LevelAsset.ActionAsset.InfoVariantRatio);

            LevelAsset.DestroyerEnabled = true;
            LevelAsset.SignalPanel.IsBossStage = true;
            ManualListenBossPauseKeyCoroutine = StartCoroutine(ManualPollingBossPauseKey());
            WorldCycler.BossStage = true;
        }

        private void BossUpdate()
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

        int _obselateStepID = -1;
        bool lastDestoryBool = false;

        protected override void OnDestroy()
        {
            if (ManualListenBossPauseKeyCoroutine != null)
            {
                StopCoroutine(ManualListenBossPauseKeyCoroutine);
            }
        }
        public void PreInit()
        {

        }

        public void IdleAction()
        {
            _ctrlPack = WorldController.UpdateInputScheme(LevelAsset, out movedTile, out movedCursor, ref LevelAsset._boughtOnce);
            RootDebug.Watch(_ctrlPack.CtrlCMD.ToString(), WatchID.YanYoumo_ExampleB);
        }

        public void UpkeepAction()
        {
            var roundGist = LevelAsset.ActionAsset.GetRoundGistByStep(LevelAsset.StepCount);
            var stage = roundGist?.Type ?? StageType.Shop;
            WorldLogic.UpkeepLogic(LevelAsset, in stage, false);//RISK 这个也要弄。
        }

        public void MajorCycle()
        {
            var roundGist = LevelAsset.ActionAsset.GetRoundGistByStep(LevelAsset.StepCount);
            var stage = roundGist?.Type ?? StageType.Shop;
            animate_Co = null;
            LevelAsset.AnimationPendingObj = new List<MoveableBase>();
            LevelAsset.DeltaCurrency = 0.0f;
            var forwardCycle = false;
            
            //RISK 这个东西也先不要用；这边弄的差不多再弄。
            WorldExecutor_Dispatcher.Root_Executor_Compound_Ordered(
                new[] { LogicCommand.UpdateUnitCursor, LogicCommand.RotateUnit },
                ref LevelAsset, in _ctrlPack, out var res);
            var tRes = (bool[])res[LogicCommand.UpdateUnitCursor];
            movedTile = tRes[0];
            movedCursor = tRes[1];

            LevelAsset.GameBoard.UpdateBoardRotate(); //TODO 旋转现在还是闪现的。这个不用着急做。

            //RISK 这里让购买单元也变成强制移动一步。
            WorldExecutor_Dispatcher.Root_Executor(LogicCommand.UpdateShop, ref LevelAsset, in _ctrlPack, out var pRes);
            movedTile |= (bool)pRes;
            movedTile |= _ctrlPack.HasFlag(ControllingCommand.CycleNext); //这个flag的实际含义和名称有冲突。

            LevelAsset.SkillMgr.SkillEnabled = LevelAsset.SkillEnabled;
            LevelAsset.SkillMgr.TriggerSkill(LevelAsset, _ctrlPack);

            forwardCycle = movedTile;
            
            if (forwardCycle)
            {
                WorldLogic.UpdateCycle(LevelAsset, stage);
            }

            //RISK 
            //这个Anykeydown不是同一帧了；所以不能用了。
            shouldCycle = WorldLogic.ShouldCycle(in _ctrlPack, true, in movedTile, in movedCursor);

            if (roundGist.HasValue)
            {
                UpdateRoundStatus(LevelAsset, roundGist.Value);
            }

            LightUpBoard();

            if (LevelAsset.GameOverEnabled)
            {
                UpdateGameOverStatus(LevelAsset);
            }

            if (((shouldCycle && movedTile)) && (!_noRequirement))
            {
                if (LevelAsset.TimeLine.RequirementSatisfied)
                {
                    LevelAsset.ReqOkCount++;
                }
            }

            Animating = shouldCycle;

            if (Animating)
            {
                AnimationTimerOrigin = Time.timeSinceLevelLoad;
                LevelAsset.MovedTileAni = movedTile;
                LevelAsset.MovedCursorAni = movedCursor;
                animate_Co = StartCoroutine(Animate()); //这里完成后会把Animating设回来。
            }
        }

        public void AnimateAction()
        {
            //目前这里基本空的，到时候可能把Animate的CoRoutine里面的东西弄出来。
            var roundGist = LevelAsset.ActionAsset.GetRoundGistByStep(LevelAsset.StepCount);
            var stage = roundGist?.Type ?? StageType.Shop;
            Debug.Assert(animate_Co != null);
            WorldLogic.UpkeepLogic(LevelAsset, in stage, Animating);
        }

        public void CleanUp()
        {
            shouldCycle = false;
            movedTile = false;
            movedCursor = false;
        }

        public void Execute()
        {
            switch (MainFSM.currentStatus)
            {
                case RootFSMStatus.PreInit:
                    PreInit();
                    break;
                case RootFSMStatus.Idle:
                    IdleAction();
                    break;
                case RootFSMStatus.Upkeep:
                    UpkeepAction();
                    break;
                case RootFSMStatus.Cycle:
                    MajorCycle();
                    break;
                case RootFSMStatus.CleanUp:
                    CleanUp();
                    break;
                case RootFSMStatus.BossInit:
                    BossInit();
                    break;
                case RootFSMStatus.Boss:
                    BossUpdate();
                    break;
                case RootFSMStatus.Animate:
                    AnimateAction();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private RootFSMBase MainFSM;

        protected void InitDestoryer()
        {
            LevelAsset.WarningDestoryer = new MeteoriteBomber { GameBoard = LevelAsset.GameBoard };
            LevelAsset.WarningDestoryer.Init(4, 1);
        }
        protected void InitShop()
        {
            LevelAsset.Shop.ShopInit(LevelAsset);
            LevelAsset.Shop.CurrentGameStateMgr = LevelAsset.GameStateMgr;
            LevelAsset.Shop.GameBoard = LevelAsset.GameBoard;
            if (LevelAsset.ActionAsset.ExcludedShop)
            {
                LevelAsset.Shop.excludedTypes = LevelAsset.ActionAsset.ShopExcludedType;
            }
        }
        protected void InitCurrencyIoMgr()
        {
            /*LevelAsset.BoardDataCollector = gameObject.AddComponent<BoardDataCollector>();
            LevelAsset.BoardDataCollector.m_Board = LevelAsset.GameBoard;*/
        }
        protected void InitCursor(Vector2Int pos)
        {
            LevelAsset.GameCursor = Instantiate(LevelAsset.CursorTemplate);
            Cursor cursor = LevelAsset.GameCursor.GetComponent<Cursor>();
            cursor.InitPosWithAnimation(pos);
            cursor.UpdateTransform(LevelAsset.GameBoard.GetFloatTransformAnimation(cursor.LerpingBoardPosition));
        }

        public override void InitLevel()
        {
            Debug.Assert(ReferenceOk);//意外的有确定Reference的……还行……
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
            //LevelAsset.TimeLine.SetGoalCount = LevelAsset.ActionAsset.TargetCount;
            //LevelAsset.SignalPanel.TgTtMission= LevelAsset.ActionAsset.TargetCount;
            LevelAsset.HintMaster.ShouldShowCheckList = false;
        }

        private void LightUpBoard()
        {
            if (_ctrlPack.HasFlag(ControllingCommand.FloatingOnGrid) || _ctrlPack.HasFlag(ControllingCommand.ClickOnGrid))
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

        //现在操纵有微妙的延迟，是因为IO的控制状态（Idle）到实际的动画（开启）（Cycle）之间还隔了一帧。
        //大体上还是要把IO变成事件、可以将FSM跳到某个状态上；要不然还得弄。
        protected override void Update()
        {
            //这里有个很好的地方，状态转移到是和Update完全解耦了。
            //这里可以让各个状态的事件间隔减少、但是绝对不是靠谱的解法；还是要用事件。
            var transitPerFrame = 3;
            for (var i = 0; i < transitPerFrame; i++)
            {
                Execute();
                MainFSM.Transit();
            }
        }
    }
}