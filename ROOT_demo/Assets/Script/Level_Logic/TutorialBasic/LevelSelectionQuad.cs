using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

namespace ROOT
{
    public class LevelSelectionQuad : MonoBehaviour
    {
        public Sprite UnSelectableThumbnail;

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
                _levelSelectable = value;
                UpdateSelectable();
            }
        }

        /*[Sirenix.OdinInspector.Button]
        public void TestEnable()
        {
            LevelSelectable = true;
        }
        
        [Sirenix.OdinInspector.Button]
        public void TestDisable()
        {
            LevelSelectable = false;
        }*/

        private void UpdateSelectable()
        {
            if (_levelSelectable)
            {
                TutorialThumbnail.sprite = cachedData.Thumbnail;
                TitleLocalize.SetTerm(cachedData.TitleTerm);
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

        private TutorialQuadDataPack cachedData;
        
        public Button InitTutorialLevelSelectionQuad(TutorialQuadDataPack data)
        {
            cachedData = data;
            TutorialThumbnail.sprite = cachedData.Thumbnail;
            TitleLocalize.SetTerm(cachedData.TitleTerm);
            ButtonLocalize.SetTerm(ScriptTerms.PlayLevel);
            return StartTutorialButton;
        }
    }
}