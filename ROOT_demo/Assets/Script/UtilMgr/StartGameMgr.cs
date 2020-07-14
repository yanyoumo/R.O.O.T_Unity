using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public class StartGameMgr : MonoBehaviour
    {
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
            LevelMasterManager.Instance.LoadLevelThenPlay<DefaultLevelMgr>();
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_START));
        }

        public void TutorialStart()
        {
            SceneManager.LoadSceneAsync(StaticName.SCENE_ID_TUTORIAL, LoadSceneMode.Additive);
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_START));
        }
    }
}