using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.iOS;
using UnityEngine.SceneManagement;

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
            switch (UnityEngine.iOS.Device.generation)
            {
                case DeviceGeneration.iPhone6:
                    DetectedScreenRatio = SupportedScreenRatio.HD;
                    DetectedInputScheme = InputScheme.TouchScreen;
                    break;
                case DeviceGeneration.iPhone7Plus:
                    DetectedScreenRatio = SupportedScreenRatio.HD;
                    DetectedInputScheme = InputScheme.TouchScreen;
                    break;
                case DeviceGeneration.iPhoneX:
                case DeviceGeneration.iPhoneXR:
                case DeviceGeneration.iPhone11Pro:
                case DeviceGeneration.iPhoneXSMax:
                    DetectedScreenRatio = SupportedScreenRatio.AppleHD;
                    DetectedInputScheme = InputScheme.TouchScreen;
                    break;
                case DeviceGeneration.iPadAir2:
                    DetectedScreenRatio = SupportedScreenRatio.XGA;
                    DetectedInputScheme = InputScheme.TouchScreen;
                    break;
                case DeviceGeneration.iPadUnknown://iPadPro4Gen
                    DetectedScreenRatio = SupportedScreenRatio.XGA;
                    DetectedInputScheme = InputScheme.TouchScreen;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            #region NotSupportingdevice
                    /*case DeviceGeneration.Unknown:
                            break;
                        case DeviceGeneration.iPhone:
                            break;
                        case DeviceGeneration.iPhone3G:
                            break;
                        case DeviceGeneration.iPhone3GS:
                            break;
                        case DeviceGeneration.iPodTouch1Gen:
                            break;
                        case DeviceGeneration.iPodTouch2Gen:
                            break;
                        case DeviceGeneration.iPodTouch3Gen:
                            break;
                        case DeviceGeneration.iPad1Gen:
                            break;
                        case DeviceGeneration.iPhone4:
                            break;
                        case DeviceGeneration.iPodTouch4Gen:
                            break;
                        case DeviceGeneration.iPad2Gen:
                            break;
                        case DeviceGeneration.iPhone4S:
                            break;
                        case DeviceGeneration.iPad3Gen:
                            break;
                        case DeviceGeneration.iPhone5:
                            break;
                        case DeviceGeneration.iPodTouch5Gen:
                            break;
                        case DeviceGeneration.iPadMini1Gen:
                            break;
                        case DeviceGeneration.iPad4Gen:
                            break;
                        case DeviceGeneration.iPhone5C:
                            break;
                        case DeviceGeneration.iPhone5S:
                            break;
                        case DeviceGeneration.iPadAir1:
                            break;
                        case DeviceGeneration.iPadMini2Gen:
                            break;
                        case DeviceGeneration.iPhone6:
                            break;
                        case DeviceGeneration.iPhone6Plus:
                            break;
                        case DeviceGeneration.iPadMini3Gen:
                            break;
                        case DeviceGeneration.iPhone6S:
                            break;
                        case DeviceGeneration.iPhone6SPlus:
                            break;
                        case DeviceGeneration.iPadPro1Gen:
                            break;
                        case DeviceGeneration.iPadMini4Gen:
                            break;
                        case DeviceGeneration.iPhoneSE1Gen:
                            break;
                        case DeviceGeneration.iPadPro10Inch1Gen:
                            break;
                        case DeviceGeneration.iPhone7:
                            break;
                        case DeviceGeneration.iPodTouch6Gen:
                            break;
                        case DeviceGeneration.iPad5Gen:
                            break;
                        case DeviceGeneration.iPadPro2Gen:
                            break;
                        case DeviceGeneration.iPadPro10Inch2Gen:
                            break;
                        case DeviceGeneration.iPhone8:
                            break;
                        case DeviceGeneration.iPhone8Plus:
                            break;
                        case DeviceGeneration.iPhoneXS:
                            break;
                        case DeviceGeneration.iPadPro11Inch:
                            break;
                        case DeviceGeneration.iPadPro3Gen:
                            break;
                        case DeviceGeneration.iPad6Gen:
                            break;
                        case DeviceGeneration.iPhone11:
                            break;
                        case DeviceGeneration.iPhone11ProMax:
                            break;
                        case DeviceGeneration.iPodTouch7Gen:
                            break;
                        case DeviceGeneration.iPad7Gen:
                            break;
                        case DeviceGeneration.iPhoneUnknown:
                            break;
                        case DeviceGeneration.iPadUnknown:
                            break;
                        case DeviceGeneration.iPodTouchUnknown:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();*/
            #endregion
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