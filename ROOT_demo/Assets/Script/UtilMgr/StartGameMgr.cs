using System;
using System.Collections;
using ROOT.SetupAsset;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ROOT.StaticPlayerPrefName;
using Random = UnityEngine.Random;

namespace ROOT.SetupAsset
{
    public partial class LevelLib
    {
        internal LevelActionAssetLib TutorialLevelActionAssetLib;
        internal LevelActionAssetLib CareerLevelActionAssetLib;
        internal LevelActionAssetLib TestingLevelActionAssetLib;
        //public LevelActionAsset[] CareerActionAssetList { internal set; get; }
    }
}

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
        Keyboard,
        Mouse,
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

        public GameObject SignalMasterRoot;
        public LevelActionAssetLib TutorialActionAssetLib;
        public LevelActionAssetLib CareerGameActionAssetLib;
        public LevelActionAssetLib TestingGameActionAssetLib;
        
        public SupportedScreenRatio PCSimulateDevice;
        public InputScheme EditorInputScheme;

        public static SupportedScreenRatio DetectedScreenRatio { get; private set; } = SupportedScreenRatio.HD;
        public static InputScheme DetectedInputScheme { get; private set; } = InputScheme.Keyboard;

        public static void SetUseMouse()
        {
            DetectedInputScheme = InputScheme.Mouse;
        }

        public static void SetUseKeyboard()
        {
            DetectedInputScheme = InputScheme.Keyboard;
        }

        public static bool UseTouchScreen => DetectedInputScheme == InputScheme.TouchScreen;
        public static bool UseKeyboard => DetectedInputScheme == InputScheme.Keyboard;
        public static bool UseMouse => DetectedInputScheme == InputScheme.Mouse;
        
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

        /*[ShowInInspector]
        public Func<int, bool> testAction = a => a > 0;*/

        [Button]
        void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
        }

        [ShowInInspector]
        public static bool DevMode => PlayerPrefs.GetInt(DEV_MODE) != 0;

        [Button]
        public void ToggleDevMode()
        {
            var newVal = !DevMode;
            PlayerPrefs.SetInt(DEV_MODE, newVal ? 1 : 0);
            PlayerPrefs.Save(); 
        } 
        
        private void CheckPlayerPrefs()
        {
            if (!PlayerPrefs.HasKey(PLAYER_ID)) PlayerPrefs.SetInt(PLAYER_ID, DateTime.UtcNow.Millisecond);
            if (!PlayerPrefs.HasKey(DEV_MODE)) PlayerPrefs.SetInt(DEV_MODE, 0);
            if (!PlayerPrefs.HasKey(MOUSE_DRAG_SENSITIVITY)) PlayerPrefs.SetInt(MOUSE_DRAG_SENSITIVITY, 50);

            if (PlayerPrefs.HasKey(TUTORIAL_PROGRESS)) PlayerPrefs.SetInt(TUTORIAL_PROGRESS, 0);
            if (PlayerPrefs.HasKey(GAMEPLAY_PROGRESS)) PlayerPrefs.SetInt(GAMEPLAY_PROGRESS, 0);

            PlayerPrefs.Save();
        }
        
        void Awake()
        {
            CheckPlayerPrefs();
            //这里不能用Time.time，因为Awake和游戏运行时间差距一般很小且固定。所以这里要去调系统时间
            //RISK 这里可能需要去测试iOS的系统，目前没有测，测了后删掉。
            Random.InitState(DateTime.UtcNow.Millisecond);
#if UNITY_EDITOR
            Input.simulateMouseWithTouches = true;
            DetectedScreenRatio = PCSimulateDevice;
            DetectedInputScheme = EditorInputScheme;
#elif UNITY_STANDALONE_WIN
            DetectedScreenRatio = SupportedScreenRatio.HD;
            DetectedInputScheme = InputScheme.Keyboard;
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
            LevelLib.Instance.TutorialLevelActionAssetLib = TutorialActionAssetLib;
            LevelLib.Instance.CareerLevelActionAssetLib = CareerGameActionAssetLib;
            LevelLib.Instance.TestingLevelActionAssetLib = TestingGameActionAssetLib;
            LevelLib.Instance.LockInLib();
        }

        private bool OnceGuard = false;
        public void CareerStart()
        {
            if (!OnceGuard)
            {
                SceneManager.LoadSceneAsync(StaticName.SCENE_ID_CAREER, LoadSceneMode.Additive);
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_START));
                OnceGuard = true;
            }
        }
    }
}