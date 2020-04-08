using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace ROOT
{
    public class TutorialMgr : MonoBehaviour
    {
        public Text ButtonText;
        public Text MainText;

        private string[] TutorialContent = { };

        private float TutorialContentCounter = 0;
        private int TutorialContentMax = 0;

        public GameObject CursorTemplate;

        private GameObject _mCursor;

        // Start is called before the first frame update
        void Awake()
        {
            TutorialContent = new[]
            {
                "你好，欢迎来到R.O.O.T.教程。",
                "这是一个基于棋盘的模拟经营游戏。",
                "这个是你的光标，使用键盘的方向间控制移动。",
                "先来试试吧。",
                "测试AAA",
                "测试BBB",
                "测试CCC",
                "测试DDD",
                "测试EEE",
            };
        }

        void Start()
        {
            TutorialContentMax = TutorialContent.Length;
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
            TutorialContentCounter++;
        }

        // Update is called once per frame
        void Update()
        {
            ButtonText.text = "继续" + DotDotDot();
            MainText.text = TutorialContentCounter < TutorialContentMax
                ? TutorialContent[Mathf.FloorToInt(TutorialContentCounter)]
                : "教程结束";

            if (TutorialContentCounter == 2.0f)
            {
                _mCursor = Instantiate(CursorTemplate);
                _mCursor.GetComponent<Cursor>().board_position = new Vector2Int(2, 3);
                TutorialContentCounter += 0.1f;
            }
        }
    }
}