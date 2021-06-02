using System;
using System.Collections;
using System.Collections.Generic;
using ROOT.Consts;
using ROOT.SetupAsset;
using ROOT.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public class BSTLevelSelectorMgr : MonoBehaviour
    {
        public static LevelActionAsset RootLevelAsset;
        public LevelSelectionBSTMaster BSTMaster;

        private Vector2 bstPanelPos => BSTMaster.LevelSelectionPanel.anchoredPosition;
        
        private void Awake()
        {
            BSTMaster.InitBSTTree(RootLevelAsset, ButtonsListener);
        }

        private void ButtonsListener(LevelActionAsset _currentUsingAsset, TextMeshProUGUI _content)
        {
            LevelMasterManager.Instance.LoadCareerSetup(_currentUsingAsset).completed += a =>
            {
                PlayerPrefs.SetFloat(StaticPlayerPrefName.Level_SelectionPanel_PosX, bstPanelPos.x);
                PlayerPrefs.SetFloat(StaticPlayerPrefName.Level_SelectionPanel_PosY, bstPanelPos.y);
                SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_BST_CAREER);
            };
        }
    }
}