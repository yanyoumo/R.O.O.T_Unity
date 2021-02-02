using System;
using Doozy.Engine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ROOT
{
    [Serializable]
    public class AdditionalGameSetup
    {
        //这个就稍微有些蠢、这个类需要能静态指定一个默认值、但是struct搞不了这件事儿；就只能用class……
        public SignalType PlayingSignalTypeA;
        public SignalType PlayingSignalTypeB;

        public AdditionalGameSetup()
        {
            PlayingSignalTypeA = SignalType.Matrix;
            PlayingSignalTypeB = SignalType.Scan;
        }
    }
    
    public class CareerSetupManger : MonoBehaviour
    {
        //public LevelActionAsset[] ClassicGameActionAssetLib => LevelLib.Instance.CareerActionAssetList;
        public static int sceneId;

        // Start is called before the first frame update
        void Awake()
        {

        }

        private void OnGUI()
        {
            //这里的函数到时候写在这里、这个和Update等效的；只不过是跟UI的更新更加相关。
        }

        public void Back()
        {
            SceneManager.LoadScene(StaticName.SCENE_ID_CAREER, LoadSceneMode.Additive);
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_CAREERSETUP);
        }

        public void Continue()
        {
            var additionalGameSetup = new AdditionalGameSetup();//这个就是需要这个界面去设置、并且注入的“数据包”。
            var actionAsset = LevelLib.Instance.CareerActionAssetList[sceneId];
            actionAsset.AdditionalGameSetup = additionalGameSetup;
            LevelMasterManager.Instance.LoadLevelThenPlay(actionAsset);
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_CAREERSETUP);
        }
    }
}
