using System;
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
        public CoreType UnitType;
        public CoreGenre UnitGenre;
        public int UnitPrice;
        public Material UnitMat;
    }
}