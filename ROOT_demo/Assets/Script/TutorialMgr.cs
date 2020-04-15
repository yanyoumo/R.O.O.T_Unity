using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace ROOT
{
    public partial class TutorialMgr : MonoBehaviour
    {
        private Canvas _tutorialCanvas;
        private TextMeshProUGUI _buttonText;
        private TextMeshProUGUI _mainText;
        private TextMeshProUGUI _hintAText;
        private TextMeshProUGUI _hintBText;
        private TextMeshProUGUI _hintCText;
        private Material _focusPanelMat;
        private Image _focusPanel;
        private Image _MainPanel;

        private string[] _tutorialContent = { };

        private int _tutorialContentCounter = 0;
        private int _tutorialContentMax = 0;

        //public GameObject CursorTemplate;

        public GameMgr MainGameMgr;

        private GameObject _mCursor;

        private readonly string NextButtonName = "NextButton";
        private readonly string NextButtonTextName = "NextButtonText";

        private readonly string MainTextName = "MainContent";
        private readonly string HintATextName = "HintTextA";
        private readonly string HintBTextName = "HintTextB";
        private readonly string HintCTextName = "HintTextC";

        private bool IsCustomButtonText = false;
        private string CustomButtonText = "";

        private Vector2 focusPanelOffset= new Vector2(-0.135f, -0.235f);
        private Vector2 focusPanelOrg = new Vector2(0.26f, 0.13f);

        void Start()
        {
            _tutorialContentMax = _tutorialContent.Length;
            _tutorialCanvas = MainGameMgr.TutorialUI;
            var tmpB = _tutorialCanvas.transform.gameObject.GetComponentsInChildren<Button>();
            var tmpT = _tutorialCanvas.transform.gameObject.GetComponentsInChildren<TextMeshProUGUI>();
            var tmpI = _tutorialCanvas.transform.gameObject.GetComponentsInChildren<Image>();

#if UNITY_EDITOR
            var tmpOT = _tutorialCanvas.transform.gameObject.GetComponentsInChildren<Text>();
            Debug.Assert(tmpOT.Length == 0, "不要再用原版Text，用TextMeshPro");
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
                }
                if (text.name == HintCTextName)
                {
                    _hintCText = text;
                }
            }

            foreach (var image in tmpI)
            {
                if (image.name == "FocusPanel")
                {
                    _focusPanel = image;
                    _focusPanelMat = _focusPanel.material;
                }

                if (image.name== "MainContentPanel")
                {
                    _MainPanel = image;
                }
            }

            foreach (var button in tmpB)
            {
                if (button.name == NextButtonName)
                {
                    button.onClick.AddListener(Next);
                    _buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                    Debug.Assert(_buttonText.name == NextButtonTextName);
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
                case 2:
                    GameObject go = MainGameMgr.GameBoard.InitUnit(new Vector2Int(2, 2), CoreType.Processor,
                        Utils.Shuffle(sidesA));
                    MainGameMgr.GameBoard.DeliverUnitRandomPlace(go, out Vector2Int deliveringPos);
                    _focusPanel.enabled = true;
                    Vector2 offset = focusPanelOrg + deliveringPos * focusPanelOffset;
                    _focusPanelMat.SetVector("_MainTex_ST", new Vector4(2.0f, 2.0f, offset.x, offset.y));
                    break;
                case 3:
                    _focusPanel.enabled = false;
                    MainGameMgr.InitCursor(new Vector2Int(2, 3));
                    MainGameMgr.InputEnabled = true;
                    MainGameMgr.CursorEnabled = true;
                    MainGameMgr.RotateEnabled = true;
                    _hintAText.enabled = true;
                    break;
                case 4:
                    GameObject goA = MainGameMgr.GameBoard.InitUnit(new Vector2Int(2, 2), CoreType.HardDrive,
                        Utils.Shuffle(sidesA));
                    MainGameMgr.GameBoard.DeliverUnitRandomPlace(goA);
                    GameObject goB = MainGameMgr.GameBoard.InitUnit(new Vector2Int(2, 2), CoreType.HardDrive,
                        Utils.Shuffle(sidesA));
                    MainGameMgr.GameBoard.DeliverUnitRandomPlace(goB);
                    MainGameMgr.GameBoard.UpdateBoardInit();
                    IsCustomButtonText = true;
                    CustomButtonText = "我试试";
                    break;
                case 5:
                    CustomButtonText = "我好了";
                    _hintAText.enabled = true;
                    _MainPanel.enabled = false;
                    break;
                case 6:
                    IsCustomButtonText = false;
                    _hintAText.enabled = false;
                    _MainPanel.enabled = true;
                    break;
                case 8:
                    MainGameMgr.InitCurrencyIOMgr();
                    MainGameMgr.UpdateDeltaCurrencyEnabled = true;
                    MainGameMgr.HintEnabled = true;
                    MainGameMgr.ForceHDDConnectionHint = true;
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
                    MainGameMgr.ForceServerConnectionHint = true;
                    break;
                case 13:
                    MainGameMgr.ForceServerConnectionHint = false;
                    MainGameMgr.ForceHDDConnectionHint = false;
                    IsCustomButtonText = true;
                    CustomButtonText = "我试试";
                    break;
                case 15:
                    CustomButtonText = "我好了";
                    _hintAText.enabled = true;
                    _hintBText.enabled = true;
                    _hintCText.enabled = true;
                    _MainPanel.enabled = false;
                    break;
                case 16:
                    IsCustomButtonText = false;
                    _MainPanel.enabled = true;
                    break;
                case 19:
                    IsCustomButtonText = true;
                    CustomButtonText = "我试试";
                    break;
                case 20:
                    CustomButtonText = "我好了";
                    _hintAText.enabled = true;
                    _hintBText.enabled = true;
                    _MainPanel.enabled = false;
                    break;
                case 21:
                    IsCustomButtonText = false;
                    _MainPanel.enabled = true;
                    break;
                case 22:
                    _hintAText.enabled = false;
                    _hintBText.enabled = false;
                    _hintCText.enabled = false;
                    break;
                case 23:
                    MainGameMgr.InitGameStateMgr();
                    MainGameMgr.PlayerDataUIEnabled = true;
                    MainGameMgr.PlayingUI.enabled = true;
                    MainGameMgr.ShopUI.gameObject.SetActive(false);
                    break;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                //TODO 按动回车继续，要写在UI上
                Next();
            }
            if (IsCustomButtonText)
            {
                _buttonText.text = CustomButtonText;
            }
            else
            {
                _buttonText.text = "继续" + DotDotDot();
            }

            if (_tutorialContentCounter < _tutorialContentMax)
            {
                _mainText.text = _tutorialContent[Mathf.FloorToInt(_tutorialContentCounter)];
            }
            else
            {
                _buttonText.text = "返回";
                _mainText.text = "教程结束";
            }
        }
    }
}