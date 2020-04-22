using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ROOT
{
    public class GameOverMgr : MonoBehaviour
    {
        public TextMeshProUGUI EndingMessage;

        void Awake()
        {
            Debug.Assert(GameGlobalStatus.CurrentGameStatus == GameStatus.Ended, "Game Status not matching");
            if (GameGlobalStatus.lastEndingTime <= 0)
            {
                float deltaMoney = Mathf.Abs(GameGlobalStatus.lastEndingIncome);
                if (GameGlobalStatus.lastEndingIncome>=0)
                {
                    EndingMessage.text = "时间到了，你赚了" + deltaMoney + "钱";
                }
                else
                {
                    EndingMessage.text = "时间到了，你赔了" + deltaMoney + "钱";
                }
            }
            else
            {
                EndingMessage.text = "你没钱了";
            }
        }

        public void GameRestart()
        {
            GameGlobalStatus.CurrentGameStatus = GameStatus.Playing;
            UnityEngine.SceneManagement.SceneManager.LoadScene(StaticName.SCENE_ID_GAMEPLAY);
        }
    }
}