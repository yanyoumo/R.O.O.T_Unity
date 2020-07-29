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
        TutorialActionBasicControl,
        TutorialActionBasicControlTouch,
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
        #region 关卡切换
        //TODO 所有基于Type都要换掉，换成基于LevelType
        [Obsolete]
        public Type LevelEnumToType(LevelType levelLogicType)
        {
            switch (levelLogicType)
            {
                case LevelType.TutorialActionBasicControl:
                    return typeof(TutorialLevelBasicControlMgr);
                case LevelType.TutorialActionBasicControlTouch:
                    return typeof(TutorialLevelBasicControlMgr);
                case LevelType.TutorialActionSignalBasic:
                    return typeof(TutorialSignalBasicMgr);
                case LevelType.TutorialActionGoalAndCycle:
                    return typeof(TutorialGoalAndCycleMgr);
                case LevelType.TutorialActionShop:
                    return typeof(TutorialShopMgr);
                case LevelType.TutorialActionDestroyer:
                    return typeof(TutorialDestroyerMgr);
                default:
                    throw new ArgumentOutOfRangeException(nameof(levelLogicType), levelLogicType, null);
            }
        }

        public void LoadLevelThenPlay(LevelType levelLogicType)
        {
            LoadLevelThenPlay(LevelEnumToType(levelLogicType), new ScoreSet(), new PerMoveData());
        }

        public void LoadLevelThenPlay(Type levelLogicType)
        {
            LoadLevelThenPlay(levelLogicType, new ScoreSet(), new PerMoveData());
        }

        public void LoadNextTutorialLevelThenPlay(Type levelLogicType)
        {
            LoadLevelThenPlay(GetNextTutorialLevel(levelLogicType), new ScoreSet(), new PerMoveData());
        }

        public static LevelType GetNextTutorialLevel(LevelType levelLogicType)
        {
            switch (levelLogicType)
            {
                case LevelType.TutorialActionBasicControl:
                    return LevelType.TutorialActionBasicControlTouch;
                case LevelType.TutorialActionBasicControlTouch:
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
        }

        [Obsolete]
        public static Type GetNextTutorialLevel(Type levelLogicType)
        {
            if (levelLogicType == typeof(TutorialLevelBasicControlMgr))
            {
                return typeof(TutorialSignalBasicMgr);
            }
            if (levelLogicType == typeof(TutorialSignalBasicMgr))
            {
                return typeof(TutorialGoalAndCycleMgr);
            }
            if (levelLogicType == typeof(TutorialGoalAndCycleMgr))
            {
                return typeof(TutorialShopMgr);
            }
            if (levelLogicType == typeof(TutorialShopMgr))
            {
                return typeof(TutorialDestroyerMgr);
            }
            throw new ArgumentOutOfRangeException();
        }

        [Obsolete]
        public void LoadLevelThenPlay(Type levelLogicType, ScoreSet nextScoreSet, PerMoveData nextPerMoveData)
        {
            //这里是一个动态到静态的转换。
            if (levelLogicType == typeof(DefaultLevelMgr))
            {
                LoadLevelThenPlay<DefaultLevelMgr>(nextScoreSet, nextPerMoveData);
                return;
            }
            if (levelLogicType == typeof(ShortEndingLevelMgr))
            {
                LoadLevelThenPlay<ShortEndingLevelMgr>(nextScoreSet, nextPerMoveData);
                return;
            }
            if (levelLogicType == typeof(TutorialLevelBasicControlMgr))
            {
                LoadLevelThenPlay<TutorialLevelBasicControlMgr>(nextScoreSet, nextPerMoveData);
                return;
            }
            if (levelLogicType == typeof(TutorialSignalBasicMgr))
            {
                LoadLevelThenPlay<TutorialSignalBasicMgr>(nextScoreSet, nextPerMoveData);
                return;
            }
            if (levelLogicType == typeof(TutorialGoalAndCycleMgr))
            {
                LoadLevelThenPlay<TutorialGoalAndCycleMgr>(nextScoreSet, nextPerMoveData);
                return;
            }
            if (levelLogicType == typeof(TutorialShopMgr))
            {
                LoadLevelThenPlay<TutorialShopMgr>(nextScoreSet, nextPerMoveData);
                return;
            }
            if (levelLogicType == typeof(TutorialDestroyerMgr))
            {
                LoadLevelThenPlay<TutorialDestroyerMgr>(nextScoreSet, nextPerMoveData);
                return;
            }
            throw new NotImplementedException();
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

            Random.InitState(Mathf.FloorToInt(Time.realtimeSinceStartup));
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
        IEnumerator LoadGamePlay_Coroutine<T>(ScoreSet nextScoreSet,PerMoveData nextPerMoveData) where T : BaseLevelMgr
        {
            //目前这个框架下，所有的Logic Scene只能是一个，但是基于LLS就没有问题。
            AsyncOperation loadSceneAsync=SceneManager.LoadSceneAsync(StaticName.SCENE_ID_ADDTIVELOGIC, LoadSceneMode.Additive);
            yield return StartCoroutine(FindLlsAfterLoad(loadSceneAsync));
            _gameMgr = _lls.SpawnLevelLogic<T>();//这里Level-logic的Awake就进行初始化了。主要是LevelLogic的实例去拿CoreLogic场景里面的东西。
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