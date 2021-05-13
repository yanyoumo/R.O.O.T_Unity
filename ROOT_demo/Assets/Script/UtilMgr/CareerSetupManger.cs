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

        public Progressor LoadingProgressor;
        public GameObject LoadingLabel;
        public UIToggle matrixToggle;
        public UIToggle scanToggle;
        public UIToggle ThermalToggle;
        public UIPopup uiPopup;
        public UIView SignalSelectionPanel;
        
        private AdditionalGameSetup _additionalGameSetup = new AdditionalGameSetup();

        private LevelActionAsset actionAsset => LevelLib.Instance.ActionAsset(levelId);
        private bool LevelIsTutorial => (actionAsset.levelType == LevelType.Tutorial);//用这个方式判断这个关卡是不是教程.

        private readonly Dictionary<string, SignalType> _dict = new Dictionary<string, SignalType>
        {
            {"MatrixCoreUIToggle", SignalType.Matrix},
            {"ThermalCoreUIToggle", SignalType.Thermo},
            {"ScanCoreUIToggle", SignalType.Scan}
        };

        
        public void triggerToggleOn(String name)
        {
            RootDebug.Log("clicked " + name + " on.", NameID.SuYuxuan_Log);
            addToSignalTypeQueue(_dict[name], _additionalGameSetup.toggleQueue);
        }

        public void triggerToggleOff(String name)
        {
            if (_additionalGameSetup.toggleQueue.Count != 0)
            {
                RootDebug.Log("clicked " + name + " off.", NameID.SuYuxuan_Log);

                removeFromSignalTypeQueue(_dict[name], _additionalGameSetup.toggleQueue);
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

        public void Continue()
        {
            var actionAsset = LevelLib.Instance.ActionAsset(levelId);
            _additionalGameSetup.updateSignal();
            RootDebug.Log("the PlayingSignalType is " + _additionalGameSetup.PlayingSignalTypeA + ", and " + _additionalGameSetup.PlayingSignalTypeB, NameID.SuYuxuan_Log);
            if (!_additionalGameSetup.PlayingSignalTypeA.Equals(_additionalGameSetup.PlayingSignalTypeB) || LevelIsTutorial)
            {
                loadingProgressorCallBack(0.1f);
                _additionalGameSetup.OrderingSignal();
                //如果是Tutorial那么就无视玩家选择、只使用内部数据。
                if (!LevelIsTutorial) actionAsset.AdditionalGameSetup = _additionalGameSetup;
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
        
        void Awake()
        {
            if (LevelIsTutorial)
            {
                SignalSelectionPanel.Hide();
            }
        }
    }
}
