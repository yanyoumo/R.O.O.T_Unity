using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ROOT
{
    public struct AdditionalGameSetup
    {
        public int GameID;
    }
    
    public class CareerSetupManger : MonoBehaviour
    {
        //public LevelActionAsset[] ClassicGameActionAssetLib => LevelLib.Instance.CareerActionAssetList;
        public Button BackButton;
        public Button ContinueButton;
        public static int sceneId;

        // Start is called before the first frame update
        void Awake()
        {
            //这种“自我初始化”建议卸载Awake里面。
            BackButton.onClick.AddListener(Back);
            ContinueButton.onClick.AddListener(Continue);
        }

        private void OnGUI()
        {
            //这里的函数到时候写在这里、这个和Update等效的；只不过是跟UI的更新更加相关。
        }

        void Back()
        {
            SceneManager.LoadScene(StaticName.SCENE_ID_CAREER, LoadSceneMode.Additive);
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_CAREERSETUP);
        }

        void Continue()
        {
            var additionalGameSetup = new AdditionalGameSetup();//这个就是需要这个界面去设置、并且注入的“数据包”。
            var actionAsset = LevelLib.Instance.CareerActionAssetList[sceneId];
            LevelMasterManager.Instance.LoadLevelThenPlay(actionAsset,additionalGameSetup);
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_CAREERSETUP);
        }
    }
}
