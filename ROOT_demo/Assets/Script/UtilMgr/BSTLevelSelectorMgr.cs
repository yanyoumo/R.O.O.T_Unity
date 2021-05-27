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

        IEnumerator DoLoading(LevelActionAsset _currentUsingAsset)
        {
            LevelMasterManager.Instance.LoadCareerSetup(_currentUsingAsset);
            yield return 0;
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_BST_CAREER);
        }

        public void ButtonsListener(LevelActionAsset _currentUsingAsset, TextMeshProUGUI _content)
        {
            StartCoroutine(DoLoading(_currentUsingAsset));
        }
    }
}