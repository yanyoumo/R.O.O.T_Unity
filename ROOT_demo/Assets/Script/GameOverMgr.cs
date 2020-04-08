using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class GameOverMgr : MonoBehaviour
    {
        void Awake()
        {
            Debug.Assert(GameGlobalStatus.CurrentGameStatus == GameStatus.Ended, "Game Status not matching");
            //TODO display last Game status.
        }

        public void GameRestart()
        {
            GameGlobalStatus.CurrentGameStatus = GameStatus.Playing;
            UnityEngine.SceneManagement.SceneManager.LoadScene(StaticName.SCENE_ID_GAMEPLAY);
        }
    }
}