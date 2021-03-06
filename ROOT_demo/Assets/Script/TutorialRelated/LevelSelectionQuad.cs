﻿using System;
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

        public Image IconBG;
        public Image TutorialIcon;
        public Image GameplayIcon;

        public Transform NewLevelIconRoot;
        public Transform LevelCompleteIconRoot;

        public Image ThumbnailBG;
        public Image LockedThumbnail;
        
        private Color SelectableColor => ColorLibManager.Instance.ColorLib.ROOT_SELECTIONQUAD_SELECTABLE;
        private Color UnSelectableColor => ColorLibManager.Instance.ColorLib.ROOT_SELECTIONQUAD_UNSELECTABLE;
        
        private bool _levelSelectable=true;
        public bool LevelSelectable
        {
            set
            {
                _levelSelectable = value;
                UpdateSelectable();
            }
        }

        private bool _newLevel = false;
        public bool SetNewLevel
        {
            set
            {
                _newLevel = value;
                NewLevelIconRoot.gameObject.SetActive(_newLevel);
            }
        }
        
        private bool _levelCompleted = false;
        public bool LevelCompleted
        {
            set
            {
                _levelCompleted = value;
                LevelCompleteIconRoot.gameObject.SetActive(_levelCompleted);
            }
        }

        private void UpdateSelectable()
        {
            ThumbnailBG.enabled = !_levelSelectable;
            LockedThumbnail.enabled = !_levelSelectable;
            TitleLocalize.SetTerm(_levelSelectable ? cachedActionAsset.TitleTerm : ScriptTerms.Locked);
            ButtonLocalize.SetTerm(_levelSelectable ? ScriptTerms.PlayLevel : ScriptTerms.Locked);
            QuadBackGround.color = _levelSelectable ? SelectableColor : UnSelectableColor;
            StartLevelButton.interactable = _levelSelectable;
        }

        private LevelActionAsset cachedActionAsset;

        public void InitLevelSelectionQuad(LevelActionAsset actionAsset, Action<LevelActionAsset, TextMeshProUGUI> buttonCallBack)
        {
            cachedActionAsset = actionAsset;
            TutorialThumbnail.sprite = actionAsset.Thumbnail;
            TitleLocalize.SetTerm(cachedActionAsset.TitleTerm);
            ButtonLocalize.SetTerm(ScriptTerms.PlayLevel);
            StartLevelButton.onClick.AddListener(() =>
            {
                buttonCallBack(cachedActionAsset, StartLevelButton.GetComponentInChildren<TextMeshProUGUI>());
            });
            
            var displayIsTutorial=actionAsset.DisplayedlevelType == LevelType.Tutorial;
            
            TutorialIcon.enabled = displayIsTutorial;
            GameplayIcon.enabled = !displayIsTutorial;

            IconBG.color = displayIsTutorial ? ColorLibManager.Instance.ColorLib.ROOT_UI_HIGHLIGHTING_BLUE : ColorLibManager.Instance.ColorLib.ROOT_UI_HIGHLIGHTING_GREEN;
        }
    }
}