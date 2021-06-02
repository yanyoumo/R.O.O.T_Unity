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

        private void Awake()
        {
            BSTMaster.InitBSTTree(RootLevelAsset, ButtonsListener);
        }

        private void ButtonsListener(LevelActionAsset _currentUsingAsset, TextMeshProUGUI _content)
        {
            LevelMasterManager.Instance.LoadCareerSetup(_currentUsingAsset).completed += a =>
            {
                SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_BST_CAREER);
            };
        }
    }
}