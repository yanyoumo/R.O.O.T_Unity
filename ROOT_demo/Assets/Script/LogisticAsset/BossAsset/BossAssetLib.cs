using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT.SetupAsset
{
    
    [Serializable]
    public class BossAdditionalSetupAsset
    {
        [HideInInspector] public BossStageType BossStageTypeVal;

        public int BossLength;
        
        [ShowIf("@BossStageTypeVal==BossStageType.Telemetry")] public int DestoryerCount;
        [ShowIf("@BossStageTypeVal==BossStageType.Telemetry")] public int InfoCount;
        [ShowIf("@BossStageTypeVal==BossStageType.Telemetry")] public int InfoVariantRatio;
        [ShowIf("@BossStageTypeVal==BossStageType.Telemetry")] public int InfoTargetRatio;
        [ShowIf("@BossStageTypeVal==BossStageType.Acquiring")] public int AcquiringTarget;
    }
    
    [Serializable]
    public class BossAsset
    {
        public StageType BossStageType;
    }

    [Serializable]
    [CreateAssetMenu(fileName = "NewAssetLib", menuName = "BossAsset/New BossAsset")]
    public class BossAssetLib : SerializedScriptableObject
    {
        public BossAsset[] BossLib;
    }
}