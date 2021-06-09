using System;
using System.Linq;
using I2.Loc;
using ROOT.Consts;
using ROOT.LevelAccessMgr;
using ROOT.SetupAsset;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static I2.Loc.ScriptTerms;

namespace ROOT
{
    public class GameOverAsset
    {
        public bool ExternalEnding = false;
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

        public TextMeshProUGUI EndingMessageTMP;

        void GameOverSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.buildIndex == StaticName.SCENE_ID_GAMEOVER)
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_GAMEOVER));
            }
        }

        private int CompleteThisLevelAndUnlockFollowing(ref LevelActionAsset actionAsset)
        {
            var unlockingLevelTerms = actionAsset.UnlockingLevel
                .Where(lv => lv != null)
                .Select(lv => lv.TitleTerm)
                .Where(s => PlayerPrefsLevelMgr.GetLevelStatus(s) == LevelStatus.Locked)
                .ToArray();
            if (unlockingLevelTerms.Length > 0)
            {
                PlayerPrefsLevelMgr.CompleteThisLevelAndUnlockFollowing(actionAsset.TitleTerm, unlockingLevelTerms);
            }

            return unlockingLevelTerms.Length;
        }

        //首先返回：要改成返回选择界面。重新开始就放在哪里。
        //GameOver界面其实很重要，主要是局间的游玩动力；现在实质机制上能展示的只有：解锁新关卡、这个只能尽量用了。
        void UpdateUIContent()
        {
            BackButton.onClick.AddListener(Back);
            EndingMessageTMP.color = ColorLibManager.Instance.ColorLib.ROOT_UI_DEFAULT_BLACK;

            OtherButtonLocalize.Term = Restart;
            OtherButton.onClick.AddListener(GameRestart);
            OtherButton.interactable = true;

            if (_lastGameAssets.GameOverAsset.ExternalEnding)
            {
                PlayerPrefsLevelMgr.PlayedThisLevel(_lastGameAssets.ActionAsset.TitleTerm);
                EndingTitleLocalize.Term = GameOver;
                EndingMessageLocalize.Term = EndingMessageTutorial;
                return;
            }

            if (_lastGameAssets.ActionAsset.DisplayedlevelType == LevelType.Tutorial)
            {
                Debug.Assert(_lastGameAssets.TutorialCompleted.HasValue);
                var tutorialCompleted = _lastGameAssets.TutorialCompleted.Value;
                EndingTitleLocalize.Term = TutorialSectionOver;

                if (tutorialCompleted)
                {
                    var unlockedLevelCount = CompleteThisLevelAndUnlockFollowing(ref _lastGameAssets.ActionAsset);
                    EndingMessageLocalize.Term = unlockedLevelCount > 0 ? EndingMessageTutorial_Unlocked : EndingMessageTutorial;
                    if (unlockedLevelCount > 0)
                    {
                        EndingMessageTMP.color = ColorLibManager.Instance.ColorLib.ROOT_UI_HIGHLIGHTING_GREEN;
                    }
                    return;
                }

                throw new NotImplementedException("教程的失败没有实质完成。");
                PlayerPrefsLevelMgr.PlayedThisLevel(_lastGameAssets.ActionAsset.TitleTerm);
                OtherButton.interactable = false;
                OtherButtonLocalize.Term = NextTutorialFailed;
                EndingMessageLocalize.Term = EndingMessageTutorialFailed;
            }

            if (_lastGameAssets.GameOverAsset.Succeed)
            {
                CompleteThisLevelAndUnlockFollowing(ref _lastGameAssets.ActionAsset);
            }
            else
            {
                PlayerPrefsLevelMgr.PlayedThisLevel(_lastGameAssets.ActionAsset.TitleTerm);
            }

            EndingTitleLocalize.Term = GameOver;
            EndingMessageParam.SetParameterValue("VALUE", _lastGameAssets.GameOverAsset.ValueInt.ToString());
            EndingMessageLocalize.Term = _lastGameAssets.GameOverAsset.Succeed
                ? _lastGameAssets.GameOverAsset.SuccessTerm
                : _lastGameAssets.GameOverAsset.FailedTerm;
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
            LevelMasterManager.Instance.LoadCareerSetup(_lastGameAssets.ActionAsset).completed += a =>
            {
                SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_GAMEOVER);
            };
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