using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT
{
    [Serializable]
    [CreateAssetMenu(fileName = "NewActionAsset")]
    public class TutorialActionAsset : ScriptableObject
    {
        [Header("Basic Data")]
        public string TitleTerm;
        [AssetSelector(Filter = "t:Sprite",Paths= "Assets/Resources/UIThumbnail/TutorialThumbnail")]
        public Sprite Thumbnail;
        [ShowInInspector]
        public LevelType LevelType;
        [Header("Actions")]
        public TutorialActionData[] Actions;

        public TutorialQuadDataPack TutorialQuadDataPack => new TutorialQuadDataPack(TitleTerm, "Play", Thumbnail);
    }
}