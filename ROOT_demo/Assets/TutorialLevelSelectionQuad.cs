using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ROOT
{
    public class TutorialLevelSelectionQuad : MonoBehaviour
    {
        public TextMeshProUGUI Title;
        public TextMeshProUGUI StartTutorialButtonText;
        public Button StartTutorialButton;
        public Image TutorialThumbnail;

        public Button InitTutorialLevelSelectionQuad(TutorialQuadDataPack data)
        {
            Title.text = data.Title;
            StartTutorialButtonText.text = data.ButtonText;
            TutorialThumbnail.sprite = data.Thumbnail;
            return StartTutorialButton;
        }
    }
}