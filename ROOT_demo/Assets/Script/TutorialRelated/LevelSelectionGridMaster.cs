using System.Collections.Generic;
using System.Linq;
using ROOT.Consts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
// ReSharper disable Unity.InstantiateWithoutParent

namespace ROOT.UI
{
    public struct LevelSelectionRowPack
    {
        public string Title;
        public LevelQuadDataPack[] TutorialData;
        public bool DevOnly;
        public int AccessID;
    }
    
    public class LevelSelectionGridMaster : MonoBehaviour
    {
        public Transform LevelSelectionPanel;
        public GameObject LevelQuadTemplate;
        public GameObject LevelSelectionRowTemplate;

        public Button[] InitLevelSelectionMainMenu(LevelSelectionRowPack[] dataPacks)
        {
            var buttons = new List<Button>();
            foreach (var pack in dataPacks)
            {
                if (!StartGameMgr.DevMode && pack.DevOnly) continue;
                var row = Instantiate(LevelSelectionRowTemplate);
                var script = row.GetComponent<LevelSelectionRow>();
                script.SelectionGrid.LevelQuadTemplate = LevelQuadTemplate;
                var button = script.SelectionGrid.InitTutorialLevelSelectionMainMenu(pack.TutorialData);
                script.TitleText = pack.Title;
                if (!StartGameMgr.DevMode && !pack.DevOnly) script.SelectionGrid.SetSelectableLevels(pack.AccessID);
                row.transform.parent = LevelSelectionPanel;
                row.transform.localScale = Vector3.one;
                buttons.AddRange(button);
            }
            return buttons.ToArray();
        }

        public void BackToMenu()
        {
            SceneManager.LoadSceneAsync(StaticName.SCENE_ID_START, LoadSceneMode.Single);
        }
    }
}