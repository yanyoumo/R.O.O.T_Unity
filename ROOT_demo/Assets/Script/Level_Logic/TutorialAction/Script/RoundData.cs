using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Linq;
using Sirenix.Serialization;

namespace ROOT.SetupAsset
{
    public struct RoundGist
    {
        public RoundData owner;
        public StageType Type;

        public int ID => owner.ID;
        public int normalReq=> owner.TypeARequirement;
        public int networkReq=> owner.TypeBRequirement;
        public int shopLength => owner.ShopLength;
        public int[] HSSwTruncatedIdx=> new[] {1};

        /*[Obsolete] public int TelemetryLength=> owner.bossStageLength;
        [Obsolete] public int DestoryerCount=> owner.DestoryerCount;
        [Obsolete] public int InfoCount=> owner.InfoCount;
        [Obsolete] public int InfoTargetRatio => owner.InfoTargetRatio;
        [Obsolete] public int InfoVariantRatio => owner.InfoVariantRatio;*/
        
        public bool SwitchHeatsink(int tCount)
        {
            return HSSwTruncatedIdx != null && (HSSwTruncatedIdx[0] != -1 && (HSSwTruncatedIdx).Contains(tCount));
        }
    }

    /// <summary>
    /// 里面的长度由类似数据的状态管理，Token是base-0计数。
    /// </summary>
    [Serializable]
    public struct RoundData
    {
        [ReadOnly] public int ID;

        [ReadOnly] public RoundType RoundTypeData;

        [Range(0, 60)] [HideIf("@RoundTypeData == RoundType.Boss")]
        public int ShopLength;

        [Space] [Range(0, 30)] [HideIf("@RoundTypeData == RoundType.Boss")]
        public int RequireLength;

        [HorizontalGroup("Split")] [VerticalGroup("Split/Left")] [HideIf("@RoundTypeData == RoundType.Boss")]
        public int TypeARequirement;

        [VerticalGroup("Split/Right")] [HideIf("@RoundTypeData == RoundType.Boss")]
        public int TypeBRequirement;

        [Space] [Range(0, 60)] [HideIf("@RoundTypeData == RoundType.Boss")]
        public int HeatSinkLength;

        [ReadOnly] [ShowIf("@RoundTypeData == RoundType.Boss")]
        public StageType bossStageType; //这里还要做一个Filter但是现在先不用。

        [Range(0, 100)] [ShowIf("@RoundTypeData == RoundType.Boss")]
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

        public (StageType, int) this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return (StageType.Shop, ShopLength);
                    case 1:
                        return (StageType.Require, RequireLength);
                    case 2:
                        return (StageType.Destoryer, HeatSinkLength);
                    case 3:
                        return (bossStageType, bossStageLength);
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public StageType GetCurrentType(int truncatedStep)
        {
            if (truncatedStep <= TotalLength)
            {
                if (RoundTypeData == RoundType.Normal)
                {
                    var tmpTStep = truncatedStep;
                    for (int i = 0; i < 3; i++)
                    {
                        tmpTStep -= this[i].Item2;
                        if (tmpTStep < 0)
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

        public RoundGist ExtractGist(StageType type) => new RoundGist {owner = this, Type = type};
    }

    public class RoundLib:IList<RoundData>
    {
        [NonSerialized]
        [OdinSerialize]
        private List<RoundData> core;

        private bool _endless;
        
        public bool HasBossRound => core.Last().RoundTypeData == RoundType.Boss;
        public StageType? BossStage => HasBossRound ? core.Last().bossStageType : (StageType?) null;
        public StageType BossStageVal
        {
            get
            {
                if (BossStage.HasValue)
                {
                    return BossStage.Value;
                }
                throw new ArgumentException("this lib has no bossStage.");
            }
        }
        public int NormalRoundCount => HasBossRound ? core.Count - 1 : core.Count;
        public bool Endless
        {
            get
            {
                if (HasBossRound&&_endless)
                {
                    throw new Exception("a round lib couldn't has boss and being endless");
                }
                return _endless;
            }
        }
        
        public AdditionalBossSetupBase PeekBossRoundGist
        {
            get
            {
                return null;
                /*var test = BossMasterMgr.Instance.BossLibDic[BossStageVal];
                var type = test.GetType();*/
            }
        }

        private RoundData GetCurrentRound(int step,out int truncatedStep)
        {
            var tmpStep = step;
            var currentRoundData = core[0];
            truncatedStep = 0;
            foreach (var neoRoundData in core)
            {
                tmpStep -= neoRoundData.TotalLength;
                if (tmpStep >= 0) continue;
                currentRoundData = neoRoundData;
                truncatedStep = tmpStep + currentRoundData.TotalLength;
                break;
            }
            return currentRoundData;
        }
        
        public RoundGist GetCurrentRoundGist(int step)
        {
            var round = GetCurrentRound(step,out var truncatedStep);
            var stage = GetCurrentType(step);
            return round.ExtractGist(stage);
        }
        
        public StageType GetCurrentType(int step)
        {
            var currentRound=GetCurrentRound(step, out int truncatedStep);
            return currentRound.GetCurrentType(truncatedStep);
        }

        public int GetTruncatedStep(int step)
        {
            GetCurrentRound(step, out var res);
            return res;
        }

        public RoundLib()//这个玩意儿必须要一个无参构造器
        {
            _endless = false;
            core = new List<RoundData>();
        }
        
        public RoundLib(bool endless)
        {
            _endless = endless;
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
}
