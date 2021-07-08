using System;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Linq;

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
        public int ID;
        
        [Range(0, 60)]
        public int ShopLength;

        [Space] [Range(0, 60)]
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
            if (truncatedStep < TotalLength)
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
}
