using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
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
    public class LevelActionAsset : SerializedScriptableObject
    {
        [Header("Basic Data")] public string TitleTerm;

        [AssetSelector(Filter = "t:Sprite", Paths = "Assets/Resources/UIThumbnail/TutorialThumbnail")]
        public Sprite Thumbnail;

        [Required] [AssetSelector(Filter = "t:Prefab", Paths = "Assets/Resources/LevelLogicPrefab")]
        public GameObject LevelLogic;

        [EnumToggleButtons] public LevelType levelType;

        [Required] [AssetSelector(Paths = "Assets/Resources/GameMode")]
        public GameModeAsset GameModeAsset;

        [Header("Career")] [ShowIf("levelType", LevelType.Career)]
        public AdditionalGameSetup AdditionalGameSetup;

        [ShowIf("levelType", LevelType.Career)]
        public UnitGist[] InitalBoard;

        [ShowIf("levelType", LevelType.Career)]
        public RoundDatasGist RoundDatasGist;

        [ShowIf("levelType", LevelType.Career)]
        [Button("Create New RoundDatas From Gist")]
        public void CreateRoundDatasFromGist()
        {
            if (RoundDatasGist.NormalRoundCount <= 0)
            {
                Debug.LogError("Can't create zero length Rounds");
            }
            else
            {
                RoundDatas = new RoundDatas();
                for (int i = 0; i < RoundDatasGist.NormalRoundCount; i++)
                {
                    RoundDatas.Add(new RoundData {ID = i});
                }

                if (RoundDatasGist.HasBossRound)
                {
                    RoundDatas.Add(new RoundData
                    {
                        ID = RoundDatas.Count,
                        RoundTypeData = RoundType.Boss,
                        bossStageType = RoundDatasGist.BossStage
                    });
                }
            }
        }

        [OdinSerialize] [ShowIf("levelType", LevelType.Career)]
        public RoundDatas RoundDatas;

        [Header("Tutorial")] [ShowIf("levelType", LevelType.Tutorial)]
        public TutorialActionData[] Actions;

        public StageType GetStageType(int step) => RoundDatas.GetCurrentType(step);

        public TutorialQuadDataPack TutorialQuadDataPack => new TutorialQuadDataPack(TitleTerm, "Play", Thumbnail);

        private bool IsEndless => !RoundDatasGist.HasBossRound && RoundDatasGist.Endless;

        public int PlayableCount => IsEndless ? int.MaxValue : RoundDatas.Sum(round => round.TotalLength);

        public bool HasEnded(int StepCount)
        {
            if (IsEndless)
            {
                return false;
            }

            return StepCount >= PlayableCount;
        }
        
        public int GetTruncatedCount(int TotalCount)
        {
            var round = RoundDatas.GetCurrentRound(TotalCount, out var res);
            return res;
        }

        private StageType CheckStage(int step)
        {
            return RoundDatas.GetCurrentType(step);
        }

        [Obsolete]
        private StageType? CheckStage(int truncatedCount, bool isFinalRound)
        {
            return null;
        }

        public RoundGist GetRoundGistByStep(int stepCount)
        {
            var round=RoundDatas.GetCurrentRound(stepCount,out var A);
            var stage=RoundDatas.GetCurrentType(stepCount,out var B);
            return ExtractGist(stage, round);
        }
        
        public static RoundGist ExtractGist(StageType type, RoundData round)
        {
            var roundGist = new RoundGist {ID=round.ID,Type = type};
            switch (type)
            {
                case StageType.Shop:
                    roundGist.normalReq = round.TypeARequirement;
                    roundGist.networkReq = round.TypeBRequirement;
                    roundGist.shopLength = round.ShopLength;
                    break;
                case StageType.Require:
                    roundGist.normalReq = round.TypeARequirement;
                    roundGist.networkReq = round.TypeBRequirement;
                    break;
                case StageType.Destoryer:
                    break;
                case StageType.Telemetry:
                    roundGist.TelemetryLength = round.bossStageLength;
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
            return roundGist;
        }
        
        [Obsolete] private bool IsTelemetry => RoundDatasGist.HasBossRound && RoundDatasGist.BossStage == StageType.Telemetry;
        [Obsolete] public int TelemetryCount => IsTelemetry ? RoundDatas.Last().bossStageLength : 0;
        [Obsolete] public int InfoCount => IsTelemetry ? RoundDatas.Last().InfoCount : 0;
        [Obsolete] public int InfoVariantRatio => IsTelemetry ? RoundDatas.Last().InfoVariantRatio : 0;
        [Obsolete] public int InfoTargetRatio => IsTelemetry ? RoundDatas.Last().InfoTargetRatio : 0;

        [Obsolete("Why?")] public Vector2Int[] StationaryRateList => null;

        [Obsolete("Why?")] public List<SignalType> ShopExcludedType => null;

        [Obsolete("Why?")] public bool ExcludedShop = false;
    }
}