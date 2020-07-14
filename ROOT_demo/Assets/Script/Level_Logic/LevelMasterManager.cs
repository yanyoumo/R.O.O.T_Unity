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
    //这个是足球场的后勤和人事部门，告诉它需要来一场如何的比赛。
    //叫裁判进来，联系球员这些动态Asset，设置好球门这些静态Asset，让后就全权交给裁判了
    //现在就是裁判确定游戏结束后，是直接处理游戏结束的场景，还是让后勤部门处理？
    //目前认为后勤也没有“重启”关卡的概念，让后期帮忙从裁判往结束场景里面传递数据。
    public partial class LevelMasterManager : MonoBehaviour
    {

        private static LevelMasterManager _instance;
        public static LevelMasterManager Instance => _instance;
        
        //只是给co-routine用一下，这个master里面原则上不留变量。
        private LevelLogicSpawner _lls;
        private BaseLevelMgr _gameMgr;
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
                    _gameMgr.LevelAsset.ItemPriceRoot = go;
                }
            }

            _gameMgr.LevelAsset.DataScreen = dataScreen;
            _gameMgr.UpdateReference();
        }
        IEnumerator FindLlsAfterLoad()
        {
            while (_lls==null)
            {
                try
                {
                    SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));
                }
                catch (ArgumentException)
                {

                }
                _lls = FindObjectOfType<LevelLogicSpawner>();
                yield return 0;
            }
        }
        IEnumerator LoadGamePlay_Coroutine<T>(ScoreSet nextScoreSet,PerMoveData nextPerMoveData) where T : BaseLevelMgr
        {
            //目前这个框架下，所有的Logic Scene只能是一个，但是基于LLS就没有问题。
            SceneManager.LoadSceneAsync(StaticName.SCENE_ID_ADDTIVELOGIC, LoadSceneMode.Additive);
            yield return StartCoroutine(FindLlsAfterLoad());
            BaseLevelMgr tmp = _lls.SpawnLevelLogic<T>();
            _lls = null;
            _gameMgr = FindObjectOfType<T>();
            SceneManager.LoadSceneAsync(tmp.LEVEL_ART_SCENE_ID, LoadSceneMode.Additive);
            yield return LinkLevel();
            Debug.Assert(_gameMgr.CheckReference());
            Debug.Assert(!_gameMgr.Playing);
            _gameMgr.InitLevel(nextScoreSet, nextPerMoveData);
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
            _gameGlobalStatus = new GameGlobalStatus { CurrentGameStatus = GameStatus.Starting };
        }

        //原则上加载等功能只写这几个。(重载不算

        public void LoadLevelThenPlay<T>(ScoreSet nextScoreSet, PerMoveData nextPerMoveData) where T : BaseLevelMgr
        {
            if (_gameGlobalStatus.CurrentGameStatus != GameStatus.Playing)
            {
                _gameGlobalStatus.CurrentGameStatus = GameStatus.Playing;
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

        private GameOverMgr GOM;

        IEnumerator FindGOMAfterLoad()
        {
            while (GOM == null)
            {
                GOM = FindObjectOfType<GameOverMgr>();
                yield return 0;
            }
        }
        IEnumerator SendLastGameAssetsToGameOverMgr(GameAssets lastGameAssets)
        {
            yield return StartCoroutine(FindGOMAfterLoad());
            Debug.Assert(GOM!=null);
            GOM.LastGameAssets = lastGameAssets;
        }

        /// <summary>
        /// Level-logic通过LevelManager和GameOverScene取得联系。
        /// </summary>
        /// <param name="lastGameAssets">已经结束关卡的参数和最终的状态。</param>
        public void LevelFinished(GameAssets lastGameAssets)
        {
            if (_gameGlobalStatus.CurrentGameStatus != GameStatus.Ended)
            {
                _gameGlobalStatus.CurrentGameStatus = GameStatus.Ended;
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
                StartCoroutine(SendLastGameAssetsToGameOverMgr(lastGameAssets));
            }
        }

        private static GameGlobalStatus _gameGlobalStatus;
        public static GameGlobalStatus GetGameGlobalStatus()
        {
            return _gameGlobalStatus;
        }
    }
}