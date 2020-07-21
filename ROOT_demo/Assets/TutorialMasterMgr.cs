using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public sealed partial class TutorialMasterMgr : MonoBehaviour
    {
        public GameObject TutorialCanvas;
        TutorialQuadDataPack[] _dataS;

        void Awake()
        {
            InitTutorialActions();
        }

        void Start()
        {
            _dataS = new TutorialQuadDataPack[tutorialActions.Length];
            for (var i = 0; i < tutorialActions.Length; i++)
            {
                _dataS[i] = tutorialActions[i].TutorialQuadDataPack;
            }

            var buttons = TutorialCanvas.GetComponentInChildren<TutorialLevelSelectionMainMenu>().InitTutorialLevelSelectionMainMenu(_dataS);

            for (var i = 0; i < buttons.Length; i++)
            {
                var buttonId = i;
                buttons[i].onClick.AddListener(() => { ButtonsListener(buttonId); });
            }
        }

        public void ButtonsListener(int buttonId)
        {
            LevelMasterManager.Instance.LoadLevelThenPlay(tutorialActions[buttonId].LevelLogicType);
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_TUTORIAL);
        }
    }
}