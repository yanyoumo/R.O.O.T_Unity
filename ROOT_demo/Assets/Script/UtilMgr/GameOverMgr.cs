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

        //可以把LevelAsset整建制传进来。
        private GameAssets _lastGameAssets;

        public GameAssets LastGameAssets
        {
            get => _lastGameAssets;
            set
            {
                _lastGameAssets = value;
                UpdateUIContent();
            }
        }
        public TextMeshProUGUI EndingMessage;
        //public static event RootEVENT.GameMajorEvent GameRequestSameRestart;

        void GameOverSceneLoaded(Scene scene,LoadSceneMode loadSceneMode)
        {
            if (scene.buildIndex==StaticName.SCENE_ID_GAMEOVER)
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_GAMEOVER));
            }
        }

        void UpdateUIContent()
        {

            float endingIncome =
                _lastGameAssets.GameStateMgr.GetCurrency() - _lastGameAssets.GameStateMgr.StartingMoney;

            if (_lastGameAssets.GameStateMgr.GetGameTime() <= 0)
            {
                float deltaMoney = Mathf.Abs(endingIncome);
                if (endingIncome >= 0)
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

        void Awake()
        {
            SceneManager.sceneLoaded += GameOverSceneLoaded;
            GameGlobalStatus currentStatus = LevelMasterManager.GetGameGlobalStatus();
            Debug.Assert(currentStatus.CurrentGameStatus == GameStatus.Ended, "Game Status not matching");
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= GameOverSceneLoaded;
        }

        public void GameRestart()
        {
            LevelMasterManager.Instance.LoadLevelThenPlay(_lastGameAssets.LevelLogicType, new ScoreSet(),new PerMoveData());
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_GAMEOVER);
        }
    }
}