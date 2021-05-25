using System;
using System.Collections.Generic;
using System.Linq;
using ROOT.Consts;
using ROOT.SetupAsset;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
// ReSharper disable Unity.InstantiateWithoutParent

namespace ROOT.UI
{
    public struct LevelSelectionRowPack
    {
        public string Title;
        //public LevelQuadDataPack[] TutorialData;
        public LevelActionAsset[] ActionAssets;
        public bool DevOnly;
        public int AccessID;
    }
    
    public class LevelSelectionGridMaster : MonoBehaviour
    {
        public Transform LevelSelectionPanel;
        public GameObject LevelQuadTemplate;
        public GameObject LevelSelectionRowTemplate;

        public void InitLevelSelectionMainMenu(LevelSelectionRowPack[] dataPacks, Action<LevelActionAsset, TextMeshProUGUI> buttonCallBack)
        {
            foreach (var pack in dataPacks)
            {
                if (!StartGameMgr.DevMode && pack.DevOnly) continue;
                var row = Instantiate(LevelSelectionRowTemplate);
                var script = row.GetComponent<LevelSelectionRow>();
                script.SelectionGrid.LevelQuadTemplate = LevelQuadTemplate;
                script.SelectionGrid.InitLevelSelectionMainMenu(pack.ActionAssets, buttonCallBack);
                script.TitleText = pack.Title;
                if (!StartGameMgr.DevMode && !pack.DevOnly) script.SelectionGrid.SetSelectableLevels(pack.AccessID);
                row.transform.parent = LevelSelectionPanel;
                row.transform.localScale = Vector3.one;
            }
        }

        public void BackToMenu()
        {
            SceneManager.LoadSceneAsync(StaticName.SCENE_ID_START, LoadSceneMode.Single);
        }
    }
}