using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace ROOT
{
    //TutorialManager全权由这个东西管理，TutorialMasterManager只传一个ActionBase进来。
    //这里负责把Visual和TutorialManager的引用建立起来。然后就TutorialManager和GameManager接手了。

    //现在这一套也要完全重写，这里应该不再区分一般Level还是Tutorial了。
    //coroutine还真有可能在这儿用。
    public class GameMasterManager : MonoBehaviour
    {

        private static GameMasterManager _instance;
        public static GameMasterManager Instance => _instance;
        
        //只是给co-routine用一下，这个master里面原则上不留变量。
        private LevelLogicSpawner lls;
        private BaseLevelMgr gameMgr;
        IEnumerator LinkLevel()
        {
            while (true)
            {
                yield return 0;
                try
                {
                    SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVEVISUAL));
                }
                catch (ArgumentException)
                {
                    continue;
                }
                break;
            }
            yield return 0;
            
            var tmpO = FindObjectsOfType<GameObject>();
            var dataScreen = FindObjectOfType<DataScreen>();

            foreach (var go in tmpO)
            {
                if (go.name == "PlayUI")
                {
                    gameMgr.LevelAsset.ItemPriceRoot = go;
                }
            }

            gameMgr.LevelAsset.DataScreen = dataScreen;
            gameMgr.UpdateReference();
        }
        IEnumerator FindLLSAfterLoad()
        {
            while (lls==null)
            {
                try
                {
                    SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));
                }
                catch (ArgumentException)
                {

                }
                lls = FindObjectOfType<LevelLogicSpawner>();
                yield return 0;
            }
        }
        IEnumerator LoadGamePlay_Coroutine<T>(ScoreSet nextScoreSet,PerMoveData nextPerMoveData) where T : BaseLevelMgr
        {
            //var tmp = new T();
            //目前这个框架下，所有的Logic Scene只能是一个，但是基于LLS就没有问题。
            SceneManager.LoadSceneAsync(StaticName.SCENE_ID_ADDTIVELOGIC, LoadSceneMode.Additive);
            yield return StartCoroutine(FindLLSAfterLoad());
            BaseLevelMgr tmp = lls.SpawnLevelLogic<T>();
            lls = null;
            gameMgr = FindObjectOfType<T>();
            SceneManager.LoadSceneAsync(tmp.LEVEL_ART_SCENE_ID, LoadSceneMode.Additive);
            yield return LinkLevel();
            Debug.Assert(gameMgr.CheckReference());
            Debug.Assert(!gameMgr.Playing);
            //这里是现有框架和之前框架冲突的地方。
            gameMgr.InitLevel(nextScoreSet, nextPerMoveData);
        }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }

            Random.InitState(Mathf.FloorToInt(Time.realtimeSinceStartup));
            //先写在这儿。
            gameGlobalStatus = new GameGlobalStatus { CurrentGameStatus = GameStatus.Starting };
        }

        //原则上加载等功能只写这几个。(重载不算
        public void LoadLevelThenPlay<T>(ScoreSet nextScoreSet, PerMoveData nextPerMoveData) where T : BaseLevelMgr
        {
            if (gameGlobalStatus.CurrentGameStatus != GameStatus.Playing)
            {
                gameGlobalStatus.CurrentGameStatus = GameStatus.Playing;
                StartCoroutine(LoadGamePlay_Coroutine<T>(nextScoreSet, nextPerMoveData));
            }
        }
        public void LoadLevelThenPlay<T>() where T : BaseLevelMgr
        {
            LoadLevelThenPlay<T>(new ScoreSet(), new PerMoveData());
        }
        public void LoadLevelThenPlay()
        {
            LoadLevelThenPlay<DefaultLevelMgr>(new ScoreSet(), new PerMoveData());
        }

        public void LevelFinished()
        {
            if (gameGlobalStatus.CurrentGameStatus != GameStatus.Ended)
            {
                gameGlobalStatus.CurrentGameStatus = GameStatus.Ended;
                SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_LEVELMASTER));
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    //将除了LEVELMASTER场景的遗留场景都干掉。
                    if (SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_LEVELMASTER)!= SceneManager.GetSceneAt(i))
                    {
                        SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i));
                    }
                }
                SceneManager.LoadSceneAsync(StaticName.SCENE_ID_GAMEOVER, LoadSceneMode.Additive);
            }
        }

        public void RestartLevel<T>() where T : BaseLevelMgr
        {
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_GAMEOVER);//在哪里Unload也要调整一下。
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_LEVELMASTER));
            //SetUpNextGameParam();
            LoadLevelThenPlay<T>();
        }

        /*private ScoreSet NextGameScoreSet = new ScoreSet(1000.0f, 100);
        private PerMoveData NextGamePerMoveData;
        private Type NextGameState;
        private TutorialActionBase NextActionBase;
        private BaseLevelMgr gameMgr;
        
        private TutorialMgr tutorialMgr;*/

        //把这个挪出去。
        private static GameGlobalStatus gameGlobalStatus;
        public static GameGlobalStatus getGameGlobalStatus()
        {
            return gameGlobalStatus;
        }

        public static void UpdateGameGlobalStatuslastEndingIncome(float lastEndingIncome)
        {
            gameGlobalStatus.lastEndingIncome = lastEndingIncome;
        }
        public static void UpdateGameGlobalStatuslastEndingTime(float lastEndingTime)
        {
            gameGlobalStatus.lastEndingTime = lastEndingTime;
        }

        /*void SetUpNextGameParam(ScoreSet scoreSet=null,PerMoveData _perMoveData=new PerMoveData())
        {
            if (scoreSet != null)
            {
                NextGameScoreSet = scoreSet;
            }
            else
            {
                NextGameScoreSet = new ScoreSet(1000.0f, 60);
            }
            NextGamePerMoveData = _perMoveData;
        }*/
    }
}