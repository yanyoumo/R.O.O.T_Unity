using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT.SetupAsset
{
    [CreateAssetMenu(fileName = "TelemetryAdditionalData", menuName = "BossAdditionalData/New TelemetryAdditionalData")]
    public class TelemetryAdditionalData : AdditionalBossSetupBase
    {
        public int DestoryerCount;
        public int InfoCount;
        public int InfoVariantRatio;
        public int InfoTargetRatio;
    }
}