using System;
using com.ootii.Messages;
using ROOT.Message;
using UnityEngine;

namespace ROOT
{
    public class CurrencyInquiryInfo : RootMessageBase
    {
        public Func<CurrencyUpdatedInfo, bool> CallBack;
        public override string Type => WorldEvent.Visual_Inquiry_Event.CurrencyInquiryEvent;
    }
    public sealed class FSMEventInquiryResponder
    {
        private FSMLevelLogic owner;

        private void CurrencyInquiryHandler(IMessage rMessage)
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
        }
    }
}
