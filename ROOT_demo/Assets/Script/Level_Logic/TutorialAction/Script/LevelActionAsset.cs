using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
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

    public enum StageType{
        Shop,
        Require,
        Destoryer,
        Ending,
    }

    public struct RoundGist
    {
        public int ID;
        public StageType Type;
        public int normalReq;
        public int networkReq;
        public int shopLength;
        public int[] HSSwTruncatedIdx;

        public bool SwitchHeatsink(int tCount)
        {
            return HSSwTruncatedIdx != null && (HSSwTruncatedIdx[0] != -1 && HSSwTruncatedIdx.Contains(tCount));
        }
    }

    /// <summary>
    /// 里面的长度由类似数据的状态管理，Token是base-0计数。
    /// </summary>
    [Serializable]
    public struct RoundData
    {
        public int ID;

        [Range(0,60)]
        public int ShopLength;

        [Space]
        [Range(0, 30)]
        public int RequireLength;
        [Indent]
        public int NormalRequirement;
        [Indent]
        public int NetworkRequirement;

        [Space]
        [Range(0, 20)]
        public int DestoryerLength;

        /*[Space]
        [Range(0, 30)]
        public int HeatSinkSwitchCount;*/

        public int TotalLength => ShopLength + RequireLength + DestoryerLength;

        public bool InRange(int truncatedCount)
        {
            return truncatedCount < TotalLength;
        }

        public StageType? CheckStage(int truncatedCount)
        {
            var dic=new List<Tuple<StageType, int>>()
            {
                new Tuple<StageType, int>(StageType.Shop,ShopLength),
                new Tuple<StageType, int>(StageType.Require,RequireLength),
                new Tuple<StageType, int>(StageType.Destoryer,DestoryerLength),
            };

            var idx = 0;
            do
            {
                if (truncatedCount < dic[idx].Item2)
                {
                    return dic[idx].Item1;
                }

                truncatedCount -= dic[idx].Item2;
                idx++;
            } while (idx < dic.Count);

            return null;
        }
    }

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
    [CreateAssetMenu(fileName = "NewActionAsset", menuName = "ActionAsset/New ActionAsset")]
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
        
        /*[Header("Career")]
        [ShowIf("levelType", LevelType.Career)]
        public TimeLineToken[] TimeLineTokens;*/
        [Header("Career")]
        [ShowIf("levelType", LevelType.Career)]
        public RoundData[] RoundDatas;

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

        public bool HasEnded(int StepCount)
        {
            return StepCount >= PlayableCount;
        }

        /// <summary>
        /// 获得改时间线上End前玩家可以玩的步长。
        /// </summary>
        [HideInInspector]
        public int PlayableCount => RoundDatas.Sum(round => round.TotalLength);

        public int GetTruncatedCount(int TotalCount, out int RoundCount)
        {
            if (RoundDatas.Length <= 0)
            {
                RoundCount = -1;
                return -1;
            }

            var res = TotalCount;

            for (var i = 0; i < RoundDatas.Length; i++)
            {
                if (res < RoundDatas[i].TotalLength)
                {
                    RoundCount = i;
                    return res;
                }
                res -= RoundDatas[i].TotalLength;

            }

            RoundCount = -1;
            return -1;
        }

        [CanBeNull]
        public RoundGist? GetRoundGistByStep(int stepCount)
        {
            var tCount = GetTruncatedCount(stepCount, out var Round);
            if (tCount==-1) return null;
            var round = RoundDatas[Round];
            var type = round.CheckStage(tCount);
            if (!type.HasValue) return null;
            return ExtractGist(type.Value, round);
        }

        public static RoundGist ExtractGist(StageType type, RoundData round)
        {
            var roundGist = new RoundGist {ID=round.ID,Type = type};
            switch (type)
            {
                case StageType.Shop:
                    roundGist.normalReq = round.NormalRequirement;
                    roundGist.networkReq = round.NetworkRequirement;
                    roundGist.shopLength = round.ShopLength;
                    break;
                case StageType.Require:
                    roundGist.normalReq = round.NormalRequirement;
                    roundGist.networkReq = round.NetworkRequirement;
                    break;
                case StageType.Destoryer:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            roundGist.HSSwTruncatedIdx = new[] {round.ShopLength + round.RequireLength};

            /*try
            {
                Utils.SpreadOutLaying(round.HeatSinkSwitchCount, round.TotalLength, out roundGist.HSSwTruncatedIdx);
            }
            catch (ArgumentException)
            {
                roundGist.HSSwTruncatedIdx = new[] {-1};
            }*/
            return roundGist;
        }
    }
}