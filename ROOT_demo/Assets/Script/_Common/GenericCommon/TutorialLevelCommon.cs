using System;
using UnityEngine;

namespace ROOT
{
    [Serializable]
    public struct TutorialQuadDataPack
    {
        public int AccessID;
        public string TitleTerm;
        public string ButtonTerm;
        public Sprite Thumbnail;

        public TutorialQuadDataPack(int accessID,string titleTerm, string _buttonTerm, string thumbnailName)
        {
            TitleTerm = titleTerm;
            ButtonTerm = _buttonTerm;
            Thumbnail = Resources.Load<Sprite>("UIThumbnail/TutorialThumbnail/" + thumbnailName);
            AccessID = accessID;
        }

        public TutorialQuadDataPack(int accessID,string titleTerm, string _buttonTerm, Sprite thumbnail)
        {
            TitleTerm = titleTerm;
            ButtonTerm = _buttonTerm;
            Thumbnail = thumbnail;
            AccessID = accessID;
        }
    }
}