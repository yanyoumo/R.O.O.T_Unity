using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ROOT
{
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

        void Awake()
        {
            Random.InitState(Time.frameCount);

            _currencyIoCalculator = gameObject.AddComponent<CurrencyIOCalculator>();
            _currencyIoCalculator.m_Board = GameBoard;
            DeltaCurrency = 0.0f;

            _gameStateMgr =new StandardGameStateMgr();
            _gameStateMgr.InitGameMode(new ScoreSet(3000.0f, 100), new PerMoveData());

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
        }
        // Start is called before the first frame update
        void Start()
        {
            _mCursor = Instantiate(CursorTemplate);
            GameBoard.InitBoardRealStart();          
            _mCursor.GetComponent<Cursor>().board_position=new Vector2Int(2,3);
            _shopMgr.UnitTemplate = GameBoard.UnitTemplate;
            _shopMgr.ShopUpdate();
        }

        private void CursorStayInBoard(Cursor cursor)
        {
            Vector2Int newPos=cursor.board_position;
            newPos.x = Mathf.Clamp(newPos.x, 0, GameBoard.BoardLength-1);
            newPos.y = Mathf.Clamp(newPos.y, 0, GameBoard.BoardLength-1);
            cursor.board_position = newPos;
        }

        string PaddingFloat(float input)
        {
            return input + "";
        }

        void UpdateCursor()
        {

        }

        // Update is called once per frame
        void Update()
        {
            DeltaCurrency = 0.0f;
            bool movedTile = false;
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
                        var mIndCursor= _warningGo[i].GetComponent<Cursor>();
                        mIndCursor.SetIndMesh();
                        mIndCursor.board_position = incomings[i];
                        CursorStayInBoard(mIndCursor);
                        mIndCursor.UpdateTransform(GameBoard.GetFloatTransform(mIndCursor.board_position));

                        Material tm=_warningGo[i].GetComponentInChildren<MeshRenderer>().material;

                        if (_warningDestoryer.GetStatus()==WarningDestoryerStatus.Warning)
                        {
                            tm.SetColor("_Color",Color.yellow);
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
            {
                var cursor = _mCursor.GetComponent<ROOT.Cursor>();
                {
                    if (Input.GetKey(KeyCode.A))
                    {
                        //Shop优先
                        /*
                         *上D 左C
                         *下A 右B
                         */
                        if (!BoughtOnce)
                        {
                            bool successBought = false;
                            if (Input.GetKeyDown(KeyCode.UpArrow))
                            {
                                successBought=_shopMgr.BuyD();
                            }
                            else if (Input.GetKeyDown(KeyCode.DownArrow))
                            {
                                successBought=_shopMgr.BuyA();
                            }
                            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                            {
                                successBought=_shopMgr.BuyC();
                            }
                            else if (Input.GetKeyDown(KeyCode.RightArrow))
                            {
                                successBought=_shopMgr.BuyB();
                            }

                            if (successBought)
                            {
                                BoughtOnce = true;
                            }
                        }
                    }
                    else
                    {
                        if (Input.GetKey(KeyCode.Z) && GameBoard.CheckBoardPosValidAndFilled(cursor.board_position))
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
                    }

                    CursorStayInBoard(cursor);
                    if (Input.GetKeyDown(KeyCode.X))
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
                GameBoard.UpdateBoard();
                cursor.UpdateTransform(GameBoard.GetFloatTransform(cursor.board_position));
            }
            {
                DeltaCurrency += _currencyIoCalculator.CalculateProcessorScore();
                DeltaCurrency += _currencyIoCalculator.CalculateServerScore();
                DeltaCurrency -= _currencyIoCalculator.CalculateCost();
                CurrencyText.text = "Currency:" + PaddingFloat(_gameStateMgr.GetCurrency());
                DeltaCurrencyText.text = "DeltaCurrency:" + DeltaCurrency;
                TimeText.text = "Time:" + _gameStateMgr.GetGameTime();
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

                if (_gameStateMgr.EndGameCheck(new ScoreSet(), new PerMoveData()))
                {
                    CurrencyText.text = "GAME OVER";
                }
            }
        }
    }
}