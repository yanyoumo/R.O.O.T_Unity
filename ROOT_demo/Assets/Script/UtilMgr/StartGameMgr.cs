using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public class StartGameMgr : MonoBehaviour
    {
        public static SupportedDevice DetectedDevice { get; private set; } = SupportedDevice.StandAlone;

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
#if UNITY_EDITOR
            float ratio = Screen.width / (float)Screen.height;
            float EPSILON = 1e-2f;
            if (Math.Abs(ratio - (16.0f / 9.0f)) < EPSILON)
            {
                DetectedDevice = SupportedDevice.StandAlone;
            }

            if (Math.Abs(ratio - (1792.0f / 828.0f)) < EPSILON)
            {
                DetectedDevice = SupportedDevice.iPXR;
            }

            if (Math.Abs(ratio - (2732.0f / 2048.0f)) < EPSILON)
            {
                DetectedDevice = SupportedDevice.iPadPro4;
            }
#elif UNITY_STANDALONE_WIN
            detectedDevice = SupportedDevice.StandAlone;
#elif UNITY_IOS
            if (UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhoneXR)
            {
                detectedDevice = SupportedDevice.iPXR;
            }
            else if (UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPadUnknown)
            {
                detectedDevice = SupportedDevice.iPadPro4;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
#else
            throw new ArgumentOutOfRangeException();
#endif
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