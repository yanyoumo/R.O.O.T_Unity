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

        //[ReadOnly] public RoundType RoundTypeData;

        [Range(0, 60)]
        public int ShopLength;

        [Space] [Range(0, 30)]
        public int RequireLength;

        [HorizontalGroup("Split")] [VerticalGroup("Split/Left")]
        public int TypeARequirement;

        [VerticalGroup("Split/Right")]
        public int TypeBRequirement;

        [Space] [Range(0, 60)]
        public int HeatSinkLength;

        [ShowInInspector] public int TotalLength => ShopLength + RequireLength + HeatSinkLength;

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
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public StageType GetCurrentType(int truncatedStep)
        {
            if (truncatedStep <= TotalLength)
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
        
        //[NonSerialized]
        //[OdinSerialize]
        public bool _hasBossStage;
        
        [NonSerialized]
        [OdinSerialize]
        public BossStageType _bossStageType;
        
        [NonSerialized]
        [OdinSerialize]
        public bool _endless;
        
        public bool HasBossRound => _hasBossStage;
        public BossStageType? BossStage => HasBossRound ? _bossStageType : (BossStageType?) null;
        public BossStageType BossStageVal
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

        private RoundData GetCurrentRound(int step, out int truncatedStep,out bool normalRoundEnded)
        {
            var tmpStep = step;
            var currentRoundData = core[0];
            truncatedStep = 0;
            normalRoundEnded = true;
            foreach (var neoRoundData in core)
            {
                tmpStep -= neoRoundData.TotalLength;
                if (tmpStep < 0)
                {
                    currentRoundData = neoRoundData;
                    truncatedStep = tmpStep + currentRoundData.TotalLength;
                    normalRoundEnded = false;
                    break;
                }
            }
            return currentRoundData;
        }

        public RoundGist GetCurrentRoundGist(int step)
        {
            var round = GetCurrentRound(step,out var truncatedStep,out var normalRoundEnded);
            if (!normalRoundEnded)
            {
                var stage = GetCurrentType(step);
                return round.ExtractGist(stage);
            }

            return new RoundGist {owner = core[0], Type = StageType.Boss};
        }
        
        public StageType GetCurrentType(int step)
        {
            var currentRound=GetCurrentRound(step, out int truncatedStep,out var normalRoundEnded);
            return !normalRoundEnded ? currentRound.GetCurrentType(truncatedStep) : StageType.Boss;
        }

        public int GetTruncatedStep(int step)
        {
            GetCurrentRound(step, out var res, out var B);
            return res;
        }

        public RoundLib()//这个玩意儿必须要一个无参构造器
        {
            _endless = false;
            core = new List<RoundData>();
        }
        
        public RoundLib(bool endless,bool hasBossStage,BossStageType bossStageType)
        {
            _endless = endless;
            _hasBossStage = hasBossStage;
            _bossStageType = bossStageType;
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
