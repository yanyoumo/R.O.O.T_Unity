using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT.Common
{
    public enum TimeLineStatus
    {
        Normal,
        NoToken,
        Disabled
    }
    
    public enum TimeLineTokenType
    {
        RequireNormal=0,//Green
        RequireNetwork = 1,//Blue
        DestoryerIncome = 2,//Red
        Ending = 3,//Black
        HeatSinkSwitch = 4,//ICON
        ShopOpened = 5,//
        BossStage = 6,//Purple
    }

    [Serializable]
    public class TimeLineToken: IComparable
    {
        public int TokenID;
        public TimeLineTokenType type;
        [ShowIf("@this.type==TimeLineTokenType.RequireNormal||this.type==TimeLineTokenType.RequireNetwork")]
        public int RequireAmount;
        public Vector2Int Range;//[Starting,Ending),Ending==-1 means Always

        public int CompareTo(object obj)
        {
            switch (obj)
            {
                case null:
                    return 1;
                case TimeLineToken other:
                    return (int) type - (int) other.type;
                default:
                    throw new ArgumentException("Object is not a TimeLineToken");
            }
        }

        public bool InRange(int count)
        {
            if (Range.y >= 0)
            {
                return count >= Range.x && count < Range.y;
            }
            else
            {
                return count >= Range.x;
            }
        }
    }
}