using System;
using System.Linq;
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

        //首先返回：要改成返回选择界面。重新开始就放在哪里。
            //GameOver界面其实很重要，主要是局间的游玩动力；现在实质机制上能展示的只有：解锁新关卡、这个只能尽量用了。
        void UpdateUIContent()
        {
            BackButton.onClick.AddListener(Back);
            if (_lastGameAssets.ActionAsset.DisplayedlevelType == LevelType.Tutorial)
            {
                Debug.Assert(_lastGameAssets.TutorialCompleted.HasValue);
                var tutorialCompleted = _lastGameAssets.TutorialCompleted.Value;
                EndingTitleLocalize.Term = ScriptTerms.TutorialSectionOver;
                
                if (tutorialCompleted)
                {
                    CompleteThisLevelAndUnlockFollowing(ref _lastGameAssets.ActionAsset);
                    EndingMessageLocalize.Term = ScriptTerms.EndingMessageTutorial;
                }
                else
                {
                    throw new NotImplementedException("教程的失败没有实质完成。");
                    PlayerPrefs.SetInt(_lastGameAssets.ActionAsset.TitleTerm, (int) LevelStatus.Played);
                    PlayerPrefs.Save();

                    OtherButton.interactable = false;
                    OtherButtonLocalize.Term = ScriptTerms.NextTutorialFailed;
                    EndingMessageLocalize.Term = ScriptTerms.EndingMessageTutorialFailed;
                }
            }
            else
            {
                if (_lastGameAssets.GameOverAsset.Succeed)
                {
                    CompleteThisLevelAndUnlockFollowing(ref _lastGameAssets.ActionAsset);
                }
                else
                {
                    PlayerThisLevelAndUnlockFollowing(ref _lastGameAssets.ActionAsset);
                }
                EndingTitleLocalize.Term = ScriptTerms.GameOver;
                EndingMessageParam.SetParameterValue("VALUE", _lastGameAssets.GameOverAsset.ValueInt.ToString());
                EndingMessageLocalize.Term = _lastGameAssets.GameOverAsset.Succeed?_lastGameAssets.GameOverAsset.SuccessTerm:_lastGameAssets.GameOverAsset.FailedTerm;
            }
            OtherButtonLocalize.Term = ScriptTerms.Restart;
            OtherButton.onClick.AddListener(GameRestart);
            OtherButton.interactable = _lastGameAssets.GameOverAsset.Succeed;
        }

        //TODO 甚至是不是把整个修改真的封装一下？
        //TODO RISK 这两个调整内容的还一定要搞一下别往回改、就是LevelStatus其实是有层级的。
        private void CompleteThisLevelAndUnlockFollowing(ref LevelActionAsset completedLevel)
        {
            PlayerPrefs.SetInt(completedLevel.TitleTerm, (int) LevelStatus.Passed);
            foreach (var s in completedLevel.UnlockingLevel.Where(lv => lv != null).Select(lv => lv.TitleTerm))
            {
                PlayerPrefs.SetInt(s, (int) LevelStatus.Unlocked);
            }

            PlayerPrefs.Save();
        }

        //TODO RISK 这两个调整内容的还一定要搞一下别往回改、就是LevelStatus其实是有层级的。
        private void PlayerThisLevelAndUnlockFollowing(ref LevelActionAsset completedLevel)
        {
            PlayerPrefs.SetInt(completedLevel.TitleTerm, (int) LevelStatus.Played);
            PlayerPrefs.Save();
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

        private void GameRestart()
        {
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_GAMEOVER);
        }

        private void Back()
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_GAMEOVER) != SceneManager.GetSceneAt(i))
                {
                    SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i));
                }
            }
            StartGameMgr.LoadThenActiveGameCoreScene();
            SceneManager.LoadSceneAsync(StaticName.SCENE_ID_BST_CAREER, LoadSceneMode.Additive).completed += a =>
            {
                //因为Unity不允许卸载最后一个场景、所以这边就要听BST场景异步加载完成后再卸载自己。
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_GAMEOVER));
            };
        }
    }
}