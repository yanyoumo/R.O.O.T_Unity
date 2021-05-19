using System.Linq;
using ROOT.Consts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
// ReSharper disable Unity.InstantiateWithoutParent

namespace ROOT.UI
{
    public class LevelSelectionGridMaster : MonoBehaviour
    {
        /*public LevelSelectionGrid TutorialGrid;
        public LevelSelectionGrid MainPlayGrid;
        public LevelSelectionGrid ConstructionGrid;*/

        public Transform LevelSelectionPanel;
        public GameObject LevelQuadTemplate;
        public GameObject LevelSelectionRowTemplate;

        /*public GameObject TestingTitle;
        public GameObject TestingGridRoot;*/
        
        private void Awake()
        {
            /*TutorialGrid.LevelQuadTemplate = LevelQuadTemplate;
            MainPlayGrid.LevelQuadTemplate = LevelQuadTemplate;
            ConstructionGrid.LevelQuadTemplate = LevelQuadTemplate;
            
            if (!StartGameMgr.DevMode)
            {
                TestingTitle.SetActive(false);
                TestingGridRoot.SetActive(false);
            }*/
        }

        public Button[] InitLevelSelectionMainMenu(
            TutorialQuadDataPack[] TutorialData,
            TutorialQuadDataPack[] CareerData,
            TutorialQuadDataPack[] TestingData)
        {
            var bARow = Instantiate(LevelSelectionRowTemplate);
            var bAScript = bARow.GetComponent<LevelSelectionRow>();
            bAScript.SelectionGrid.LevelQuadTemplate = LevelQuadTemplate;
            var bA = bAScript.SelectionGrid.InitTutorialLevelSelectionMainMenu(TutorialData);
            bARow.transform.parent = LevelSelectionPanel;
            
            var bBRow = Instantiate(LevelSelectionRowTemplate);
            var bBScript = bBRow.GetComponent<LevelSelectionRow>();
            bBScript.SelectionGrid.LevelQuadTemplate = LevelQuadTemplate;
            var bB = bBScript.SelectionGrid.InitTutorialLevelSelectionMainMenu(CareerData);
            bBRow.transform.parent = LevelSelectionPanel;

            //TODO DevMode 还要补，还有抽象化流程。
            
            /*var bA = TutorialGrid.InitTutorialLevelSelectionMainMenu(TutorialData);
            var bB = MainPlayGrid.InitTutorialLevelSelectionMainMenu(CareerData);
            if (StartGameMgr.DevMode)
            {
                var bC = ConstructionGrid.InitTutorialLevelSelectionMainMenu(TestingData);
                return bA.Concat(bB).Concat(bC).ToArray();
            }
            return bA.Concat(bB).ToArray();*/
            //return null;
            return bA.Concat(bB).ToArray();
        }

        public void BackToMenu()
        {
            SceneManager.LoadSceneAsync(StaticName.SCENE_ID_START, LoadSceneMode.Single);
        }
    }
}