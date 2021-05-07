using System;
using ROOT.Common;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Internal;

namespace ROOT.SetupAsset
{
    [Serializable]
    public struct TutorialActionData
    {
        [VerticalGroup("BasicData"), LabelText("Main Idx"), TableColumnWidth(300, false)]
        public int ActionIdx;

        [VerticalGroup("BasicData"), LabelText("Sub Idx"), TableColumnWidth(300, false)]
        public int ActionSubIdx;

        [VerticalGroup("BasicData"), LabelText("Type"), TableColumnWidth(300, false), EnumPaging]
        public TutorialActionType ActionType;

        [VerticalGroup("DetailedData"), ShowIf("ActionType", TutorialActionType.Text), LabelWidth(30), MultiLineProperty]
        public string Text;

        [VerticalGroup("DetailedData"), ShowIf("ActionType", TutorialActionType.Text)]
        public bool DoppelgangerToggle;

        [VerticalGroup("DetailedData"), LabelWidth(135)]
        [ShowIf("@this.ActionType==TutorialActionType.Text&&this.DoppelgangerToggle")]
        public string DoppelgangerText;

        [VerticalGroup("DetailedData")]
        [ShowIf("@this.ActionType==TutorialActionType.CreateUnit||this.ActionType==TutorialActionType.CreateCursor||this.ActionType==TutorialActionType.MoveCursorToPos")]
        public Vector2Int Pos;

        [VerticalGroup("DetailedData"), ShowIf("ActionType", TutorialActionType.CreateUnit)]
        public SignalType Core;

        [VerticalGroup("DetailedData"), ShowIf("ActionType", TutorialActionType.CreateUnit)]
        public HardwareType HardwareType;

        [VerticalGroup("DetailedData"), ShowIf("ActionType", TutorialActionType.CreateUnit)]
        public SideType[] Sides;

        [VerticalGroup("DetailedData"), ShowIf("ActionType", TutorialActionType.CreateUnit), Range(1, 5)]
        public int Tier;

        [VerticalGroup("DetailedData"), ShowIf("ActionType", TutorialActionType.CreateUnit)]
        public bool IsStationary;

        [VerticalGroup("DetailedData"), ShowIf("ActionType", TutorialActionType.CreateUnit)]
        public UnitTag Tag;

        [VerticalGroup("DetailedData"), ShowIf("ActionType", TutorialActionType.HandOn)]
        public TutorialCheckType HandOnCheckType;

        [VerticalGroup("DetailedData"), ShowIf("ActionType", TutorialActionType.HandOn)]
        public String HandOnMission;

        [VerticalGroup("DetailedData"), ShowIf("ActionType", TutorialActionType.HandOn)]
        public int TimeOutSec;//0的话就强制60sec
        
        [VerticalGroup("DetailedData"), ShowIf("@this.ShowUnitTag()")]
        public UnitTag TargetTag;

        [VerticalGroup("DetailedData"), ShowIf("@this.ShowSet()")]
        public bool Set; //Or unset

        [VerticalGroup("DetailedData"), LabelText("Set"), ShowIf("ActionType", TutorialActionType.HighLightUI)]
        public bool HLSet; //Or unset

        [VerticalGroup("DetailedData")]
        [LabelText("Custom HighLight Pos")]
        [ShowIf("@this.ActionType==TutorialActionType.HighLightUI&&this.HLSet")]
        public bool HLCustomPos;

        [VerticalGroup("DetailedData")]
        [ShowIf("@this.ActionType==TutorialActionType.HighLightUI&&this.HLSet&&this.HLCustomPos")]
        public Vector2 HighLightingPos;

        [VerticalGroup("DetailedData")]
        [ShowIf("@this.ActionType==TutorialActionType.HighLightUI&&this.HLSet&&this.HLCustomPos")]
        public Vector2 HighLightingScale;

        [VerticalGroup("DetailedData")] [LabelText("Select All")] [ShowIf("@this.ShowAllClear()")]
        public bool AllClear;

        [VerticalGroup("DetailedData")] [ShowIf("@this.ShowUITag()")]
        public UITag UITag;

        [VerticalGroup("DetailedData")] [ShowIf("ActionType", TutorialActionType.SetTimeline)]
        public TimeLineStatus TimeLineStatus;

        [VerticalGroup("DetailedData")] [ShowIf("@this.ActionType==TutorialActionType.HighLightGrid&&this.Set")]
        public GridHighLightType HighLightType;

        [VerticalGroup("DetailedData")] [ShowIf("@this.ShowPoses()")]
        public Vector2Int[] poses;

        [VerticalGroup("DetailedData")] [ShowIf("ActionType", TutorialActionType.ToggleFSMCoreFeat)]
        public FSMFeatures FSMCoreFeat;

        [VerticalGroup("DetailedData")]
        [ShowIf("ActionType", TutorialActionType.ToggleTutorialHintPage)]
        [ShowIf("Set")]
        public int Numpage;

        bool ShowSet()
        {
            var IsToggleFSMCoreFeat = ActionType == TutorialActionType.ToggleFSMCoreFeat;
            var IsToggleGameplayUI = ActionType == TutorialActionType.ToggleGameplayUI;
            var IsSetUnitStationary = ActionType == TutorialActionType.SetUnitStationary;
            var HighLightGrid = ActionType == TutorialActionType.HighLightGrid;
            var ToggleTutorialHintPage = ActionType == TutorialActionType.ToggleTutorialHintPage;
            return IsToggleFSMCoreFeat || IsToggleGameplayUI || IsSetUnitStationary || HighLightGrid ||
                   ToggleTutorialHintPage;
        }

        bool ShowUnitTag()
        {
            var IsSetUnitStationary = ActionType == TutorialActionType.SetUnitStationary;
            var IsMoveCursorToUnitByTag = ActionType == TutorialActionType.MoveCursorToUnitByTag;
            var IsDeleteUnit = ActionType == TutorialActionType.DeleteUnit;
            return IsSetUnitStationary || IsMoveCursorToUnitByTag || (IsDeleteUnit && (!AllClear));
        }

        bool ShowUITag()
        {
            var isToggleGameplayUI = ActionType == TutorialActionType.ToggleGameplayUI;
            var isHighLightUI = ActionType == TutorialActionType.HighLightUI;
            var notAllClear = Set || !AllClear;
            return (isToggleGameplayUI && notAllClear) || (isHighLightUI && HLSet && !HLCustomPos);
        }

        bool ShowAllClear()
        {
            var isToggleGameplayUI = ActionType == TutorialActionType.ToggleGameplayUI;
            var isHighLightGrid = ActionType == TutorialActionType.HighLightGrid;
            var isDeleteUnit = ActionType == TutorialActionType.DeleteUnit;
            return isDeleteUnit || (isToggleGameplayUI && !Set) || (isHighLightGrid && !Set);
        }

        bool ShowPoses()
        {
            var notAllClear = Set || !AllClear;
            return ActionType == TutorialActionType.HighLightGrid && notAllClear;
        }
    }
}