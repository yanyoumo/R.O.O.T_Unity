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

        [ShowIf("ActionType", TutorialActionType.CreateUnit)] [HorizontalGroup("Split")] [VerticalGroup("Split/Left")]
        public SignalType Core;

        [ShowIf("ActionType", TutorialActionType.CreateUnit)] [VerticalGroup("Split/Left")]
        public HardwareType HardwareType;

        [ShowIf("ActionType", TutorialActionType.CreateUnit)] [VerticalGroup("Split/Right")]
        public SideType[] Sides;

        [ShowIf("ActionType", TutorialActionType.CreateUnit)] [Range(1, 5)]
        public int Tier;

        [Space]
        [ShowIf("ActionType", TutorialActionType.HandOn)]
        public TutorialCheckType HandOnCheckType;
    }
}