using ROOT.Message;

namespace ROOT
{
    //这个是一些对时事件的总Payload；
    public class TimingEventInfo : RootMessageBase
    {
        public StageType CurrentStageType;
        public bool BoardCouldIOCurrencyData;
        public bool UnitCouldGenerateIncomeData;
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
}