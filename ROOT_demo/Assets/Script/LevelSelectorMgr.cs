using System;
using System.Collections;
using System.Collections.Generic;
using ROOT.SetupAsset;
using ROOT.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public sealed partial class LevelSelectorMgr : MonoBehaviour
    {
        public GameObject TutorialCanvas;
        public GameObject DozzyLevelSelectionCanvas;
        TutorialQuadDataPack[] _dataS;
        private TextMeshProUGUI content;
        private bool Loading = false;
        public bool IsCareer = false;

        private LevelActionAsset[] ActionAssetList => IsCareer ? LevelLib.Instance.CareerActionAssetList : LevelLib.Instance.TutorialActionAssetList;
        private int ActionAssetCount => ActionAssetList.Length;

        void Start()
        {
            _dataS = new TutorialQuadDataPack[ActionAssetCount];
            for (var i = 0; i < ActionAssetCount; i++)
            {
                _dataS[i] = ActionAssetList[i].TutorialQuadDataPack;
            }

            //var buttons = TutorialCanvas.GetComponentInChildren<TutorialLevelSelectionMainMenu>().InitTutorialLevelSelectionMainMenu(_dataS);
            var buttons = DozzyLevelSelectionCanvas.GetComponentInChildren<LevelSelectionGridMaster>().InitTutorialLevelSelectionMainMenu(_dataS);

            for (var i = 0; i < buttons.Length; i++)
            {
                var buttonId = i;
                TextMeshProUGUI tmp = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
                buttons[i].onClick.AddListener(() => { ButtonsListener(buttonId, tmp); });
            }
        }

        IEnumerator DoLoading(int buttonId)
        {
            LevelMasterManager.Instance.LoadCareerSetup(buttonId);
            yield return 0;
            SceneManager.UnloadSceneAsync(IsCareer ? StaticName.SCENE_ID_CAREER : StaticName.SCENE_ID_TUTORIAL);
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