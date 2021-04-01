using System;
using System.Collections.Generic;
using ROOT;
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
    
    public class CurrencyUpdatedInfo : RootMessageBase
    {
        public int TotalIncomesVal = int.MaxValue;
        public int BaseIncomesVal = int.MaxValue;
        public int BonusIncomesVal = int.MaxValue;
        public int CurrencyVal = int.MaxValue;
        public override string Type => WorldEvent.CurrencyUpdatedEvent;
    }

    public class HintEventInfo : RootMessageBase
    {
        public HintEventType HintEventType;
        public bool BoolData;
        public String StringData = "";
        public override string Type => WorldEvent.HintRelatedEvent;
    }
    
    public class ShopTierOffsetChangedData:RootMessageBase
    {
        public bool UpwardOrDownward = true;
        public override string Type => WorldEvent.ShopTierOffsetChangedEvent;
    }
    
    namespace ROOT.Message.Inquiry
    {
        public class BalancingSignalSetupInquiry : RootMessageBase
        {
            public Action<Func<int, int, float>> BalancingSignalFuncCallBack;
            public override string Type => WorldEvent.BalancingSignalSetupInquiry;
        }
        
        public class AcquiringCostTargetInquiry : RootMessageBase
        {
            public Action<int> AcquiringCostTargetCallBack;
            public override string Type => WorldEvent.AcquiringCostTargetInquiry;
        }

        public class BoardGridThermoZoneInquiry : RootMessageBase
        {
            public Action<List<Vector2Int>> BoardGridThermoZoneInquiryCallBack;
            public override string Type => WorldEvent.BoardGridThermoZoneInquiry;
        }
    }
}