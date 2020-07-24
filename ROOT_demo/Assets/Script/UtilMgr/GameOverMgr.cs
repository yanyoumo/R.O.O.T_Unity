using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
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
        //public TextMeshProUGUI EndingTitle;
        public Localize EndingTitleLocalize;
        public Localize EndingMessageLocalize;
        public LocalizationParamsManager EndingMessageParam;
        //public TextMeshProUGUI EndingMessage;

        public Button BackButton;
        //public Text BackButtonText;
        public Button OtherButton;
        public Localize OtherButtonLocalize;
        private Type nextLevelType;
        //public Text OtherButtonText;

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
                try
                {
                    nextLevelType = LevelMasterManager.GetNextTutorialLevel(_lastGameAssets.LevelLogicType);
                }
                catch (ArgumentOutOfRangeException)
                {
                    OtherButton.interactable = false;//没有下一关了。
                    nextLevelType = null;
                }
                BackButton.onClick.AddListener(Back);
                OtherButton.onClick.AddListener(NextTutorial);
                OtherButtonLocalize.Term = ScriptTerms.NextTutorial;

                EndingTitleLocalize.Term = ScriptTerms.TutorialSectionOver;
                EndingMessageLocalize.Term = ScriptTerms.EndingMessageTutorial;
            }
            else
            {
                BackButton.onClick.AddListener(Back);
                OtherButton.onClick.AddListener(GameRestart);
                OtherButtonLocalize.Term = ScriptTerms.Restart;
                EndingTitleLocalize.Term = ScriptTerms.GameOver;
                float endingIncome = _lastGameAssets.GameStateMgr.GetCurrency() - _lastGameAssets.GameStateMgr.StartingMoney;

                if (_lastGameAssets.GameStateMgr.GetGameTime() <= 0)
                {
                    int deltaMoney = Mathf.FloorToInt(Mathf.Abs(endingIncome));
                    EndingMessageParam.SetParameterValue("VALUE", deltaMoney.ToString());
                    EndingMessageLocalize.Term = endingIncome >= 0 ? ScriptTerms.EndingMessageNormal_EarnedMoney : ScriptTerms.EndingMessageNormal_NoEarnedMoney;
                }
                else
                {
                    EndingMessageLocalize.Term = ScriptTerms.EndingMessageNormal_NoMoney;
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
            LevelMasterManager.Instance.LoadNextTutorialLevelThenPlay(_lastGameAssets.LevelLogicType);
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