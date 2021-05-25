using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ROOT.Consts;
using ROOT.SetupAsset;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT.UI
{
    public class LevelSelectionBSTMaster : MonoBehaviour
    {
        public RectTransform TreeBranchLineRoot;
        public RectTransform LevelSelectionPanel;
        public GameObject LevelQuadTemplate;
        public GameObject BSTLineTemplate;

        private readonly float SpaceX = 350.0f;
        private readonly float SpaceY = 330.0f;
        private readonly Vector2 _treeRootPos = new Vector2(150, -160f);
        
        public void BackToMenu()
        {
            SceneManager.LoadSceneAsync(StaticName.SCENE_ID_START, LoadSceneMode.Single);
        }

        private Vector2 PosIDToPos(Vector2Int v2Int)=>_treeRootPos + new Vector2(v2Int.x * SpaceX, v2Int.y * SpaceY);
        
        private void GenerateSignalQuad(Vector2Int Pos, LevelActionAsset actionAsset, Action<LevelActionAsset, TextMeshProUGUI> buttonCallBack)
        {
            var quadObj = Instantiate(LevelQuadTemplate, LevelSelectionPanel);
            var rectTransform = quadObj.GetComponent<RectTransform>();
            var quad = quadObj.GetComponent<LevelSelectionQuad>();
            rectTransform.anchorMax = Vector2.up;
            rectTransform.anchorMin = Vector2.up;
            rectTransform.anchoredPosition = PosIDToPos(Pos);
            quad.InitLevelSelectionQuad(actionAsset, buttonCallBack);
        }

        private void GenerateActionAssetQuadAndIter(Vector2Int nodePOS,Vector2Int lastNodePOS, LevelActionAsset actionAsset, Action<LevelActionAsset, TextMeshProUGUI> buttonCallBack)
        {
            GenerateSignalQuad(nodePOS, actionAsset, buttonCallBack);
            if (lastNodePOS.x>=0)
            {
                var lineObj = Instantiate(BSTLineTemplate, TreeBranchLineRoot);
                var line = lineObj.GetComponent<BSTLine>();
                line.A.anchoredPosition = PosIDToPos(lastNodePOS);
                line.B.anchoredPosition = PosIDToPos(nodePOS);
                line.UpdateLine();
            }
            var lastNodePos = nodePOS;
            if (actionAsset.UnlockingLevel.Length == 0)
            {
                return;
            }
            nodePOS.x++;
            for (var i = 0; i < actionAsset.UnlockingLevel.Length; i++)
            {
                GenerateActionAssetQuadAndIter(nodePOS + Vector2Int.down * i, lastNodePos,actionAsset.UnlockingLevel[i], buttonCallBack);
            }
        }

        public void InitBSTTree(LevelActionAsset rootActionAsset, Action<LevelActionAsset, TextMeshProUGUI> buttonCallBack)
        {
            GenerateActionAssetQuadAndIter(Vector2Int.zero,-Vector2Int.one, rootActionAsset, buttonCallBack);
            
            var quadRects = LevelSelectionPanel.GetComponentsInChildren<RectTransform>().Where(t => t.parent == LevelSelectionPanel.transform);
            var maxX = quadRects.Max(r => r.anchoredPosition.x);
            var maxY = quadRects.Max(r => r.anchoredPosition.y);
            LevelSelectionPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxX + 150);
            LevelSelectionPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maxY + 75);
        }
    }
}