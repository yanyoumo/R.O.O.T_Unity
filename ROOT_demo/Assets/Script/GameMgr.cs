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
        public float lastEndingIncome;
        public float lastEndingTime;
    }

    //要把Asset和Logic彻底拆开。
    /// <summary>
    /// 世界本身的运行逻辑、应该类比于物理世界，高程度独立。
    /// </summary>
    internal static class GameLogic//WORLD-LOGIC
    {
        //对，这种需要影响场景怎么办？
        //本来是为了保证WRD-LOGIC的独立性（体现形而上学的概念）；
        //就是弄成了静态类，但是现在看估计得弄成单例？
        private static Vector2Int ClampPosInBoard(Vector2Int pos, Board gameBoard)
        {
            Vector2Int newPos = pos;
            newPos.x = Mathf.Clamp(newPos.x, 0, gameBoard.BoardLength - 1);
            newPos.y = Mathf.Clamp(newPos.y, 0, gameBoard.BoardLength - 1);
            return newPos;
        }

        private static void UpdateShop(ShopMgr shopMgr, ref bool boughtOnce)
        {
            if (!boughtOnce)
            {
                bool successBought = false;
                if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SHOPBUY1))
                {
                    successBought = shopMgr.Buy(0);
                }
                else if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SHOPBUY2))
                {
                    successBought = shopMgr.Buy(1);
                }
                else if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SHOPBUY3))
                {
                    successBought = shopMgr.Buy(2);
                }
                else if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SHOPBUY4))
                {
                    successBought = shopMgr.Buy(3);
                }

                if (successBought)
                {
                    boughtOnce = true;
                }
            }
        }

        internal static void UpdateDestoryer(GameAssets currentLevelAsset)
        {
            if (currentLevelAsset.WarningGo != null)
            {
                if (currentLevelAsset.WarningGo.Length > 0)
                {
                    foreach (var go in currentLevelAsset.WarningGo)
                    {
                        currentLevelAsset.Owner.WorldLogicRequestDestroy(go);
                        currentLevelAsset.WarningGo = null;
                    }
                }
            }

            if (currentLevelAsset.WarningDestoryer.GetStatus() != WarningDestoryerStatus.Dormant)
            {
                Vector2Int[] incomings = currentLevelAsset.WarningDestoryer.NextStrikingPos(out int count);
                currentLevelAsset.WarningGo = new GameObject[count];
                for (int i = 0; i < count; i++)
                {
                    currentLevelAsset.WarningGo[i] = currentLevelAsset.Owner.WorldLogicRequestInstantiate(currentLevelAsset.CursorTemplate);
                    var mIndCursor = currentLevelAsset.WarningGo[i].GetComponent<Cursor>();
                    mIndCursor.SetIndMesh();
                    mIndCursor.InitPosWithAnimation(incomings[i]);
                    CursorStayInBoard(currentLevelAsset);
                    mIndCursor.UpdateTransform(
                        currentLevelAsset.GameBoard.GetFloatTransform(mIndCursor.CurrentBoardPosition));

                    Material tm = currentLevelAsset.WarningGo[i].GetComponentInChildren<MeshRenderer>().material;

                    if (currentLevelAsset.WarningDestoryer.GetStatus() == WarningDestoryerStatus.Warning)
                    {
                        tm.SetColor("_Color", Color.yellow);
                    }
                    else if (currentLevelAsset.WarningDestoryer.GetStatus() == WarningDestoryerStatus.Striking)
                    {
                        tm.SetColor("_Color", new Color(1.0f, 0.2f, 0.0f));
                    }
                    else
                    {
                        Debug.Assert(false, "Internal Error");
                    }
                }
            }
        }

        internal static void CursorStayInBoard(GameAssets currentLevelAsset)
        {
            currentLevelAsset.Cursor.SetPosWithAnimation(
                ClampPosInBoard(currentLevelAsset.Cursor.CurrentBoardPosition, currentLevelAsset.GameBoard),
                PosSetFlag.Current);
            currentLevelAsset.Cursor.SetPosWithAnimation(
                ClampPosInBoard(currentLevelAsset.Cursor.NextBoardPosition, currentLevelAsset.GameBoard),
                PosSetFlag.Next);
        }

        internal static void UpdateRotate(GameAssets currentLevelAsset)
        {
            if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_ROTATEUNIT))
            {
                if (currentLevelAsset.GameBoard.CheckBoardPosValidAndFilled(currentLevelAsset.Cursor
                    .CurrentBoardPosition))
                {
                    GameObject unit =
                        currentLevelAsset.GameBoard.FindUnitUnderBoardPos(currentLevelAsset.Cursor
                            .CurrentBoardPosition);
                    if (unit)
                    {
                        unit.GetComponentInChildren<Unit>().UnitRotateCw();
                        currentLevelAsset.GameBoard.UpdateBoard();
                    }
                }
            }
        }

        internal static void UpdateCursor(GameAssets currentLevelAsset, out bool movedTile, out bool movedCursor)
        {
            movedTile = false;
            movedCursor = false;
            currentLevelAsset.AnimationPendingObj.Add(currentLevelAsset.Cursor);
            Unit movingUnit = null;
            if (Input.GetButton(StaticName.INPUT_BUTTON_NAME_MOVEUNIT) &&
                currentLevelAsset.GameBoard.CheckBoardPosValidAndFilled(currentLevelAsset.Cursor.CurrentBoardPosition))
            {
                if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORLEFT))
                {
                    if (currentLevelAsset.GameBoard.CheckBoardPosValidAndEmpty(currentLevelAsset.Cursor.GetWestCoord()))
                    {
                        GameObject unit =
                            currentLevelAsset.GameBoard.FindUnitUnderBoardPos(currentLevelAsset.Cursor
                                .CurrentBoardPosition);
                        if (unit)
                        {
                            Vector2Int oldKey = currentLevelAsset.Cursor.CurrentBoardPosition;
                            movingUnit = unit.GetComponentInChildren<Unit>();
                            movingUnit.MoveLeft();
                            currentLevelAsset.GameBoard.UpdateUnitBoardPosAnimation(oldKey);
                            movedTile = true;
                        }

                        currentLevelAsset.Cursor.MoveLeft();
                    }
                }

                if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORUP))
                {
                    if (currentLevelAsset.GameBoard.CheckBoardPosValidAndEmpty(currentLevelAsset.Cursor.GetNorthCoord())
                    )
                    {
                        GameObject unit =
                            currentLevelAsset.GameBoard.FindUnitUnderBoardPos(currentLevelAsset.Cursor
                                .CurrentBoardPosition);
                        if (unit)
                        {
                            Vector2Int oldKey = currentLevelAsset.Cursor.CurrentBoardPosition;
                            movingUnit = unit.GetComponentInChildren<Unit>();
                            movingUnit.MoveUp();
                            currentLevelAsset.GameBoard.UpdateUnitBoardPosAnimation(oldKey);
                            movedTile = true;
                        }

                        currentLevelAsset.Cursor.MoveUp();
                    }
                }

                if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORDOWN))
                {
                    if (currentLevelAsset.GameBoard.CheckBoardPosValidAndEmpty(currentLevelAsset.Cursor.GetSouthCoord())
                    )
                    {
                        GameObject unit =
                            currentLevelAsset.GameBoard.FindUnitUnderBoardPos(currentLevelAsset.Cursor
                                .CurrentBoardPosition);
                        if (unit)
                        {
                            Vector2Int oldKey = currentLevelAsset.Cursor.CurrentBoardPosition;
                            movingUnit = unit.GetComponentInChildren<Unit>();
                            movingUnit.MoveDown();
                            currentLevelAsset.GameBoard.UpdateUnitBoardPosAnimation(oldKey);
                            movedTile = true;
                        }

                        currentLevelAsset.Cursor.MoveDown();
                    }
                }

                if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORRIGHT))
                {
                    if (currentLevelAsset.GameBoard.CheckBoardPosValidAndEmpty(currentLevelAsset.Cursor.GetEastCoord()))
                    {
                        GameObject unit =
                            currentLevelAsset.GameBoard.FindUnitUnderBoardPos(currentLevelAsset.Cursor
                                .CurrentBoardPosition);
                        if (unit)
                        {
                            Vector2Int oldKey = currentLevelAsset.Cursor.CurrentBoardPosition;
                            movingUnit = unit.GetComponentInChildren<Unit>();
                            movingUnit.MoveRight();
                            currentLevelAsset.GameBoard.UpdateUnitBoardPosAnimation(oldKey);
                            movedTile = true;
                        }

                        currentLevelAsset.Cursor.MoveRight();
                    }
                }
            }
            else
            {
                if (currentLevelAsset.GameBoard.CheckBoardPosValid(currentLevelAsset.Cursor.CurrentBoardPosition))
                {
                    if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORLEFT))
                    {
                        movedCursor = true;
                        currentLevelAsset.Cursor.MoveLeft();
                    }

                    if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORRIGHT))
                    {
                        movedCursor = true;
                        currentLevelAsset.Cursor.MoveRight();
                    }

                    if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORUP))
                    {
                        movedCursor = true;
                        currentLevelAsset.Cursor.MoveUp();
                    }

                    if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORDOWN))
                    {
                        movedCursor = true;
                        currentLevelAsset.Cursor.MoveDown();
                    }
                }
            }

            if (movingUnit)
            {
                Debug.Assert(movingUnit);
                currentLevelAsset.AnimationPendingObj.Add(movingUnit);
            }

            movedCursor |= movedTile;
            CursorStayInBoard(currentLevelAsset);
        }

        internal static void UpdateInput(GameAssets currentLevelAsset, out bool movedTile, out bool movedCursor,
            ref bool boughtOnce)
        {
            movedTile = false;
            movedCursor = false;
            if (currentLevelAsset.ShopEnabled)
            {
                UpdateShop(currentLevelAsset.ShopMgr, ref boughtOnce);
            }

            if (currentLevelAsset.CursorEnabled)
            {
                UpdateCursor(currentLevelAsset, out movedTile, out movedCursor);
            }

            if (currentLevelAsset.RotateEnabled)
            {
                //旋转的动画先没有吧。
                UpdateRotate(currentLevelAsset);
            }
        }

        internal static void UpdateDeltaCurrency(GameAssets currentLevelAsset)
        {
            currentLevelAsset.DeltaCurrency = 0.0f;
            currentLevelAsset.DeltaCurrency += currentLevelAsset.CurrencyIoCalculator.CalculateProcessorScore();
            currentLevelAsset.DeltaCurrency += currentLevelAsset.CurrencyIoCalculator.CalculateServerScore();
            currentLevelAsset.DeltaCurrency -= currentLevelAsset.CurrencyIoCalculator.CalculateCost();

            //System.Diagnostics.Debug.Assert(currentLevelAsset._gameStateMgr != null, nameof(currentLevelAsset._gameStateMgr) + " != null");
            currentLevelAsset.DataScreen.SetLCD(currentLevelAsset.GameStateMgr.GetCurrency(), RowEnum.CurrentMoney);
            currentLevelAsset.DataScreen.SetAlertLevel(currentLevelAsset.GameStateMgr.GetCurrencyRatio(), RowEnum.CurrentMoney);
            currentLevelAsset.DataScreen.SetLCD(currentLevelAsset.DeltaCurrency, RowEnum.DeltaMoney);
        }
        internal static void UpdatePlayerDataAndUI(GameAssets currentLevelAsset, bool movedTile = true)
        {
            currentLevelAsset.DataScreen.SetLCD(currentLevelAsset.GameStateMgr.GetGameTime(), RowEnum.Time);
            currentLevelAsset.DataScreen.SetAlertLevel(currentLevelAsset.GameStateMgr.GetTimeRatio(), RowEnum.Time);
#if UNITY_EDITOR
            if (movedTile || Input.GetButton(StaticName.DEBUG_INPUT_BUTTON_NAME_FORCESTEP))
            {
#else
            if (movedTile)
            {
#endif
                if (currentLevelAsset.BoughtOnce)
                {
                    currentLevelAsset.BoughtOnce = false;
                }

                if (currentLevelAsset.ShopEnabled)
                {
                    currentLevelAsset.ShopMgr.ShopPreAnimationUpdate();
                }

                if (currentLevelAsset.WarningDestoryer != null && currentLevelAsset.DestoryerEnabled)
                {
                    currentLevelAsset.WarningDestoryer.Step();
                }

                currentLevelAsset.GameStateMgr.PerMove(new ScoreSet(), new PerMoveData(currentLevelAsset.DeltaCurrency, 1));
            }
        }
        internal static void UpdateLogic(GameAssets currentLevelAsset, out bool movedTile, out bool movedCursor)
        {
            currentLevelAsset.DeltaCurrency = 0.0f;
            movedTile = false;
            movedCursor = false;
            {
                if (currentLevelAsset.DestoryerEnabled)
                {
                    GameLogic.UpdateDestoryer(currentLevelAsset);
                }
                if (currentLevelAsset.InputEnabled)
                {
                    var cursor = currentLevelAsset.GameCursor.GetComponent<Cursor>();
                    GameLogic.UpdateInput(currentLevelAsset, out movedTile, out movedCursor, ref currentLevelAsset.BoughtOnce);
                    currentLevelAsset.GameBoard.UpdateBoardRotate();//TODO 旋转现在还是闪现的。这个不用着急做。
                }
                if (currentLevelAsset.UpdateDeltaCurrencyEnabled)
                {
                    UpdateDeltaCurrency(currentLevelAsset);
                }
                if (currentLevelAsset.PlayerDataUiEnabled)
                {
                    UpdatePlayerDataAndUI(currentLevelAsset,movedTile);
                }
            }
        }
    }

    /// <summary>
    /// 一个每个关卡都有这么一个类，在Lvl-WRD之间传来传去。这个类只有一个，做成最通用的样子。
    /// </summary>
    public sealed class GameAssets//ASSET 这里不应该有任何之际逻辑（有些便于操作的除外
    {
        public BaseLevelMgr Owner;//裁判同时要担任神使，神要通过这里影响世界。
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

        //public bool UnitEnabled = true;
        public bool ShopEnabled = true;
        public bool UpdateDeltaCurrencyEnabled = true;
        public bool DestoryerEnabled = false;
        public bool HintEnabled = true;
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
        public static event RootEVENT.GameMajorEvent GameStarted;
        public static event RootEVENT.GameMajorEvent GameOverReached;
        public static event RootEVENT.GameMajorEvent GameCompleteReached;
        //ASSET
        internal GameAssets LevelAsset;
        //LVL-Logic还负责和Inspector交互。需要把这里的引用传到Asset里面
        //原则上这些引用只有一开始用一下，之后不能从这里调。
        public GameObject CursorTemplate_IO;
        public Board GameBoard_IO;
        //Lvl-Logic实际用的判断逻辑。
        public bool Playing { get; private set;  }
        private bool LogicFrameAnimeFrameToggle = true;
        private bool readyToGo = false;
        private bool referenceOK = false;
        private bool pendingCleanUp;
        
        private float animationTimerOrigin = 0.0f;//都是秒
        private float animationDuration = 0.1f;//都是秒

        void LinkGameAsset()
        {
            //还要在这里把Asset的数据填好。这里只搞要从Inspector里面进来的。
            LevelAsset.CursorTemplate = CursorTemplate_IO;
            LevelAsset.GameBoard = GameBoard_IO;
        }

        void Awake()
        {
            LevelAsset=new GameAssets();
            //时序现在很乱。
            LevelAsset.Owner = this;
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

        //之后Tutorial和一般的Level要分开。
        public void SetReady_Tutorial(ScoreSet scoreSet = null, PerMoveData perMoveData = new PerMoveData(), Type _gameStateMgrType = null)
        {
            Debug.Assert(false);//先关了。
            //这里是默认都关了
            /*Debug.Assert(referenceOK);
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));
            LevelAsset.DisableAllFeature();

            readyToGo = true;
            SetLevelAsset();
            GameStarted?.Invoke();*/
        }

        //之后Tutorial和一般的Level要分开。
        public void SetReady_GamePlay(ScoreSet scoreSet=null,PerMoveData perMoveData = new PerMoveData(), Type _gameStateMgrType = null)
        {
            Debug.Assert(referenceOK);//意外的有确定Reference的……还行……
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));

            InitCurrencyIOMgr();
            LevelAsset.DeltaCurrency = 0.0f;

            LevelAsset.GameStateMgr = _gameStateMgrType!=null ? GameStateMgr.GenerateGameStateMgrByType(_gameStateMgrType) : new StandardGameStateMgr();
            LevelAsset.GameStateMgr.InitGameMode(scoreSet ?? new ScoreSet(), perMoveData);

            InitShop();
            InitDestoryer();

            LevelAsset.EnableAllFeature();
            InitCursor(new Vector2Int(2, 3));

            LevelAsset.GameBoard.InitBoardRealStart();
            LevelAsset.GameBoard.UpdateBoardAnimation();
            StartShop();

            readyToGo = true;

            GameStarted?.Invoke();
        }

        public bool CheckReference()
        {
            bool res = true;
            res &= (LevelAsset.DataScreen != null);
            res &= (LevelAsset.Item1PriceTmp != null);
            res &= (LevelAsset.Item2PriceTmp != null);
            res &= (LevelAsset.Item3PriceTmp != null);
            res &= (LevelAsset.Item4PriceTmp != null);
            return res;
        }

        public void UpdateReference()
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
            LinkGameAsset();
            referenceOK = CheckReference();
        }

        private void InitDestoryer()
        {
            LevelAsset.WarningDestoryer = new MeteoriteBomber();
            LevelAsset.WarningDestoryer.SetBoard(ref LevelAsset.GameBoard);
            LevelAsset.WarningDestoryer.Init(5, 2);
        }

        #region TutorialShellRegion

        public void InitDestoryer(TutorialMgr invoker)
        {
            Debug.Assert(invoker, "这个函数只能在教程里面调。");
            InitDestoryer();
        }

        public void ForceSetDestoryerShell(TutorialMgr invoker, Vector2Int nextIncome)
        {
            Debug.Assert(invoker, "这个函数只能在教程里面调。");
            ((MeteoriteBomber)LevelAsset.WarningDestoryer).ForceSetDestoryer(invoker,nextIncome);
        }

        public void ForceWindDestoryer(TutorialMgr invoker)
        {
            Debug.Assert(invoker, "这个函数只能在教程里面调。");
            do
            {
                LevelAsset.WarningDestoryer.Step();
            } while (((MeteoriteBomber)LevelAsset.WarningDestoryer).Counter>0);
        }

        public void InitShop(TutorialMgr invoker)
        {
            Debug.Assert(invoker, "这个函数只能在教程里面调。");
            InitShop();
        }
        public void StartShop(TutorialMgr invoker)
        {
            Debug.Assert(invoker, "这个函数只能在教程里面调。");
            StartShop();
        }

        public void InitGameStateMgr(TutorialMgr invoker)
        {
            Debug.Assert(invoker, "这个函数只能在教程里面调。");
            InitGameStateMgr();
        }

        public void InitCurrencyIOMgr(TutorialMgr invoker)
        {
            Debug.Assert(invoker, "这个函数只能在教程里面调。");
            InitCurrencyIOMgr();
        }

        public void InitCursor(TutorialMgr invoker, Vector2Int pos)
        {
            Debug.Assert(invoker, "这个函数只能在教程里面调。");
            InitCursor(pos);
        }

        #endregion

        private void InitShop()
        {
            LevelAsset.ShopMgr = gameObject.AddComponent<ShopMgr>();
            LevelAsset.ShopMgr.UnitTemplate = LevelAsset.GameBoard.UnitTemplate;
            LevelAsset.ShopMgr.ShopInit();
            LevelAsset.ShopMgr.ItemPriceTexts_TMP = new[] { LevelAsset.Item1PriceTmp, LevelAsset.Item2PriceTmp, LevelAsset.Item3PriceTmp, LevelAsset.Item4PriceTmp };
            LevelAsset.ShopMgr.CurrentGameStateMgr = LevelAsset.GameStateMgr;
            LevelAsset.ShopMgr.GameBoard = LevelAsset.GameBoard;
        }
        
        private void StartShop()
        {
            LevelAsset.ShopMgr.ShopStart();
        }
        
        private void InitGameStateMgr()
        {
            LevelAsset.GameStateMgr = new StandardGameStateMgr();
            LevelAsset.GameStateMgr.InitGameMode(new ScoreSet(1000.0f, 60), new PerMoveData());
        }
        
        private void InitCurrencyIOMgr()
        {
            LevelAsset.CurrencyIoCalculator = gameObject.AddComponent<CurrencyIOCalculator>();
            LevelAsset.CurrencyIoCalculator.m_Board = LevelAsset.GameBoard;
        }
        
        private void InitCursor(Vector2Int pos)
        {
            LevelAsset.GameCursor = Instantiate(LevelAsset.CursorTemplate);
            Cursor cursor=LevelAsset.GameCursor.GetComponent<Cursor>();
            cursor.InitPosWithAnimation(pos);
            cursor.UpdateTransform(LevelAsset.GameBoard.GetFloatTransformAnimation(cursor.LerpingBoardPosition));
        }
       
        void UpdateHint()
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

        //这个严格来说就已经属于检测了。
        private void UpdateGameOverStatus(GameAssets currentLevelAsset)
        {
            //这个函数就很接近裁判要做的事儿了。
            if (currentLevelAsset.GameStateMgr.EndGameCheck(new ScoreSet(), new PerMoveData()))
            {
                //CurrencyText.text = "GAME OVER";
                GameMasterManager.UpdateGameGlobalStatuslastEndingIncome(currentLevelAsset.GameStateMgr.GetCurrency() - currentLevelAsset.GameStateMgr.StartingMoney);
                GameMasterManager.UpdateGameGlobalStatuslastEndingTime(currentLevelAsset.GameStateMgr.GetGameTime());
                //此时要把GameOverScene所需要的内容填好。
                pendingCleanUp = true;
                GameOverReached?.Invoke();
            }
        }

        void Update()
        {
            if ((!readyToGo) || (pendingCleanUp))
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
                
                animationTimerOrigin = Time.timeSinceLevelLoad;
                LevelAsset.AnimationPendingObj = new List<MoveableBase>();

                GameLogic.UpdateLogic(LevelAsset, out movedTile, out movedCursor);

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
                //这个使用coroutine重写。
                var cursor = LevelAsset.GameCursor.GetComponent<Cursor>();

                float animationTimer = Time.timeSinceLevelLoad - animationTimerOrigin;
                float animationLerper = animationTimer / animationDuration;
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

    public class GameMgr : BaseLevelMgr //LEVEL-LOGIC/每一关都有一个这个类。
    {

    }
}