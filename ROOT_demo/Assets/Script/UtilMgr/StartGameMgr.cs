using System;
using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
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
        Keyboard,
        Mouse,
        TouchScreen,
    }

    public partial class LevelLib : MonoBehaviour
    {
        internal LevelActionAssetLib TutorialLevelActionAssetLib;
        public LevelActionAsset[] CareerActionAssetList { internal set;get; }
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
        public LevelActionAssetLib ClassicGameActionAssetLib;
        public LevelActionAssetLib CareerGameActionAssetLib;

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

        [Serializable]
        struct GameSettingJSON
        {
            public int startingMoney;
        }

        void AddListeners()
        {

        }

        /*int rollD6()
        {
            return Random.Range(1, 7);
        }

        int doSearch_pureRandom()
        {
            var res = new int[6];
            for (var i = 0; i < res.Length; i++)
            {
                res[i] = rollD6();
            }
            var resT1 = res[0] * 100 + res[1] * 10 + res[2];
            var resT2 = res[3] * 100 + res[4] * 10 + res[5];
            return resT1 - resT2;
        }

        int doSearch_BasicTechniqueA()
        {
            var res = new int[6];
            int valA0 = rollD6();
            int valA1 = rollD6();
            res[0] = valA0 > valA1 ? valA0 : valA1;
            res[3] = valA0 < valA1 ? valA0 : valA1;
            int valA2 = rollD6();
            int valA3 = rollD6();
            res[1] = valA2 > valA3 ? valA2 : valA3;
            res[4] = valA2 < valA3 ? valA2 : valA3;
            int valA4 = rollD6();
            int valA5 = rollD6();
            res[2] = valA4 > valA5 ? valA4 : valA5;
            res[5] = valA4 < valA5 ? valA4 : valA5;
            var resT1 = res[0] * 100 + res[1] * 10 + res[2];
            var resT2 = res[3] * 100 + res[4] * 10 + res[5];
            return resT1 - resT2;
        }
        
        int doSearch_BasicTechniqueB()
        {
            var res = new int[] {0, 0, 0, 0, 0, 0};

            int valA0 = rollD6();
            int valA1 = rollD6();
            int delA = Math.Abs(valA0 - valA1);
            if (delA<=1)
            {
                res[0] = valA0 >= valA1 ? valA0 : valA1;
                res[3] = valA0 >= valA1 ? valA1 : valA0;
            }
            else if (delA <= 3)
            {
                res[1] = valA0 >= valA1 ? valA0 : valA1;
                res[4] = valA0 >= valA1 ? valA1 : valA0;
            }
            else
            {
                res[2] = valA0 >= valA1 ? valA0 : valA1;
                res[5] = valA0 >= valA1 ? valA1 : valA0;
            }

            int valB0 = rollD6();
            int valB1 = rollD6();
            int delB = Math.Abs(valA0 - valA1);

            if (delB<=1)
            {
                if (res[0]!=0)
                {
                    res[1] = valB0 >= valB1 ? valB1 : valB0;
                    res[4] = valB0 >= valB1 ? valB0 : valB1;
                }
                else
                {
                    res[0] = valB0 >= valB1 ? valB0 : valB1;
                    res[3] = valB0 >= valB1 ? valB1 : valB0;
                }
            }
            else if (delA <= 3)
            {
                if (res[0] != 0)
                {
                    res[1] = valB0 >= valB1 ? valB0 : valB1;
                    res[4] = valB0 >= valB1 ? valB1 : valB0;
                }
                else
                {
                    res[2] = valB0 >= valB1 ? valB0 : valB1;
                    res[5] = valB0 >= valB1 ? valB1 : valB0;
                }
            }
            else
            {
                if (res[2]!=0)
                {
                    res[1] = valB0 >= valB1 ? valB0 : valB1;
                    res[4] = valB0 >= valB1 ? valB1 : valB0;
                }
                else
                {
                    res[2] = valB0 >= valB1 ? valB0 : valB1;
                    res[5] = valB0 >= valB1 ? valB1 : valB0;
                }
            }
            
            int valC0 = rollD6();
            int valC1 = rollD6();

            int indexC0 = -1;
            int indexC1 = -1;
            
            for (int i = 0; i < 6; i++)
            {
                if (res[i]==0)
                {
                    res[i] = valC0;
                    indexC0 = i;
                    break;
                }
            }
            
            for (int i = 0; i < 6; i++)
            {
                if (res[i]==0)
                {
                    res[i] = valC1;
                    indexC1 = i;
                    break;
                }
            }
            
            var resTC0 = res[0] * 100 + res[1] * 10 + res[2];
            var resTC1 = res[3] * 100 + res[4] * 10 + res[5];
            var resTCA=resTC0 - resTC1;

            var tmp = res[indexC1];
            res[indexC1] = res[indexC0];
            res[indexC0] = tmp;
            
            var resTC2 = res[0] * 100 + res[1] * 10 + res[2];
            var resTC3 = res[3] * 100 + res[4] * 10 + res[5];
            var resTCB= resTC2 - resTC3;
            
            if (resTCA<0&&resTCB<0)
            {
                return resTCA;
            }
            if (resTCA < 0 || resTCB < 0)
            {
                return resTCA >= 0 ? resTCA : resTCB;
            }
            return resTCA <= resTCB ? resTCA : resTCB;
        }

        bool CompareVal((int, int) A, (int, int) B)
        {
            return A.Item1 > B.Item1;
        }

        int doSearch_SemiTheoreticalBest()
        {
            var raw_res = new int[6];
            for (var i = 0; i < raw_res.Length; i++)
            {
                raw_res[i] = rollD6();
            }

            var indexLib = new (int, int)[15];
            indexLib[0] = (0, 1);
            indexLib[1] = (0, 2);
            indexLib[2] = (0, 3);
            indexLib[3] = (0, 4);
            indexLib[4] = (0, 5);
            indexLib[5] = (1, 2);
            indexLib[6] = (1, 3);
            indexLib[7] = (1, 4);
            indexLib[8] = (1, 5);
            indexLib[9] = (2, 3);
            indexLib[10] = (2, 4);
            indexLib[11] = (2, 5);
            indexLib[12] = (3, 4);
            indexLib[13] = (3, 5);
            indexLib[14] = (4, 5);

            var dels = new (int, int)[15];
            for (var i = 0; i < indexLib.Length; i++)
            {
                dels[i].Item1 = Math.Abs(raw_res[indexLib[i].Item1] - raw_res[indexLib[i].Item2]);
                dels[i].Item2 = i;
            }

            var delOrdered = dels.OrderBy(v => v.Item1).ToArray();

            (int, int) first = (0, 0);
            (int, int) second = (0, 0);
            (int, int) third = (0, 0);

            first = indexLib[delOrdered[0].Item1];
            int j = 0;
            for (int i = 1; i < delOrdered.Length; i++)
            {
                var tmp = indexLib[delOrdered[i].Item1];
                if (first.Item1 != tmp.Item1 && first.Item1 != tmp.Item2)
                {
                    if (first.Item2 != tmp.Item1 && first.Item2 != tmp.Item2)
                    {
                        second = tmp;
                        j = 1;
                    }
                }
            }

            for (int i = j + 1; i < delOrdered.Length; i++)
            {
                var tmp = indexLib[delOrdered[i].Item1];
                if (second.Item1 != tmp.Item1 && second.Item1 != tmp.Item2)
                {
                    if (second.Item2 != tmp.Item1 && second.Item2 != tmp.Item2)
                    {
                        third = tmp;
                    }
                }
            }

            var res = new int[6];
            res[0] = raw_res[first.Item1] >= raw_res[first.Item2] ? raw_res[first.Item1] : raw_res[first.Item2];
            res[3] = raw_res[first.Item1] >= raw_res[first.Item2] ? raw_res[first.Item2] : raw_res[first.Item1];
            res[1] = raw_res[second.Item1] >= raw_res[second.Item2] ? raw_res[second.Item2] : raw_res[second.Item1];
            res[4] = raw_res[second.Item1] >= raw_res[second.Item2] ? raw_res[second.Item1] : raw_res[second.Item2];
            res[2] = raw_res[third.Item1] >= raw_res[third.Item2] ? raw_res[third.Item2] : raw_res[third.Item1];
            res[5] = raw_res[third.Item1] >= raw_res[third.Item2] ? raw_res[third.Item1] : raw_res[third.Item2];

            var resT1 = res[0] * 100 + res[1] * 10 + res[2];
            var resT2 = res[3] * 100 + res[4] * 10 + res[5];
            return resT1 - resT2;
        }

        [Button]
        void SemiTheoreticalBest()
        {
            int Trial = 1000000;
            int badCount = 0;
            int critCount = 0;
            int goodCount = 0;
            int fairCount = 0;
            for (int i = 0; i < Trial; i++)
            {
                int res = doSearch_SemiTheoreticalBest();
                if (res < 0||res>=100)
                {
                    badCount++;
                }
                else if (res == 0)
                {
                    critCount++;
                }
                else if(res<=10)
                {
                    goodCount++;
                }
                else
                {
                    fairCount++;
                }
            }

            float badPer = (badCount / (float) Trial) * 100;
            float critPer = (critCount / (float) Trial) * 100;
            float goodPer = (goodCount / (float) Trial) * 100;
            float fairPer = (fairCount / (float) Trial) * 100;
            Debug.Log("Using Random Method Searching--bad:" + badPer + "% crit:" + critPer +
                      "% goodPer:" + goodPer + "% fairPer:" + fairPer+"%");
        }
        
        [Button]
        void PureRandomSearch()
        {
            int Trial = 1000000;
            int badCount = 0;
            int critCount = 0;
            int goodCount = 0;
            int fairCount = 0;
            for (int i = 0; i < Trial; i++)
            {
                int res = doSearch_pureRandom();
                if (res < 0||res>=100)
                {
                    badCount++;
                }
                else if (res == 0)
                {
                    critCount++;
                }
                else if(res<=10)
                {
                    goodCount++;
                }
                else
                {
                    fairCount++;
                }
            }

            float badPer = (badCount / (float) Trial) * 100;
            float critPer = (critCount / (float) Trial) * 100;
            float goodPer = (goodCount / (float) Trial) * 100;
            float fairPer = (fairCount / (float) Trial) * 100;
            Debug.Log("Using Random Method Searching--bad:" + badPer + "% crit:" + critPer +
                      "% goodPer:" + goodPer + "% fairPer:" + fairPer+"%");
        }

        [Button]
        void BasicTechniqueA()
        {
            int Trial = 1000000;
            int badCount = 0;
            int critCount = 0;
            int goodCount = 0;
            int fairCount = 0;
            for (int i = 0; i < Trial; i++)
            {
                int res = doSearch_BasicTechniqueA();
                if (res < 0||res>=100)
                {
                    badCount++;
                }
                else if (res == 0)
                {
                    critCount++;
                }
                else if(res<=10)
                {
                    goodCount++;
                }
                else
                {
                    fairCount++;
                }
            }

            float badPer = (badCount / (float) Trial) * 100;
            float critPer = (critCount / (float) Trial) * 100;
            float goodPer = (goodCount / (float) Trial) * 100;
            float fairPer = (fairCount / (float) Trial) * 100;
            Debug.Log("Using Random Method Searching--bad:" + badPer + "% crit:" + critPer +
                      "% goodPer:" + goodPer + "% fairPer:" + fairPer+"%");
        }

        [Button]
        void BasicTechniqueB()
        {
            int Trial = 1000000;
            int badCount = 0;
            int critCount = 0;
            int goodCount = 0;
            int fairCount = 0;
            for (int i = 0; i < Trial; i++)
            {
                int res = doSearch_BasicTechniqueB();
                if (res < 0||res>=100)
                {
                    badCount++;
                }
                else if (res == 0)
                {
                    critCount++;
                }
                else if(res<=10)
                {
                    goodCount++;
                }
                else
                {
                    fairCount++;
                }
            }

            float badPer = (badCount / (float) Trial) * 100;
            float critPer = (critCount / (float) Trial) * 100;
            float goodPer = (goodCount / (float) Trial) * 100;
            float fairPer = (fairCount / (float) Trial) * 100;
            Debug.Log("Using Random Method Searching--bad:" + badPer + "% crit:" + critPer +
                      "% goodPer:" + goodPer + "% fairPer:" + fairPer+"%");
        }*/
        
        void Awake()
        {
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
            LevelActionAsset[] tutorialArray= TutorialActionAssetLib.ActionAssetList;
            int levelLengthCount = tutorialArray.Length + CareerGameActionAssetLib.ActionAssetList.Length;
            LevelLib.Instance.CareerActionAssetList = new LevelActionAsset[levelLengthCount];
            for (var i = 0; i < levelLengthCount; i++)
            {
                var tmp = i<tutorialArray.Length ? tutorialArray[i] : CareerGameActionAssetLib.ActionAssetList[i- tutorialArray.Length];
                LevelLib.Instance.CareerActionAssetList[i] = tmp;
            }
            LevelLib.Instance.LockInLib();



#if !UNITY_EDITOR
            var gameSetting = new GameSettingJSON {startingMoney = Mathf.RoundToInt(Random.value*100)};
            FileIOUtility.WriteString(JsonUtility.ToJson(gameSetting), "GameSetting.json");

            var GameSettingString = FileIOUtility.ReadString("GameSetting.json");
            var gameSettingB = JsonUtility.FromJson<GameSettingJSON>(GameSettingString);
            Debug.Log(gameSettingB + "::" + gameSettingB.startingMoney);
#endif
        }

        public void GameStart()
        {
            LevelMasterManager.Instance.LoadLevelThenPlay(ClassicGameActionAssetLib.ActionAssetList[0]);
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_START));
        }

        public void TutorialStart()
        {
            /*SceneManager.LoadSceneAsync(StaticName.SCENE_ID_TUTORIAL, LoadSceneMode.Additive);
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_START));*/
        }

        public void CareerStart()
        {
            SceneManager.LoadSceneAsync(StaticName.SCENE_ID_CAREER, LoadSceneMode.Additive);
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_START));
        }
    }
}