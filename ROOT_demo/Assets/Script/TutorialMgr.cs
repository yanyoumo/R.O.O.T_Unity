using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace ROOT
{
    public sealed partial class TutorialMgr : MonoBehaviour
    {
        private Canvas _tutorialCanvas;
        //private TextMeshProUGUI _buttonText;
        private TextMeshProUGUI _mainText;
        private TextMeshProUGUI _hintAText;
        private TextMeshProUGUI _hintBText;
        private TextMeshProUGUI _hintCText;
        private TextMeshProUGUI _NextHintText;
        private Material _focusPanelMat;
        private Image _focusPanel;
        private Image _mainPanel;

        private string[] _tutorialContent = { };

        private int _tutorialContentCounter = 0;
        private int _tutorialContentMax = 0;

        public GameMgr MainGameMgr;

        private GameObject _mCursor;

        private const string FocusPanelName = "FocusPanel";
        private const string MainContentPanelName = "MainContentPanel";

        private const string MainTextName = "MainContent";
        private const string HintATextName = "HintTextA";
        private const string HintBTextName = "HintTextB";
        private const string HintCTextName = "HintTextC";
        private const string EnterHint = "EnterHint";

        private bool _isCustomButtonText = false;
        private string _customButtonText = "";

        private Vector2 _focusPanelOffset= new Vector2(-0.135f, -0.235f);
        private Vector2 _focusPanelOrg = new Vector2(0.26f, 0.13f);

        private RectTransform mainRectTransform;

        void Start()
        {
            _tutorialContentMax = _tutorialContent.Length;
            _tutorialCanvas = MainGameMgr.TutorialUI;
            //var tmpB = _tutorialCanvas.transform.gameObject.GetComponentsInChildren<Button>();
            var tmpT = _tutorialCanvas.transform.gameObject.GetComponentsInChildren<TextMeshProUGUI>();
            var tmpI = _tutorialCanvas.transform.gameObject.GetComponentsInChildren<Image>();

#if UNITY_EDITOR
            var tmpOt = _tutorialCanvas.transform.gameObject.GetComponentsInChildren<Text>();
            Debug.Assert(tmpOt.Length == 0, "不要再用原版Text，用TextMeshPro");
#endif

            foreach (var text in tmpT)
            {
                if (text.name == MainTextName)
                {
                    _mainText = text;
                }

                if (text.name== HintATextName)
                {
                    _hintAText = text;
                }
                if (text.name == HintBTextName)
                {
                    _hintBText = text;
                    _hintBText.gameObject.SetActive(false);
                }
                if (text.name == HintCTextName)
                {
                    _hintCText = text;
                }

                if (text.name== EnterHint)
                {
                    _NextHintText = text;
                }
            }

            foreach (var image in tmpI)
            {
                if (image.name == FocusPanelName)
                {
                    _focusPanel = image;
                    _focusPanelMat = _focusPanel.material;
                }

                if (image.name== MainContentPanelName)
                {
                    _mainPanel = image;
                    mainRectTransform=_mainPanel.GetComponent<RectTransform>();
                }
            }
        }

        private string DotDotDot()
        {
            string res = "";
            float time = Time.timeSinceLevelLoad;
            int intTime = Mathf.FloorToInt(time * 15);
            for (int i = 0; i < (intTime % 6 + 1); i++)
            {
                res += ".";
            }

            return res;
        }

        public void Next()
        {
            EventSystem.current.SetSelectedGameObject(null);
            if (_tutorialContentCounter == _tutorialContentMax)
            {
                GameGlobalStatus.CurrentGameStatus = GameStatus.Starting;
                UnityEngine.SceneManagement.SceneManager.LoadScene(StaticName.SCENE_ID_START);
                return;
            }             
            _tutorialContentCounter++;
            UpdateNext();
        }

        public void UpdateNext()
        {
            SideType[] sidesA =
            {
                SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection
            };
            switch (_tutorialContentCounter)
            {
                case 1:
                    GameObject go = MainGameMgr.GameBoard.InitUnit(new Vector2Int(2, 2), CoreType.Processor,Utils.Shuffle(sidesA));
                    MainGameMgr.GameBoard.DeliverUnitRandomPlace(go);
                    break;
                case 2:
                    MainGameMgr.InitCursor(this,new Vector2Int(2, 3));
                    MainGameMgr.InputEnabled = true;
                    MainGameMgr.CursorEnabled = true;
                    MainGameMgr.RotateEnabled = true;
                    _hintAText.enabled = true;
                    break;
                case 3:
                    GameObject goA = MainGameMgr.GameBoard.InitUnit(new Vector2Int(2, 2), CoreType.HardDrive,Utils.Shuffle(sidesA));
                    GameObject goB = MainGameMgr.GameBoard.InitUnit(new Vector2Int(2, 2), CoreType.HardDrive,Utils.Shuffle(sidesA));
                    MainGameMgr.GameBoard.DeliverUnitRandomPlace(goA);
                    MainGameMgr.GameBoard.DeliverUnitRandomPlace(goB);
                    MainGameMgr.GameBoard.UpdateBoardInit();
                    _isCustomButtonText = true;
                    _customButtonText = "我试试";
                    break;
                case 4:
                    _customButtonText = "我好了";
                    _hintAText.enabled = true;
                    _mainPanel.enabled = false;
                    break;
                case 5:
                    _isCustomButtonText = false;
                    _mainPanel.enabled = true;
                    break;
                case 8:
                    MainGameMgr.InitCurrencyIOMgr(this);
                    MainGameMgr.UpdateDeltaCurrencyEnabled = true;
                    MainGameMgr.HintEnabled = true;
                    MainGameMgr.ForceHDDConnectionHint = true;
                    _isCustomButtonText = true;
                    _customButtonText = "我试试";
                    break;
                case 9:
                    _customButtonText = "我好了";
                    _mainPanel.enabled = false;
                    break;
                case 10:
                    _isCustomButtonText = false;
                    _mainPanel.enabled = true;
                    break;
                case 12:
                    GameObject goC = MainGameMgr.GameBoard.InitUnit(new Vector2Int(2, 2), CoreType.Server,
                        Utils.Shuffle(sidesA));
                    GameObject goD = MainGameMgr.GameBoard.InitUnit(new Vector2Int(2, 2), CoreType.NetworkCable,
                        Utils.Shuffle(sidesA));
                    GameObject goE = MainGameMgr.GameBoard.InitUnit(new Vector2Int(2, 2), CoreType.NetworkCable,
                        Utils.Shuffle(sidesA));
                    MainGameMgr.GameBoard.DeliverUnitRandomPlace(goC);
                    MainGameMgr.GameBoard.DeliverUnitRandomPlace(goD);
                    MainGameMgr.GameBoard.DeliverUnitRandomPlace(goE);
                    MainGameMgr.GameBoard.UpdateBoardInit();
                    MainGameMgr.ForceHDDConnectionHint = false;
                    _hintCText.enabled = true;
                    break;
                case 13:
                    _isCustomButtonText = true;
                    _customButtonText = "我试试";
                    //_hintBText.enabled = true;
                    _hintBText.gameObject.SetActive(true);
                    break;
                case 14:
                    _customButtonText = "我好了";
                    _mainPanel.enabled = false;
                    break;
                case 15:
                    _mainPanel.enabled = true;
                    break;
                case 16:
                    _isCustomButtonText = true;
                    _customButtonText = "我试试";
                    break;
                case 17:
                    _customButtonText = "我好了";
                    _mainPanel.enabled = false;
                    break;
                case 18:
                    _isCustomButtonText = false;
                    _mainPanel.enabled = true;
                    break;
                case 20:
                    _hintAText.enabled = false;
                    //_hintBText.enabled = false;
                    _hintBText.gameObject.SetActive(false);
                    _hintCText.enabled = false;
                    MainGameMgr.InitGameStateMgr(this);
                    MainGameMgr.PlayerDataUIEnabled = true;
                    MainGameMgr.PlayingUI.enabled = true;
                    MainGameMgr.ShopUI.gameObject.SetActive(false);
                    break;
                case 28:
                    mainRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1000f);
                    MainGameMgr.ShopEnabled = true;
                    MainGameMgr.InitShop(this);
                    MainGameMgr.StartShop(this);
                    MainGameMgr.ShopUI.gameObject.SetActive(true);
                    break;
                case 35:
                    _isCustomButtonText = true;
                    _customButtonText = "我试试";
                    break;
                case 36:
                    _customButtonText = "我好了";
                    _mainPanel.enabled = false;
                    break;
                case 37:
                    _isCustomButtonText = false;
                    _mainPanel.enabled = true;
                    MainGameMgr.InitDestoryer(this);
                    MainGameMgr.DestoryerEnabled = true;
                    MainGameMgr.ForceSetDestoryerShell(this, new Vector2Int(2, 4));
                    MainGameMgr.ForceWindDestoryer(this);
                    break;
                default:
                    break;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_QUIT))
            {
                GameGlobalStatus.CurrentGameStatus = GameStatus.Starting;
                UnityEngine.SceneManagement.SceneManager.LoadScene(StaticName.SCENE_ID_START);
            }
            else
            {
                if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_NEXT))
                {
                    Next();
                }

                if (_isCustomButtonText)
                {
                    _NextHintText.text = "按回车：" + _customButtonText;
                }
                else
                {
                    _NextHintText.text = "按回车继续" + DotDotDot();
                }

                if (_tutorialContentCounter < _tutorialContentMax)
                {
                    _mainText.text = _tutorialContent[Mathf.FloorToInt(_tutorialContentCounter)];
                }
                else
                {
                    _NextHintText.text = "按回车返回";
                    _mainText.text = "教程结束";
                }
            }
        }
    }
}