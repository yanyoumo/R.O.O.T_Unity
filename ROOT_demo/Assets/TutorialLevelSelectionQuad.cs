using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ROOT
{
    public class TutorialLevelSelectionQuad : MonoBehaviour
    {
        //public TextMeshProUGUI Title;
        //public TextMeshProUGUI StartTutorialButtonText;
        public Button StartTutorialButton;
        public Image TutorialThumbnail;
        public Localize ButtonLocalize;
        public Localize TitleLocalize;

        public Button InitTutorialLevelSelectionQuad(TutorialQuadDataPack data)
        {
            //StartTutorialButtonText.text = data.ButtonText;
            TutorialThumbnail.sprite = data.Thumbnail;
            TitleLocalize.SetTerm(data.TitleTerm);
            ButtonLocalize.SetTerm(ScriptTerms.PlayLevel);
            return StartTutorialButton;
        }
    }
}