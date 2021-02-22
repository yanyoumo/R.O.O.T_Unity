using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace ROOT
{
    public class RoundDatas:IList<RoundData>
    {
        public int Length => core.Count;
        
        [NonSerialized]
        [OdinSerialize]
        private List<RoundData> core;

        //[Obsolete]
        //public static implicit operator RoundData[](RoundDatas datas) => null;

        public RoundGist GetCurrentRoundGist(int step,out int truncatedStep)
        {
            var round = GetCurrentRound(step,out truncatedStep);
            var stage = GetCurrentType(step,out truncatedStep);
            return new RoundGist();
            //return LevelActionAsset.ExtractGist(stage, round);
        }
        
        public RoundData GetCurrentRound(int step,out int truncatedStep)
        {
            var tmpStep = step;
            var currentRoundData = core[0];
            truncatedStep = 0;
            foreach (var neoRoundData in core)
            {
                tmpStep -= neoRoundData.TotalLength;
                if (tmpStep<0)
                {
                    currentRoundData = neoRoundData;
                    truncatedStep = tmpStep + currentRoundData.TotalLength;
                    break;
                }
            }
            return currentRoundData;
        }
        
        public StageType GetCurrentType(int step)
        {
            return GetCurrentType(step, out var A);
        }

        public StageType GetCurrentType(int step,out int truncatedStep)
        {
            var currentRound=GetCurrentRound(step, out truncatedStep);
            return currentRound.GetCurrentType(truncatedStep);
        }
        
        public RoundDatas()
        {
            core = new List<RoundData>();
        }
        
        #region INTERFACE

        public IEnumerator<RoundData> GetEnumerator()
        {
            return core.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(RoundData item)
        {
            core.Add(item);
        }

        public void Clear()
        {
            core.Clear();
        }

        public bool Contains(RoundData item)
        {
            return core.Contains(item);
        }

        public void CopyTo(RoundData[] array, int arrayIndex)
        {
            core.CopyTo(array, arrayIndex);
        }

        public bool Remove(RoundData item)
        {
            return core.Remove(item);
        }

        public int Count => core.Count;
        public bool IsReadOnly => false;
        public int IndexOf(RoundData item)
        {
            return core.IndexOf(item);
        }

        public void Insert(int index, RoundData item)
        {
            core.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            core.RemoveAt(index);
        }

        public RoundData this[int index]
        {
            get => core[index];
            set => core[index] = value;
        }
        #endregion
    }
    
    //[Serializable]
    /*public struct NeoRoundData
    {
        [ReadOnly]
        public int ID;
        
        [ReadOnly]
        public RoundType RoundTypeData;
        
        [Range(0,60)]
        [HideIf("@RoundTypeData == RoundType.Boss")]
        public int ShopLength;

        [Space]
        [Range(0, 30)]
        [HideIf("@RoundTypeData == RoundType.Boss")]
        public int RequireLength;
        [HorizontalGroup("Split")]
        [VerticalGroup("Split/Left")]
        [HideIf("@RoundTypeData == RoundType.Boss")]
        public int TypeARequirement;
        [VerticalGroup("Split/Right")]
        [HideIf("@RoundTypeData == RoundType.Boss")]
        public int TypeBRequirement;

        [Space]
        [Range(0, 60)]
        [HideIf("@RoundTypeData == RoundType.Boss")]
        public int HeatSinkLength;
        
        [ReadOnly]
        [ShowIf("@RoundTypeData == RoundType.Boss")]
        public StageType bossStageType;//这里还要做一个Filter但是现在先不用。
        
        [Range(0, 100)]
        [ShowIf("@RoundTypeData == RoundType.Boss")]
        public int bossStageLength;
        
        [ShowIf("@RoundTypeData == RoundType.Boss&&bossStageType==StageType.Telemetry")]
        public int DestoryerCount;
        [ShowIf("@RoundTypeData == RoundType.Boss&&bossStageType==StageType.Telemetry")]
        public int InfoCount;
        [ShowIf("@RoundTypeData == RoundType.Boss&&bossStageType==StageType.Telemetry")]
        public int InfoVariantRatio;
        [ShowIf("@RoundTypeData == RoundType.Boss&&bossStageType==StageType.Telemetry")]
        public int InfoTargetRatio;
        
        
        [ShowIf("@RoundTypeData == RoundType.Boss&&bossStageType==StageType.Acquiring")]
        public int AcquiringTarget;

        [ShowInInspector]
        public int TotalLength => RoundTypeData == RoundType.Normal
                ? ShopLength + RequireLength + HeatSinkLength
                : bossStageLength;

        public (StageType,int) this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return (StageType.Shop,ShopLength);
                    case 1:
                        return (StageType.Require,RequireLength);
                    case 2:
                        return (StageType.Destoryer,HeatSinkLength);
                    case 3:
                        return (bossStageType,bossStageLength);
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public StageType GetCurrentType(int truncatedStep)
        {
            if (truncatedStep<=TotalLength)
            {
                if (RoundTypeData == RoundType.Normal)
                {
                    var tmpTStep = truncatedStep;
                    for (int i = 0; i < 3; i++)
                    {
                        tmpTStep -= this[i].Item2;
                        if (tmpTStep<0)
                        {
                            return this[i].Item1;
                        }
                    }
                    throw new ArgumentException();
                }
                else
                {
                    return bossStageType;
                }
            }
            throw new ArgumentException();
        }
    }

    [Serializable]
    [CreateAssetMenu(fileName = "NewNeoActionAsset", menuName = "Neo ActionAsset/New Neo ActionAsset")]
    public class NeoLevelActionAsset : SerializedScriptableObject
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
                    RoundDatas.Add(new NeoRoundData {ID = i});
                }

                if (RoundDatasGist.HasBossRound)
                {
                    RoundDatas.Add(new NeoRoundData
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

        public RoundGist? GetRoundGistByStep(int stepCount)
        {
            var round=RoundDatas.GetCurrentRound(stepCount,out var A);
            var stage=RoundDatas.GetCurrentType(stepCount,out var B);
            return ExtractGist(stage, round);
        }
        
        public static RoundGist ExtractGist(StageType type, NeoRoundData round)
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
    }*/
}