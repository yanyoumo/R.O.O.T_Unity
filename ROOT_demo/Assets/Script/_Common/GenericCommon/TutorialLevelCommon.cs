using System;
using UnityEngine;

namespace ROOT
{
    [Serializable]
    public struct TutorialQuadDataPack
    {
        public string TitleTerm;
        public string ButtonTerm;
        public Sprite Thumbnail;

        public TutorialQuadDataPack(string titleTerm, string _buttonTerm, string thumbnailName)
        {
            TitleTerm = titleTerm;
            ButtonTerm = _buttonTerm;
            Thumbnail = Resources.Load<Sprite>("UIThumbnail/TutorialThumbnail/" + thumbnailName);
        }

        public TutorialQuadDataPack(string titleTerm, string _buttonTerm, Sprite thumbnail)
        {
            TitleTerm = titleTerm;
            ButtonTerm = _buttonTerm;
            Thumbnail = thumbnail;
        }
    }
}