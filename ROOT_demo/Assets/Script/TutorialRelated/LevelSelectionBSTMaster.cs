using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ROOT.Consts;
using ROOT.LevelAccessMgr;
using ROOT.SetupAsset;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT.UI
{
    public class LevelSelectionBSTMaster : MonoBehaviour
    {
        public RectTransform TreeBranchLineRoot;
        public RectTransform TreeBranchRoot;
        public RectTransform LevelSelectionPanel;
        public GameObject LevelQuadTemplate;
        public GameObject BSTLineTemplate;

        private readonly float SpaceX = 350.0f;
        private readonly float SpaceY = 330.0f;
        private readonly Vector2 _treeRootPos = new Vector2(150, -175f);
        
        public void BackToMenu()
        {
            SceneManager.LoadSceneAsync(StaticName.SCENE_ID_START, LoadSceneMode.Single);
        }

        private Vector2 PosIDToPos(Vector2Int v2Int)=>_treeRootPos + new Vector2(v2Int.x * SpaceX, v2Int.y * SpaceY);
        
        private void GenerateSignalQuad(Vector2Int Pos, LevelActionAsset actionAsset, 
            Action<LevelActionAsset, TextMeshProUGUI> buttonCallBack
            ,bool _selectable,bool _newLevel,bool _levelCompleted)
        {
            var quadObj = Instantiate(LevelQuadTemplate, TreeBranchRoot);
            var rectTransform = quadObj.GetComponent<RectTransform>();
            var quad = quadObj.GetComponent<LevelSelectionQuad>();
            rectTransform.anchorMax = Vector2.up;
            rectTransform.anchorMin = Vector2.up;
            rectTransform.anchoredPosition = PosIDToPos(Pos);
            quad.InitLevelSelectionQuad(actionAsset, buttonCallBack);
            quad.LevelSelectable = _selectable;
            quad.SetNewLevel = _newLevel;
            quad.LevelCompleted = _levelCompleted;
        }

        private (bool, bool, bool) LevelStatusToBoolData(string titleTerm, bool levelStub)
        {
            var isNewLevel = false;
            var levelCompleted = false;
            var nextIsLevelStub = true;

            if (StartGameMgr.DevMode)
            {
                return (false, false, false);
            }

            if (levelStub)
            {
                return (false, false, true/*not relevant*/);
            }
            
            var currentLevelStatus = PlayerPrefsLevelMgr.GetLevelStatus(titleTerm);
            switch (currentLevelStatus)
            {
                case LevelStatus.Locked:
                    Debug.Assert(false, "this level should be stub,which could not reach here.");
                    break;
                case LevelStatus.Unlocked:
                    isNewLevel = true;
                    break;
                case LevelStatus.Played:
                    break;
                case LevelStatus.Passed:
                    levelCompleted = true;
                    nextIsLevelStub = false;
                    break;
            }

            return (isNewLevel, levelCompleted, nextIsLevelStub);
        }

        private void CreateBSTLine(Vector2Int nodePOS, Vector2Int lastNodePOS)
        {
            var lineObj = Instantiate(BSTLineTemplate, TreeBranchLineRoot);
            var line = lineObj.GetComponent<BSTLine>();
            line.A.anchoredPosition = PosIDToPos(lastNodePOS);
            line.B.anchoredPosition = PosIDToPos(nodePOS);
            line.UpdateLine();
        }

        private void GenerateActionAssetQuad_Iter(LevelActionAsset actionAsset, int index, bool UpOrDown, Vector2Int nextNodeRoot, Vector2Int lastNodePOS, Action<LevelActionAsset, TextMeshProUGUI> buttonCallBack, bool isLevelStub)
        {
            if (actionAsset == null || (actionAsset.IsTestingLevel && !StartGameMgr.DevMode)) return;
            GenerateActionAssetQuad(nextNodeRoot + (UpOrDown ? Vector2Int.up : Vector2Int.down) * index, lastNodePOS, actionAsset, buttonCallBack, isLevelStub);
        }

        private void GenerateActionAssetQuad(Vector2Int crtNodePOS, Vector2Int lastNodePOS, LevelActionAsset actionAsset, Action<LevelActionAsset, TextMeshProUGUI> buttonCallBack, bool isLevelStub)
        {
            var (isNewLevel, levelCompleted, nextIsLevelStub) = LevelStatusToBoolData(actionAsset.TitleTerm, isLevelStub);
            GenerateSignalQuad(crtNodePOS, actionAsset, buttonCallBack, !isLevelStub, isNewLevel, levelCompleted);

            if (lastNodePOS.x >= 0) CreateBSTLine(crtNodePOS, lastNodePOS);

            if (isLevelStub || (actionAsset.UnlockingLevel.Length == 0 && actionAsset.UnlockingLevel_Upper.Length == 0)) return;

            var nextNodeRoot = new Vector2Int(crtNodePOS.x + 1, crtNodePOS.y);

            for (var i = 0; i < actionAsset.UnlockingLevel.Length; i++)
            {
                GenerateActionAssetQuad_Iter(actionAsset.UnlockingLevel[i], i, false, nextNodeRoot, crtNodePOS, buttonCallBack, nextIsLevelStub);
            }
            
            for (var i = 0; i < actionAsset.UnlockingLevel_Upper.Length; i++)
            {
                GenerateActionAssetQuad_Iter(actionAsset.UnlockingLevel_Upper[i], i + 1, true, nextNodeRoot, crtNodePOS, buttonCallBack, nextIsLevelStub);
            }
        }

        private void CreateActualTree(LevelActionAsset rootActionAsset, Action<LevelActionAsset, TextMeshProUGUI> buttonCallBack)
        {
            GenerateActionAssetQuad(Vector2Int.zero, -Vector2Int.one, rootActionAsset, buttonCallBack, false);
        }

        private void UpdateLevelSelectionPanelSize()
        {
            var quadRects = TreeBranchRoot.GetComponentsInChildren<RectTransform>().Where(t => t.parent == TreeBranchRoot.transform);
            var maxX = quadRects.Max(r => r.anchoredPosition.x);
            var minX = quadRects.Min(r => r.anchoredPosition.x);
            var maxY = quadRects.Max(r => r.anchoredPosition.y);
            var minY = quadRects.Min(r => r.anchoredPosition.y);
            LevelSelectionPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (maxX - minX) + 125);
            LevelSelectionPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (maxY - minY) + 350);
            TreeBranchRoot.anchoredPosition = new Vector2(TreeBranchRoot.anchoredPosition.x, -(maxY + 175));
        }

        private void UpdateLevelSelectionPanelPos()
        {
            var posX = PlayerPrefs.GetFloat(StaticPlayerPrefName.LEVEL_SELECTION_PANEL_POS_X);
            var posY = PlayerPrefs.GetFloat(StaticPlayerPrefName.LEVEL_SELECTION_PANEL_POS_Y);
            LevelSelectionPanel.anchoredPosition = new Vector2(posX, posY);
        }
        
        public void InitBSTTree(LevelActionAsset rootActionAsset, Action<LevelActionAsset, TextMeshProUGUI> buttonCallBack)
        {
            CreateActualTree(rootActionAsset, buttonCallBack);
            UpdateLevelSelectionPanelSize();
            UpdateLevelSelectionPanelPos();
        }
    }
}