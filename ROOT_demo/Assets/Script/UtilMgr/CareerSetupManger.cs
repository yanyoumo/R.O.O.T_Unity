using System;
using System.Collections;
using Doozy.Engine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Doozy.Engine.Progress;
using ROOT.Consts;
using ROOT.SetupAsset;
using Random = UnityEngine.Random;

namespace ROOT.UI
{
    public class CareerSetupManger : MonoBehaviour
    {
        public static int levelId;
        AdditionalGameSetup additionalGameSetup = new AdditionalGameSetup();

        public Progressor LoadingProgressor;
        public GameObject LoadingLabel;
        private static UIToggle matrixToggle = new UIToggle();
        private static UIToggle scanToggle = new UIToggle();
        private static UIToggle ThermalToggle= new UIToggle();
        private bool isTutorial;

        static Dictionary<string, SignalType> _dict = new Dictionary<string, SignalType>
        {
            {"MatrixCoreUIToggle", SignalType.Matrix},
            {"ThermalCoreUIToggle", SignalType.Thermo},
            {"ScanCoreUIToggle", SignalType.Scan}
        };

        private UIPopup uiPopup;

        // Start is called before the first frame update
        void Awake()
        {
            //TODO 需要在这里判断；如果是Tutorial的话、就不显示已有的框架了。
            matrixToggle = GameObject.Find("MatrixCoreUIToggle").GetComponent<UIToggle>();
            scanToggle = GameObject.Find("ScanCoreUIToggle").GetComponent<UIToggle>();
            ThermalToggle = GameObject.Find("ThermalCoreUIToggle").GetComponent<UIToggle>();
            uiPopup = GameObject.Find("UIPopup").GetComponent<UIPopup>();
            var actionAsset = LevelLib.Instance.ActionAsset(levelId);
            isTutorial = (actionAsset.levelType == LevelType.Tutorial);//用这个方式判断这个关卡是不是教程.
            if (isTutorial) 
            {
                GameObject.Find("View - CareerSetup_CoreSelection").GetComponent<UIView>().Hide();
            }
        }

        private void OnGUI()
        {
            //这里的函数到时候写在这里、这个和Update等效的；只不过是跟UI的更新更加相关。
        }

        public void triggerToggleOn(String name)
        {
            RootDebug.Log("clicked " + name + " on.", NameID.SuYuxuan_Log);
            addToSignalTypeQueue(_dict[name], additionalGameSetup.toggleQueue);
        }

        public void triggerToggleOff(String name)
        {
            if (additionalGameSetup.toggleQueue.Count != 0)
            {
                RootDebug.Log("clicked " + name + " off.", NameID.SuYuxuan_Log);

                removeFromSignalTypeQueue(_dict[name], additionalGameSetup.toggleQueue);
            }
        }

        private void removeFromSignalTypeQueue(SignalType removeType, Queue<SignalType> toggleQueue)
        {
            if (toggleQueue.Peek().Equals(removeType))
            {
                turnOffToggle(removeType);
                toggleQueue.Dequeue();
                RootDebug.Log("remove " + removeType + ", and the queue size is " + toggleQueue.Count,
                    NameID.SuYuxuan_Log);
            }

            if (toggleQueue.Contains(removeType))
            {
                SignalType remainType = toggleQueue.Dequeue();
                turnOffToggle(toggleQueue.Dequeue());
                toggleQueue.Enqueue(remainType);
                RootDebug.Log(
                    "remove " + removeType + ", keep " + remainType + " and the queue size is " + toggleQueue.Count,
                    NameID.SuYuxuan_Log);
            }
        }

        private void addToSignalTypeQueue(SignalType type, Queue<SignalType> toggleQueue)
        {
            RootDebug.Log("toggleQueue size is " + toggleQueue.Count, NameID.SuYuxuan_Log);
            if (toggleQueue.Count < 2)
            {
                toggleQueue.Enqueue(type);
                RootDebug.Log("add " + type + " into queue.", NameID.SuYuxuan_Log);
            }
            else
            {
                SignalType removeType = toggleQueue.Dequeue();
                RootDebug.Log("remove " + removeType + " out of queue.", NameID.SuYuxuan_Log);
                turnOffToggle(removeType);
                toggleQueue.Enqueue(type);
                RootDebug.Log("add " + type + " into queue.", NameID.SuYuxuan_Log);
                RootDebug.Log("toggle queue length is  " + toggleQueue.Count, NameID.SuYuxuan_Log);
            }
        }

        private void turnOffToggle(SignalType removeType)
        {
            switch (removeType)
            {
                case SignalType.Matrix:
                    matrixToggle.IsOn = false;
                    break;
                case SignalType.Scan:
                    scanToggle.IsOn = false;
                    break;
                case SignalType.Thermo:
                    ThermalToggle.IsOn = false;
                    break;
                default:
                    break;
            }
        }

        public void Back()
        {
            SceneManager.LoadScene(StaticName.SCENE_ID_CAREER, LoadSceneMode.Additive);
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_CAREERSETUP);
        }

        public void uiPopUpDisappear()
        {
            uiPopup.Hide();
        }

        private IEnumerator Pendingkill()
        {
            yield return new WaitForSeconds(Mathf.Lerp(0.025f, 0.15f, Random.value));
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_CAREERSETUP);
        }
        
        private bool loadingProgressorCallBack(float val, bool completed = false)
        {
            LoadingProgressor.SetProgress(val);
            if (!completed) return true;
            StartCoroutine(Pendingkill());
            return true;
        }

        public void Continue()
        {
            var actionAsset = LevelLib.Instance.ActionAsset(levelId);
            additionalGameSetup.updateSignal();
            RootDebug.Log("the PlayingSignalType is " + additionalGameSetup.PlayingSignalTypeA + ", and " + additionalGameSetup.PlayingSignalTypeB, NameID.SuYuxuan_Log);
            if (!additionalGameSetup.PlayingSignalTypeA.Equals(additionalGameSetup.PlayingSignalTypeB) || isTutorial)
            {
                loadingProgressorCallBack(0.1f);
                additionalGameSetup.OrderingSignal();
                actionAsset.AdditionalGameSetup = additionalGameSetup;
                LevelMasterManager.Instance.LoadLevelThenPlay(actionAsset, null, loadingProgressorCallBack);
                //这个卸载函数现在放到那个回调函数里面去了。-youmo
            }
            else
            {
                RootDebug.Log("additionalGameSetup is not properly setup, player hasn't selected two cores", NameID.SuYuxuan_Log);
                //need to add an animation or pop up in the UI to tell the player to correctly select cores
                //Otherwise, the game should not proceed
                uiPopup.Show();
            }
        }
    }
}
