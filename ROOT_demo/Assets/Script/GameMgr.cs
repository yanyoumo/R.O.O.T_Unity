using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
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

    public static class GameGlobalStatus
    {
        public static GameStatus CurrentGameStatus;
        public static float lastEndingCurrent;
        public static float lastEndingTime;
    }

    public class GameMgr : MonoBehaviour
    {
        //TODO https://shimo.im/docs/Dd86KXTqHJpqxwYX
        public UnityEngine.UI.Text CurrencyText;
        public UnityEngine.UI.Text DeltaCurrencyText;
        public UnityEngine.UI.Text TimeText;

        public GameObject CursorTemplate;
        private GameObject _mCursor;
        public Board GameBoard;
        public float DeltaCurrency { get; private set; }

        private CurrencyIOCalculator _currencyIoCalculator;
        private GameStateMgr _gameStateMgr;
        private ShopMgr _shopMgr;
        private IWarningDestoryer _warningDestoryer;
        private GameObject[] _warningGo;

        public Text Item1Price;
        public Text Item2Price;
        public Text Item3Price;
        public Text Item4Price;

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

        public Canvas PlayingUI;
        public Canvas TutorialUI;

        private TutorialMgr _tutorialMgr;

        private bool LogicFrameAnimeFrameToggle=true;
        private bool movedTileAni=false;
        private bool movedCursorAni = false;
        private List<MoveableBase> animationPendingObj;

        public Image tmpHintPanel;
        //public Text tmpHintText;
        public TextMeshProUGUI tmpHintText;

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

        void Awake()
        {
#if UNITY_EDITOR
            GameGlobalStatus.CurrentGameStatus = GameStatus.Tutorial;
#endif
            Debug.Assert(GameGlobalStatus.CurrentGameStatus == GameStatus.Tutorial|| GameGlobalStatus.CurrentGameStatus == GameStatus.Playing);
            Random.InitState(Mathf.FloorToInt(Time.realtimeSinceStartup));
            if (GameGlobalStatus.CurrentGameStatus == GameStatus.Playing)
            {
                InitCurrencyIOMgr();
                DeltaCurrency = 0.0f;

                _gameStateMgr = new StandardGameStateMgr();
#if UNITY_EDITOR
                _gameStateMgr.InitGameMode(new ScoreSet(10000.0f, 600000), new PerMoveData());
#else
                _gameStateMgr.InitGameMode(new ScoreSet(1000.0f, 60), new PerMoveData());
#endif


                _shopMgr = gameObject.AddComponent<ShopMgr>();
                _shopMgr.UnitTemplate = GameBoard.UnitTemplate;
                _shopMgr.ShopInit();
                _shopMgr.ItemPriceTexts = new[] { Item1Price, Item2Price, Item3Price, Item4Price };
                _shopMgr.CurrentGameStateMgr = this._gameStateMgr;
                _shopMgr.GameBoard = this.GameBoard;

                _warningDestoryer = new MeteoriteBomber();
                _warningDestoryer.SetBoard(ref GameBoard);
                _warningDestoryer.Init(5, 2);
                PlayingUI.enabled = true;
                TutorialUI.enabled = false;
                EnableAllFeature();
            }
            else if (GameGlobalStatus.CurrentGameStatus == GameStatus.Tutorial)
            {
                PlayingUI.enabled = false;
                TutorialUI.enabled = true;
                DisableAllFeature();
                _tutorialMgr = gameObject.AddComponent<TutorialMgr>();
                _tutorialMgr.MainGameMgr = this;
            }
        }

        public void InitCurrencyIOMgr()
        {
            _currencyIoCalculator = gameObject.AddComponent<CurrencyIOCalculator>();
            _currencyIoCalculator.m_Board = GameBoard;
        }

        public void InitCursor(Vector2Int pos)
        {
            _mCursor = Instantiate(CursorTemplate);
            Cursor cursor=_mCursor.GetComponent<Cursor>();
            cursor.InitPosWithAnimation(pos);
            cursor.UpdateTransform(GameBoard.GetFloatTransformAnimation(cursor.LerpingBoardPosition));
        }

        // Start is called before the first frame update
        void Start()
        {
            if (GameGlobalStatus.CurrentGameStatus == GameStatus.Playing)
            {
                InitCursor(new Vector2Int(2, 3));

                GameBoard.InitBoardRealStart();
                GameBoard.UpdateBoardAnimation();

                //得最后做
                _shopMgr.ShopStart();
            }
            else if (GameGlobalStatus.CurrentGameStatus == GameStatus.Tutorial)
            {
                PlayingUI.enabled = false;
                TutorialUI.enabled = true;
            }
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
            if (_gameStateMgr!=null)
            {               
                CurrencyText.text = ":" + Utils.PaddingFloat4Digit(_gameStateMgr.GetCurrency());
            }
            else
            {
                CurrencyText.text = "No _gameStateMgr";
            }
            if (DeltaCurrency > 0)
            {
                DeltaCurrencyText.color = Color.green;
                DeltaCurrencyText.text = ":" + Utils.PaddingFloat4Digit(Mathf.Abs(DeltaCurrency));
            }
            else
            {
                DeltaCurrencyText.color = Color.red;
                DeltaCurrencyText.text = ":" + Utils.PaddingFloat4Digit(Mathf.Abs(DeltaCurrency));
            }
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
            TimeText.text = ":" + Utils.PaddingFloat4Digit(_gameStateMgr.GetGameTime());
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

                _shopMgr.ShopPreAnimationUpdate();
                _warningDestoryer.Step();
                _gameStateMgr.PerMove(new ScoreSet(), new PerMoveData(DeltaCurrency, 1));
            }
        }

        void UpdateGameOverStatus()
        {
            if (_gameStateMgr.EndGameCheck(new ScoreSet(), new PerMoveData()))
            {
                CurrencyText.text = "GAME OVER";
                GameGlobalStatus.CurrentGameStatus = GameStatus.Ended;
                GameGlobalStatus.lastEndingCurrent = _gameStateMgr.GetCurrency();
                GameGlobalStatus.lastEndingCurrent = _gameStateMgr.GetGameTime();
                UnityEngine.SceneManagement.SceneManager.LoadScene(StaticName.SCENE_ID_GAMEOVER);
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
            //TODO 从LF到AF的数据应该再多一些。
            bool movedTile=false;
            bool movedCusor = false;
            bool pressedAny = Input.anyKeyDown;
            if (LogicFrameAnimeFrameToggle)
            {
                animationTimerOrigin = Time.timeSinceLevelLoad;
                animationPendingObj = new List<MoveableBase>();

                UpdateLogic(out movedTile,out movedCusor);
                LogicFrameAnimeFrameToggle = !(pressedAny & (movedTile | movedCusor));
                if (!LogicFrameAnimeFrameToggle)
                {
                    movedTileAni = movedTile;
                    movedCursorAni = movedCusor;
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
                    //if (animationLerper >= (1.0f - Mathf.Epsilon/10.0f))
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
                tmpHintPanel.enabled = Input.GetButton(StaticName.INPUT_BUTTON_NAME_HINTCTRL);
                tmpHintText.enabled = Input.GetButton(StaticName.INPUT_BUTTON_NAME_HINTCTRL);
                UpdateHint();
            }
        }
    }
}