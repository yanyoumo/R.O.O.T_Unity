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

        internal GameObject GameCursor;
        internal Cursor Cursor => GameCursor.GetComponent<Cursor>();
        internal CurrencyIOCalculator CurrencyIoCalculator;
        internal GameStateMgr GameStateMgr;
        internal TutorialMgr TutorialMgr;
        internal ShopMgr ShopMgr;
        internal IWarningDestoryer WarningDestoryer;
        internal GameObject[] WarningGo;

        internal TextMeshPro Item1PriceTmp;
        internal TextMeshPro Item2PriceTmp;
        internal TextMeshPro Item3PriceTmp;
        internal TextMeshPro Item4PriceTmp;

        public float DeltaCurrency { get; internal set; }

        //下面是给【指引】弄得。
        public bool InputEnabled = true;
        public bool CursorEnabled = true;
        public bool RotateEnabled = true;

        /// <summary>
        /// 开启商店，先决条件为：PlayerDataUiEnabled开启。
        /// </summary>
        public bool ShopEnabled = true;
        public bool UpdateDeltaCurrencyEnabled = true;
        /// <summary>
        /// 摧毁模组的计算，但是同时需要PlayerDataUiEnabled开启才进行步进。
        /// </summary>
        public bool DestoryerEnabled = false;
        public bool HintEnabled = true;
        /// <summary>
        ///更新玩家每一轮的信息，包含更新LCD；为商店开启购买的先决条件，为摧毁模组步进的先决条件。
        /// </summary>
        public bool PlayerDataUiEnabled = true;
        public bool GameOverEnabled = true;

        public bool ForceHddConnectionHint = false;
        public bool ForceServerConnectionHint = false;


        internal bool BoughtOnce = false;
        internal bool MovedTileAni = false;
        internal bool MovedCursorAni = false;
        internal List<MoveableBase> AnimationPendingObj;

        //一些辅助函数可以在这里。

        internal void EnableAllFeature()
        {
            InputEnabled = true;
            CursorEnabled = true;
            RotateEnabled = true;
            ShopEnabled = true;
            UpdateDeltaCurrencyEnabled = true;
            DestoryerEnabled = true;
            HintEnabled = true;
            PlayerDataUiEnabled = true;
            GameOverEnabled = true;
        }

        internal void DisableAllFeature()
        {
            InputEnabled = false;
            CursorEnabled = false;
            RotateEnabled = false;
            ShopEnabled = false;
            UpdateDeltaCurrencyEnabled = false;
            DestoryerEnabled = false;
            HintEnabled = false;
            PlayerDataUiEnabled = false;
            GameOverEnabled = false;
        }
    }

    //TODO 然后就是Level中的时序怎么弄了。
    public abstract class BaseLevelMgr : MonoBehaviour//LEVEL-LOGIC/每一关都有一个这个类。
    {
        //事件完全由LVL-Logic管理。
        /*public static event RootEVENT.GameMajorEvent GameStarted;
        public static event RootEVENT.GameMajorEvent GameOverReached;
        public static event RootEVENT.GameMajorEvent GameCompleteReached;*/
        //ASSET
        protected internal GameAssets LevelAsset;
        //LVL-Logic还负责和Inspector交互。需要把这里的引用传到Asset里面
        //原则上这些引用只有一开始用一下，之后不能从这里调。
        //private GameObject CursorTemplateIo;
        //private Board GameBoardIo;
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

        void Awake()
        {
            //因为这个也是动态生成的了，UnitTemplate和Board都要动态的找了。
            LevelAsset = new GameAssets();
            //时序现在很乱。
            LevelAsset.CursorTemplate = Resources.Load("Cursor/Prefab/Cursor", typeof(GameObject)) as GameObject;
            LevelAsset.GameBoard = FindObjectOfType<Board>();
            LevelAsset.Owner = this;
            LevelAsset.LevelLogicType = this.GetType();
            //LinkGameAsset();
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

        public abstract void InitLevel(ScoreSet scoreSet = null, PerMoveData perMoveData = new PerMoveData());
        /*protected abstract void InitDestoryer();
        protected abstract void InitShop();
        protected abstract void InitGameStateMgr();
        protected abstract void InitCurrencyIoMgr();
        protected abstract void InitCursor(Vector2Int pos);*/

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
        public virtual void UpdateReference()
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
        protected virtual void UpdateHint()
        {
            LevelAsset.GameBoard.ResetUnitEmission();
            if (Input.GetButton(StaticName.INPUT_BUTTON_NAME_HINTHDD) || LevelAsset.ForceHddConnectionHint)
            {
                LevelAsset.GameBoard.DisplayConnectedHDDUnit();
            }

            if (Input.GetButton(StaticName.INPUT_BUTTON_NAME_HINTNET) || LevelAsset.ForceServerConnectionHint)
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
        //原则上这个不让被重载了。
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
            bool movedTile =false;
            bool movedCursor = false;
            bool pressedAny = Input.anyKeyDown;
            if (LogicFrameAnimeFrameToggle)
            {
                //更新Lvl信息
                //更新物理
                //检查Asset。
                
                AnimationTimerOrigin = Time.timeSinceLevelLoad;
                LevelAsset.AnimationPendingObj = new List<MoveableBase>();

                WorldLogic.UpdateLogic(LevelAsset, out movedTile, out movedCursor);

                if (LevelAsset.GameOverEnabled)
                {

                    UpdateGameOverStatus(LevelAsset);
                }

                LogicFrameAnimeFrameToggle = !(pressedAny & (movedTile | movedCursor));
                if (!LogicFrameAnimeFrameToggle)
                {
                    LevelAsset.MovedTileAni = movedTile;
                    LevelAsset.MovedCursorAni = movedCursor;
                }
            }
            else
            {
                //StartCoroutine(AnimationCoroutine());
                //但是此时可以跑一些和输入无关的代码了。
                //这个使用coroutine重写。//直接重写没P用，要想想怎么弄。
                //coroutine对于ROOT这个框架似乎就是没啥用，但是对于kRTS可能还有点用。
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
                }

                LevelAsset.GameBoard.UpdateBoardAnimation();
                cursor.UpdateTransform(LevelAsset.GameBoard.GetFloatTransformAnimation(cursor.LerpingBoardPosition));
            }
            if (LevelAsset.HintEnabled)
            {
                UpdateHint();
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

            LevelAsset.EnableAllFeature();
            InitCursor(new Vector2Int(2, 3));

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
        /*protected void InitGameStateMgr()
        {
            LevelAsset.GameStateMgr = new StandardGameStateMgr();
            LevelAsset.GameStateMgr.InitGameMode(new ScoreSet(1000.0f, 60), new PerMoveData());
        }*/
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