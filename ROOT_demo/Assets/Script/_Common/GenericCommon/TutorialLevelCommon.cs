using System;
using UnityEngine;

namespace ROOT
{
    [Serializable]
    public struct LevelQuadDataPack
    {
        public int AccessID;
        public string TitleTerm;
        public string ButtonTerm;
        public Sprite Thumbnail;

        public LevelQuadDataPack(int accessID,string titleTerm, string _buttonTerm, string thumbnailName)
        {
            TitleTerm = titleTerm;
            ButtonTerm = _buttonTerm;
            Thumbnail = Resources.Load<Sprite>("UIThumbnail/TutorialThumbnail/" + thumbnailName);
            AccessID = accessID;
        }

        public LevelQuadDataPack(int accessID,string titleTerm, string _buttonTerm, Sprite thumbnail)
        {
            TitleTerm = titleTerm;
            ButtonTerm = _buttonTerm;
            Thumbnail = thumbnail;
            AccessID = accessID;
        }
    }
}