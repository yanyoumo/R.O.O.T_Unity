using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ROOT.UI
{
    public class LevelSelectionGridMaster : MonoBehaviour
    {
        public LevelSelectionGrid TutorialGrid;
        public LevelSelectionGrid MainPlayGrid;
        public LevelSelectionGrid ConstructionGrid;
        
        public GameObject LevelQuadTemplate;

        public GameObject TestingTitle;
        public GameObject TestingGridRoot;
        
        private void Awake()
        {
            TutorialGrid.LevelQuadTemplate = LevelQuadTemplate;
            MainPlayGrid.LevelQuadTemplate = LevelQuadTemplate;
            ConstructionGrid.LevelQuadTemplate = LevelQuadTemplate;
            
            if (!StartGameMgr.DevMode)
            {
                TestingTitle.SetActive(false);
                TestingGridRoot.SetActive(false);
            }
        }

        public Button[] InitLevelSelectionMainMenu(
            TutorialQuadDataPack[] TutorialData,
            TutorialQuadDataPack[] CareerData,
            TutorialQuadDataPack[] TestingData)
        {
            var bA = TutorialGrid.InitTutorialLevelSelectionMainMenu(TutorialData);
            var bB = MainPlayGrid.InitTutorialLevelSelectionMainMenu(CareerData);
            if (StartGameMgr.DevMode)
            {
                var bC = ConstructionGrid.InitTutorialLevelSelectionMainMenu(TestingData);
                return bA.Concat(bB).Concat(bC).ToArray();
            }
            return bA.Concat(bB).ToArray();
        }

        public void BackToMenu()
        {
            SceneManager.LoadSceneAsync(StaticName.SCENE_ID_START, LoadSceneMode.Single);
        }
    }
}