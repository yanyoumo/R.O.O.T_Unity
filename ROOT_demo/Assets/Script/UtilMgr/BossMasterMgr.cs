using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using ROOT.SetupAsset;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT.SetupAsset
{
    [Serializable]
    public class AdditionalBossSetupBase:ScriptableObject
    {
        public int BossLength;
    }
    
    public class BossMasterMgr : MonoBehaviour
    {
        public BossAssetLib BossLib;
        
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

