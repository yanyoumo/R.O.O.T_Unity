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

        private void Awake()
        {
            TutorialGrid.LevelQuadTemplate = LevelQuadTemplate;
            MainPlayGrid.LevelQuadTemplate = LevelQuadTemplate;
            ConstructionGrid.LevelQuadTemplate = LevelQuadTemplate;
        }

        public Button[] InitTutorialLevelSelectionMainMenu(TutorialQuadDataPack[] data)
        {
            //var bA=TutorialGrid.InitTutorialLevelSelectionMainMenu(data);
            var bB=MainPlayGrid.InitTutorialLevelSelectionMainMenu(data);
            //var bC=ConstructionGrid.InitTutorialLevelSelectionMainMenu(data);
            return bB;
        }
        
        public void BackToMenu()
        {
            SceneManager.LoadSceneAsync(StaticName.SCENE_ID_START, LoadSceneMode.Single);
        }
    }
}