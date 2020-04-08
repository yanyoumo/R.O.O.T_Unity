using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace ROOT
{
    public class TutorialMgr : MonoBehaviour
    {
        private Text _buttonText;
        private Canvas _tutorialCanvas;
        private Text _mainText;
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

        private bool IsCustomButtonText = false;
        private string CustomButtonText = "";

        // Start is called before the first frame update
        void Awake()
        {
            _tutorialContent = new[]
            {
                "你好，欢迎来到R.O.O.T.教程。",
                "这是一个基于棋盘的模拟经营游戏。",
                "这个是你的光标，使用键盘的【方向键】控制移动，先来试试吧。",
                "好的，下面是这个游戏中最重要的元素，我们称为单位。",
                "将光标移动上去后，按住【空格】键同时使用方向键，就可以拖动这个单位。",
                "在光标选中后单位后，点按【左Shift】可以进行旋转。",
                "来，再给你几个单位，随便试试先，习惯一下操作。",
                "",
                "好的，教程继续。"
            };
        }

        void Start()
        {
            _tutorialContentMax = _tutorialContent.Length;
            _tutorialCanvas = MainGameMgr.TutorialUI;
            Button[] tmpB = _tutorialCanvas.transform.gameObject.GetComponentsInChildren<Button>();
            Text[] tmpT = _tutorialCanvas.transform.gameObject.GetComponentsInChildren<Text>();
            Image[] tmpI = _tutorialCanvas.transform.gameObject.GetComponentsInChildren<Image>();
            foreach (var text in tmpT)
            {
                if (text.name == MainTextName)
                {
                    _mainText = text;
                }
            }

            foreach (var image in tmpI)
            {
                if (image.name == "FocusPanel")
                {
                    _focusPanel = image;
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
                    _buttonText = button.GetComponentInChildren<Text>();
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
            switch (_tutorialContentCounter)
            {
                case 2:
                    MainGameMgr.InitCursor(new Vector2Int(2, 3));
                    MainGameMgr.InputEnabled = true;
                    MainGameMgr.CursorEnabled = true;
                    break;
                case 3:
                    SideType[] sidesA =
                    {
                        SideType.PCB, SideType.ParallelConnector, SideType.ParallelConnector, SideType.ParallelConnector
                    };
                    MainGameMgr.GameBoard.Units.Add(new Vector2Int(2, 2),
                        MainGameMgr.GameBoard.InitUnit(new Vector2Int(2, 2), CoreType.Processor, Utils.Shuffle(sidesA)));
                    MainGameMgr.GameBoard.UpdateBoard();
                    MainGameMgr.InputEnabled = false;
                    _focusPanel.enabled = true;
                    break;
                case 4:
                    _focusPanel.enabled = false;
                    MainGameMgr.InputEnabled = true;
                    break;
                case 5:
                    MainGameMgr.RotateEnabled = true;
                    break;
                case 6:
                    SideType[] sidesB =
                    {
                        SideType.PCB, SideType.ParallelConnector, SideType.PCB, SideType.ParallelConnector
                    };
                    //TODO 这里需要变成读取然后随机拿两个空的位置。
                    MainGameMgr.GameBoard.Units.Add(new Vector2Int(0, 0),
                        MainGameMgr.GameBoard.InitUnit(new Vector2Int(0, 0), CoreType.Processor, Utils.Shuffle(sidesB)));
                    MainGameMgr.GameBoard.Units.Add(new Vector2Int(5, 5),
                        MainGameMgr.GameBoard.InitUnit(new Vector2Int(5, 5), CoreType.Processor, Utils.Shuffle(sidesB)));
                    MainGameMgr.GameBoard.UpdateBoard();
                    IsCustomButtonText = true;
                    CustomButtonText = "我试试";
                    break;
                case 7:
                    CustomButtonText = "我好了";
                    _MainPanel.enabled = false;
                    break;
                case 8:
                    IsCustomButtonText = false;
                    _MainPanel.enabled = true;
                    break;
            }
        }

        // Update is called once per frame
        void Update()
        {
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