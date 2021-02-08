using System;
using Doozy.Engine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace ROOT
{
    [Serializable]
    public class AdditionalGameSetup
    {
        //这个就稍微有些蠢、这个类需要能静态指定一个默认值、但是struct搞不了这件事儿；就只能用class……
        public SignalType PlayingSignalTypeA;
        public SignalType PlayingSignalTypeB;
        public Queue<SignalType> toggleQueue = new Queue<SignalType>();

        public AdditionalGameSetup()
        {   
        }

        public void updateSignal() 
        {
            if (toggleQueue.Count == 2) 
            {
                Debug.Log("update signal");
                PlayingSignalTypeA = toggleQueue.Dequeue();
                PlayingSignalTypeB = toggleQueue.Dequeue();
            }
        }
    }
    
    public class CareerSetupManger : MonoBehaviour
    {
        //public LevelActionAsset[] ClassicGameActionAssetLib => LevelLib.Instance.CareerActionAssetList;
        public static int sceneId;
        AdditionalGameSetup additionalGameSetup = new AdditionalGameSetup();
        static Dictionary<string, SignalType> _dict = new Dictionary<string, SignalType>
        {
            {"MatrixCoreUIToggle", SignalType.Matrix},
            {"ThermalCoreUIToggle", SignalType.Thermo},
            {"ScanCoreUIToggle", SignalType.Scan}
        };

        // Start is called before the first frame update
        void Awake()
        {

        }

        private void OnGUI()
        {
            //这里的函数到时候写在这里、这个和Update等效的；只不过是跟UI的更新更加相关。
        }

        public void triggerToggleOn(String name) 
        {
            Debug.Log("clicked " + name +" on.");
            addToSignalTypeQueue(_dict[name], additionalGameSetup.toggleQueue);
        }

        public void triggerToggleOff(String name) 
        {
            if (additionalGameSetup.toggleQueue.Count != 0)
            {
                Debug.Log("clicked " + name + " off.");
                removeFromSignalTypeQueue(_dict[name], additionalGameSetup.toggleQueue);
            }
        }

        private void removeFromSignalTypeQueue(SignalType removeType, Queue<SignalType> toggleQueue) 
        {
            if (toggleQueue.Peek().Equals(removeType))
            {
                turnOffToggle(removeType);
                toggleQueue.Dequeue();
                Debug.Log("remove " + removeType + ", and the queue size is " + toggleQueue.Count);
            }
            if (toggleQueue.Contains(removeType)) 
            {
                SignalType remainType = toggleQueue.Dequeue();
                turnOffToggle(toggleQueue.Dequeue());
                toggleQueue.Enqueue(remainType);
                Debug.Log("remove " + removeType + ", keep " + remainType + " and the queue size is " + toggleQueue.Count);
            }
        }

        private void addToSignalTypeQueue(SignalType type, Queue<SignalType> toggleQueue) 
        {
            Debug.Log("toggleQueue size is " + toggleQueue.Count);
            if (toggleQueue.Count < 2)
            {
                toggleQueue.Enqueue(type);
                Debug.Log("add " + type +" into queue.");
            }
            else 
            {
                SignalType removeType = toggleQueue.Dequeue();
                Debug.Log("remove " + removeType + " out of queue.");
                turnOffToggle(removeType);
                toggleQueue.Enqueue(type);
                Debug.Log("add " + type + " into queue.");
                Debug.Log("toggle queue length is  " + toggleQueue.Count);
            }
        }

        private void turnOffToggle(SignalType removeType) 
        {
            switch (removeType)
            {
                case SignalType.Matrix:
                    UIToggle matrixToggle = GameObject.Find("MatrixCoreUIToggle").GetComponent<UIToggle>();
                    matrixToggle.IsOn = false;
                    break;
                case SignalType.Scan:
                    UIToggle scanToggle = GameObject.Find("ScanCoreUIToggle").GetComponent<UIToggle>();
                    scanToggle.IsOn = false;
                    break;
                case SignalType.Thermo:
                    UIToggle ThermalToggle = GameObject.Find("ThermalCoreUIToggle").GetComponent<UIToggle>();
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
            var actionAsset = LevelLib.Instance.CareerActionAssetList[sceneId];
            additionalGameSetup.updateSignal();
            Debug.Log("the PlayingSignalType is " + additionalGameSetup.PlayingSignalTypeA + ", and " + additionalGameSetup.PlayingSignalTypeB);
            if (!additionalGameSetup.PlayingSignalTypeA.Equals(additionalGameSetup.PlayingSignalTypeB))
            {
                actionAsset.AdditionalGameSetup = additionalGameSetup;
                LevelMasterManager.Instance.LoadLevelThenPlay(actionAsset);
                SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_CAREERSETUP);
            }
            else 
            {
                Debug.LogWarning("additionalGameSetup is not properly setup, player hasn't selected two cores");
                //need to add an animation or pop up in the UI to tell the player to correctly select cores
                //Otherwise, the game should not proceed
            }

        }
    }
}
