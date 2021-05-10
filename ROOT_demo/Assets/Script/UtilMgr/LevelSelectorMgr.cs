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

        private bool ActionAssetIsTutorial(int i) => i < TutorialActionAssetList.Length;
        
        private TutorialQuadDataPack[] QuadDataPacksFromActionAssetList(LevelActionAsset[] list)
        {
            var res = new TutorialQuadDataPack[list.Length];
            for (var i = 0; i < list.Length; i++)
            {
                res[i] = list[i].TutorialQuadDataPack;
            }
            return res;
        }

        private LevelSelectionGridMaster GridMaster;
        
        void Start()
        {
            var _tutorial_dataS = QuadDataPacksFromActionAssetList(TutorialActionAssetList);
            var _career_dataS = QuadDataPacksFromActionAssetList(CareerActionAssetList);
            var _testing_dataS = QuadDataPacksFromActionAssetList(TestingActionAssetList);

            GridMaster = DozzyLevelSelectionCanvas.GetComponentInChildren<LevelSelectionGridMaster>();
            var buttons = GridMaster.InitLevelSelectionMainMenu(_tutorial_dataS,_career_dataS,_testing_dataS);

            for (var i = 0; i < buttons.Length; i++)
            {
                var buttonId = i; var tmp = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
                buttons[i].onClick.AddListener(() => { ButtonsListener(buttonId, tmp); });
            }
            
            if (!StartGameMgr.DevMode)
            {
                GridMaster.TutorialGrid.SetSelectableLevels(PlayerPrefs.GetInt(StaticPlayerPrefName.GAME_PROGRESS));
                GridMaster.MainPlayGrid.SetSelectableLevels(PlayerPrefs.GetInt(StaticPlayerPrefName.GAME_PROGRESS));
            }
        }

        IEnumerator DoLoading(int buttonId)
        {
            LevelMasterManager.Instance.LoadCareerSetup(buttonId);
            yield return 0;
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_CAREER);
        }

        public void ButtonsListener(int buttonId, TextMeshProUGUI _content)
        {
            Loading = true;
            content = _content;
            StartCoroutine(DoLoading(buttonId));
        }
    }
}