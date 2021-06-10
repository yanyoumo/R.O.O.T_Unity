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

        public RectTransform ScanToggleRectTransform;//这个玩意儿的解锁条件还得想想。
        
        private Vector2 bstPanelPos => BSTMaster.LevelSelectionPanel.anchoredPosition;
        
        private void Awake()
        {
            BSTMaster.InitBSTTree(RootLevelAsset, ButtonsListener);
        }

        private void ButtonsListener(LevelActionAsset _currentUsingAsset, TextMeshProUGUI _content)
        {
            LevelMasterManager.Instance.LoadCareerSetup(_currentUsingAsset).completed += a =>
            {
                PlayerPrefs.SetFloat(StaticPlayerPrefName.LEVEL_SELECTION_PANEL_POS_X, bstPanelPos.x);
                PlayerPrefs.SetFloat(StaticPlayerPrefName.LEVEL_SELECTION_PANEL_POS_Y, bstPanelPos.y);
                SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_BST_CAREER);
            };
        }
    }
}