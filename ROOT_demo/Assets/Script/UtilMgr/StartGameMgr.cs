using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.iOS;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace ROOT
{
    public enum SupportedScreenRatio
    {
        XGA, //4:3/iPadAir2/iPadPro4Gen
        HD, //16:9/StandAlone/iPhone7Plus/iPhone6
        AppleHD, //2.1645:1/iPhoneX/iPhoneXR/iPhone11Pro/iPhoneXSMax
    }

    public enum InputScheme
    {
        KeyboardMouse,
        TouchScreen,
    }

    public class StartGameMgr : MonoBehaviour
    {
        /*public enum SupportedDevice
        {
            StandAlone,//1920x1080-1.78-16:9
            iPhone6,//1334x750-1.78-16:9
            iPhone7Plus,//1920x1080-1.78-16:9
            iPhoneX,//2436x1125-2.165
            iPhoneXR,//1792x828-2.164
            iPhone11Pro,//2436x1125-2.165
            iPhoneXSMax,//2688x1242-2.164
            iPadAir2,//2048x1536-1.33-4:3
            iPadPro4Gen,//2732x2048-1.33-4:3
        }*/

        public LevelActionAssetLib LevelActionAssetLib;
        public LevelActionAssetLib LevelTouchActionAssetLib;
        public LevelActionAssetLib ClassicGameActionAssetLib;
        public LevelActionAssetLib CareerGameActionAssetLib;

        public SupportedScreenRatio PCSimulateDevice;
        public InputScheme EditorInputScheme;

        public static SupportedScreenRatio DetectedScreenRatio { get; private set; } = SupportedScreenRatio.HD;
        public static InputScheme DetectedInputScheme { get; private set; } = InputScheme.KeyboardMouse;

        public static bool UseTouchScreen => DetectedInputScheme == InputScheme.TouchScreen;

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
            Input.simulateMouseWithTouches = true;
            DetectedScreenRatio = PCSimulateDevice;
            DetectedInputScheme = EditorInputScheme;
#elif UNITY_STANDALONE_WIN
            DetectedScreenRatio = SupportedScreenRatio.HD;
            DetectedInputScheme = InputScheme.KeyboardMouse;
#elif UNITY_IOS
            {
                switch (UnityEngine.iOS.Device.generation)
                {
                    case DeviceGeneration.iPhone5S:
                    case DeviceGeneration.iPhone6:
                    case DeviceGeneration.iPhone6Plus:
                    case DeviceGeneration.iPhone6S:
                    case DeviceGeneration.iPhone6SPlus:
                    case DeviceGeneration.iPhoneSE1Gen:
                    case DeviceGeneration.iPhone7:
                    case DeviceGeneration.iPhone7Plus:
                    case DeviceGeneration.iPhone8:
                    case DeviceGeneration.iPhone8Plus:
                        DetectedScreenRatio = SupportedScreenRatio.HD;
                        break;
                    case DeviceGeneration.iPhoneX:
                    case DeviceGeneration.iPhoneXS:
                    case DeviceGeneration.iPhoneXSMax:
                    case DeviceGeneration.iPhoneXR:
                    case DeviceGeneration.iPhone11:
                    case DeviceGeneration.iPhone11Pro:
                    case DeviceGeneration.iPhone11ProMax:
                        DetectedScreenRatio = SupportedScreenRatio.AppleHD;
                        break;
                    case DeviceGeneration.iPadAir1:
                    case DeviceGeneration.iPadMini2Gen:
                    case DeviceGeneration.iPadMini3Gen:
                    case DeviceGeneration.iPadMini4Gen:
                    case DeviceGeneration.iPadAir2:
                    case DeviceGeneration.iPadPro1Gen:
                    case DeviceGeneration.iPad5Gen:
                    case DeviceGeneration.iPadPro2Gen:
                    case DeviceGeneration.iPadPro10Inch1Gen:
                    case DeviceGeneration.iPadPro10Inch2Gen:
                    case DeviceGeneration.iPadPro11Inch:
                    case DeviceGeneration.iPad6Gen:
                    case DeviceGeneration.iPadPro3Gen:
                    case DeviceGeneration.iPadAir3Gen:
                    case DeviceGeneration.iPadMini5Gen:
                    case DeviceGeneration.iPad7Gen:
                    case DeviceGeneration.iPadUnknown:
                        DetectedScreenRatio = SupportedScreenRatio.XGA;
                        break;
                    default:
                        DetectedScreenRatio = SupportedScreenRatio.HD;
                        break;
                }

                DetectedInputScheme = InputScheme.TouchScreen;
            }
#else
            throw new ArgumentOutOfRangeException();
#endif
            Debug.Assert(SceneManager.sceneCount == 1, "More than one scene loaded");
            StartCoroutine(LoadLevelMasterSceneAndSetActive());

            LevelLib.Instance.LevelActionAssetLib = UseTouchScreen ? LevelTouchActionAssetLib : LevelActionAssetLib;
            LevelLib.Instance.LockInLib();
        }

        public void GameStart()
        {
            //LevelMasterManager.Instance.LoadLevelThenPlay(ClassicGameActionAssetLib.ActionAssetList[0].LevelLogic, ClassicGameActionAssetLib.ActionAssetList[0]);
            Random.InitState((int)(Time.time*1000));
            int IDX=Mathf.FloorToInt(Random.value * CareerGameActionAssetLib.ActionAssetList.Length);
            LevelMasterManager.Instance.LoadLevelThenPlay(CareerGameActionAssetLib.ActionAssetList[IDX].LevelLogic, CareerGameActionAssetLib.ActionAssetList[IDX]);
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_START));
        }

        public void TutorialStart()
        {
            SceneManager.LoadSceneAsync(StaticName.SCENE_ID_TUTORIAL, LoadSceneMode.Additive);
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_START));
        }
    }
}