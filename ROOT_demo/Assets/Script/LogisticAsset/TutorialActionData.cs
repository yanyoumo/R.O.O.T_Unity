using System;
using ROOT.Common;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT.SetupAsset
{
    [Serializable]
    public struct TutorialActionData
    {
        [VerticalGroup("BasicData")] [LabelText("Main Idx")] [TableColumnWidth(300, false)]
        public int ActionIdx;

        [VerticalGroup("BasicData")] [LabelText("Sub Idx")] [TableColumnWidth(300, false)]
        public int ActionSubIdx;

        [VerticalGroup("BasicData")]
        [LabelText("Type")] 
        [TableColumnWidth(300, false)]
        [EnumPaging]
        public TutorialActionType ActionType;

        [VerticalGroup("DetailedData")] [ShowIf("ActionType", TutorialActionType.Text)] [LabelWidth(30)]
        public string Text;

        [VerticalGroup("DetailedData")] [ShowIf("ActionType", TutorialActionType.Text)]
        public bool DoppelgangerToggle;

        [VerticalGroup("DetailedData")]
        [ShowIf("@this.ActionType==TutorialActionType.Text&&this.DoppelgangerToggle")]
        [LabelWidth(135)]
        public string DoppelgangerText;

        [VerticalGroup("DetailedData")]
        [ShowIf("@this.ActionType==TutorialActionType.CreateUnit||this.ActionType==TutorialActionType.CreateCursor||this.ActionType==TutorialActionType.MoveCursorToPos")]
        public Vector2Int Pos;

        [VerticalGroup("DetailedData")] [ShowIf("ActionType", TutorialActionType.CreateUnit)]
        public SignalType Core;

        [VerticalGroup("DetailedData")] [ShowIf("ActionType", TutorialActionType.CreateUnit)]
        public HardwareType HardwareType;

        [VerticalGroup("DetailedData")] [ShowIf("ActionType", TutorialActionType.CreateUnit)]
        public SideType[] Sides;

        [VerticalGroup("DetailedData")] [ShowIf("ActionType", TutorialActionType.CreateUnit)] [Range(1, 5)]
        public int Tier;

        [VerticalGroup("DetailedData")] [ShowIf("ActionType", TutorialActionType.CreateUnit)]
        public bool IsStationary;

        [VerticalGroup("DetailedData")] [ShowIf("ActionType", TutorialActionType.CreateUnit)]
        public UnitTag Tag;

        [VerticalGroup("DetailedData")] [ShowIf("ActionType", TutorialActionType.HandOn)]
        public TutorialCheckType HandOnCheckType;

        [VerticalGroup("DetailedData")] [ShowIf("ActionType", TutorialActionType.HandOn)]
        public String HandOnMission;

        [VerticalGroup("DetailedData")]
        [ShowIf("@this.ActionType==TutorialActionType.SetUnitStationary||this.ActionType==TutorialActionType.MoveCursorToUnitByTag")]
        public UnitTag TargetTag;

        [VerticalGroup("DetailedData")]
        [ShowIf("@this.ActionType==TutorialActionType.ToggleGameplayUI||this.ActionType==TutorialActionType.SetUnitStationary||this.ActionType==TutorialActionType.HighLightGrid")]
        public bool Set; //Or unset

        [VerticalGroup("DetailedData")] [LabelText("Set")] [ShowIf("ActionType", TutorialActionType.HighLightUI)]
        public bool HLSet; //Or unset

        [VerticalGroup("DetailedData")][LabelText("Select All")]
        [ShowIf("@(this.ActionType==TutorialActionType.ToggleGameplayUI&&!this.Set)||(this.ActionType==TutorialActionType.HighLightGrid&&!this.Set)")]
        public bool AllClear;
        
        [VerticalGroup("DetailedData")] [ShowIf("@(this.ActionType==TutorialActionType.ToggleGameplayUI)&&(!(!this.Set&&this.AllClear))||(this.ActionType==TutorialActionType.HighLightUI&&this.HLSet)")]
        public UITag UITag;

        [VerticalGroup("DetailedData")] [ShowIf("ActionType", TutorialActionType.SetTimeline)]
        public TimeLineStatus TimeLineStatus;

        [VerticalGroup("DetailedData")] 
        [ShowIf("@this.ActionType==TutorialActionType.HighLightGrid&&this.Set")]
        public GridHighLightType HighLightType;

        [VerticalGroup("DetailedData")]
        [ShowIf("@this.ActionType==TutorialActionType.HighLightGrid&&(!(!this.Set&&this.AllClear))")]
        public Vector2Int[] poses;
    }
}