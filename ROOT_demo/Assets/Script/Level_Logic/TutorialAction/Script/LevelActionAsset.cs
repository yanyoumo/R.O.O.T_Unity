using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
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
        //这个的顺序不能变！
        Text,
        CreateUnit,
        CreateCursor,
        ShowText,
        HideText,
        End,
        ShowCheckList,
        HideCheckList,
    }

    [Serializable]
    public struct UnitGist
    {
        [Header("Basic")]
        public CoreType Core;
        public SideType[] Sides;
        [Range(1,5)]
        public int Tier;

        [Header("OnBoardInfo")]
        public Vector2Int Pos;
        public bool IsStation;
    }

    //TODO Yuxuan 自行学习ScriptObject概念，使用跟随逻辑链学习本类的用途和使用方式。
    //1、重新制作关卡“第三关”、“第四关”
    //2、制作全新关卡“第五关”。

    [Serializable]
    public struct TutorialActionData
    {
        [HorizontalGroup("Split")]
        [VerticalGroup("Split/Left")]
        [LabelWidth(80)]
        public int ActionIdx;
        [EnumPaging]
        [VerticalGroup("Split/Right")]
        public TutorialActionType ActionType;
        [ShowIf("ActionType", TutorialActionType.Text)]
        [LabelWidth(30)]
        public string Text;
        [ShowIf("ActionType", TutorialActionType.Text)]
        [HorizontalGroup("Doppelganger")]
        [LabelWidth(135)]
        public bool DoppelgangerToggle;
        [ShowIf("@this.ActionType==TutorialActionType.Text&&this.DoppelgangerToggle")]
        [HorizontalGroup("Doppelganger")]
        [LabelWidth(135)]
        public string DoppelgangerText;
        [ShowIf("@this.ActionType==TutorialActionType.CreateUnit||this.ActionType==TutorialActionType.CreateCursor")]
        public Vector2Int Pos;
        [ShowIf("ActionType", TutorialActionType.CreateUnit)]
        [VerticalGroup("Split/Left")]
        public CoreType Core;
        [ShowIf("ActionType", TutorialActionType.CreateUnit)]
        [VerticalGroup("Split/Right")]
        public SideType[] Sides;
        [ShowIf("ActionType", TutorialActionType.CreateUnit)]
        [Range(1,5)]
        public int Tier;
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
        [Required]
        [AssetSelector(Paths = "Assets/Resources/GameMode")]
        public GameModeAsset GameModeAsset;

        public bool ExcludedShop=false;
        [ShowIf("@this.ExcludedShop==true")]
        public List<CoreType> ShopExcludedType;

        [Header("Tutorial")]
        [ShowIf("levelType", LevelType.Tutorial)]
        public TutorialActionData[] Actions;
        
        [Header("Career")]
        [ShowIf("levelType", LevelType.Career)]
        public TimeLineToken[] TimeLineTokens;
        [Space]
        [ShowIf("levelType", LevelType.Career)]
        public UnitGist[] InitalBoard;
        [ShowIf("levelType", LevelType.Career)]
        [Range(0, 100)]
        public int TargetCount;
        [ShowIf("levelType", LevelType.Career)]
        [ShowInInspector]
        [SerializeField]
        public Vector2Int[] StationaryRateList;

        public TutorialQuadDataPack TutorialQuadDataPack => new TutorialQuadDataPack(TitleTerm, "Play", Thumbnail);

        /// <summary>
        /// 获得改时间线上End前玩家可以玩的步长。
        /// </summary>
        [HideInInspector]
        public int PlayableCount
        {
            get
            {
                if (TimeLineTokens.Any(token => token.type == TimeLineTokenType.Ending))
                {
                    return TimeLineTokens.Where(token => token.type == TimeLineTokenType.Ending).ToArray()[0].Range.x;
                }
                else
                {
                    return -1;
                }
            }
        }
    }
}