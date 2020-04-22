using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class StartGameMgr : MonoBehaviour
    {
        void Awake()
        {
            //游戏中理论最早点。
            GameGlobalStatus.CurrentGameStatus = GameStatus.Starting;
            GameGlobalStatus.lastEndingIncome = -1;
            GameGlobalStatus.lastEndingTime = -1;
        }

        public void GameStart()
        {
            GameGlobalStatus.CurrentGameStatus = GameStatus.Playing;
            UnityEngine.SceneManagement.SceneManager.LoadScene(StaticName.SCENE_ID_GAMEPLAY);
        }

        public void TutorialStart()
        {
            GameGlobalStatus.CurrentGameStatus = GameStatus.Tutorial;
            UnityEngine.SceneManagement.SceneManager.LoadScene(StaticName.SCENE_ID_GAMEPLAY);
        }
    }
}