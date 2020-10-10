using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
// https://shimo.im/docs/Dd86KXTqHJpqxwYX
// ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable
namespace ROOT
{
    public enum GameStatus
    {
        Starting,
        Playing,
        Tutorial,
        Ended
    }

    public class GameGlobalStatus
    {
        public GameStatus CurrentGameStatus;
    }

    /// <summary>
    /// 一个每个关卡都有这么一个类，在Lvl-WRD之间传来传去。这个类只有一个，做成最通用的样子。
    /// </summary>
    public sealed class GameAssets//ASSET 这里不应该有任何之际逻辑（有些便于操作的除外
    {
        public int StepCount = 0;
        public float LevelProgress = 0.0f;
        public bool BuyingCursor = false;
        public int BuyingID = -1;
        /// <summary>
        /// 裁判同时要担任神使，神要通过这里影响世界。
        /// </summary>
        public LevelLogic Owner;
        public bool? TutorialCompleted = null;
        public LevelActionAsset ActionAsset;
        //这些引用在Asset外面要设好，在WRD-LOGIC里面也要处理。
        public GameObject CursorTemplate;
        public GameObject ItemPriceRoot;
        public Board GameBoard;
        public DataScreen DataScreen;
        public HintMaster HintMaster;
        public TimeLine TimeLine;
        public CostLine CostLine;
        public CostChart CostChart;
        public CoreType? DestoryedCoreType;
        public SignalPanel SignalPanel;

        internal GameObject GameCursor;
        internal Cursor Cursor => GameCursor.GetComponent<Cursor>();

        internal BoardDataCollector BoardDataCollector;
        internal GameStateMgr GameStateMgr;
        internal ShopBase Shop;
        internal IWarningDestoryer WarningDestoryer;
        internal GameObject[] WarningGo;

        public float DeltaCurrency { get; internal set; }

        //CoreFunctionFlag
        public bool InputEnabled = true;
        public bool CurrencyEnabled = true;
        public bool CurrencyIOEnabled = true;
        public bool CurrencyIncomeEnabled = true;
        public bool CycleEnabled = true;
        //FeatureFunctionFlag
        public bool CursorEnabled = true;
        public bool RotateEnabled = true;
        public bool ShopEnabled = true;
        public bool DestroyerEnabled = false;
        //LevelLogicFlag
        public bool GameOverEnabled = true;
        //UtilsFlag
        public bool LCDCurrencyEnabled = true;
        public bool LCDDeltaCurrencyEnabled = true;
        public bool LCDTimeEnabled = true;
        public bool HintEnabled = true;
        public bool ForceHddConnectionHint = false;
        public bool ForceServerConnectionHint = false;
        //internal flag 
        internal bool _boughtOnce = false;
        public bool BoughtOnce
        {
            get => _boughtOnce;
            internal set => _boughtOnce = value;
        }
        internal bool MovedTileAni = false;
        internal bool MovedCursorAni = false;
        internal List<MoveableBase> AnimationPendingObj;

        //一些辅助函数可以在这里。

        internal void EnableAllCoreFunction()
        {
            InputEnabled = true;
            CurrencyEnabled = true;
            CycleEnabled = true;
        }
        internal void DisableAllCoreFunction()
        {
            InputEnabled = false;
            CurrencyEnabled = false;
            CycleEnabled = false;
        }

        internal void EnableAllCoreFunctionAndFeature()
        {
            InputEnabled = true;
            CursorEnabled = true;
            CurrencyIOEnabled = true;
            CurrencyIncomeEnabled = true;
            RotateEnabled = true;
            ShopEnabled = true;
            LCDCurrencyEnabled = true;
            LCDDeltaCurrencyEnabled = true;
            LCDTimeEnabled = true;
            CurrencyEnabled = true;
            DestroyerEnabled = true;
            HintEnabled = true;
            CycleEnabled = true;
            GameOverEnabled = true;
        }
        internal void DisableAllCoreFunctionAndFeature()
        {
            InputEnabled = false;
            CursorEnabled = false;
            CurrencyIOEnabled = false;
            CurrencyIncomeEnabled = false;
            RotateEnabled = false;
            ShopEnabled = false;
            LCDCurrencyEnabled = false;
            LCDDeltaCurrencyEnabled = false;
            LCDTimeEnabled = false;
            CurrencyEnabled = false;
            DestroyerEnabled = false;
            HintEnabled = false;
            CycleEnabled = false;
            GameOverEnabled = false;
        }
    }

    public abstract class LevelLogic : MonoBehaviour//LEVEL-LOGIC/每一关都有一个这个类。
    {
        private bool _noRequirement;
        protected int RequirementSatisfiedCycleCount = 0;
        public bool IsTutorialLevel=false;
        //ASSET
        protected internal GameAssets LevelAsset;
        private ControllingPack _ctrlPack;
        protected ControllingPack CtrlPack => _ctrlPack;

        //Lvl-Logic实际用的判断逻辑。
        public bool Playing { get; private set;  }
        protected bool Animating = false;
        protected bool ReadyToGo = false;
        protected bool ReferenceOk = false;
        protected bool PendingCleanUp;

        protected float AnimationTimerOrigin = 0.0f;//都是秒
        public static readonly float AnimationDuration = 0.15f;//都是秒

        public readonly int LEVEL_LOGIC_SCENE_ID = StaticName.SCENE_ID_ADDTIVELOGIC;//这个游戏的这两个参数是写死的
        public readonly int LEVEL_ART_SCENE_ID = StaticName.SCENE_ID_ADDTIVEVISUAL;//但是别的游戏的这个值多少是需要重写的。

        //public abstract LevelType GetLevelType { get; }

        private Cursor cursor => LevelAsset.Cursor;
        private float animationTimer => Time.timeSinceLevelLoad - AnimationTimerOrigin;
        //private float animationLerper => animationTimer / AnimationDuration;
        private float AnimationLerper
        {
            get
            {
                float res=animationTimer / AnimationDuration;
                return Mathf.Min(res, 1.0f);
            }
        }

        protected virtual void UpdateLogicLevelReference()
        {
            LevelAsset.CursorTemplate = Resources.Load<GameObject>("Cursor/Prefab/Cursor");
            LevelAsset.GameBoard = FindObjectOfType<Board>();
            LevelAsset.Owner = this;
        }

        protected virtual void Awake()
        {
            LevelAsset = new GameAssets();
            UpdateLogicLevelReference();
        }

        //这两个函数是WorldLogic要通过LvlLogic去影响世界/因为Unity的规定。
        //这样不得不得出的结论就是裁判要兼任神使这一工作（是个隐坑
        internal T WorldLogicRequestInstantiate<T>(T obj) where T: Object
        {
            return Instantiate(obj);
        }

        internal void WorldLogicRequestDestroy(Object obj)
        {
            Destroy(obj);
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
            LevelAsset.CostChart = FindObjectOfType<CostChart>();
            LevelAsset.SignalPanel = FindObjectOfType<SignalPanel>();
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
            if (currentLevelAsset.GameStateMgr.EndGameCheck())
            {
                PendingCleanUp = true;
                LevelMasterManager.Instance.LevelFinished(LevelAsset);
                return true;
            }
            else
            {
                return false;
            }
        }


        private bool ShouldCycle(in ControllingPack ctrlPack, in bool pressedAny, in bool movedTile, in bool movedCursor)
        {
            bool shouldCycle = false;
            if (StartGameMgr.UseTouchScreen)
            {
                shouldCycle = movedTile|ctrlPack.HasFlag(ControllingCommand.CycleNext);
            }
            else
            {
                shouldCycle = (pressedAny & (movedTile | movedCursor)) | ctrlPack.HasFlag(ControllingCommand.CycleNext);
            }

            return shouldCycle;
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

            if (!Playing)
            {
                Playing = true;
            }

            System.Diagnostics.Debug.Assert(LevelAsset.GameBoard != null, nameof(LevelAsset.GameBoard) + " != null");
            var pressedAny = Input.anyKeyDown;
            _ctrlPack = new ControllingPack {CtrlCMD = ControllingCommand.Nop};
            bool shouldCycle = false;
            bool movedTile = false;
            if (!Animating)
            {
                LevelAsset.AnimationPendingObj = new List<MoveableBase>();

                WorldLogic.UpdateLogic(LevelAsset, out _ctrlPack, out movedTile, out var movedCursor);

                if (LevelAsset.GameOverEnabled)
                {
                    UpdateGameOverStatus(LevelAsset);
                }

                shouldCycle = ShouldCycle(in _ctrlPack, in pressedAny, in movedTile, in movedCursor);
                Animating = shouldCycle;
                if (shouldCycle && movedTile && (!_noRequirement))
                {
                    if (LevelAsset.TimeLine.RequirementSatisfied)
                    {
                        RequirementSatisfiedCycleCount++;
                    }
                }

                if (Animating)
                {
                    AnimationTimerOrigin = Time.timeSinceLevelLoad;
                    LevelAsset.MovedTileAni = movedTile;
                    LevelAsset.MovedCursorAni = movedCursor;
                    StartCoroutine(Animate()); //这里完成后会把Animating设回来。
                }
            }

            if (LevelAsset.HintEnabled)
            {
                LevelAsset.HintMaster.UpdateHintMaster(_ctrlPack);
            }

            if (shouldCycle && movedTile)
            {
                var roundGist = LevelAsset.ActionAsset.GetRoundGistByStep(LevelAsset.StepCount);

                if (roundGist.HasValue && roundGist.Value.Type == StageType.Require)
                {
                    LevelAsset.GameBoard.UpdatePatternDiminishing();
                }
            }
        }

        int ObselateStepID = -1;
        private bool lastDestoryBool = false;
        protected bool UpdateCareerGameOverStatus(GameAssets currentLevelAsset)
        {
            //这个函数就很接近裁判要做的事儿了。
            var NormalRval = 0;
            var NetworkRval = 0;
            var ShoudOpenShop = false;
            var ShoudCurrencyIO = false;
            var ShoudCurrencyIncome = false;

            var roundGist = LevelAsset.ActionAsset.GetRoundGistByStep(LevelAsset.StepCount);
            if (!roundGist.HasValue) return false;
            var round = roundGist.Value;
            
            if (LevelAsset.ActionAsset.HasEnded(LevelAsset.StepCount))
            {
                if (!IsTutorialLevel)
                {
                    PendingCleanUp = true;
                    LevelMasterManager.Instance.LevelFinished(LevelAsset);
                }
                return true;
            }

            ShoudOpenShop = round.Type == StageType.Shop;
            ShoudCurrencyIncome = round.Type == StageType.Require;
            ShoudCurrencyIO = (round.Type == StageType.Require || round.Type == StageType.Destoryer);

            if (round.Type == StageType.Require|| round.Type == StageType.Shop)
            {
                NormalRval += round.normalReq;
                NetworkRval += round.networkReq;
            }

            var tCount=LevelAsset.ActionAsset.GetTruncatedCount(LevelAsset.StepCount, out var count);
            if (round.SwitchHeatsink(tCount))
            {
                if (ObselateStepID==-1||ObselateStepID != LevelAsset.StepCount)
                {
                    LevelAsset.GameBoard.UpdatePatternID();
                }

                ObselateStepID = LevelAsset.StepCount;
            }

            //TODO 这里还要识别ShouldDestoryer准备关闭前的机制。
            var ShouldDestoryer = (round.Type == StageType.Destoryer);

            if (LevelAsset.DestroyerEnabled && !ShouldDestoryer)
            {
                LevelAsset.WarningDestoryer.ForceReset();
            }

            if (lastDestoryBool&&!ShouldDestoryer)
            {
                LevelAsset.GameBoard.DestoryHeatsinkOverlappedUnit();
            }
            lastDestoryBool = ShouldDestoryer;

            //RISK 这里把Destroyer目前完全关了。
            //LevelAsset.DestroyerEnabled = ShouldDestoryer;
            LevelAsset.DestroyerEnabled = false;

            LevelAsset.CurrencyIncomeEnabled = ShoudCurrencyIncome;
            LevelAsset.CurrencyIOEnabled = ShoudCurrencyIO;

            int harDriverCountInt = 0;
            int NetworkCountInt = 0;

            if (NormalRval == 0 && NetworkRval == 0)
            {
                _noRequirement = true;
                currentLevelAsset.TimeLine.RequirementSatisfied = true;
            }
            else
            {
                _noRequirement = false;
                currentLevelAsset.BoardDataCollector.CalculateProcessorScore(out harDriverCountInt);
                var valA = (harDriverCountInt >= NormalRval);
                currentLevelAsset.BoardDataCollector.CalculateServerScore(out NetworkCountInt);
                var valB = (NetworkCountInt >= NetworkRval);
                currentLevelAsset.TimeLine.RequirementSatisfied = valA && valB;
            }

            if (LevelAsset.Shop is IRequirableShop shop)
            {
                if (ShoudOpenShop)
                {
                    if (!LevelAsset.Shop.ShopOpening)
                    {
                        var normalDataSurplus = NormalRval - harDriverCountInt;
                        var networkDataSurplus = NetworkRval - NetworkCountInt;
                        if (normalDataSurplus > 0 || networkDataSurplus > 0)
                        {
                            shop.SetRequire(round.shopLength, normalDataSurplus, networkDataSurplus);
                        }
                    }
                }
                else
                {
                    shop.ResetRequire();
                }
            }

            LevelAsset.Shop.ShopOpening = ShoudOpenShop;

            LevelAsset.SignalPanel.TGTNormalSignal=NormalRval;
            LevelAsset.SignalPanel.TGTNetworkSignal = NetworkRval;
            LevelAsset.SignalPanel.CRTNormalSignal= harDriverCountInt;
            LevelAsset.SignalPanel.CRTNetworkSignal = NetworkCountInt;

            return false;
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
            LevelAsset.BoardDataCollector = gameObject.AddComponent<BoardDataCollector>();
            LevelAsset.BoardDataCollector.m_Board = LevelAsset.GameBoard;
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