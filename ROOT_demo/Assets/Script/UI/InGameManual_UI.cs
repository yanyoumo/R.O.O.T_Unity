using System;
using System.Collections;
using System.Collections.Generic;
using Doozy.Engine.UI;
using Doozy.Engine.UI.Base;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace ROOT.UI
{
    public class InGameManual_UI : MonoBehaviour
    {
        private static readonly Dictionary<string, string> TestManualContent = new Dictionary<string, string>()
        {
            {"AAA","1"},
            {"BBB","2"},
            {"CCC","3"},
            {"DDD","4"},
            {"EEE","5"},
            {"FFF","6"},
        };

        //public static event UIEvent.InGameManualFootterUpdate InGameManualFootterUpdateEvent;

        public void Awake()
        {
            int index = 0;
            foreach (var keyValuePair in TestManualContent)
            {
                var go = Instantiate(InGameManualSectionSelectorPrefab, InGameManualSectionList.transform);
                go.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, -50.0f * index);
                var uiButton = go.GetComponent<UIButton>();
                go.name = keyValuePair.Key;
                uiButton.TextMeshProLabel.text = keyValuePair.Key;
                uiButton.OnClick.OnTrigger = new UIAction {Action = ShowSectionList};
                index++;
            }

            InGameManualFooterUpdate();
        }

        public GameObject InGameManualSectionSelectorPrefab;
        
        public TextMeshProUGUI InGameManualFooter;
        public UIButton LastPageButton;
        public UIButton NextPageButton;

        public UIView InGameManualSectionList;
        public UIView InGameManualSectionContent;
        public TextMeshProUGUI InGameManualSectionContentTMP;
        
        [ReadOnly]public int TotalPageCount=10;
        [ReadOnly]public int CurrentPageCount=0;//Base-0 Indexing

        Action<int> Test;

        private void ShowSectionList(GameObject go)
        {
            InGameManualSectionContentTMP.text = TestManualContent[go.name];
            InGameManualSectionList.Show();
            InGameManualSectionContent.Hide();
        }
        
        public void ShowSectionContent()
        {
            InGameManualSectionList.Hide();
            InGameManualSectionContent.Show();
        }
        
        private void InGameManualFooterUpdate()
        {
            InGameManualFooter.text = Utils.PaddingNum2Digit(CurrentPageCount+1) + "/" + Utils.PaddingNum2Digit(TotalPageCount);
        }

        private void PageChanged()
        {
            CurrentPageCount %= TotalPageCount;
            LastPageButton.Interactable = CurrentPageCount > 0;
            NextPageButton.Interactable = CurrentPageCount < TotalPageCount - 1;
            InGameManualFooterUpdate();
        }
        
        //[Button]
        public void NextPage()
        {
            CurrentPageCount++;
            PageChanged();
        }
        
        //[Button]
        public void LastPage()
        {
            CurrentPageCount--;
            PageChanged();
        }
    }
}