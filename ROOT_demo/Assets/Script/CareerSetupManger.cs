using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ROOT
{
    public class CareerSetupManger : MonoBehaviour
    {
        public LevelActionAssetLib ClassicGameActionAssetLib;
        public Button BackButton;
        public Button ContinueButton;

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
            /***
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_CAREERSETUP) != SceneManager.GetSceneAt(i))
                {
                    SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i));
                }
            }
            */
            SceneManager.LoadScene(StaticName.SCENE_ID_CAREER, LoadSceneMode.Single);
        }

        void Continue()
        {
            LevelMasterManager.Instance.LoadLevelThenPlay(ClassicGameActionAssetLib.ActionAssetList[0].LevelLogic, ClassicGameActionAssetLib.ActionAssetList[0]);
        }
    }
}
