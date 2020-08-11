using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace ROOT
{

    public enum LevelType
    {
        Tutorial,
        Career,
        Classic
    }

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
    public struct UnitGist
    {
        [Header("Basic")]
        public CoreType Core;
        public SideType[] Sides;

        [Header("OnBoardInfo")]
        public Vector2Int Pos;
        public bool IsStation;
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
    public class LevelActionAsset : ScriptableObject
    {
        [Header("Basic Data")]
        public string TitleTerm;
        [AssetSelector(Filter = "t:Sprite",Paths= "Assets/Resources/UIThumbnail/TutorialThumbnail")]
        public Sprite Thumbnail;
        [Required]
        [AssetSelector(Filter = "t:Prefab", Paths = "Assets/Resources/LevelLogicPrefab")]
        public GameObject LevelLogic;
        [EnumToggleButtons]
        public LevelType levelType;

        [Header("Tutorial")]
        [ShowIf("levelType", LevelType.Tutorial)]
        public TutorialActionData[] Actions;

        [Header("Classic")]
        [ShowIf("levelType", LevelType.Classic)]
        [Range(0,10000)]
        public int InitialCurrency;
        [ShowIf("levelType", LevelType.Classic)]
        [Range(0,100)]
        public int InitialTime;

        [Header("Career")]
        [ShowIf("levelType", LevelType.Career)]
        public TimeLineToken[] TimeLineTokens;
        [Space]
        [ShowIf("levelType", LevelType.Career)]
        public UnitGist[] InitalBoard;

        public TutorialQuadDataPack TutorialQuadDataPack => new TutorialQuadDataPack(TitleTerm, "Play", Thumbnail);
    }
}