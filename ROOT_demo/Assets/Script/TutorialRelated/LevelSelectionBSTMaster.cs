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

        private readonly float SpaceX = 325.0f;
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
            var quadObj = Instantiate(LevelQuadTemplate, LevelSelectionPanel);
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

        private void GenerateActionAssetQuadAndIter(Vector2Int nodePOS,Vector2Int lastNodePOS, LevelActionAsset actionAsset, Action<LevelActionAsset, TextMeshProUGUI> buttonCallBack,bool terminatingLevel)
        {
            var isNewLevel = false;
            var levelCompleted = false;
            var nextIsTerminatingLevel = false;
            if (!StartGameMgr.DevMode)
            {
                if (PlayerPrefs.HasKey(actionAsset.TitleTerm))
                {
                    var currentLevelStatus = (LevelStatus) PlayerPrefs.GetInt(actionAsset.TitleTerm);
                    switch (currentLevelStatus)
                    {
                        case LevelStatus.Locked:
                            nextIsTerminatingLevel = true;
                            break;
                        case LevelStatus.Unlocked:
                            nextIsTerminatingLevel = true;
                            isNewLevel = !terminatingLevel;
                            break;
                        case LevelStatus.Played:
                            break;
                        case LevelStatus.Passed:
                            levelCompleted = !terminatingLevel;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    PlayerPrefs.SetInt(actionAsset.TitleTerm, (int) LevelStatus.Locked);
                    PlayerPrefs.Save();
                }
            }

            GenerateSignalQuad(nodePOS, actionAsset, buttonCallBack, !terminatingLevel, isNewLevel, levelCompleted);

            if (lastNodePOS.x>=0)
            {
                var lineObj = Instantiate(BSTLineTemplate, TreeBranchLineRoot);
                var line = lineObj.GetComponent<BSTLine>();
                line.A.anchoredPosition = PosIDToPos(lastNodePOS);
                line.B.anchoredPosition = PosIDToPos(nodePOS);
                line.UpdateLine();
            }
            
            if (terminatingLevel)
            {
                return;
            }

            var lastNodePos = nodePOS;
            if (actionAsset.UnlockingLevel.Length == 0)
            {
                return;
            }
            nodePOS.x++;
            for (var i = 0; i < actionAsset.UnlockingLevel.Length; i++)
            {
                if (!StartGameMgr.DevMode && lastNodePos == Vector2Int.zero && i>0) break; //除掉TestLevels。
                if (actionAsset.UnlockingLevel[i] != null)//这是允许放一个NULL就可以手动往下挪一行这件事儿。
                {
                    GenerateActionAssetQuadAndIter(nodePOS + Vector2Int.down * i, lastNodePos, actionAsset.UnlockingLevel[i], buttonCallBack, nextIsTerminatingLevel);
                }
            }
        }

        public void InitBSTTree(LevelActionAsset rootActionAsset, Action<LevelActionAsset, TextMeshProUGUI> buttonCallBack)
        {
            GenerateActionAssetQuadAndIter(Vector2Int.zero, -Vector2Int.one, rootActionAsset, buttonCallBack, false);
            
            var quadRects = LevelSelectionPanel.GetComponentsInChildren<RectTransform>().Where(t => t.parent == LevelSelectionPanel.transform);
            var maxX = quadRects.Max(r => r.anchoredPosition.x);
            var maxY = quadRects.Max(r => Mathf.Abs(r.anchoredPosition.y));
            LevelSelectionPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxX + 125);
            LevelSelectionPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maxY + 175);
        }
    }
}