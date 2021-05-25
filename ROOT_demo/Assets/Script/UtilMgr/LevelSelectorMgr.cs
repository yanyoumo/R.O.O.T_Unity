using System.Collections;
using ROOT.Consts;
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
        //TutorialQuadDataPack[] _tutorial_dataS;
        private TextMeshProUGUI content;
        private bool Loading = false;
        //public bool IsCareer = false;

        //这三个List就先以这个次序弄吧。
        private LevelActionAsset[] TutorialActionAssetList => LevelLib.Instance.TutorialActionAssetList;
        private LevelActionAsset[] CareerActionAssetList => LevelLib.Instance.CareerActionAssetList;
        private LevelActionAsset[] TestingActionAssetList => LevelLib.Instance.TestingActionAssetList;

        private LevelSelectionGridMaster GridMaster;
        
        void Start()
        {
            GridMaster = DozzyLevelSelectionCanvas.GetComponentInChildren<LevelSelectionGridMaster>();

            var tutorialRowData = new LevelSelectionRowPack
            {
                ActionAssets= TutorialActionAssetList,
                Title = "教程关卡",
                DevOnly = false,
                AccessID = PlayerPrefs.GetInt(StaticPlayerPrefName.GAME_PROGRESS),
            };
            
            var gameplayRowData = new LevelSelectionRowPack
            {
                ActionAssets= CareerActionAssetList,
                Title = "正式关卡",
                DevOnly = false,
                AccessID = PlayerPrefs.GetInt(StaticPlayerPrefName.GAME_PROGRESS),
            };
            
            var testingRowData = new LevelSelectionRowPack
            {
                ActionAssets= TestingActionAssetList,
                Title = "测试关卡",
                DevOnly = true,
                AccessID = -1,
            };

            GridMaster.InitLevelSelectionMainMenu(
                new[]
                {
                    tutorialRowData, gameplayRowData, testingRowData
                }, ButtonsListener
            );
        }

        IEnumerator DoLoading(LevelActionAsset _currentUsingAsset)
        {
            LevelMasterManager.Instance.LoadCareerSetup(_currentUsingAsset);
            yield return 0;
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_CAREER);
        }

        public void ButtonsListener(LevelActionAsset _currentUsingAsset, TextMeshProUGUI _content)
        {
            Loading = true;
            content = _content;
            StartCoroutine(DoLoading(_currentUsingAsset));
        }
    }
}