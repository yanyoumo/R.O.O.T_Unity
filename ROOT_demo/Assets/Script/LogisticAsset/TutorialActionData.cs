using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT.SetupAsset
{
    [Serializable]
    public struct TutorialActionData
    {
        public int ActionIdx;
        public TutorialActionType ActionType;

        [Space]
        [ShowIf("ActionType", TutorialActionType.Text)] [LabelWidth(30)]
        public string Text;

        [ShowIf("ActionType", TutorialActionType.Text)] [HorizontalGroup("Doppelganger")] [LabelWidth(135)]
        public bool DoppelgangerToggle;

        [ShowIf("@this.ActionType==TutorialActionType.Text&&this.DoppelgangerToggle")]
        [HorizontalGroup("Doppelganger")]
        [LabelWidth(135)]
        public string DoppelgangerText;

        [Space]
        [ShowIf("@this.ActionType==TutorialActionType.CreateUnit||this.ActionType==TutorialActionType.CreateCursor")]
        public Vector2Int Pos;

        [ShowIf("ActionType", TutorialActionType.CreateUnit)]
        public SignalType Core;

        [ShowIf("ActionType", TutorialActionType.CreateUnit)]
        public HardwareType HardwareType;

        [ShowIf("ActionType", TutorialActionType.CreateUnit)]
        public SideType[] Sides;

        [ShowIf("ActionType", TutorialActionType.CreateUnit)] [Range(1, 5)]
        public int Tier;

        [ShowIf("ActionType", TutorialActionType.CreateUnit)]
        public bool IsStationary;

        [ShowIf("ActionType", TutorialActionType.CreateUnit)]
        public UnitTag Tag;
        
        [Space]
        [ShowIf("ActionType", TutorialActionType.HandOn)]
        public TutorialCheckType HandOnCheckType;
        
        [ShowIf("ActionType", TutorialActionType.HandOn)]
        public String HandOnMission;

        [Space] 
        [ShowIf("ActionType", TutorialActionType.SetUnitStationary)]
        public UnitTag TargetTag;
        
        [Space] 
        [ShowIf("ActionType", TutorialActionType.SetUnitStationary)]
        public bool Set;//Or unset
    }
}