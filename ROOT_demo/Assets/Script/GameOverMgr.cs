using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    //TODO 这货要变成由GameMasterManager直接管理的一环。
    public class GameOverMgr : MonoBehaviour
    {
        public TextMeshProUGUI EndingMessage;
        //public static event RootEVENT.GameMajorEvent GameRequestSameRestart;
        public bool sceneNotActive;

        void GameOverSceneLoaded(Scene scene,LoadSceneMode loadSceneMode)
        {
            if (scene.buildIndex==StaticName.SCENE_ID_GAMEOVER)
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_GAMEOVER));
            }
        }

        void Awake()
        {
            SceneManager.sceneLoaded += GameOverSceneLoaded;
            GameGlobalStatus currentStatus = GameMasterManager.getGameGlobalStatus();
            Debug.Assert(currentStatus.CurrentGameStatus == GameStatus.Ended, "Game Status not matching");
            if (currentStatus.lastEndingTime <= 0)
            {
                float deltaMoney = Mathf.Abs(currentStatus.lastEndingIncome);
                if (currentStatus.lastEndingIncome>=0)
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

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= GameOverSceneLoaded;
        }

        public void GameRestart()
        {
            //TODO 这里需要拿到之前玩的关卡类型。这里怎么处理？
            GameMasterManager.Instance.RestartLevel<DefaultLevelMgr>();
        }
    }
}