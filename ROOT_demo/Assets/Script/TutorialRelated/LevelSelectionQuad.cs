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

        public int LevelAccessID = -1;
        public Button StartTutorialButton;
        public Image TutorialThumbnail;
        public Localize ButtonLocalize;
        public Localize TitleLocalize;
        public Image QuadBackGround;

        public Color SelectableColor => ColorUtilityWrapper.ParseHtmlStringNotNull("#B7E6FD");
        public Color UnSelectableColor => ColorUtilityWrapper.ParseHtmlStringNotNull("#8C8C8C");
        
        private bool _levelSelectable=true;

        public bool LevelSelectable
        {
            set
            {
                _levelSelectable = LevelAccessID == -1 || value;
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
                StartTutorialButton.interactable = true;
            }
            else
            {
                TutorialThumbnail.sprite = UnSelectableThumbnail;
                TitleLocalize.SetTerm(ScriptTerms.Locked);
                ButtonLocalize.SetTerm(ScriptTerms.Locked);
                QuadBackGround.color = UnSelectableColor;
                StartTutorialButton.interactable = false;
            }
        }

        /*private LevelQuadDataPack cachedData;
        
        public Button InitLevelSelectionQuad(LevelQuadDataPack data)
        {
            cachedData = data;
            TutorialThumbnail.sprite = cachedData.Thumbnail;
            TitleLocalize.SetTerm(cachedData.TitleTerm);
            ButtonLocalize.SetTerm(ScriptTerms.PlayLevel);
            LevelAccessID = data.AccessID;
            return StartTutorialButton;
        }*/

        private LevelActionAsset cachedActionAsset;

        public void InitLevelSelectionQuad(LevelActionAsset actionAsset, Action<LevelActionAsset, TextMeshProUGUI> buttonCallBack)
        {
            cachedActionAsset = actionAsset;
            TutorialThumbnail.sprite = actionAsset.Thumbnail;
            TitleLocalize.SetTerm(cachedActionAsset.TitleTerm);
            ButtonLocalize.SetTerm(ScriptTerms.PlayLevel);
            LevelAccessID = cachedActionAsset.AcessID;
            StartTutorialButton.onClick.AddListener(() =>
            {
                buttonCallBack(cachedActionAsset, StartTutorialButton.GetComponentInChildren<TextMeshProUGUI>());
            });
        }
    }
}