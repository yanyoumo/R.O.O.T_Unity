using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT
{

    public enum TutorialActionType
    {
        Text,
        CreateUnit,
        CreateCursor,
        ShowText,
        HideText,
        End,
    }

    [Serializable]
    public struct TutorialActionData
    {
        [HorizontalGroup("Split")]
        [VerticalGroup("Split/Left")]
        public int ActionIdx;
        [EnumPaging]
        [VerticalGroup("Split/Right")]
        public TutorialActionType ActionType;
        [ShowIf("ActionType", TutorialActionType.Text)]
        public string Text;
        [ShowIf("@this.ActionType==TutorialActionType.CreateUnit||this.ActionType==TutorialActionType.CreateCursor")]
        public Vector2Int Pos;
        [ShowIf("ActionType", TutorialActionType.CreateUnit)]
        [VerticalGroup("Split/Left")]
        public CoreType Core;
        [ShowIf("ActionType", TutorialActionType.CreateUnit)]
        [VerticalGroup("Split/Right")]
        public SideType[] Sides;
    }

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