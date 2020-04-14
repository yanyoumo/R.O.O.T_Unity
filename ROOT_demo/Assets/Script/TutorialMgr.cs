using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace ROOT
{
    public class TutorialMgr : MonoBehaviour
    {
        private Canvas _tutorialCanvas;
        private TextMeshProUGUI _buttonText;
        private TextMeshProUGUI _mainText;
        private TextMeshProUGUI _hintAText;
        //private TextMeshProUGUI _hintBText;
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
        //private readonly string HintBTextName = "HintTextB";

        private bool IsCustomButtonText = false;
        private string CustomButtonText = "";

        private Vector2 focusPanelOffset= new Vector2(-0.135f, -0.235f);
        private Vector2 focusPanelOrg = new Vector2(0.26f, 0.13f);

        // Start is called before the first frame update
        void Awake()
        {
            _tutorialContent = new[]
            {
                /*000*/"你好，欢迎来到R.O.O.T.教程。",
                /*001*/"这是一个基于棋盘的模拟经营游戏。",
                /*002*/"首先，这个是游戏中最重要的元素，我们称为单位。",
                /*003*/"然后，这个是你的光标。",
                /*004*/"来，再给你几个单位，随便试试先，习惯一下操作。",
                /*005*/"",
                /*006*/"你也一定注意到了，只从外形上来看，又两大类单元。一个方的，一个圆的。",
                /*007*/"方形是发射端，圆形是接收端。并且接收端可以串联下去。",
                /*008*/"除了形状，上面的图案也很重要。你已经接触过的是【处理器和硬盘】这一组发射端和接收端。",
                /*009*/"来，这时另外一组。这组称为【服务器和网线】。",
                /*00X*/"好的，教程继续。",
                /*00X*/"好的，教程继续。",
                /*00X*/"好的，教程继续。",
                /*00X*/"好的，教程继续。",
                /*00X*/"好的，教程继续。",
                /*00X*/"好的，教程继续。",
                /*00X*/"好的，教程继续。",
            };
            /*_tutorialContent = new[]
            {
                "你好，欢迎来到R.O.O.T.教程。",
                "这是一个基于棋盘的模拟经营游戏。",
                "这个是你的光标，使用键盘的【方向键】控制移动，先来试试吧。",
                "好的，下面是这个游戏中最重要的元素，我们称为单位。",
                "将光标移动上去后，按住【空格】键同时使用方向键，就可以拖动这个单位。",
                "在光标选中后单位后，点按【左Shift】可以进行旋转。",
                "来，再给你几个单位，随便试试先，习惯一下操作。",
                "",
                "好的，教程继续。",
                "现在把目光放回到一个单元上，咱们会看到一个单元的四个边的颜色不一样。这个就是另外一个概念，一个单元会有四个不同的接口。分为绿色和红色。",
                "绿色代表接通，红色代表无法接通。\n换句话说，两个单元如果通过绿色接口贴近的话，就可以将两个单元连接起来。",
                "试试吧，再给你两个，把所有的单元都链接起来试试。",
                "",
                "好的，教程继续。",
                "此时可以看到，闪烁代表连接上了，并且这个链接状态是可以传递的。只不过需要从一个“源头”开始。",
                "当然，这只是一种单元的玩法，咱们给换成另一种玩法的单位。",
                "",
                "好的，教程继续。",
                "好的，教程继续。",
                "好的，教程继续。",
                "好的，教程继续。",
                "好的，教程继续。",
                "好的，教程继续。",
                "好的，教程继续。",
                "好的，教程继续。",
                "好的，教程继续。",
                "好的，教程继续。",
                "好的，教程继续。",
                "好的，教程继续。",
                "好的，教程继续。",
                "好的，教程继续。",
                "好的，教程继续。",
                "好的，教程继续。",
                "好的，教程继续。",
            };*/
        }

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
                /*if (text.name == HintBTextName)
                {
                    _hintBText = text;
                }*/
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
            SideType[] sidesB =
            {
                SideType.NoConnection, SideType.Connection, SideType.NoConnection, SideType.Connection
            };
            switch (_tutorialContentCounter)
            {
                case 2:
                    GameObject go = MainGameMgr.GameBoard.InitUnit(new Vector2Int(2, 2), CoreType.Processor, Utils.Shuffle(sidesA));
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
                        Utils.Shuffle(sidesB));
                    MainGameMgr.GameBoard.DeliverUnitRandomPlace(goA);
                    GameObject goB = MainGameMgr.GameBoard.InitUnit(new Vector2Int(2, 2), CoreType.HardDrive,
                        Utils.Shuffle(sidesB));
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
                case 7:
                    MainGameMgr.ForceHDDConnectionHint = true;
                    break;
                case 9:
                    GameObject goC = MainGameMgr.GameBoard.InitUnit(new Vector2Int(2, 2), CoreType.Server,
                        Utils.Shuffle(sidesA));
                    MainGameMgr.GameBoard.DeliverUnitRandomPlace(goC);
                    GameObject goD = MainGameMgr.GameBoard.InitUnit(new Vector2Int(2, 2), CoreType.NetworkCable,
                        Utils.Shuffle(sidesB));
                    MainGameMgr.GameBoard.DeliverUnitRandomPlace(goD);
                    MainGameMgr.GameBoard.DeliverUnitRandomPlace(goD);
                    MainGameMgr.GameBoard.UpdateBoardInit();
                    MainGameMgr.ForceServerConnectionHint = true;
                    break;
            }
            /*switch (_tutorialContentCounter)
            {
                case 2:
                    MainGameMgr.InitCursor(new Vector2Int(2, 3));
                    MainGameMgr.InputEnabled = true;
                    MainGameMgr.CursorEnabled = true;
                    break;
                case 3:

                    GameObject go = MainGameMgr.GameBoard.InitUnit(new Vector2Int(2, 2), CoreType.Processor,
                        Utils.Shuffle(sidesA));
                    MainGameMgr.GameBoard.DeliverUnitRandomPlace(go, out Vector2Int deliveringPos);
                    MainGameMgr.InputEnabled = false;
                    _focusPanel.enabled = true;
                    Vector2 offset = focusPanelOrg + deliveringPos * focusPanelOffset;
                    _focusPanelMat.SetVector("_MainTex_ST", new Vector4(2.0f, 2.0f, offset.x, offset.y));
                    break;
                case 4:
                    _focusPanel.enabled = false;
                    MainGameMgr.InputEnabled = true;
                    break;
                case 5:
                    MainGameMgr.RotateEnabled = true;
                    break;
                case 6:
                    //TODO 这里需要变成读取然后随机拿两个空的位置。
                    GameObject goA = MainGameMgr.GameBoard.InitUnit(new Vector2Int(2, 2), CoreType.HardDrive,
                        Utils.Shuffle(sidesB));
                    MainGameMgr.GameBoard.DeliverUnitRandomPlace(goA);
                    GameObject goB = MainGameMgr.GameBoard.InitUnit(new Vector2Int(2, 2), CoreType.HardDrive,
                        Utils.Shuffle(sidesB));
                    MainGameMgr.GameBoard.DeliverUnitRandomPlace(goB);
                    MainGameMgr.GameBoard.UpdateBoardInit();
                    IsCustomButtonText = true;
                    CustomButtonText = "我试试";
                    break;
                case 7:
                    CustomButtonText = "我好了";
                    _hintAText.enabled = true;
                    _MainPanel.enabled = false;
                    break;
                case 8:
                    IsCustomButtonText = false;
                    _hintAText.enabled = false;
                    _MainPanel.enabled = true;
                    break;
                case 9:
                    MainGameMgr.InputEnabled = false;
                    _focusPanel.enabled = true;
                    var units = MainGameMgr.GameBoard.Units.Values.ToArray();
                    offset = units[0].GetComponentInChildren<Unit>().CurrentBoardPosition;
                    offset = focusPanelOrg + offset * focusPanelOffset;
                    _focusPanelMat.SetVector("_MainTex_ST", new Vector4(2.0f, 2.0f, offset.x, offset.y));
                    break;
                case 11:
                    //试试吧，再给你两个，把所有的单元都链接起来试试。
                    _focusPanel.enabled = false;
                    MainGameMgr.InputEnabled = true;
                    GameObject goC = MainGameMgr.GameBoard.InitUnit(new Vector2Int(2, 2), CoreType.HardDrive,
                        Utils.Shuffle(sidesB));
                    MainGameMgr.GameBoard.DeliverUnitRandomPlace(goC);
                    GameObject goD = MainGameMgr.GameBoard.InitUnit(new Vector2Int(2, 2), CoreType.HardDrive,
                        Utils.Shuffle(sidesB));
                    MainGameMgr.GameBoard.DeliverUnitRandomPlace(goD);
                    MainGameMgr.GameBoard.UpdateBoardInit();

                    MainGameMgr.InitCurrencyIOMgr();
                    MainGameMgr.UpdateDeltaCurrencyEnabled = true;
                    MainGameMgr.HintEnabled = true;
                    MainGameMgr.ForceHDDConnectionHint = true;

                    IsCustomButtonText = true;
                    CustomButtonText = "我试试";
                    break;
                case 12:
                    CustomButtonText = "我好了";
                    _hintAText.enabled = true;
                    //_hintBText.enabled = true;
                    _MainPanel.enabled = false;
                    break;
                case 13:
                    IsCustomButtonText = false;
                    _hintAText.enabled = false;
                    //_hintBText.enabled = false;
                    _MainPanel.enabled = true;
                    //MainGameMgr.ForceHDDConnectionHint = false;
                    IsCustomButtonText = true;
                    CustomButtonText = "我试试";
                    break;
                case 16:
                    MainGameMgr.ForceHDDConnectionHint = false;
                    MainGameMgr.ForceServerConnectionHint = true;
                    MainGameMgr.GameBoard.ForceChangeUnitCoreType(this);
                    MainGameMgr.GameBoard.UpdateBoardAnimation();

                    CustomButtonText = "我好了";
                    _hintAText.enabled = true;
                    //_hintBText.enabled = true;
                    _MainPanel.enabled = false;
                    break;
                case 17:
                    IsCustomButtonText = false;
                    _hintAText.enabled = false;
                    //_hintBText.enabled = false;
                    _MainPanel.enabled = true;
                    break;
            }*/
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