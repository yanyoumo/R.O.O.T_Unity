using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public sealed partial class TutorialMasterMgr : MonoBehaviour
    {
        public GameObject TutorialCanvas;
        TutorialQuadDataPack[] _dataS;
        private TextMeshProUGUI content;
        private bool Loading = false;
        public TutorialActionAssetLib TutorialActionAssetLib;
        public TutorialActionAsset[] ActionAssetList => TutorialActionAssetLib.TutorialActionAssetList;
        public int ActionAssetCount => TutorialActionAssetLib.TutorialActionAssetList.Length;

        void Start()
        {
            _dataS = new TutorialQuadDataPack[ActionAssetCount];
            for (var i = 0; i < ActionAssetCount; i++)
            {
                _dataS[i] = ActionAssetList[i].TutorialQuadDataPack;
            }

            var buttons = TutorialCanvas.GetComponentInChildren<TutorialLevelSelectionMainMenu>().InitTutorialLevelSelectionMainMenu(_dataS);

            for (var i = 0; i < buttons.Length; i++)
            {
                var buttonId = i;
                TextMeshProUGUI tmp = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
                buttons[i].onClick.AddListener(() => { ButtonsListener(buttonId, tmp); });
            }
        }

        IEnumerator DoLoading(int buttonId)
        {
            LevelMasterManager.Instance.LoadLevelThenPlay(ActionAssetList[buttonId].LevelType);
            yield return 0;
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_TUTORIAL);
        }

        public void ButtonsListener(int buttonId, TextMeshProUGUI _content)
        {
            Loading = true;
            content = _content;
            StartCoroutine(DoLoading(buttonId));
        }

        public void Update()
        {
            if (Loading)
            {
                int count = Mathf.FloorToInt((Time.time*50) % 5);
                string res= "加载中";
                for (int i = 0; i < count; i++)
                {
                    res += ".";
                }
                content.text = res;
            }
        }
    }
}