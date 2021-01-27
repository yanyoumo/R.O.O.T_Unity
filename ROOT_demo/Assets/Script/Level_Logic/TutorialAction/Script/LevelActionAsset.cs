using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
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
        public SignalType Core;
        [ShowIf("ActionType", TutorialActionType.CreateUnit)]
        [VerticalGroup("Split/Left")]
        public HardwareType HardwareType;
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
        public List<SignalType> ShopExcludedType;

        [Header("Tutorial")]
        [ShowIf("levelType", LevelType.Tutorial)]
        public TutorialActionData[] Actions;

        [Header("Career")] 
        [ShowIf("levelType", LevelType.Career)]
        public RoundDatasGist RoundDatasGist;
        
        [ShowIf("levelType", LevelType.Career)]
        public RoundData[] RoundDatas;
        
        [ShowIf("levelType", LevelType.Career)]
        [Button("Create New RoundDatas From Gist")]
        public void CreateRoundDatasFromGist()
        {
            if (RoundDatasGist.NormalRoundCount<=0)
            {
                Debug.LogError("Can't create zero length Rounds");
            }
            else
            {
                if (RoundDatasGist.HasBossRound)
                {
                    RoundDatas = new RoundData[RoundDatasGist.NormalRoundCount+1];
                    RoundDatas[RoundDatas.Length - 1].RoundType = RoundType.Boss;
                }
                else
                {
                    RoundDatas = new RoundData[RoundDatasGist.NormalRoundCount];
                }
            }
        }
        
        [ShowIf("levelType", LevelType.Career)]
        [Button("Extract RoundData Gist")]
        public void ExtractRoundDatasToGist()
        {
            var normalCount = RoundDatas.Count(round => round.RoundType == RoundType.Normal);
            var bossCount = RoundDatas.Count(round => round.RoundType == RoundType.Boss);
            RoundDatasGist = new RoundDatasGist
            {
                NormalRoundCount = normalCount,
                HasBossRound = (bossCount > 0),
            };
        }
        
        [Space]
        [ShowIf("levelType", LevelType.Career)]
        public UnitGist[] InitalBoard;
        [ShowIf("levelType", LevelType.Career)]
        [Range(0, 10000)]
        public int InfoCount;
        [Range(0, 1.0f)]
        public float InfoVariantRatio;
        [Range(0, 1.0f)]
        public float InfoTargetRatio;
        [ShowIf("levelType", LevelType.Career)]
        [ShowInInspector]
        [SerializeField]
        public Vector2Int[] StationaryRateList;

        public TutorialQuadDataPack TutorialQuadDataPack => new TutorialQuadDataPack(TitleTerm, "Play", Thumbnail);

        public bool HasEnded(int StepCount)
        {
            return StepCount >= PlayableCount;
        }

        public int TelemetryCount => RoundDatas[RoundDatas.Length - 1].DestoryerLength;

        /// <summary>
        /// 获得改时间线上End前玩家可以玩的步长。
        /// </summary>
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
            var tCount = GetTruncatedCount(stepCount, out var _round);
            if (tCount==-1) return null;

            var round = RoundDatas[_round];
            var type = round.CheckStage(tCount, _round == RoundDatas.Length - 1);

            if (!type.HasValue) return null;

            var stage = type.Value;
            if (_round == RoundDatas.Length - 1 && stage == StageType.Destoryer)
            {
                stage = StageType.Telemetry;
            }

            return ExtractGist(stage, round);
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
                case StageType.Telemetry:
                    roundGist.TelemetryLength = round.DestoryerLength;
                    roundGist.DestoryerCount = round.DestoryerCount;
                    roundGist.InfoCount = round.InfoCount;
                    break;
                case StageType.Ending:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            //roundGist.HSSwTruncatedIdx = new[] {round.ShopLength + round.RequireLength};
            //RISK 商店第一步切换的话，上一轮的摧毁和这个切换有个时序冲突。所以现在在第二步切换。
            roundGist.HSSwTruncatedIdx = new[] {1};

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

        //[ReadOnly]
        public AdditionalGameSetup AdditionalGameSetup;
    }
}