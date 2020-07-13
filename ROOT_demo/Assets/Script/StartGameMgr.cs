using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public class StartGameMgr : MonoBehaviour
    {
        //TODO 现在的问题还是LoadScene这个流程没有办法传数据。
        //TODO 实在不行就游戏开始就都建出来，然后创建Tutorial的时候换手。
        //public static event RootEVENT.GameMajorEvent RequestGameStart;
        //private GameMasterManager gameMasterManager;
        private bool gameMasterManagerReady = false;


        void Awake()
        {
            //游戏中理论最早点。
            SceneManager.sceneLoaded += GamePlaySceneLoaded;
        }

        void Start()
        {
            SceneManager.LoadSceneAsync(StaticName.SCENE_ID_LEVELMASTER, LoadSceneMode.Additive);
        }

        void GamePlaySceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.buildIndex == StaticName.SCENE_ID_LEVELMASTER)
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_LEVELMASTER));
            }
        }

        public void GameStart()
        {
            //RequestGameStart?.Invoke();
            //GameMasterManager.Instance.LoadLevelThenPlay();
            GameMasterManager.Instance.LoadLevelThenPlay<ShortEndingLevelMgr>();
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_START));
        }

        void Update()
        {
            /*if (gameMasterManager != null)
            {
                if (!gameMasterManagerReady)
                {
                    gameMasterManagerReady = true;
                    //RequestGameStart += gameMasterManager.LoadGamePlay_Full;
                    //RequestGameStart += gameMasterManager.LoadLevelThenPlay;
                }
            }
            else
            {
                gameMasterManager = FindObjectOfType<GameMasterManager>();
            }*/
        }

        public void TutorialStart()
        {
            //RequestGameStart -= gameMasterManager.LoadGamePlay_Full;
            //RequestGameStart -= gameMasterManager.LoadLevelThenPlay;
            SceneManager.LoadSceneAsync(StaticName.SCENE_ID_TUTORIAL, LoadSceneMode.Additive);
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_START));
        }
    }
}