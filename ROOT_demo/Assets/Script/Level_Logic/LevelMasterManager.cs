using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace ROOT
{
    public enum LevelType
    {
        NONE,
        PlayLevel,
        SimpleLevel,
        TutorialActionBasicControl,
        TutorialActionSignalBasic,
        TutorialActionGoalAndCycle,
        TutorialActionShop,
        TutorialActionDestroyer
    }
    /// <summary>
    /// 这个是足球场的后勤和人事部门，告诉它需要来一场如何的比赛。
    /// 叫裁判进来，联系球员这些动态Asset，设置好球门这些静态Asset，让后就全权交给裁判了
    /// 现在就是裁判确定游戏结束后，是直接处理游戏结束的场景，还是让后勤部门处理？
    /// 目前认为后勤也没有“重启”关卡的概念，让后期帮忙从裁判往结束场景里面传递数据。
    /// </summary>
    ///
    public sealed class LevelMasterManager : MonoBehaviour
    {
        public static bool IsTutorialLevel(LevelType levelType)
        {
            return !(levelType == LevelType.PlayLevel || levelType == LevelType.SimpleLevel);
        }

        #region 关卡切换
        public void LoadLevelThenPlay(LevelType levelLogicType,TutorialActionAsset actionAsset=null)
        {
            LoadLevelThenPlay(levelLogicType, new ScoreSet(), new PerMoveData(), actionAsset);
        }

        /*public static LevelType GetNextTutorialLevel(LevelType levelLogicType)
        {
            //TODO 这个当然是不行的。
            switch (levelLogicType)
            {
                case LevelType.TutorialActionBasicControl:
                    return LevelType.TutorialActionSignalBasic;
                case LevelType.TutorialActionSignalBasic:
                    return LevelType.TutorialActionGoalAndCycle;
                case LevelType.TutorialActionGoalAndCycle:
                    return LevelType.TutorialActionShop;
                case LevelType.TutorialActionShop:
                    return LevelType.TutorialActionDestroyer;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }*/

        public void LoadLevelThenPlay(LevelType levelLogicType, ScoreSet nextScoreSet, PerMoveData nextPerMoveData, TutorialActionAsset actionAsset = null)
        {
            switch (levelLogicType)
            {
                case LevelType.TutorialActionBasicControl:
                    LoadLevelThenPlay<TutorialLevelBasicControlMgr>(nextScoreSet, nextPerMoveData, actionAsset);
                    return;
                case LevelType.TutorialActionSignalBasic:
                    LoadLevelThenPlay<TutorialSignalBasicMgr>(nextScoreSet, nextPerMoveData, actionAsset);
                    return;
                case LevelType.TutorialActionGoalAndCycle:
                    LoadLevelThenPlay<TutorialGoalAndCycleMgr>(nextScoreSet, nextPerMoveData, actionAsset);
                    return;
                case LevelType.TutorialActionShop:
                    LoadLevelThenPlay<TutorialShopMgr>(nextScoreSet, nextPerMoveData, actionAsset);
                    return;
                case LevelType.TutorialActionDestroyer:
                    LoadLevelThenPlay<TutorialDestroyerMgr>(nextScoreSet, nextPerMoveData, actionAsset);
                    return;
                case LevelType.PlayLevel:
                    LoadLevelThenPlay<DefaultLevelMgr>(nextScoreSet, nextPerMoveData, actionAsset);
                    return;
                case LevelType.SimpleLevel:
                    LoadLevelThenPlay<ShortEndingLevelMgr>(nextScoreSet, nextPerMoveData, actionAsset);
                    return;
                case LevelType.NONE:
                default:
                    throw new ArgumentOutOfRangeException(nameof(levelLogicType), levelLogicType, null);
            }
        }

        #endregion

        private static LevelMasterManager _instance;
        public static LevelMasterManager Instance => _instance;
        
        //只是给co-routine用一下，这个master里面原则上不留变量。
        private LevelLogicSpawner _lls;
        private BaseLevelMgr _gameMgr;
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

            Random.InitState(Mathf.FloorToInt(Time.time));
            //先写在这儿。
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
        IEnumerator LoadGamePlay_Coroutine<T>(ScoreSet nextScoreSet,PerMoveData nextPerMoveData, TutorialActionAsset actionAsset) where T : BaseLevelMgr
        {
            //目前这个框架下，所有的Logic Scene只能是一个，但是基于LLS就没有问题。
            AsyncOperation loadSceneAsync=SceneManager.LoadSceneAsync(StaticName.SCENE_ID_ADDTIVELOGIC, LoadSceneMode.Additive);
            yield return StartCoroutine(FindLlsAfterLoad(loadSceneAsync));
            _gameMgr = _lls.SpawnLevelLogic<T>();//这里Level-logic的Awake就进行初始化了。主要是LevelLogic的实例去拿CoreLogic场景里面的东西。
            if (_gameMgr is BaseTutorialMgr mgr)
            {
                Debug.Assert(actionAsset!=null);
                mgr.TutorialActionAsset = actionAsset;
                mgr.LevelAsset.ActionAsset = actionAsset;
            }
            _lls = null;
            loadSceneAsync = SceneManager.LoadSceneAsync(_gameMgr.LEVEL_ART_SCENE_ID, LoadSceneMode.Additive);
            yield return _gameMgr.UpdateArtLevelReference(loadSceneAsync);//这里是第二次的LinkLevel。匹配ArtScene里面的引用//和第三次的Init里面的UpdateReference。通过根引用去查找其他引用。
#if DEBUG
            Debug.Assert(_gameMgr.CheckReference());
            Debug.Assert(!_gameMgr.Playing);
#endif
            _gameMgr.InitLevel(nextScoreSet, nextPerMoveData);//最后的初始化和启动游戏，运行此之前，需要的引用必须齐整。
        }
        //原则上加载等功能只写这几个。基于Type的在另一个partial里面(重载不算
        public void LoadLevelThenPlay<T>(ScoreSet nextScoreSet, PerMoveData nextPerMoveData, TutorialActionAsset actionAsset=null) where T : BaseLevelMgr
        {
            if (_gameGlobalStatus.CurrentGameStatus != GameStatus.Playing)
            {
                _gameGlobalStatus.CurrentGameStatus = GameStatus.Playing;
                StartCoroutine(LoadGamePlay_Coroutine<T>(nextScoreSet, nextPerMoveData, actionAsset));
            }
        }
        public void LoadLevelThenPlay<T>() where T : BaseLevelMgr
        {
            LoadLevelThenPlay<T>(new ScoreSet(), new PerMoveData());
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