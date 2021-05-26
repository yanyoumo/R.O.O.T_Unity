using System;
using I2.Loc;
using ROOT.SetupAsset;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ROOT
{
    public class LevelSelectionQuad : MonoBehaviour
    {
        public Sprite UnSelectableThumbnail;

        //public int LevelAccessID = -1;
        public Button StartLevelButton;
        public Image TutorialThumbnail;
        public Localize ButtonLocalize;
        public Localize TitleLocalize;
        public Image QuadBackGround;

        public Image TutorialIcon;
        public Image GameplayIcon;

        public Color SelectableColor => ColorUtilityWrapper.ParseHtmlStringNotNull("#B7E6FD");
        public Color UnSelectableColor => ColorUtilityWrapper.ParseHtmlStringNotNull("#8C8C8C");
        
        private bool _levelSelectable=true;

        public bool LevelSelectable
        {
            set
            {
                _levelSelectable = value;
                UpdateSelectable();
            }
        }

        private void UpdateSelectable()
        {
            if (_levelSelectable)
            {
                TutorialThumbnail.sprite = cachedActionAsset.Thumbnail;
                TitleLocalize.SetTerm(cachedActionAsset.TitleTerm);
                ButtonLocalize.SetTerm(ScriptTerms.PlayLevel);
                QuadBackGround.color = SelectableColor;
                StartLevelButton.interactable = true;
            }
            else
            {
                TutorialThumbnail.sprite = UnSelectableThumbnail;
                TitleLocalize.SetTerm(ScriptTerms.Locked);
                ButtonLocalize.SetTerm(ScriptTerms.Locked);
                QuadBackGround.color = UnSelectableColor;
                StartLevelButton.interactable = false;
            }
        }

        private LevelActionAsset cachedActionAsset;

        public void InitLevelSelectionQuad(LevelActionAsset actionAsset, Action<LevelActionAsset, TextMeshProUGUI> buttonCallBack)
        {
            cachedActionAsset = actionAsset;
            TutorialThumbnail.sprite = actionAsset.Thumbnail;
            TitleLocalize.SetTerm(cachedActionAsset.TitleTerm);
            ButtonLocalize.SetTerm(ScriptTerms.PlayLevel);
            //LevelAccessID = cachedActionAsset.AcessID;
            StartLevelButton.onClick.AddListener(() =>
            {
                buttonCallBack(cachedActionAsset, StartLevelButton.GetComponentInChildren<TextMeshProUGUI>());
            });
            
            TutorialIcon.enabled = actionAsset.levelType == LevelType.Tutorial;
            GameplayIcon.enabled = actionAsset.levelType != LevelType.Tutorial;
        }
    }
}