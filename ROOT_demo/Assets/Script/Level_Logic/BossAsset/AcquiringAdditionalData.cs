using UnityEngine;

namespace ROOT.SetupAsset
{
    [CreateAssetMenu(fileName = "AcquiringAdditionalData", menuName = "BossAdditionalData/New AcquiringAdditionalData")]
    public class AcquiringAdditionalData : AdditionalBossSetupBase
    {
        public int AcquiringTarget;
    }
}