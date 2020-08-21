using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;
//TODO https://shimo.im/docs/Dd86KXTqHJpqxwYX
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
        public CoreType? DestoryedCoreType;

        internal GameObject GameCursor;
        internal Cursor Cursor => GameCursor.GetComponent<Cursor>();

        internal BoardDataCollector BoardDataCollector;
        internal GameStateMgr GameStateMgr;
        internal ShopMgr ShopMgr;
        internal IWarningDestoryer WarningDestoryer;
        internal GameObject[] WarningGo;

        internal TextMeshPro Item1PriceTmp;
        internal TextMeshPro Item2PriceTmp;
        internal TextMeshPro Item3PriceTmp;
        internal TextMeshPro Item4PriceTmp;

        public float DeltaCurrency { get; internal set; }

        //CoreFunctionFlag
        public bool InputEnabled = true;
        public bool CurrencyEnabled = true;
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
        public static readonly float AnimationDuration = 0.1f;//都是秒

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
            LevelAsset.HintMaster.HideTutorialFrame = false;
            PopulateArtLevelReference();
        }
        public abstract void InitLevel();

        public virtual bool CheckReference()
        {
            bool res = true;
            res &= (LevelAsset.DataScreen != null);
            res &= (LevelAsset.Item1PriceTmp != null);
            res &= (LevelAsset.Item2PriceTmp != null);
            res &= (LevelAsset.Item3PriceTmp != null);
            res &= (LevelAsset.Item4PriceTmp != null);
            return res;
        }
        public virtual void PopulateArtLevelReference()
        {
            var tempT = LevelAsset.ItemPriceRoot.GetComponentsInChildren<TextMeshPro>();
            foreach (var text in tempT)
            {
                if (text.gameObject.name == "UnitAPrice_1")
                {
                    LevelAsset.Item1PriceTmp = text;
                }
                if (text.gameObject.name == "UnitAPrice_2")
                {
                    LevelAsset.Item2PriceTmp = text;
                }
                if (text.gameObject.name == "UnitAPrice_3")
                {
                    LevelAsset.Item3PriceTmp = text;
                }
                if (text.gameObject.name == "UnitAPrice_4")
                {
                    LevelAsset.Item4PriceTmp = text;
                }
            }
            ReferenceOk = CheckReference();
        }
        protected virtual void StartShop()
        {
            LevelAsset.ShopMgr.ShopStart();
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
        protected virtual void UpdateSignalHint(in ControllingPack ctrlPack)
        {
            LevelAsset.GameBoard.ResetUnitEmission();
            var pressed = ctrlPack.HasFlag(ControllingCommand.SignalHint);
            if (pressed || LevelAsset.ForceHddConnectionHint)
            {
                LevelAsset.GameBoard.DisplayConnectedHDDUnit();
            }

            if (pressed || LevelAsset.ForceServerConnectionHint)
            {
                LevelAsset.GameBoard.DisplayConnectedServerUnit();
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
                    if (LevelAsset.ShopMgr)
                    {
                        LevelAsset.ShopMgr.ShopUpdateAnimation(AnimationLerper);
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

                if (LevelAsset.ShopMgr)
                {
                    LevelAsset.ShopMgr.ShopPostAnimationUpdate();
                }
            }

            Animating = false;
            yield break;
        }

        //原则上这个不让被重载。
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
            if (!Animating)
            {
                LevelAsset.AnimationPendingObj = new List<MoveableBase>();

                WorldLogic.UpdateLogic(LevelAsset,out _ctrlPack, out var movedTile, out var movedCursor);

                if (LevelAsset.GameOverEnabled)
                {
                    UpdateGameOverStatus(LevelAsset);
                }
                bool shouldCycle = ShouldCycle(in _ctrlPack, in pressedAny, in movedTile, in movedCursor);
                Animating = shouldCycle;
                if (shouldCycle && movedTile&& (!_noRequirement))
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
                    StartCoroutine(Animate());//这里完成后会把Animating设回来。
                }
            }
            if (LevelAsset.HintEnabled)
            {
                UpdateSignalHint(_ctrlPack);
                LevelAsset.HintMaster.UpdateHintMaster(_ctrlPack);
            }
        }

        

        protected bool UpdateCareerGameOverStatus(GameAssets currentLevelAsset)
        {
            //这个函数就很接近裁判要做的事儿了。
            int NormalRval = 0;
            int NetworkRval = 0;

            foreach (var actionAssetTimeLineToken in currentLevelAsset.ActionAsset.TimeLineTokens)
            {
                if (actionAssetTimeLineToken.InRange(currentLevelAsset.StepCount))
                {
                    if (actionAssetTimeLineToken.type == TimeLineTokenType.Ending)
                    {
                        if (!IsTutorialLevel)
                        {
                            PendingCleanUp = true;
                            LevelMasterManager.Instance.LevelFinished(LevelAsset);
                        }
                        return true;
                    }
                    else if (actionAssetTimeLineToken.type == TimeLineTokenType.RequireNormal)
                    {
                        NormalRval += actionAssetTimeLineToken.RequireAmount;
                    }
                    else if (actionAssetTimeLineToken.type == TimeLineTokenType.RequireNetwork)
                    {
                        NetworkRval += actionAssetTimeLineToken.RequireAmount;
                    }
                }
            }
            if (NormalRval == 0 && NetworkRval == 0)
            {
                _noRequirement = true;
                currentLevelAsset.TimeLine.RequirementSatisfied = true;
            }
            else
            {
                _noRequirement = false;
                currentLevelAsset.BoardDataCollector.CalculateProcessorScore(out int harDriverCountInt);
                bool valA = (harDriverCountInt >= NormalRval);
                currentLevelAsset.BoardDataCollector.CalculateServerScore(out int NetworkCountInt);
                bool valB = (NetworkCountInt >= NetworkRval);
                currentLevelAsset.TimeLine.RequirementSatisfied = valA && valB;
            }

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
            LevelAsset.WarningDestoryer = new MeteoriteBomber();
            LevelAsset.WarningDestoryer.SetBoard(ref LevelAsset.GameBoard);
            LevelAsset.WarningDestoryer.Init(5, 2);
        }
        protected void InitShop()
        {
            LevelAsset.ShopMgr = gameObject.AddComponent<ShopMgr>();
            LevelAsset.ShopMgr.UnitTemplate = LevelAsset.GameBoard.UnitTemplate;
            LevelAsset.ShopMgr.ShopInit();
            LevelAsset.ShopMgr.ItemPriceTexts_TMP = new[] { LevelAsset.Item1PriceTmp, LevelAsset.Item2PriceTmp, LevelAsset.Item3PriceTmp, LevelAsset.Item4PriceTmp };
            LevelAsset.ShopMgr.CurrentGameStateMgr = LevelAsset.GameStateMgr;
            LevelAsset.ShopMgr.GameBoard = LevelAsset.GameBoard;
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