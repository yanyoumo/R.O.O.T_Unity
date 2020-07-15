﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ROOT
{
    public struct TutorialQuadDataPack
    {
        public string TitleTerm;
        public string ButtonText;
        public Sprite Thumbnail;

        public TutorialQuadDataPack(string titleTerm, string _buttonText, string thumbnailName)
        {
            TitleTerm = titleTerm;
            ButtonText = _buttonText;
            Thumbnail = Resources.Load<Sprite>("UIThumbnail/TutorialThumbnail/" + thumbnailName);
        }
    }

    public class TutorialLevelSelectionMainMenu : MonoBehaviour
    {
        public GameObject TutorialQuadTemplate;

        public RectTransform TutorialQuadPos1;
        public RectTransform TutorialQuadPos2;
        public RectTransform TutorialQuadPos3;
        public RectTransform TutorialQuadPos4;
        public RectTransform TutorialQuadPos5;
        public RectTransform TutorialQuadPos6;

        private RectTransform[] TutorialQuadPosS;
        private TutorialLevelSelectionQuad[] TutorialQuadS;

        void Awake()
        {
            TutorialQuadPosS = new[]
            {
                TutorialQuadPos1,
                TutorialQuadPos2,
                TutorialQuadPos3,
                TutorialQuadPos4,
                TutorialQuadPos5,
                TutorialQuadPos6,
            };
        }

        public Button[] InitTutorialLevelSelectionMainMenu(TutorialQuadDataPack[] data)
        {
            Debug.Assert(data.Length < 7);
            Button[] res = new Button[data.Length];
            TutorialQuadS = new TutorialLevelSelectionQuad[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                TutorialQuadS[i]=Instantiate(TutorialQuadTemplate, TutorialQuadPosS[i]).GetComponentInChildren<TutorialLevelSelectionQuad>();
                res[i] = TutorialQuadS[i].InitTutorialLevelSelectionQuad(data[i]);
            }
            return res;
        }
    }
}