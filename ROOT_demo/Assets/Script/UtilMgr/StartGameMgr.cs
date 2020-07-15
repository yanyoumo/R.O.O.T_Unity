using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public class StartGameMgr : MonoBehaviour
    {
        IEnumerator LoadLevelMasterSceneAndSetActive()
        {
            SceneManager.LoadSceneAsync(StaticName.SCENE_ID_LEVELMASTER, LoadSceneMode.Additive);
            while (true)
            {
                yield return 0;
                try
                {
                    SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_LEVELMASTER));
                }
                catch (ArgumentException)
                {
                    continue;
                }
                break;
            }
        }

        void Awake()
        {
            Debug.Assert(SceneManager.sceneCount == 1, "More than one scene loaded");
            StartCoroutine(LoadLevelMasterSceneAndSetActive());
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