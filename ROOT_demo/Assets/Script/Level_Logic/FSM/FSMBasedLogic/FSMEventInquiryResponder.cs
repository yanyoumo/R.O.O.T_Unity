namespace ROOT
{
    /*public class CurrencyInquiryInfo : RootMessageBase
    {
        public Func<CurrencyUpdatedInfo, bool> CallBack;
        public override string Type => WorldEvent.Visual_Inquiry_Event.CurrencyInquiryEvent;
    }*/
    
    //这个框架看起来很fancy、但是复杂度比较高；先按住不用、UI模块以cache框架优先。
    //但是这个Responder框架也可以留下来。
    public sealed class FSMEventInquiryResponder
    {
        private FSMLevelLogic owner;

        public FSMEventInquiryResponder(FSMLevelLogic _owner)
        {
            owner = _owner;
        }

        /*private void CurrencyInquiryHandler(IMessage rMessage)
        {
            if (rMessage is CurrencyInquiryInfo info)
            {
                WorldExecutor.UpdateBoardData_Instantly(ref owner.LevelAsset);
                var message = new CurrencyUpdatedInfo()
                {
                    CurrencyVal = Mathf.RoundToInt(owner.LevelAsset.GameStateMgr.GetCurrency()),
                    IncomesVal = Mathf.RoundToInt(owner.LevelAsset.DeltaCurrency),
                };
                info.CallBack(message);
            }
        }

        public FSMEventInquiryResponder(FSMLevelLogic _owner)
        {
            owner = _owner;
            MessageDispatcher.AddListener(WorldEvent.Visual_Inquiry_Event.CurrencyInquiryEvent, CurrencyInquiryHandler);
        }

        ~FSMEventInquiryResponder()
        {
            MessageDispatcher.RemoveListener(WorldEvent.Visual_Inquiry_Event.CurrencyInquiryEvent, CurrencyInquiryHandler);
        }*/
    }
}
