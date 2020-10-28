using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
    //继承后就不能序列化？
    [Serializable]
    public class SkillBase
    {
        [VerticalGroup("Basic Data"),TableColumnWidth(200,Resizable = false)]
        [ReadOnly]
        [PropertyOrder(-2)]
        public readonly SkillType SklType;
        [VerticalGroup("Basic Data"), TableColumnWidth(200, Resizable = false)]
        [ReadOnly]
        [PropertyOrder(-1)]
        public readonly int Tier;

        [ShowIf("@this.SklType != SkillType.FastForward")]
        [PropertyOrder(0)]
        public int Cost;
        [ShowIf("@this.SklType == SkillType.TimeFromMoney")]
        [PropertyOrder(1)]
        public int TimeGain;
        [ShowIf("@this.SklType == SkillType.FastForward")]
        [PropertyOrder(2)]
        public int FastForwardCount;
        [ShowIf("@this.SklType == SkillType.FastForward")]
        [PropertyOrder(3)]
        public float AdditionalIncome;
        [ShowIf("@this.SklType == SkillType.Swap")]
        [PropertyOrder(4)]
        public int radius;
        [ShowIf("@this.SklType == SkillType.Discount")]
        [PropertyOrder(5)]
        public float Discount;

        public SkillBase(SkillType sklType,int _tier)
        {
            this.SklType = sklType;
            Tier = _tier;
        }
    }

    [Serializable]
    [CreateAssetMenu(fileName = "NewSkillData", menuName = "SkillData/New SkillData")]
    public class SkillData : ScriptableObject
    {
        [TableList(ShowIndexLabels = true, DrawScrollView = false)]
        public List<SkillBase> SkillDataList = new List<SkillBase>()
        {
            new SkillBase(SkillType.FastForward,1),
            new SkillBase(SkillType.FastForward,2),
            new SkillBase(SkillType.TimeFromMoney,1),
            new SkillBase(SkillType.TimeFromMoney,2),
            new SkillBase(SkillType.Swap,1),
            new SkillBase(SkillType.Swap,2),
            new SkillBase(SkillType.Swap,3),
            new SkillBase(SkillType.Discount,1),
            new SkillBase(SkillType.RefreshHeatSink,1),
            new SkillBase(SkillType.RefreshHeatSink,2),
        };
    }
}