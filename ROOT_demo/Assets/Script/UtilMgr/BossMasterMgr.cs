using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ROOT.SetupAsset;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT.SetupAsset
{
    /*[Serializable]
    public abstract class AdditionalBossSetupBase
    {
        public int BossLength;
    }*/
    
    public class BossMasterMgr : MonoBehaviour
    {
        public BossAssetLib BossLib;

        //TODO 这个东西先这样、就反正能用。
       /*public readonly Dictionary<BossStageType, AdditionalBossSetupBase> BossLibDic =
            new Dictionary<BossStageType, AdditionalBossSetupBase>
            {
                {BossStageType.Telemetry,new TelemetryAdditionalData()},
                {BossStageType.Acquiring,new AcquiringAdditionalData()},
            };*/
        
        [NotNull] private static BossMasterMgr _instance;
        public static BossMasterMgr Instance => _instance;
        
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }
        }
    }
}

