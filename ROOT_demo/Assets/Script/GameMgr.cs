using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    public class GameMgr : MonoBehaviour
    {
        public static event RootEVENT.GameMajorEvent GameStarted;
        public static event RootEVENT.GameMajorEvent GameOverReached;
        public static event RootEVENT.GameMajorEvent GameCompleteReached;
        //TODO https://shimo.im/docs/Dd86KXTqHJpqxwYX
        public GameObject CursorTemplate;
        private GameObject _mCursor;
        public Board GameBoard;
        public float DeltaCurrency { get; private set; }

        private CurrencyIOCalculator _currencyIoCalculator;
        private GameStateMgr _gameStateMgr;
        private ShopMgr _shopMgr;
        private IWarningDestoryer _warningDestoryer;
        private GameObject[] _warningGo;

        public GameObject ItemPriceRoot;

        private TextMeshPro Item1Price_TMP;
        private TextMeshPro Item2Price_TMP;
        private TextMeshPro Item3Price_TMP;
        private TextMeshPro Item4Price_TMP;

        private bool BoughtOnce = false;

        //下面是给【指引】弄得。
        public bool InputEnabled = true;
        public bool CursorEnabled = true;
        public bool RotateEnabled = true;

        //public bool UnitEnabled = true;
        public bool ShopEnabled = true;
        public bool UpdateDeltaCurrencyEnabled = true;
        public bool DestoryerEnabled = false;
        public bool HintEnabled = true;
        public bool PlayerDataUIEnabled = true;
        public bool GameOverEnabled = true;

        public bool ForceHDDConnectionHint = false;
        public bool ForceServerConnectionHint = false;

        private TutorialMgr _tutorialMgr;

        private bool LogicFrameAnimeFrameToggle=true;
        private bool movedTileAni=false;
        private bool movedCursorAni = false;
        private List<MoveableBase> animationPendingObj;

        public ScoreWriting sW;

        public DataScreen dataScreen;

        private bool readyToGo = false;
        public bool Playing { private set; get; }
        private bool referenceOK = false;
        private bool pendingCleanUp;

        public void SetReady_Tutorial(ScoreSet scoreSet = null, PerMoveData perMoveData = new PerMoveData(), Type _gameStateMgrType = null)
        {
            //这里是默认都关了
            Debug.Assert(referenceOK);
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));
            DisableAllFeature();

            readyToGo = true;
            GameStarted?.Invoke();
        }

        public void SetReady_GamePlay(ScoreSet scoreSet=null,PerMoveData perMoveData = new PerMoveData(), Type _gameStateMgrType = null)
        {
            Debug.Assert(referenceOK);
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));

            InitCurrencyIOMgr();
            DeltaCurrency = 0.0f;

            _gameStateMgr = _gameStateMgrType!=null ? GameStateMgr.GenerateGameStateMgrByType(_gameStateMgrType) : new StandardGameStateMgr();
            _gameStateMgr.InitGameMode(scoreSet ?? new ScoreSet(), perMoveData);

            InitShop();
            InitDestoryer();

            EnableAllFeature();
            InitCursor(new Vector2Int(2, 3));

            GameBoard.InitBoardRealStart();
            GameBoard.UpdateBoardAnimation();
            StartShop();

            readyToGo = true;
            GameStarted?.Invoke();
        }

        public bool CheckReference()
        {
            bool res = true;
            res &= (dataScreen != null);
            res &= (Item1Price_TMP != null);
            res &= (Item2Price_TMP != null);
            res &= (Item3Price_TMP != null);
            res &= (Item4Price_TMP != null);
            return res;
        }

        public void UpdateReference()
        {
            var tempT = ItemPriceRoot.GetComponentsInChildren<TextMeshPro>();
            foreach (var text in tempT)
            {
                if (text.gameObject.name == "UnitAPrice_1")
                {
                    Item1Price_TMP = text;
                }
                if (text.gameObject.name == "UnitAPrice_2")
                {
                    Item2Price_TMP = text;
                }
                if (text.gameObject.name == "UnitAPrice_3")
                {
                    Item3Price_TMP = text;
                }
                if (text.gameObject.name == "UnitAPrice_4")
                {
                    Item4Price_TMP = text;
                }
            }
            referenceOK = CheckReference();
        }

        private void EnableAllFeature()
        {
            InputEnabled = true;
            CursorEnabled = true;
            RotateEnabled = true;
            ShopEnabled = true;
            UpdateDeltaCurrencyEnabled = true;
            DestoryerEnabled = true;
            HintEnabled = true;
            PlayerDataUIEnabled = true;
            GameOverEnabled = true;
        }

        private void DisableAllFeature()
        {
            InputEnabled = false;
            CursorEnabled = false;
            RotateEnabled = false;
            ShopEnabled = false;
            UpdateDeltaCurrencyEnabled = false;
            DestoryerEnabled = false;
            HintEnabled = false;
            PlayerDataUIEnabled = false;
            GameOverEnabled = false;
        }

        private void InitDestoryer()
        {
            _warningDestoryer = new MeteoriteBomber();
            _warningDestoryer.SetBoard(ref GameBoard);
            _warningDestoryer.Init(5, 2);
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
            ((MeteoriteBomber) _warningDestoryer).ForceSetDestoryer(invoker,nextIncome);
        }

        public void ForceWindDestoryer(TutorialMgr invoker)
        {
            Debug.Assert(invoker, "这个函数只能在教程里面调。");
            do
            {
                _warningDestoryer.Step();
            } while (((MeteoriteBomber) _warningDestoryer).Counter>0);
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

        //先不删，做参考。
        /*void Awake()
        {
            _tutorialMgr = FindObjectOfType<TutorialMgr>();
            if (_tutorialMgr == null)
            {
                InitCurrencyIOMgr();
                DeltaCurrency = 0.0f;

                _gameStateMgr = new StandardGameStateMgr();
                _gameStateMgr.InitGameMode(new ScoreSet(1000.0f, 60), new PerMoveData());

                InitShop();

                InitDestoryer();
                PlayingUI.enabled = true;
                TutorialUI.enabled = false;
                EnableAllFeature();
            }
            else
            {
                DisableAllFeature();
            }
        }
        // Start is called before the first frame update
        void Start()
        {
            if (_tutorialMgr == null)
            {
                InitCursor(new Vector2Int(2, 3));

                GameBoard.InitBoardRealStart();
                GameBoard.UpdateBoardAnimation();

                //得最后做
                StartShop();
            }
            else
            {
                PlayingUI.enabled = false;
                TutorialUI.enabled = true;
            }
        }*/

        private void InitShop()
        {
            _shopMgr = gameObject.AddComponent<ShopMgr>();
            _shopMgr.UnitTemplate = GameBoard.UnitTemplate;
            _shopMgr.ShopInit();
            //_shopMgr.ItemPriceTexts = new[] { Item1Price, Item2Price, Item3Price, Item4Price };
            _shopMgr.ItemPriceTexts_TMP = new[] { Item1Price_TMP, Item2Price_TMP, Item3Price_TMP, Item4Price_TMP };
            _shopMgr.CurrentGameStateMgr = this._gameStateMgr;
            _shopMgr.GameBoard = this.GameBoard;
        }
        
        private void StartShop()
        {
            _shopMgr.ShopStart();
        }
        
        private void InitGameStateMgr()
        {
            _gameStateMgr = new StandardGameStateMgr();
            _gameStateMgr.InitGameMode(new ScoreSet(1000.0f, 60), new PerMoveData());
        }
        
        private void InitCurrencyIOMgr()
        {
            _currencyIoCalculator = gameObject.AddComponent<CurrencyIOCalculator>();
            _currencyIoCalculator.m_Board = GameBoard;
        }
        
        private void InitCursor(Vector2Int pos)
        {
            _mCursor = Instantiate(CursorTemplate);
            Cursor cursor=_mCursor.GetComponent<Cursor>();
            cursor.InitPosWithAnimation(pos);
            cursor.UpdateTransform(GameBoard.GetFloatTransformAnimation(cursor.LerpingBoardPosition));
        }
        
        private Vector2Int ClampPosInBoard(Vector2Int pos)
        {
            Vector2Int newPos = pos;
            newPos.x = Mathf.Clamp(newPos.x, 0, GameBoard.BoardLength - 1);
            newPos.y = Mathf.Clamp(newPos.y, 0, GameBoard.BoardLength - 1);
            return newPos;
        }

        private void CursorStayInBoard(Cursor cursor)
        {
            cursor.SetPosWithAnimation(ClampPosInBoard(cursor.CurrentBoardPosition), PosSetFlag.Current);
            cursor.SetPosWithAnimation(ClampPosInBoard(cursor.NextBoardPosition), PosSetFlag.Next);
        }

        void UpdateDestoryer()
        {
            if (_warningGo != null)
            {
                if (_warningGo.Length > 0)
                {
                    foreach (var go in _warningGo)
                    {
                        Destroy(go);
                        _warningGo = null;
                    }
                }
            }

            if (_warningDestoryer.GetStatus() != WarningDestoryerStatus.Dormant)
            {
                Vector2Int[] incomings = _warningDestoryer.NextStrikingPos(out int count);
                _warningGo = new GameObject[count];
                for (int i = 0; i < count; i++)
                {
                    _warningGo[i] = Instantiate(CursorTemplate);
                    var mIndCursor = _warningGo[i].GetComponent<Cursor>();
                    mIndCursor.SetIndMesh();
                    mIndCursor.InitPosWithAnimation(incomings[i]);
                    CursorStayInBoard(mIndCursor);
                    mIndCursor.UpdateTransform(GameBoard.GetFloatTransform(mIndCursor.CurrentBoardPosition));

                    Material tm = _warningGo[i].GetComponentInChildren<MeshRenderer>().material;

                    if (_warningDestoryer.GetStatus() == WarningDestoryerStatus.Warning)
                    {
                        tm.SetColor("_Color", Color.yellow);
                    }
                    else if (_warningDestoryer.GetStatus() == WarningDestoryerStatus.Striking)
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

        public void AddAnimationPendingObj(MoveableBase pending)
        {
            if (animationPendingObj!=null)
            {
                if (pending!=null)
                {                    
                    animationPendingObj.Add(pending);
                }
            }
        }

        void UpdateShop()
        {
            if (!BoughtOnce)
            {
                bool successBought = false;
                if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SHOPBUY1))
                {
                    successBought = _shopMgr.Buy(0);
                }
                else if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SHOPBUY2))
                {
                    successBought = _shopMgr.Buy(1);
                }
                else if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SHOPBUY3))
                {
                    successBought = _shopMgr.Buy(2);
                }
                else if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SHOPBUY4))
                {
                    successBought = _shopMgr.Buy(3);
                }

                if (successBought)
                {
                    BoughtOnce = true;
                }
            }
        }

        void UpdateCursor(ref Cursor cursor, out bool movedTile, out bool movedCursor)
        {
            movedTile = false;
            movedCursor = false;
            animationPendingObj.Add(cursor);
            Unit movingUnit = null;
            if (Input.GetButton(StaticName.INPUT_BUTTON_NAME_MOVEUNIT) && GameBoard.CheckBoardPosValidAndFilled(cursor.CurrentBoardPosition))
            {
                if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORLEFT))
                {
                    if (GameBoard.CheckBoardPosValidAndEmpty(cursor.GetWestCoord()))
                    {
                        GameObject unit = GameBoard.FindUnitUnderBoardPos(cursor.CurrentBoardPosition);
                        if (unit)
                        {
                            Vector2Int oldKey = cursor.CurrentBoardPosition;
                            movingUnit=unit.GetComponentInChildren<Unit>();
                            movingUnit.MoveLeft();
                            GameBoard.UpdateUnitBoardPosAnimation(oldKey);
                            movedTile = true;
                        }

                        cursor.MoveLeft();
                    }
                }

                if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORUP))
                {
                    if (GameBoard.CheckBoardPosValidAndEmpty(cursor.GetNorthCoord()))
                    {
                        GameObject unit = GameBoard.FindUnitUnderBoardPos(cursor.CurrentBoardPosition);
                        if (unit)
                        {
                            Vector2Int oldKey = cursor.CurrentBoardPosition;
                            movingUnit = unit.GetComponentInChildren<Unit>();
                            movingUnit.MoveUp();
                            GameBoard.UpdateUnitBoardPosAnimation(oldKey);
                            movedTile = true;
                        }

                        cursor.MoveUp();
                    }
                }

                if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORDOWN))
                {
                    if (GameBoard.CheckBoardPosValidAndEmpty(cursor.GetSouthCoord()))
                    {
                        GameObject unit = GameBoard.FindUnitUnderBoardPos(cursor.CurrentBoardPosition);
                        if (unit)
                        {
                            Vector2Int oldKey = cursor.CurrentBoardPosition;
                            movingUnit=unit.GetComponentInChildren<Unit>();
                            movingUnit.MoveDown();
                            GameBoard.UpdateUnitBoardPosAnimation(oldKey);
                            movedTile = true;
                        }

                        cursor.MoveDown();
                    }
                }

                if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORRIGHT))
                {
                    if (GameBoard.CheckBoardPosValidAndEmpty(cursor.GetEastCoord()))
                    {
                        GameObject unit = GameBoard.FindUnitUnderBoardPos(cursor.CurrentBoardPosition);
                        if (unit)
                        {
                            Vector2Int oldKey = cursor.CurrentBoardPosition;
                            movingUnit=unit.GetComponentInChildren<Unit>();
                            movingUnit.MoveRight();
                            GameBoard.UpdateUnitBoardPosAnimation(oldKey);
                            movedTile = true;
                        }

                        cursor.MoveRight();
                    }
                }
            }
            else
            {
                if (GameBoard.CheckBoardPosValid(cursor.CurrentBoardPosition))
                {
                    if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORLEFT))
                    {
                        movedCursor = true;
                        cursor.MoveLeft();
                    }

                    if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORRIGHT))
                    {
                        movedCursor = true;
                        cursor.MoveRight();
                    }

                    if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORUP))
                    {
                        movedCursor = true;
                        cursor.MoveUp();
                    }

                    if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORDOWN))
                    {
                        movedCursor = true;
                        cursor.MoveDown();
                    }
                }
            }

            if (movingUnit)
            {
                Debug.Assert(movingUnit);
                animationPendingObj.Add(movingUnit);
            }

            movedCursor |= movedTile;
            CursorStayInBoard(cursor);
        }

        void UpdateRotate(ref Cursor cursor)
        {
            if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_ROTATEUNIT))
            {
                if (GameBoard.CheckBoardPosValidAndFilled(cursor.CurrentBoardPosition))
                {
                    GameObject unit = GameBoard.FindUnitUnderBoardPos(cursor.CurrentBoardPosition);
                    if (unit)
                    {
                        unit.GetComponentInChildren<Unit>().UnitRotateCw();
                        GameBoard.UpdateBoard();
                    }
                }
            }
        }

        void UpdateInput(ref Cursor cursor, out bool movedTile, out bool movedCursor)
        {
            movedTile = false;
            movedCursor = false;
            if (ShopEnabled)
            {
                UpdateShop();
            }
            if (CursorEnabled)
            {
                UpdateCursor(ref cursor, out movedTile,out movedCursor);
            }
            if (RotateEnabled)
            {
                //旋转的动画先没有吧。
                UpdateRotate(ref cursor);
            }

        }

        void UpdateDeltaCurrency()
        {
            DeltaCurrency = 0.0f;
            DeltaCurrency += _currencyIoCalculator.CalculateProcessorScore();
            DeltaCurrency += _currencyIoCalculator.CalculateServerScore();
            DeltaCurrency -= _currencyIoCalculator.CalculateCost();

            System.Diagnostics.Debug.Assert(_gameStateMgr != null, nameof(_gameStateMgr) + " != null");
            dataScreen.SetLCD(_gameStateMgr.GetCurrency(), RowEnum.CurrentMoney);
            dataScreen.SetAlertLevel(_gameStateMgr.GetCurrencyRatio(), RowEnum.CurrentMoney);
            dataScreen.SetLCD(DeltaCurrency, RowEnum.DeltaMoney);
        }

        void UpdateHint()
        {
            GameBoard.ResetUnitEmission();
            if (Input.GetButton(StaticName.INPUT_BUTTON_NAME_HINTHDD) || ForceHDDConnectionHint)
            {
                GameBoard.DisplayConnectedHDDUnit();
            }

            if (Input.GetButton(StaticName.INPUT_BUTTON_NAME_HINTNET) || ForceServerConnectionHint)
            {
                GameBoard.DisplayConnectedServerUnit();
            }
        }

        void UpdatePlayerDataAndUI(bool movedTile = true)
        {
            dataScreen.SetLCD(_gameStateMgr.GetGameTime(),RowEnum.Time);
            dataScreen.SetAlertLevel(_gameStateMgr.GetTimeRatio(),RowEnum.Time);
#if UNITY_EDITOR
            if (movedTile|| Input.GetButton(StaticName.DEBUG_INPUT_BUTTON_NAME_FORCESTEP))
            {
#else
            if (movedTile)
            {
#endif
                if (BoughtOnce)
                {
                    BoughtOnce = false;
                }

                if (ShopEnabled)
                {
                    _shopMgr.ShopPreAnimationUpdate();
                }

                if (_warningDestoryer != null && DestoryerEnabled)
                {
                    _warningDestoryer.Step();
                }

                _gameStateMgr.PerMove(new ScoreSet(), new PerMoveData(DeltaCurrency, 1));
            }
        }

        void UpdateGameOverStatus()
        {
            if (_gameStateMgr.EndGameCheck(new ScoreSet(), new PerMoveData()))
            {
                //CurrencyText.text = "GAME OVER";
                GameMasterManager.UpdateGameGlobalStatuslastEndingIncome(_gameStateMgr.GetCurrency() - _gameStateMgr.StartingMoney);
                GameMasterManager.UpdateGameGlobalStatuslastEndingTime(_gameStateMgr.GetGameTime());
                //此时要把GameOverScene所需要的内容填好。
                pendingCleanUp = true;//“一次性”设计思路这里还要调整。
                GameOverReached?.Invoke();
            }
        }

        // Update is called once per frame
        void UpdateLogic(out bool movedTile, out bool movedCursor)
        {
            DeltaCurrency = 0.0f;
            movedTile = false;
            movedCursor = false;
            {
                if (DestoryerEnabled)
                {
                    UpdateDestoryer();
                }

                if (InputEnabled)
                {
                    var cursor = _mCursor.GetComponent<ROOT.Cursor>();
                    UpdateInput(ref cursor, out movedTile,out movedCursor);
                    GameBoard.UpdateBoardRotate();//TODO 旋转现在还是闪现的。这个不用着急做。
                }

                if (UpdateDeltaCurrencyEnabled)
                {
                    UpdateDeltaCurrency();
                }

                if (PlayerDataUIEnabled)
                {
                    UpdatePlayerDataAndUI(movedTile);
                }

                if (GameOverEnabled)
                {
                    UpdateGameOverStatus();
                }
            }
        }

        //都是秒
        private float animationTimerOrigin = 0.0f;
        
        private float animationDuration = 0.1f;
        
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
            System.Diagnostics.Debug.Assert(GameBoard != null, nameof(GameBoard) + " != null");
            bool movedTile =false;
            bool movedCursor = false;
            bool pressedAny = Input.anyKeyDown;
            if (LogicFrameAnimeFrameToggle)
            {
                animationTimerOrigin = Time.timeSinceLevelLoad;
                animationPendingObj = new List<MoveableBase>();

                UpdateLogic(out movedTile,out movedCursor);
                LogicFrameAnimeFrameToggle = !(pressedAny & (movedTile | movedCursor));
                if (!LogicFrameAnimeFrameToggle)
                {
                    movedTileAni = movedTile;
                    movedCursorAni = movedCursor;
                }
            }
            else
            {
                var cursor = _mCursor.GetComponent<ROOT.Cursor>();

                float animationTimer = Time.timeSinceLevelLoad - animationTimerOrigin;
                float animationLerper = animationTimer / animationDuration;
                if (animationPendingObj.Count > 0)
                {
                    animationLerper = Mathf.Min(animationLerper, 1.0f);
                    foreach (var moveableBase in animationPendingObj)
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

                    if (movedTileAni)
                    {
                        if (_shopMgr)
                        {
                            _shopMgr.ShopUpdateAnimation(animationLerper);
                        }
                    }
                }

                //Debug.Log(cursor.LerpingBoardPosition.ToString());
                if (animationLerper >= 1.0f)
                {
                    //AnimationEnding
                    foreach (var moveableBase in animationPendingObj)
                    {
                        //完成后的pingpong
                        moveableBase.SetPosWithAnimation(moveableBase.NextBoardPosition, PosSetFlag.All);
                    }

                    if (movedTileAni)
                    {
                        if (GameBoard != null)
                        {
                            GameBoard.UpdateBoardPostAnimation();
                        }

                        if (_shopMgr)
                        {
                            _shopMgr.ShopPostAnimationUpdate();
                        }
                    }

                    LogicFrameAnimeFrameToggle = true;
                }

                GameBoard.UpdateBoardAnimation();
                cursor.UpdateTransform(GameBoard.GetFloatTransformAnimation(cursor.LerpingBoardPosition));
            }
            if (HintEnabled)
            {
                UpdateHint();
            }
        }
    }
}