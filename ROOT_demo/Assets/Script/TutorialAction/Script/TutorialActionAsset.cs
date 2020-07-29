using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT
{
    [CreateAssetMenu(fileName = "NewActionAsset")]
    public class TutorialActionAsset : ScriptableObject
    {
        [Header("Basic Data")]
        public string TitleTerm;
        [AssetSelector(Filter = "t:Sprite",Paths= "Assets/Resources/UIThumbnail/TutorialThumbnail")]
        public Sprite Thumbnail;
        [ShowInInspector]
        public Type LevelLogicType;
        [Header("Actions")]
        public TutorialActionData[] Actions;
    }
}