﻿using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
    //继承后就不能序列化？
    [Serializable]
    public class SkillDataUnit
    {
        [VerticalGroup("Basic Data"),TableColumnWidth(200,Resizable = false)]
        [ReadOnly]
        [PropertyOrder(-2)]
        public readonly SkillType SklType;
        [VerticalGroup("Basic Data"), TableColumnWidth(200, Resizable = false)]
        [ReadOnly]
        [PropertyOrder(-1)]
        public readonly int Tier;

        [PreviewField(Alignment = ObjectFieldAlignment.Center)]
        [PropertyOrder(0)]
        public Sprite SkillIcon;

        [ShowIf("@this.SklType != SkillType.FastForward")]
        [PropertyOrder(1)]
        public int Cost;
        //[ShowIf("@this.SklType != SkillType.FastForward")]
        [PropertyOrder(2)]
        public int CountLimit = -1;
        [ShowIf("@this.SklType == SkillType.TimeFromMoney")]
        [PropertyOrder(3)]
        public int TimeGain;
        [ShowIf("@this.SklType == SkillType.FastForward")]
        [PropertyOrder(4)]
        public int FastForwardCount;
        [ShowIf("@this.SklType == SkillType.FastForward")]
        [PropertyOrder(5)]
        public float AdditionalIncome;
        [ShowIf("@this.SklType == SkillType.Swap")]
        [PropertyOrder(6)]
        public int radius;
        [ShowIf("@this.SklType == SkillType.Discount")]
        [PropertyOrder(7)]
        public int Discount;



        public SkillDataUnit(SkillType sklType,int _tier)
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
        public List<SkillDataUnit> SkillDataList = new List<SkillDataUnit>()
        {
            new SkillDataUnit(SkillType.FastForward,1),
            new SkillDataUnit(SkillType.FastForward,2),
            new SkillDataUnit(SkillType.TimeFromMoney,1),
            new SkillDataUnit(SkillType.TimeFromMoney,2),
            new SkillDataUnit(SkillType.Swap,1),
            new SkillDataUnit(SkillType.Swap,2),
            new SkillDataUnit(SkillType.Swap,3),
            new SkillDataUnit(SkillType.Discount,1),
            new SkillDataUnit(SkillType.RefreshHeatSink,1),
            new SkillDataUnit(SkillType.ResetHeatSink,1),
        };
    }

    public class InstancedSkillData
    {
        public bool SkillEnabled = true;
        public int RemainingCount = -1;

        public readonly SkillType SklType;
        public readonly int Tier;
        public readonly int CountLimit = -1;
        public readonly Sprite SkillIcon;

        public readonly int Cost;
        public readonly int TimeGain;
        public readonly int FastForwardCount;
        public readonly float AdditionalIncome;
        public readonly int radius;
        public readonly int Discount;

        public InstancedSkillData(SkillDataUnit _skillDataUnit)
        {
            SklType = _skillDataUnit.SklType;
            Tier = _skillDataUnit.Tier;
            CountLimit = _skillDataUnit.CountLimit;
            RemainingCount = _skillDataUnit.CountLimit;
            SkillIcon = _skillDataUnit.SkillIcon;

            Cost = _skillDataUnit.Cost;
            TimeGain = _skillDataUnit.TimeGain;
            FastForwardCount = _skillDataUnit.FastForwardCount;
            AdditionalIncome = _skillDataUnit.AdditionalIncome;
            radius = _skillDataUnit.radius;
            Discount = _skillDataUnit.Discount;
        }
    }
}