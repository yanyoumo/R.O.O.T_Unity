using System;
using System.Collections;
using System.Collections.Generic;
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

    public interface ITutorialLevel
    {
        void InitDestoryer(TutorialMgr invoker);
        void ForceSetDestoryerShell(TutorialMgr invoker, Vector2Int nextIncome);
        void ForceWindDestoryer(TutorialMgr invoker);
        void InitShop(TutorialMgr invoker);
        void StartShop(TutorialMgr invoker);
        void InitGameStateMgr(TutorialMgr invoker);
        void InitCurrencyIOMgr(TutorialMgr invoker);
        void InitCursor(TutorialMgr invoker, Vector2Int pos);
    }
    
    /// <summary>
    /// 一个每个关卡都有这么一个类，在Lvl-WRD之间传来传去。这个类只有一个，做成最通用的样子。
    /// </summary>
    public sealed class GameAssets//ASSET 这里不应该有任何之际逻辑（有些便于操作的除外
    {
        /// <summary>
        /// 裁判同时要担任神使，神要通过这里影响世界。
        /// </summary>
        public BaseLevelMgr Owner;
        public Type LevelLogicType;
        public ScoreSet StartingScoreSet;
        public PerMoveData StartingPerMoveData;
        //这些引用在Asset外面要设好，在WRD-LOGIC里面也要处理。
        public GameObject CursorTemplate;
        public GameObject ItemPriceRoot;
        public Board GameBoard;
        public DataScreen DataScreen;
        public HintMaster HintMaster;

        internal GameObject GameCursor;
        internal Cursor Cursor => GameCursor.GetComponent<Cursor>();

        internal CurrencyIOCalculator CurrencyIoCalculator;
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
        public bool LCDTimeEnabled = true;
        public bool HintEnabled = true;
        public bool ForceHddConnectionHint = false;
        public bool ForceServerConnectionHint = false;
        //internal flag 
        internal bool BoughtOnce = false;
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
            LCDTimeEnabled = false;
            CurrencyEnabled = false;
            DestroyerEnabled = false;
            HintEnabled = false;
            CycleEnabled = false;
            GameOverEnabled = false;
        }
    }

    public abstract class BaseLevelMgr : MonoBehaviour//LEVEL-LOGIC/每一关都有一个这个类。
    {
        //ASSET
        protected internal GameAssets LevelAsset;
        private ControllingPack _ctrlPack;
        protected ControllingPack CtrlPack => _ctrlPack;

        //Lvl-Logic实际用的判断逻辑。
        public bool Playing { get; private set;  }
        protected bool LogicFrameAnimeFrameToggle = true;
        protected bool ReadyToGo = false;
        protected bool ReferenceOk = false;
        protected bool PendingCleanUp;

        protected float AnimationTimerOrigin = 0.0f;//都是秒
        protected float AnimationDuration = 0.1f;//都是秒

        public readonly int LEVEL_LOGIC_SCENE_ID = StaticName.SCENE_ID_ADDTIVELOGIC;//这个游戏的这两个参数是写死的
        public readonly int LEVEL_ART_SCENE_ID = StaticName.SCENE_ID_ADDTIVEVISUAL;//但是别的游戏的这个值多少是需要重写的。

        protected virtual void UpdateLogicLevelReference()
        {
            LevelAsset.CursorTemplate = Resources.Load("Cursor/Prefab/Cursor", typeof(GameObject)) as GameObject;
            LevelAsset.GameBoard = FindObjectOfType<Board>();
            LevelAsset.Owner = this;
            LevelAsset.LevelLogicType = this.GetType();
        }

        void Awake()
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

        protected void InvokeGameStartedEvent()
        {
            //LevelMasterManager.Instance
            //GameStarted?.Invoke();
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
            LevelAsset.HintMaster.HideTutorialFrame = false;
            PopulateArtLevelReference();
        }
        public abstract void InitLevel(ScoreSet scoreSet = null, PerMoveData perMoveData = new PerMoveData());

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
            if (currentLevelAsset.GameStateMgr.EndGameCheck(new ScoreSet(), new PerMoveData()))
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

        IEnumerator AnimationCoroutine()
        {
            //这个使用coroutine重写。
            var cursor = LevelAsset.Cursor;

            float animationTimer = Time.timeSinceLevelLoad - AnimationTimerOrigin;
            float animationLerper = animationTimer / AnimationDuration;
            if (LevelAsset.AnimationPendingObj.Count > 0)
            {
                animationLerper = Mathf.Min(animationLerper, 1.0f);
                foreach (var moveableBase in LevelAsset.AnimationPendingObj)
                {
                    //
                    if (moveableBase.NextBoardPosition == moveableBase.CurrentBoardPosition)
                    {
                        moveableBase.SetPosWithAnimation(moveableBase.NextBoardPosition,
                            PosSetFlag.CurrentAndLerping);
                    }
                    else
                    {
                        moveableBase.LerpingBoardPosition = moveableBase.LerpBoardPos(animationLerper);
                    }
                }

                if (LevelAsset.MovedTileAni)
                {
                    if (LevelAsset.ShopMgr)
                    {
                        LevelAsset.ShopMgr.ShopUpdateAnimation(animationLerper);
                    }
                }
            }

            //Debug.Log(cursor.LerpingBoardPosition.ToString());
            if (animationLerper >= 1.0f)
            {
                //AnimationEnding
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

                LogicFrameAnimeFrameToggle = true;
                yield break;
            }

            LevelAsset.GameBoard.UpdateBoardAnimation();
            cursor.UpdateTransform(LevelAsset.GameBoard.GetFloatTransformAnimation(cursor.LerpingBoardPosition));
            yield return null;
        }
        //原则上这个不让被重载。
        protected virtual void Update()
        {
            //StartCoroutine(CoroutineTest());

            if ((!ReadyToGo) || (PendingCleanUp))
            {
                Playing = false;
                return;
            }

            if (!Playing)
            {
                Playing = true;
            }
            //TODO 从LF到AF的数据应该再多一些。
            System.Diagnostics.Debug.Assert(LevelAsset.GameBoard != null, nameof(LevelAsset.GameBoard) + " != null");
            var pressedAny = Input.anyKeyDown;
            _ctrlPack = new ControllingPack {CtrlCMD = ControllingCommand.Nop};
            if (LogicFrameAnimeFrameToggle)
            {
                //更新Lvl信息
                //更新物理
                //检查Asset。
                
                AnimationTimerOrigin = Time.timeSinceLevelLoad;
                LevelAsset.AnimationPendingObj = new List<MoveableBase>();

                WorldLogic.UpdateLogic(LevelAsset,out _ctrlPack, out var movedTile, out var movedCursor);

                if (LevelAsset.GameOverEnabled)
                {
                    UpdateGameOverStatus(LevelAsset);
                }
                if (StartGameMgr.UseTouchScreen)
                {
                    LogicFrameAnimeFrameToggle = !movedTile;
                }
                else
                {
                    LogicFrameAnimeFrameToggle = !(pressedAny & (movedTile | movedCursor));
                }
                if (!LogicFrameAnimeFrameToggle)
                {
                    LevelAsset.MovedTileAni = movedTile;
                    LevelAsset.MovedCursorAni = movedCursor;
                }
            }
            else
            {
                var cursor = LevelAsset.Cursor;

                var animationTimer = Time.timeSinceLevelLoad - AnimationTimerOrigin;
                var animationLerper = animationTimer / AnimationDuration;
                if (LevelAsset.AnimationPendingObj.Count > 0)
                {
                    animationLerper = Mathf.Min(animationLerper, 1.0f);
                    foreach (var moveableBase in LevelAsset.AnimationPendingObj)
                    {
                        //
                        if (moveableBase.NextBoardPosition == moveableBase.CurrentBoardPosition)
                        {
                            moveableBase.SetPosWithAnimation(moveableBase.NextBoardPosition,
                                PosSetFlag.CurrentAndLerping);
                        }
                        else
                        {
                            moveableBase.LerpingBoardPosition = moveableBase.LerpBoardPos(animationLerper);
                        }
                    }

                    if (LevelAsset.MovedTileAni)
                    {
                        if (LevelAsset.ShopMgr)
                        {
                            LevelAsset.ShopMgr.ShopUpdateAnimation(animationLerper);
                        }
                    }
                }

                if (animationLerper >= 1.0f)
                {
                    //AnimationEnding
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

                    LogicFrameAnimeFrameToggle = true;
                }

                LevelAsset.GameBoard.UpdateBoardAnimation();
                cursor.UpdateTransform(LevelAsset.GameBoard.GetFloatTransformAnimation(cursor.LerpingBoardPosition));
            }
            if (LevelAsset.HintEnabled)
            {
                UpdateSignalHint(_ctrlPack);
                LevelAsset.HintMaster.UpdateHintMaster(_ctrlPack);
            }
        }
    }

    public class DefaultLevelMgr : BaseLevelMgr //LEVEL-LOGIC/每一关都有一个这个类。
    {
        public override void InitLevel(ScoreSet scoreSet = null, PerMoveData perMoveData = new PerMoveData())
        {
            Debug.Assert(ReferenceOk);//意外的有确定Reference的……还行……
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));

            InitCurrencyIoMgr();
            LevelAsset.DeltaCurrency = 0.0f;

            LevelAsset.GameStateMgr = new StandardGameStateMgr();
            LevelAsset.GameStateMgr.InitGameMode(scoreSet ?? new ScoreSet(), perMoveData);

            InitShop();
            InitDestoryer();
            InitCursor(new Vector2Int(2, 3));
            LevelAsset.EnableAllCoreFunctionAndFeature();
            LevelAsset.GameBoard.InitBoardRealStart();
            LevelAsset.GameBoard.UpdateBoardAnimation();
            StartShop();

            ReadyToGo = true;

            LevelAsset.StartingScoreSet = scoreSet;
            LevelAsset.StartingPerMoveData = perMoveData;

            InvokeGameStartedEvent();
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
            LevelAsset.CurrencyIoCalculator = gameObject.AddComponent<CurrencyIOCalculator>();
            LevelAsset.CurrencyIoCalculator.m_Board = LevelAsset.GameBoard;
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