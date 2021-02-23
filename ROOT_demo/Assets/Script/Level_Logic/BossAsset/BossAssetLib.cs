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
        [OdinSerialize]
        public AdditionalBossSetupBase BossSetup=new TelemetryAdditionalData();
    }

    [Serializable]
    [CreateAssetMenu(fileName = "NewAssetLib", menuName = "BossAsset/New BossAsset")]
    public class BossAssetLib : SerializedScriptableObject
    {
        public BossAsset[] BossLib;
    }
}