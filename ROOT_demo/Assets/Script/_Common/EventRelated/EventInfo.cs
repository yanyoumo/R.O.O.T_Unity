using System;
using System.Collections.Generic;
using ROOT;
using ROOT.Common;
using ROOT.Consts;
using ROOT.Message;
using UnityEngine;

namespace ROOT.Message
{
    //这个是一些对时事件的总Payload；
    public class TimingEventInfo : RootMessageBase
    {
        public StageType CurrentStageType;
        public StageType NextStageType;
        public bool BoardCouldIOCurrencyData;
        public bool UnitCouldGenerateIncomeData;
        public int CountDownTiming => StaticNumericData.StageWarningThreshold;
        public override string Type { get; set; }
    }

    public class GamePauseInfo : RootMessageBase
    {
        public bool GamePaused;
        public override string Type => WorldEvent.GamePauseEvent;
    }
    
    public class CurrencyUpdatedInfo : RootMessageBase
    {
        public float TotalIncomesVal = int.MaxValue;
        public float BaseIncomesVal = int.MaxValue;
        public float BonusIncomesVal = int.MaxValue;
        public float CurrencyVal = int.MaxValue;
        public override string Type => WorldEvent.CurrencyUpdatedEvent;
    }

    public class HintEventInfo : RootMessageBase
    {
        public HintEventType HintEventType;
        public bool BoolData;
        public String StringData = "";
        public override string Type => WorldEvent.HintRelatedEvent;
    }

    public class ShopTierOffsetChangedData : RootMessageBase
    {
        public bool UpwardOrDownward = true;
        public override string Type => WorldEvent.ShopTierOffsetChangedEvent;
    }

    public class HighLightingUIChangedData : RootMessageBase
    {
        public bool Toggle = false;
        public bool CustomBool = false;
        public Vector2 pos = Vector2.zero;
        public Vector2 scale = Vector2.zero;
        public UITag uiTag = UITag.Currency_Career;
        public override string Type => WorldEvent.HighLightingUIChangedEvent;
    }

    public class HintPageChangedData : RootMessageBase
    {
        public bool TutorialOrGameplay = true;
        public bool Toggle = false;
        public bool AdditiveOrReplace = false;
        public int PageNum = 0;
        public override string Type => WorldEvent.HintScreenChangedEvent;
    }
    
    public class BoardGridHighLightSetData : RootMessageBase
    {
        public bool Set;
        public GridHighLightType HLType;
        public bool AllClear;
        public Vector2Int[] Poses;
        public override string Type => WorldEvent.BoardGridHighLightSetEvent;
    }

    public class ToggleGameplayUIData : RootMessageBase
    {
        public bool Set;
        public bool SelectAll;
        public UITag UITag;
        public override string Type => WorldEvent.ToggleGamePlayUIEvent;
    }
        
    public class CursorMovedEventData : RootMessageBase
    {
        public Vector2Int CurrentPosition;
        public override string Type => WorldEvent.CursorMovedEvent;
    }
    
    namespace Inquiry
    {
        public class BoardSignalUpdatedInfo : RootMessageBase
        {
            public BoardSignalUpdatedData SignalData;
            public override string Type => WorldEvent.BoardSignalUpdatedEvent;
        }
        
        public class BalancingSignalSetupInquiryData : RootMessageBase
        {
            public Action<Func<int, int, float>> BalancingSignalFuncCallBack;
            public override string Type => WorldEvent.BalancingSignalSetupInquiry;
        }

        public class AcquiringCostTargetInquiryData : RootMessageBase
        {
            public Action<int> AcquiringCostTargetCallBack;
            public override string Type => WorldEvent.AcquiringCostTargetInquiry;
        }

        public class CurrentSignalTypeInquiryData : RootMessageBase
        {
            public Action<SignalType, SignalType> CurrentSignalCallBack;
            public override string Type => WorldEvent.CurrentSignalTypeInquiry;
        }
    }
}