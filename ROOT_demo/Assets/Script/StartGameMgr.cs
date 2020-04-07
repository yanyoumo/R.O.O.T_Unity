using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class StartGameMgr : MonoBehaviour
    {
        public void GameStart()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(StaticName.SCENE_ID_GAMEPLAY);
        }
    }
}