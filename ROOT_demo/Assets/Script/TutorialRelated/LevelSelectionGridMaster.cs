using System.Collections.Generic;
using System.Linq;
using ROOT.Consts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
// ReSharper disable Unity.InstantiateWithoutParent

namespace ROOT.UI
{
    public class LevelSelectionRowPack
    {
        public string Title;
        public LevelQuadDataPack[] TutorialData;
        public bool DevOnly = false;
    }
    
    public class LevelSelectionGridMaster : MonoBehaviour
    {
        public Transform LevelSelectionPanel;
        public GameObject LevelQuadTemplate;
        public GameObject LevelSelectionRowTemplate;

        public Button[] InitLevelSelectionMainMenu(LevelSelectionRowPack[] dataPacks)
        {
            var buttons = new List<Button>();
            foreach (var levelSelectionRowPack in dataPacks)
            {
                if (!StartGameMgr.DevMode && levelSelectionRowPack.DevOnly) continue;
                var row = Instantiate(LevelSelectionRowTemplate);
                var script = row.GetComponent<LevelSelectionRow>();
                script.SelectionGrid.LevelQuadTemplate = LevelQuadTemplate;
                var button = script.SelectionGrid.InitTutorialLevelSelectionMainMenu(levelSelectionRowPack.TutorialData);
                script.TitleText = levelSelectionRowPack.Title;
                row.transform.parent = LevelSelectionPanel;
                buttons.AddRange(button);
            }
            return buttons.ToArray();
        }
        
        public Button[] InitLevelSelectionMainMenu(
            LevelQuadDataPack[] TutorialData,
            LevelQuadDataPack[] CareerData,
            LevelQuadDataPack[] TestingData)
        {
            var bARow = Instantiate(LevelSelectionRowTemplate);
            var bAScript = bARow.GetComponent<LevelSelectionRow>();
            bAScript.SelectionGrid.LevelQuadTemplate = LevelQuadTemplate;
            var bA = bAScript.SelectionGrid.InitTutorialLevelSelectionMainMenu(TutorialData);
            bAScript.TitleText = "教程关卡";
            bARow.transform.parent = LevelSelectionPanel;
            
            var bBRow = Instantiate(LevelSelectionRowTemplate);
            var bBScript = bBRow.GetComponent<LevelSelectionRow>();
            bBScript.SelectionGrid.LevelQuadTemplate = LevelQuadTemplate;
            var bB = bBScript.SelectionGrid.InitTutorialLevelSelectionMainMenu(CareerData);
            bBScript.TitleText = "正式关卡";
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