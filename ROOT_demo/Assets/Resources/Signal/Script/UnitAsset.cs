using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
    //public UnitRules Rules;
    //日了，这个东西只能通过Mono实例/Prefab指定；不好整。
    //这套流程由信号系统搞定了；这里就变成纯粹的硬件配置了（）
    [CreateAssetMenu(fileName = "NewUnitAsset", menuName = "Unit/New UnitAsset")]
    [Serializable]
    public class UnitAsset : ScriptableObject
    {
        [Title("Basic Data")]
        public SignalType UnitType;
        public CoreGenre UnitGenre;
        public float UnitPrice;
        [Title("Ratio Data")]
        [Range(0,100)]
        public int BaseRate;
        [Range(0,4)]
        public int NecessaryPortCount;
        [Range(0,100)]
        public int AdditionalPortRate;
        [Title("Aesthetic Data")]
        public Material UnitMat;
    }
}