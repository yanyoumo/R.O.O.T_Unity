using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace ROOT
{
    /// <summary>
    /// 这个是足球场的后勤和人事部门，告诉它需要来一场如何的比赛。
    /// 叫裁判进来，联系球员这些动态Asset，设置好球门这些静态Asset，让后就全权交给裁判了
    /// 现在就是裁判确定游戏结束后，是直接处理游戏结束的场景，还是让后勤部门处理？
    /// 目前认为后勤也没有“重启”关卡的概念，让后期帮忙从裁判往结束场景里面传递数据。
    /// </summary>
    ///
    public sealed class LevelMasterManager : MonoBehaviour
    {
        private static LevelMasterManager _instance;
        public static LevelMasterManager Instance => _instance;
        
        //只是给co-routine用一下，这个master里面原则上不留变量。
        private LevelLogicSpawner _lls;
        private FSMLevelLogic _gameLogic;
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
            _gameGlobalStatus = new GameGlobalStatus { CurrentGameStatus = GameStatus.Starting };
        }
        IEnumerator FindLlsAfterLoad(AsyncOperation aOP)
        {
            while (!aOP.isDone)
            {
                yield return 0;
            }
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));
            _lls = FindObjectOfType<LevelLogicSpawner>();
        }

        IEnumerator LoadGamePlay_Coroutine(LevelActionAsset actionAsset)
        {
            //目前这个框架下，所有的Logic Scene只能是一个，但是基于LLS就没有问题。
            AsyncOperation loadSceneAsync = SceneManager.LoadSceneAsync(StaticName.SCENE_ID_ADDTIVELOGIC, LoadSceneMode.Additive);
            yield return StartCoroutine(FindLlsAfterLoad(loadSceneAsync));
            _gameLogic = _lls.SpawnLevelLogic(actionAsset.LevelLogic);//这里Level-logic的Awake就进行初始化了。主要是LevelLogic的实例去拿CoreLogic场景里面的东西。
            Debug.Log(_gameLogic.LevelAsset);
            _gameLogic.LevelAsset.ActionAsset = actionAsset;
            _lls = null;
            loadSceneAsync = SceneManager.LoadSceneAsync(_gameLogic.LEVEL_ART_SCENE_ID, LoadSceneMode.Additive);
            yield return _gameLogic.UpdateArtLevelReference(loadSceneAsync);//这里是第二次的LinkLevel。匹配ArtScene里面的引用//和第三次的Init里面的UpdateReference。通过根引用去查找其他引用。
#if DEBUG
            Debug.Assert(_gameLogic.CheckReference());
            Debug.Assert(!_gameLogic.Playing);
#endif
            _gameLogic.InitLevel();//最后的初始化和启动游戏，运行此之前，需要的引用必须齐整。
        }

        public void LoadLevelThenPlay(LevelActionAsset actionAsset,AdditionalGameSetup _additionalGameSetup=null)
        {
            //TODO additionalGameSetup还没有实际地接进去、主要是方便UI那边先接着。
            if (_gameGlobalStatus.CurrentGameStatus == GameStatus.Playing) return;
            _gameGlobalStatus.CurrentGameStatus = GameStatus.Playing;
            if (_additionalGameSetup != null)
            {
                actionAsset.AdditionalGameSetup = _additionalGameSetup;
            }
            StartCoroutine(LoadGamePlay_Coroutine(actionAsset));
        }

        public void LoadCareerSetup(int buttonId)
        {
            CareerSetupManger.sceneId = buttonId;//RISK 从理论上讲、如果那个的加载用个Coroutine、这个数据就能直接传进去了。但是先这样吧。
            SceneManager.LoadSceneAsync(StaticName.SCENE_ID_CAREERSETUP, LoadSceneMode.Additive);
        }

        private GameOverMgr _gameOverMgr;

        private IEnumerator FindGameOverMgrAfterLoad()
        {
            while (_gameOverMgr == null)
            {
                _gameOverMgr = FindObjectOfType<GameOverMgr>();
                yield return 0;
            }
        }

        private IEnumerator SendLastGameAssetsToGameOverMgr(GameAssets lastGameAssets)
        {
            yield return StartCoroutine(FindGameOverMgrAfterLoad());
            System.Diagnostics.Debug.Assert(_gameOverMgr != null, nameof(_gameOverMgr) + " != null");
            _gameOverMgr.LastGameAssets = lastGameAssets;
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