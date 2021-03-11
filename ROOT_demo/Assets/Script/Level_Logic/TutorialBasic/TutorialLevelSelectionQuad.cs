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
        public Button StartTutorialButton;
        public Image TutorialThumbnail;
        public Localize ButtonLocalize;
        public Localize TitleLocalize;

        public Button InitTutorialLevelSelectionQuad(TutorialQuadDataPack data)
        {
            TutorialThumbnail.sprite = data.Thumbnail;
            TitleLocalize.SetTerm(data.TitleTerm);
            ButtonLocalize.SetTerm(ScriptTerms.PlayLevel);
            return StartTutorialButton;
        }
    }
}