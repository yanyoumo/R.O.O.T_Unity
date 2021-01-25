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
        public Localize EndingTitleLocalize;
        public Localize EndingMessageLocalize;
        public LocalizationParamsManager EndingMessageParam;

        public Button BackButton;
        public Button OtherButton;
        public Localize OtherButtonLocalize;

        void GameOverSceneLoaded(Scene scene,LoadSceneMode loadSceneMode)
        {
            if (scene.buildIndex==StaticName.SCENE_ID_GAMEOVER)
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_GAMEOVER));
            }
        }

        void UpdateUIContent()
        {
            if (_lastGameAssets.Owner.IsTutorialLevel)
            {
                System.Diagnostics.Debug.Assert(_lastGameAssets.TutorialCompleted != null, "_lastGameAssets.TutorialCompleted != null");
                bool tutorialCompleted = _lastGameAssets.TutorialCompleted.Value;
                BackButton.onClick.AddListener(Back);
                EndingTitleLocalize.Term = ScriptTerms.TutorialSectionOver;
                if (tutorialCompleted)
                {
                    if (LevelLib.Instance.GetNextTutorialActionAsset(_lastGameAssets.ActionAsset) == null)
                    {
                        OtherButton.interactable = false;//没有下一关了。
                    }
                    OtherButton.onClick.AddListener(NextTutorial);
                    OtherButtonLocalize.Term = ScriptTerms.NextTutorial;
                    EndingMessageLocalize.Term = ScriptTerms.EndingMessageTutorial;
                }
                else
                {
                    OtherButton.interactable = false;
                    OtherButtonLocalize.Term = ScriptTerms.NextTutorialFailed;
                    EndingMessageLocalize.Term = ScriptTerms.EndingMessageTutorialFailed;
                }
            }
            else
            {
                BackButton.onClick.AddListener(Back);
                OtherButton.interactable = false;//HACK 先关了
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
            LevelActionAsset nextLevelActionAsset = LevelLib.Instance.GetNextTutorialActionAsset(_lastGameAssets.ActionAsset);
            LevelMasterManager.Instance.LoadLevelThenPlay(nextLevelActionAsset);
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_GAMEOVER);
        }

        public void GameRestart()
        {
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_GAMEOVER);
        }

        public void Back()
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
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