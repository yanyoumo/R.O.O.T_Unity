using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{ 
    [Serializable]
    public class UnitNeighbouringDataAsset
    {
        [VerticalGroup("LV1")]
        [HorizontalGroup("LV1/LV2-1")]
        public bool FliteringSignalType;
        [HorizontalGroup("LV1/LV2-1")]
        [ShowIf("FliteringSignalType")]
        public SignalType TargetingSignalType;
        [VerticalGroup("LV1")]
        [HorizontalGroup("LV1/LV2-2")]
        public bool FliteringHardwareType;
        [HorizontalGroup("LV1/LV2-2")]
        [ShowIf("FliteringHardwareType")]
        public HardwareType TargetingHardwareType;
        [VerticalGroup("LV1")]
        [LabelText("4Dir Or 8Dir")]
        public bool FourDirOrEightDir;
        [VerticalGroup("LV1")]
        public Texture NeighbouringSprite;
        [VerticalGroup("LV1")]
        public Color ColorTint;
    }
}