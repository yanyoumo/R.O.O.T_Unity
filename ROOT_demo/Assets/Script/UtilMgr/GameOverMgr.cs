using System;
using I2.Loc;
using ROOT.Consts;
using ROOT.SetupAsset;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ROOT
{
    public class GameOverAsset
    {
        public bool Succeed = false;
        public string SuccessTerm = "";
        public string FailedTerm = "";
        public int ValueInt = 0;
    }
    
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
            BackButton.onClick.AddListener(Back);
            //这里控制游戏结束部分的代码。
            if (_lastGameAssets.Owner.UseTutorialVer)
            {
                bool tutorialCompleted = _lastGameAssets.TutorialCompleted.Value;
                EndingTitleLocalize.Term = ScriptTerms.TutorialSectionOver;
                if (tutorialCompleted)
                {
                    PlayerPrefs.SetInt(_lastGameAssets.ActionAsset.TitleTerm, 0);
                    PlayerPrefs.Save();
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
                EndingTitleLocalize.Term = ScriptTerms.GameOver;
                OtherButtonLocalize.Term = ScriptTerms.Restart;
                OtherButton.onClick.AddListener(GameRestart);
                OtherButton.interactable = _lastGameAssets.GameOverAsset.Succeed;
                EndingMessageParam.SetParameterValue("VALUE", _lastGameAssets.GameOverAsset.ValueInt.ToString());

                EndingMessageLocalize.Term = _lastGameAssets.GameOverAsset.Succeed?_lastGameAssets.GameOverAsset.SuccessTerm:_lastGameAssets.GameOverAsset.FailedTerm;
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