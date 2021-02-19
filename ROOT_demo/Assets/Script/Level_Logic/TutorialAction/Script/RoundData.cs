using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Linq;

namespace ROOT
{
    [Serializable]
    public struct RoundDatasGist
    {
        public int NormalRoundCount;
        public bool HasBossRound;
        [ShowIf("HasBossRound")]
        [ValueDropdown("BossStageFliter")]
        public StageType BossStage;
        //下面Boss的数量提出来放成一个好配置的。
        private static IEnumerable<StageType> BossStageFliter = Enumerable.Range((int)StageType.Telemetry, 2).Cast<StageType>();
    }
    
    public struct RoundGist
    {
        //这个Struct里面的数据千万不能随便删、Rider虽然显示没有引用、但是！
        //这个可以通过scriptableObject存东西的！！！
        //删了之后可能就炸了。
        public int ID;
        public StageType Type;
        public int normalReq;
        public int networkReq;
        public int shopLength;
        public int[] HSSwTruncatedIdx;

        public int TelemetryLength;
        public int DestoryerCount;
        public int InfoCount;

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

        public RoundType RoundType;
        
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
        [Range(0, 100)]
        public int DestoryerLength;
        [Range(0, 20)]
        public int DestoryerCount;
        [Range(0, 20)]
        public int InfoCount;

        public int TotalLength => ShopLength + RequireLength + DestoryerLength;

        public bool InRange(int truncatedCount)
        {
            return truncatedCount < TotalLength;
        }

        public StageType? CheckStage(int truncatedCount,bool isFinalRound)
        {
            //RISK 这里现在把最后一个Round的Destoryer部分变成Boss阶段。
            //Hmmmm还是尽量少改代码的狗皮膏药，先逻辑和相关东西弄明白后这里得重新搞。
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
                    if (isFinalRound && dic[idx].Item1 == StageType.Destoryer)
                    {
                        return StageType.Telemetry;
                    }
                    return dic[idx].Item1;
                }

                truncatedCount -= dic[idx].Item2;
                idx++;
            } while (idx < dic.Count);

            return null;
        }
    }
}
