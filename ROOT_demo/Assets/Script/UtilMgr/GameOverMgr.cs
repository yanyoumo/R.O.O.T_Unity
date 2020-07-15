using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        public TextMeshProUGUI EndingTitle;
        public TextMeshProUGUI EndingMessage;

        public Button BackButton;
        public Text BackButtonText;
        public Button OtherButton;
        public Text OtherButtonText;

        void GameOverSceneLoaded(Scene scene,LoadSceneMode loadSceneMode)
        {
            if (scene.buildIndex==StaticName.SCENE_ID_GAMEOVER)
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_GAMEOVER));
            }
        }

        void UpdateUIContent()
        {
            if (_lastGameAssets.LevelLogicType.BaseType == typeof(BaseTutorialMgr))
            {
                BackButton.onClick.AddListener(Back);
                OtherButton.onClick.AddListener(NextTutorial);
                OtherButtonText.text = "Next";
                OtherButton.interactable = false;//TODO 到时候改成能去下一关。

                EndingTitle.text = "SECTION OVER";
                EndingMessage.text = "本节教程结束~";
            }
            else
            {
                BackButton.onClick.AddListener(Back);
                OtherButton.onClick.AddListener(GameRestart);
                OtherButtonText.text = "Restart";

                EndingTitle.text = "GAME OVER";
                float endingIncome = _lastGameAssets.GameStateMgr.GetCurrency() - _lastGameAssets.GameStateMgr.StartingMoney;

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

        public void NextTutorial()
        {
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_GAMEOVER);
        }

        public void GameRestart()
        {
            LevelMasterManager.Instance.LoadLevelThenPlay(_lastGameAssets.LevelLogicType, new ScoreSet(),new PerMoveData());
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_GAMEOVER);
        }

        public void Back()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_GAMEOVER) != SceneManager.GetSceneAt(i))
                {
                    SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i));
                }
            }
            SceneManager.LoadScene(StaticName.SCENE_ID_START, LoadSceneMode.Single);
        }
    }
}