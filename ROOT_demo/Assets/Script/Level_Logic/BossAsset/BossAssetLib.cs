using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace ROOT.SetupAsset
{
    [Serializable]
    public class BossAsset
    {
        public StageType BossStageType;
        [OdinSerialize][ShowInInspector]
        public Type SetupBaseType;
    }

    [Serializable] [CreateAssetMenu(fileName = "NewAssetLib", menuName = "BossAsset/New BossAsset")]
    public class BossAssetLib:ScriptableObject
    {
        public BossAsset[] BossLib;
    }
}