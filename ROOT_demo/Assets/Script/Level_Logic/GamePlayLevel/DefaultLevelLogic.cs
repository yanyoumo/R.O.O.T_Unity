using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

// https://shimo.im/docs/Dd86KXTqHJpqxwYX
// ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable
namespace ROOT
{
    public abstract class LevelLogic : MonoBehaviour //LEVEL-LOGIC/每一关都有一个这个类。
    {
        #region BossStage

        protected IEnumerator ManualPollingBossPauseKey()
        {
            yield return 0;
            while (true)
            {
                yield return 0;
                if (Input.GetButtonDown(StaticName.INPUT_BUTTON_BOSS_PAUSE))
                {
                    //这个按钮需要在Animation状态插入，因为逻辑帧比率降低了，就是需要这里放一个线程出来。
                    WorldExecutor_Dispatcher.Root_Executor_void_PUBLIC(LogicCommand.BossTryUnpause, ref LevelAsset);
                }
            }
        }

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

        public Coroutine BossStagePauseCostCo { private set; get; }
        public void StartBossStageCost()
        {
            BossStagePauseCostCo=StartCoroutine(BossStagePauseCost());
        }
        public void StopBossStageCost()
        {
            StopCoroutine(BossStagePauseCostCo);
            BossStagePauseCostCo = null;
        }
        private bool _noRequirement;

        #endregion

        [ReadOnly] public bool IsTutorialLevel = false;

        //ASSET
        protected internal GameAssets LevelAsset;
        protected Cursor cursor => LevelAsset.Cursor;
        protected ControllingPack _ctrlPack;
        public ControllingPack CtrlPack => _ctrlPack;

        //Lvl-Logic实际用的判断逻辑。
        [ReadOnly] public bool Playing { get; set; }
        [ReadOnly] public bool Animating = false;
        [ReadOnly] public bool ReadyToGo = false;
        [ReadOnly] public bool ReferenceOk = false;
        [ReadOnly] public bool PendingCleanUp;

        protected float AnimationTimerOrigin = 0.0f; //都是秒
        public static float AnimationDuration => WorldCycler.AnimationTimeLongSwitch ? BossAnimationDuration : DefaultAnimationDuration;
        public static readonly float DefaultAnimationDuration = 0.15f; //都是秒
        public static readonly float BossAnimationDuration = 1.5f; //都是秒
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

        [ReadOnly] public readonly int LEVEL_LOGIC_SCENE_ID = StaticName.SCENE_ID_ADDTIVELOGIC; //这个游戏的这两个参数是写死的
        [ReadOnly] public readonly int LEVEL_ART_SCENE_ID = StaticName.SCENE_ID_ADDTIVEVISUAL; //但是别的游戏的这个值多少是需要重写的。
        
        protected virtual void UpdateLogicLevelReference()
        {
            LevelAsset.CursorTemplate = Resources.Load<GameObject>("Cursor/Prefab/Cursor");
            LevelAsset.GameBoard = FindObjectOfType<Board>();
            LevelAsset.AirDrop = LevelAsset.GameBoard.AirDrop;
            LevelAsset.AirDrop.GameAsset = LevelAsset;
            LevelAsset.Owner = this;
        }

        protected virtual void Awake()
        {
            LevelAsset = new GameAssets();
            UpdateLogicLevelReference();
        }

        /// <summary>
        /// 需要允许各个Level去自定义如何Link。
        /// </summary>
        /// <param name="aOP">上一个Loading核心逻辑场景的异步操作实例</param>
        /// <returns></returns>
        public virtual IEnumerator UpdateArtLevelReference(AsyncOperation aOP)
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

        public abstract void InitLevel();

        public virtual bool CheckReference()
        {
            bool res = true;
            res &= (LevelAsset.DataScreen != null);
            return res;
        }

        public virtual void PopulateArtLevelReference()
        {
            ReferenceOk = CheckReference();
        }

        protected virtual void StartShop()
        {
            LevelAsset.Shop.ShopStart();
        }

        protected virtual bool UpdateGameOverStatus(GameAssets currentLevelAsset)
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
        private float _bossInfoSprayTimerInterval => _bossInfoSprayTimerIntervalBase+ _bossInfoSprayTimerIntervalOffset;//TODO 这个可能要做成和Anime时长相关的随机数。
        private float _bossInfoSprayTimerIntervalOffset = 0.0f;
        private float _bossInfoSprayTimer=0.0f;
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
                    catch(IndexOutOfRangeException)
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


        //原则上这个不让被重载。
        //TODO Digong需要了解一些主干的Update流程。
        //未来需要将动画部分移动至随机位置。
        protected virtual void Update()
        {
            if ((!ReadyToGo) || (PendingCleanUp))
            {
                Playing = false;
                return;
            }

            if (!Playing) Playing = true;
            _ctrlPack = new ControllingPack {CtrlCMD = ControllingCommand.Nop};

            bool? AutoDrive = null;
            bool shouldCycle = false, movedTile = false;
            var roundGist = LevelAsset.ActionAsset.GetRoundGistByStep(LevelAsset.StepCount);
            var stage = roundGist?.Type ?? StageType.Shop;

            if (stage == StageType.Boss)
            {
                //TODO 之后Boss部分就在这儿搞。
                if (!WorldCycler.BossStage)
                {
                    BossInit();
                }
                BossUpdate();
            }
            
            if (!Animating)//很可能动画要改成准轮询的，换句话说程序要有能打断动画的“能力”。
            {
                //Solid_Logic
                animate_Co = null;
                LevelAsset.AnimationPendingObj = new List<MoveableBase>();

                // ShouldCycle这个放到WorldLogic里面去了。
                WorldLogic.UpdateLogic(LevelAsset, in stage, out _ctrlPack, out movedTile, out var movedCursor,out shouldCycle,out AutoDrive);

                if (roundGist.HasValue)
                {
                    UpdateRoundStatus(LevelAsset, roundGist.Value);
                }

                if (_ctrlPack.HasFlag(ControllingCommand.FloatingOnGrid)|| _ctrlPack.HasFlag(ControllingCommand.ClickOnGrid))
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

                if (LevelAsset.GameOverEnabled)
                {
                    UpdateGameOverStatus(LevelAsset);
                }

                Animating = shouldCycle;

                if ((AutoDrive.HasValue && AutoDrive.Value || shouldCycle && movedTile) && (!_noRequirement))
                {
                    if (LevelAsset.TimeLine.RequirementSatisfied)
                    {
                        LevelAsset.ReqOkCount++;
                    }
                }

                if (Animating)
                { 
                    AnimationTimerOrigin = Time.timeSinceLevelLoad;
                    LevelAsset.MovedTileAni = movedTile;
                    LevelAsset.MovedCursorAni = movedCursor;
                    animate_Co=StartCoroutine(Animate()); //这里完成后会把Animating设回来。
                }
            }
            else
            {
                //Animation_Logic_Upkeep
                Debug.Assert(animate_Co != null);
                WorldLogic.UpkeepLogic(LevelAsset, in stage, Animating);
            }

            if (LevelAsset.HintEnabled)
            {
                LevelAsset.HintMaster.UpdateHintMaster(_ctrlPack);
            }

            if ((shouldCycle) && (movedTile || AutoDrive.HasValue))
            {
                if (roundGist.HasValue && roundGist.Value.Type == StageType.Require)
                {
                    LevelAsset.GameBoard.UpdatePatternDiminishing();
                }
            }

            LevelAsset.LevelProgress = LevelAsset.StepCount / (float)LevelAsset.ActionAsset.PlayableCount;

            if (_ctrlPack.HasFlag(ControllingCommand.CameraMov))
            {
                var cam = LevelAsset.CineCam;
                cam.m_XAxis.Value += _ctrlPack.CameraMovement.x;
                cam.m_YAxis.Value += _ctrlPack.CameraMovement.y*0.05f;
            }
        }

        protected bool UpdateCareerGameOverStatus(GameAssets currentLevelAsset)
        {
            if (LevelAsset.ActionAsset.HasEnded(LevelAsset.StepCount))
            {
                if (!IsTutorialLevel)
                {
                    PendingCleanUp = true;
                    LevelMasterManager.Instance.LevelFinished(LevelAsset);
                }
                return true;
            }
            return false;
        }

        int _obselateStepID = -1;
        bool lastDestoryBool = false;

        protected void UpdateRoundStatus(GameAssets currentLevelAsset,RoundGist roundGist)
        {
            //这个函数就很接近裁判要做的事儿了。
            int normalRval = 0, networkRval = 0;
            bool shouldOpenShop, shouldCurrencyIo, shouldCurrencyIncome, 
                shouldDestoryer, SkillAllowed, bossStage;
            var tCount = LevelAsset.ActionAsset.GetTruncatedCount(LevelAsset.StepCount, out var count);

            bossStage = roundGist.Type == StageType.Boss;
            shouldOpenShop = roundGist.Type == StageType.Shop;
            SkillAllowed = roundGist.Type != StageType.Shop;
            shouldCurrencyIncome = roundGist.Type == StageType.Require;
            shouldCurrencyIo = (roundGist.Type == StageType.Require || roundGist.Type == StageType.Destoryer);
            shouldDestoryer = (roundGist.Type == StageType.Destoryer);

            if (roundGist.Type == StageType.Require || roundGist.Type == StageType.Shop)
            {
                normalRval += roundGist.normalReq;
                networkRval += roundGist.networkReq;
            }

            if (roundGist.SwitchHeatsink(tCount))
            {
                if (_obselateStepID == -1 || _obselateStepID != LevelAsset.StepCount)
                {
                    LevelAsset.GameBoard.UpdatePatternID();
                }

                _obselateStepID = LevelAsset.StepCount;
            }

            if ((LevelAsset.DestroyerEnabled && !shouldDestoryer) && !WorldCycler.BossStage)
            {
                LevelAsset.WarningDestoryer.ForceReset();
            }

            if ((lastDestoryBool && !shouldDestoryer) && !WorldCycler.NeedAutoDriveStep.HasValue)
            {
                //这个的触发实际和商店的切换HeatSink冲突了。 Resolved
                LevelAsset.GameBoard.DestoryHeatsinkOverlappedUnit();
            }

            lastDestoryBool = shouldDestoryer;

            //RISK 这里把Destroyer目前完全关了。现在Boss阶段也要用。
            //LevelAsset.DestroyerEnabled = ShouldDestoryer;
            LevelAsset.DestroyerEnabled = WorldCycler.BossStage;
            LevelAsset.CurrencyIncomeEnabled = shouldCurrencyIncome;
            LevelAsset.CurrencyIOEnabled = shouldCurrencyIo;

            int harDriverCountInt = 0, networkCountInt = 0;
            _noRequirement = (normalRval == 0 && networkRval == 0);

            if (_noRequirement)
            {
                currentLevelAsset.TimeLine.RequirementSatisfied = true;
            }
            else
            {
                /*currentLevelAsset.BoardDataCollector.CalculateProcessorScore(out harDriverCountInt);
                currentLevelAsset.BoardDataCollector.CalculateServerScore(out networkCountInt);*/
                //下面两个函数应该是等效的，只不过目前还没有实际逻辑。
                SignalMasterMgr.Instance.CalAllScoreBySignal(SignalType.Matrix, currentLevelAsset.GameBoard, out harDriverCountInt);
                SignalMasterMgr.Instance.CalAllScoreBySignal(SignalType.Scan, currentLevelAsset.GameBoard, out networkCountInt);
                currentLevelAsset.TimeLine.RequirementSatisfied = (harDriverCountInt >= normalRval) && (networkCountInt >= networkRval);
            }

            if (LevelAsset.Shop is IRequirableShop shop)
            {
                if (shouldOpenShop)
                {
                    if (!LevelAsset.Shop.ShopOpening)
                    {
                        int normalDataSurplus = normalRval - harDriverCountInt,
                            networkDataSurplus = networkRval - networkCountInt;
                        if (normalDataSurplus > 0 || networkDataSurplus > 0)
                        {
                            shop.SetRequire(roundGist.shopLength, normalDataSurplus, networkDataSurplus);
                        }
                    }
                }
                else
                {
                    shop.ResetRequire();
                }
            }

            //CheckDiscount这个函数只能每次调一次，还是需要guard一下。
            var discount = 0;
            if (!LevelAsset.Shop.ShopOpening && shouldOpenShop)
            {
                discount = LevelAsset.SkillMgr.CheckDiscount();
                RootDebug.Log(discount.ToString(), NameID.YanYoumo_Log);
            }
            LevelAsset.Shop.OpenShop(shouldOpenShop, discount);
            LevelAsset.SkillEnabled = SkillAllowed;

            LevelAsset.SignalPanel.TgtNormalSignal = normalRval;
            LevelAsset.SignalPanel.TgtNetworkSignal = networkRval;
            LevelAsset.SignalPanel.CrtNormalSignal = harDriverCountInt;
            LevelAsset.SignalPanel.CrtNetworkSignal = networkCountInt;
            LevelAsset.SignalPanel.NetworkTier = LevelAsset.GameBoard.GetTotalTierCountByCoreType(CoreType.NetworkCable);
            LevelAsset.SignalPanel.NormalTier = LevelAsset.GameBoard.GetTotalTierCountByCoreType(CoreType.HardDrive);
        }

        protected virtual void OnDestroy()
        {
            if (ManualListenBossPauseKeyCoroutine != null)
            {
                StopCoroutine(ManualListenBossPauseKeyCoroutine);
            }
        }
    }

    public class DefaultLevelLogic : LevelLogic //LEVEL-LOGIC/每一关都有一个这个类。
    {
        public override void InitLevel()
        {
            Debug.Assert(ReferenceOk); //意外的有确定Reference的……还行……
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));

            InitCurrencyIoMgr();
            LevelAsset.DeltaCurrency = 0.0f;

            LevelAsset.GameStateMgr = new GameStateMgr();
            LevelAsset.GameStateMgr.InitGameMode(LevelAsset.ActionAsset.GameModeAsset);
            LevelAsset.CostChart.CurrencyVal = Mathf.RoundToInt(LevelAsset.GameStateMgr.GetCurrency());

            InitShop();
            StartShop();
            InitDestoryer();
            InitCursor(new Vector2Int(2, 3));
            LevelAsset.EnableAllCoreFunctionAndFeature();
            LevelAsset.GameBoard.UpdateBoardAnimation();
            LevelAsset.ActionAsset = null;

            ReadyToGo = true;
        }

        protected void InitDestoryer()
        {
            LevelAsset.WarningDestoryer = new MeteoriteBomber { GameBoard = LevelAsset.GameBoard };
            LevelAsset.WarningDestoryer.Init(5, 2);
        }
        protected void InitShop()
        {
            LevelAsset.Shop.ShopInit(LevelAsset);
            LevelAsset.Shop.CurrentGameStateMgr = LevelAsset.GameStateMgr;
            LevelAsset.Shop.GameBoard = LevelAsset.GameBoard;
        }
        protected void InitCurrencyIoMgr()
        {
            //LevelAsset.BoardDataCollector = gameObject.AddComponent<BoardDataCollector>();
            //LevelAsset.BoardDataCollector.m_Board = LevelAsset.GameBoard;
        }
        protected void InitCursor(Vector2Int pos)
        {
            LevelAsset.GameCursor = Instantiate(LevelAsset.CursorTemplate);
            Cursor cursor = LevelAsset.GameCursor.GetComponent<Cursor>();
            cursor.InitPosWithAnimation(pos);
            cursor.UpdateTransform(LevelAsset.GameBoard.GetFloatTransformAnimation(cursor.LerpingBoardPosition));
        }
    }
}