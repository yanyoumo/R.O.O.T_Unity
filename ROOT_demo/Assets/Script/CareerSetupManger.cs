using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ROOT
{
    public class CareerSetupManger : MonoBehaviour
    {
        //public LevelActionAsset[] ClassicGameActionAssetLib => LevelLib.Instance.CareerActionAssetList;
        public Button BackButton;
        public Button ContinueButton;
        public static int sceneId;

        // Start is called before the first frame update
        void Start()
        {
            BackButton.onClick.AddListener(Back);
            ContinueButton.onClick.AddListener(Continue);
        }

        // Update is called once per frame
        void Update()
        {

        }

        void Back()
        {
            SceneManager.LoadScene(StaticName.SCENE_ID_CAREER, LoadSceneMode.Additive);
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_CAREERSETUP);
        }

        void Continue()
        {
            var actionAsset = LevelLib.Instance.CareerActionAssetList[sceneId];
            LevelMasterManager.Instance.LoadLevelThenPlay(actionAsset);
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_CAREERSETUP);
        }
    }
}
