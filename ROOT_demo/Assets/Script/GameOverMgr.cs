using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class GameOverMgr : MonoBehaviour
    {
        public void GameRestart()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(StaticName.SCENE_ID_GAMEPLAY);
        }
    }
}