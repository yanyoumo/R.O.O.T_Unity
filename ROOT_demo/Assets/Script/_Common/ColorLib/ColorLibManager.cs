using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace ROOT.SetupAsset
{
    public class ColorLibManager : MonoBehaviour
    {
        [NotNull] private static ColorLibManager _instance;
        public static ColorLibManager Instance => _instance;
        
        public ColorLibAsset ColorLib;

        [Obsolete("这个放在Asset里面或者怎么管理。")]
        private Dictionary<SignalType, Color> SignalColorLib => new Dictionary<SignalType, Color>
        {
            {SignalType.Matrix, ColorLib.ROOT_SIGNAL_MATRIX},
            {SignalType.Cluster, ColorLib.ROOT_MASTER_CLUSTER},
            {SignalType.Scan, ColorLib.ROOT_SIGNAL_SCAN},
            {SignalType.Thermo, ColorLib.ROOT_SIGNAL_THREMO},
            {SignalType.Firewall, ColorLib.ROOT_SIGNAL_FIREWALL},
        };

        public Color GetColorBySignalType(SignalType signalType)
        {
            try
            {
                return SignalColorLib[signalType];
            }
            catch (KeyNotFoundException)
            {
                Debug.LogError("Key " + signalType + " is not present in Color Lib, please add.");
            }
            return Color.clear;
        }

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