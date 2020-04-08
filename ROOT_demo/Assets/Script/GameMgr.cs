using System.Collections;
using System.Collections.Generic;
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

        public Text ItemAPrice;
        public Text ItemBPrice;
        public Text ItemCPrice;
        public Text ItemDPrice;

        private bool BoughtOnce = false;

        //下面是给【指引】弄得。
        public bool InputEnabled = true;
        public bool CursorEnabled = true;
        public bool RotateEnabled = true;

        //public bool UnitEnabled = true;
        public bool ShopEnabled = true;
        public bool UpdateDeltaCurrencyEnabled = true;
        public bool DestoryerEnabled = true;
        public bool HintEnabled = true;
        public bool PlayerDataUIEnabled = true;
        public bool GameOverEnabled = true;

        public Canvas PlayingUI;
        public Canvas TutorialUI;

        private TutorialMgr _tutorialMgr;

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
            //GameGlobalStatus.CurrentGameStatus = GameStatus.Tutorial;
#endif
            Random.InitState(Time.frameCount);
            if (GameGlobalStatus.CurrentGameStatus == GameStatus.Playing)
            {
                _currencyIoCalculator = gameObject.AddComponent<CurrencyIOCalculator>();
                _currencyIoCalculator.m_Board = GameBoard;
                DeltaCurrency = 0.0f;

                _gameStateMgr = new StandardGameStateMgr();
                _gameStateMgr.InitGameMode(new ScoreSet(1000.0f, 60), new PerMoveData());

                _shopMgr = gameObject.AddComponent<ShopMgr>();
                _shopMgr.InitPrice();
                _shopMgr.InitSideCoreWeight();
                _shopMgr.ItemAPriceText = this.ItemAPrice;
                _shopMgr.ItemBPriceText = this.ItemBPrice;
                _shopMgr.ItemCPriceText = this.ItemCPrice;
                _shopMgr.ItemDPriceText = this.ItemDPrice;
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

        public void InitCursor(Vector2Int pos)
        {
            _mCursor = Instantiate(CursorTemplate);
            _mCursor.GetComponent<Cursor>().board_position = pos;
        }

        // Start is called before the first frame update
        void Start()
        {
            if (GameGlobalStatus.CurrentGameStatus == GameStatus.Playing)
            {
                InitCursor(new Vector2Int(2, 3));
                GameBoard.InitBoardRealStart();
                _shopMgr.UnitTemplate = GameBoard.UnitTemplate;
                _shopMgr.ShopUpdate();
            }
            else if (GameGlobalStatus.CurrentGameStatus == GameStatus.Tutorial)
            {
                PlayingUI.enabled = false;
                TutorialUI.enabled = true;
            }
        }

        private void CursorStayInBoard(Cursor cursor)
        {
            Vector2Int newPos = cursor.board_position;
            newPos.x = Mathf.Clamp(newPos.x, 0, GameBoard.BoardLength - 1);
            newPos.y = Mathf.Clamp(newPos.y, 0, GameBoard.BoardLength - 1);
            cursor.board_position = newPos;
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
                    //Debug.Log("Drawing=" + incomings[i].ToString());
                    _warningGo[i] = Instantiate(CursorTemplate);
                    var mIndCursor = _warningGo[i].GetComponent<Cursor>();
                    mIndCursor.SetIndMesh();
                    mIndCursor.board_position = incomings[i];
                    CursorStayInBoard(mIndCursor);
                    mIndCursor.UpdateTransform(GameBoard.GetFloatTransform(mIndCursor.board_position));

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

        void UpdateShop()
        {
            if (!BoughtOnce)
            {
                bool successBought = false;
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    successBought = _shopMgr.BuyD();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    successBought = _shopMgr.BuyC();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    successBought = _shopMgr.BuyA();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    successBought = _shopMgr.BuyB();
                }

                if (successBought)
                {
                    BoughtOnce = true;
                }
            }
        }

        void UpdateCursor(ref Cursor cursor, out bool movedTile)
        {
            movedTile = false;
            if (Input.GetKey(KeyCode.Space) && GameBoard.CheckBoardPosValidAndFilled(cursor.board_position))
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    if (GameBoard.CheckBoardPosValidAndEmpty(cursor.GetWestUnit()))
                    {
                        GameObject unit = GameBoard.FindUnitUnderBoardPos(cursor.board_position);
                        if (unit)
                        {
                            Vector2Int oldKey = cursor.board_position;
                            unit.GetComponentInChildren<Unit>().MoveLeft();
                            GameBoard.UpdateUnitBoardPos(oldKey);
                            movedTile = true;
                        }

                        cursor.MoveLeft();
                    }
                }

                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    if (GameBoard.CheckBoardPosValidAndEmpty(cursor.GetNorthUnit()))
                    {
                        GameObject unit = GameBoard.FindUnitUnderBoardPos(cursor.board_position);
                        if (unit)
                        {
                            Vector2Int oldKey = cursor.board_position;
                            unit.GetComponentInChildren<Unit>().MoveUp();
                            GameBoard.UpdateUnitBoardPos(oldKey);
                            movedTile = true;
                        }

                        cursor.MoveUp();
                    }
                }

                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if (GameBoard.CheckBoardPosValidAndEmpty(cursor.GetSouthUnit()))
                    {
                        GameObject unit = GameBoard.FindUnitUnderBoardPos(cursor.board_position);
                        if (unit)
                        {
                            Vector2Int oldKey = cursor.board_position;
                            unit.GetComponentInChildren<Unit>().MoveDown();
                            GameBoard.UpdateUnitBoardPos(oldKey);
                            movedTile = true;
                        }

                        cursor.MoveDown();
                    }
                }

                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    if (GameBoard.CheckBoardPosValidAndEmpty(cursor.GetEastUnit()))
                    {
                        GameObject unit = GameBoard.FindUnitUnderBoardPos(cursor.board_position);
                        if (unit)
                        {
                            Vector2Int oldKey = cursor.board_position;
                            unit.GetComponentInChildren<Unit>().MoveRight();
                            GameBoard.UpdateUnitBoardPos(oldKey);
                            movedTile = true;
                        }

                        cursor.MoveRight();
                    }
                }
            }
            else
            {
                if (GameBoard.CheckBoardPosValid(cursor.board_position))
                {
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        cursor.MoveLeft();
                    }

                    if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        cursor.MoveRight();
                    }

                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        cursor.MoveUp();
                    }

                    if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        cursor.MoveDown();
                    }
                }
            }

            CursorStayInBoard(cursor);
        }

        void UpdateRotate(ref Cursor cursor)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                if (GameBoard.CheckBoardPosValidAndFilled(cursor.board_position))
                {
                    GameObject unit = GameBoard.FindUnitUnderBoardPos(cursor.board_position);
                    if (unit)
                    {
                        unit.GetComponentInChildren<Unit>().UnitRotateCw();
                    }
                }
            }
        }

        void UpdateInput(ref Cursor cursor, out bool movedTile)
        {
            movedTile = false;
            //var cursor = _mCursor.GetComponent<ROOT.Cursor>();
            if (ShopEnabled)
            {
                UpdateShop();
            }
            if (CursorEnabled)
            {
                UpdateCursor(ref cursor, out movedTile);
            }
            if (RotateEnabled)
            {
                UpdateRotate(ref cursor);
            }

        }

        void UpdateDeltaCurrency()
        {
            DeltaCurrency = 0.0f;
            DeltaCurrency += _currencyIoCalculator.CalculateProcessorScore();
            DeltaCurrency += _currencyIoCalculator.CalculateServerScore();
            DeltaCurrency -= _currencyIoCalculator.CalculateCost();
            CurrencyText.text = ":" + Utils.PaddingFloat4Digit(_gameStateMgr.GetCurrency());
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
            if (Input.GetKey(KeyCode.F1))
            {
                GameBoard.DisplayConnectedHDDUnit();
            }

            if (Input.GetKey(KeyCode.F2))
            {
                GameBoard.DisplayConnectedServerUnit();
            }
        }

        void UpdatePlayerDataUI(bool movedTile = true)
        {
            TimeText.text = ":" + Utils.PaddingFloat4Digit(_gameStateMgr.GetGameTime());
            if (movedTile)
            {
                if (BoughtOnce)
                {
                    BoughtOnce = false;
                }

                _shopMgr.ShopUpdate();
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
        void Update()
        {
            DeltaCurrency = 0.0f;
            bool movedTile = false;
            /*if (GameGlobalStatus.CurrentGameStatus==GameStatus.Tutorial)
            {
                
            }*/
            {
                if (DestoryerEnabled)
                {
                    UpdateDestoryer();
                }

                if (InputEnabled)
                {
                    var cursor = _mCursor.GetComponent<ROOT.Cursor>();
                    UpdateInput(ref cursor, out movedTile);
                    GameBoard.UpdateBoard();
                    cursor.UpdateTransform(GameBoard.GetFloatTransform(cursor.board_position));
                }

                if (UpdateDeltaCurrencyEnabled)
                {
                    UpdateDeltaCurrency();
                }

                if (HintEnabled)
                {
                    UpdateHint();
                }

                if (PlayerDataUIEnabled)
                {
                    UpdatePlayerDataUI(movedTile);
                }

                if (GameOverEnabled)
                {
                    UpdateGameOverStatus();
                }
            }
        }
    }
}