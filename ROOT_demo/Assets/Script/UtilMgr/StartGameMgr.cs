using System;
using DG.Tweening;
using ROOT.Consts;
using ROOT.LevelAccessMgr;
using ROOT.SetupAsset;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ROOT.Consts.StaticPlayerPrefName;
using Random = UnityEngine.Random;

namespace ROOT
{
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

        public LevelActionAsset RootLevelAsset;
        /*public LevelActionAssetLib TutorialActionAssetLib;
        public LevelActionAssetLib CareerGameActionAssetLib;
        public LevelActionAssetLib TestingGameActionAssetLib;*/

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

        [Button]
        void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
        }

        [ShowInInspector] public static bool DevMode => PlayerPrefs.GetInt(DEV_MODE) != 0;

        [Button]
        public void ToggleDevMode()
        {
            var newVal = !DevMode;
            PlayerPrefs.SetInt(DEV_MODE, newVal ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void SetUpPlayerPrefs()
        {
            if (!PlayerPrefs.HasKey(PLAYER_ID)) PlayerPrefs.SetInt(PLAYER_ID, DateTime.UtcNow.Millisecond);
            if (!PlayerPrefs.HasKey(DEV_MODE)) PlayerPrefs.SetInt(DEV_MODE, 0);
            if (!PlayerPrefs.HasKey(MOUSE_DRAG_SENSITIVITY)) PlayerPrefs.SetInt(MOUSE_DRAG_SENSITIVITY, 50);
            if (!PlayerPrefs.HasKey(LEVEL_SELECTION_PANEL_POS_X))
                PlayerPrefs.SetFloat(LEVEL_SELECTION_PANEL_POS_X, 0.0f);
            if (!PlayerPrefs.HasKey(LEVEL_SELECTION_PANEL_POS_Y))
                PlayerPrefs.SetFloat(LEVEL_SELECTION_PANEL_POS_Y, 0.0f);
            if (!PlayerPrefs.HasKey(RootLevelAsset.TitleTerm))
                PlayerPrefsLevelMgr.SetUpRootLevelStatus(RootLevelAsset.TitleTerm);
            if (!PlayerPrefs.HasKey(UNLOCK_SCAN)) PlayerPrefs.SetInt(UNLOCK_SCAN, 0);

            PlayerPrefs.Save();
        }

        public GameObject ControllingEventPrefab;

        public static void LoadThenActiveGameCoreScene() =>
            SceneManager.LoadSceneAsync(StaticName.SCENE_ID_LEVELMASTER, LoadSceneMode.Additive).completed += a =>
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_LEVELMASTER));
            };

        void Awake()
        {
            DOTween.Init();
            SetUpPlayerPrefs();
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
(DetectedScreenRatio,DetectedInputScheme) = MobileDeviceMgr.AdaptMobileScreen();
#else
throw new ArgumentOutOfRangeException();
#endif
            if (GameObject.FindWithTag(StaticTagName.TAG_CONTROLLING_EVENT_MGR) == null)
            {
                Instantiate(ControllingEventPrefab);
            }

            Debug.Assert(SceneManager.sceneCount == 1, "More than one scene loaded");
            LoadThenActiveGameCoreScene();
//LevelLib.Instance.LockInLib();
        }

        private bool OnceGuard = false;

        public void CareerStart()
        {
            if (!OnceGuard)
            {
                BSTLevelSelectorMgr.RootLevelAsset = RootLevelAsset;
                SceneManager.LoadSceneAsync(StaticName.SCENE_ID_BST_CAREER, LoadSceneMode.Additive);
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_START));
                OnceGuard = true;
            }
        }
    }
}