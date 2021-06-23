using Doozy.Engine.UI;
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
        public RectTransform ScanToggleRectTransform;
        
        private Vector2 bstPanelPos => BSTMaster.LevelSelectionPanel.anchoredPosition;
        private bool PlayerCouldUnlockScan => PlayerPrefs.GetInt(StaticPlayerPrefName.COULD_UNLOCK_SCAN, 0) == 1;
        private bool PlayerScanUnlocked => (PlayerPrefs.GetInt(StaticPlayerPrefName.SCAN_UNLOCKED, 0) == 1);
        
        private void Awake()
        {
            BSTMaster.InitBSTTree(RootLevelAsset, ButtonsListener);
            ScanToggleRectTransform.gameObject.SetActive(PlayerCouldUnlockScan || StartGameMgr.DevMode);
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