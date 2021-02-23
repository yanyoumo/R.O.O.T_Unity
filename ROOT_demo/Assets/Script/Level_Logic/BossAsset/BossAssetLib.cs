using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace ROOT.SetupAsset
{
    
    [Serializable]
    public class BossAdditionalSetupAsset
    {
        [HideInInspector] public StageType BossStageType;

        [ShowIf("@BossStageType==StageType.Telemetry")] public int DestoryerCount;
        [ShowIf("@BossStageType==StageType.Telemetry")] public int InfoCount;
        [ShowIf("@BossStageType==StageType.Telemetry")] public int InfoVariantRatio;
        [ShowIf("@BossStageType==StageType.Telemetry")] public int InfoTargetRatio;
        [ShowIf("@BossStageType==StageType.Acquiring")] public int AcquiringTarget;
    }
    
    [Serializable]
    public class BossAsset
    {
        public StageType BossStageType;
        public AdditionalBossSetupBase BossSetup;
    }

    [Serializable]
    [CreateAssetMenu(fileName = "NewAssetLib", menuName = "BossAsset/New BossAsset")]
    public class BossAssetLib : SerializedScriptableObject
    {
        public BossAdditionalSetupAsset test;
        public BossAsset[] BossLib;
    }
}