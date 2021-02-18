using System;
using com.ootii.Messages;
using ROOT.Message;
using TMPro;
using UnityEngine;
using static ROOT.WorldEvent.Visual_Event;

namespace ROOT
{
    public abstract class RoundRelatedUIBase : MonoBehaviour
    {
        protected StageType StageType = StageType.Shop;
        
        protected virtual void RoundTypeChangedHandler(IMessage rmessage)
        {
            if (rmessage is TimingEventInfo info)
            {
                StageType = info.CurrentStageType;
            }
        }
        
        protected virtual void Awake()
        {
            MessageDispatcher.AddListener(WorldEvent.Timing_Event.InGameStatusChangedEvent, RoundTypeChangedHandler);
        }

        protected virtual void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.Timing_Event.InGameStatusChangedEvent, RoundTypeChangedHandler);
        }
    }
    
    public class CurrencyUpdatedInfo : RootMessageBase
    {
        public int IncomesVal = -1;
        public int CurrencyVal = -1;
        public override string Type => CurrencyUpdatedEvent;
    }

    public class CurrencyInquiryInfo : RootMessageBase
    {
        public Func<CurrencyUpdatedInfo, bool> CallBack;
        public override string Type => WorldEvent.Visual_Inquiry_Event.CurrencyInquiryEvent;
    }
    
    public class CostChart : RoundRelatedUIBase
    {
        public TextMeshPro Currency;
        public TextMeshPro Incomes;

        private bool RequestCostChartInfoCallBack(CurrencyUpdatedInfo info)
        {
            CostChartUpdateCore(info.CurrencyVal, info.IncomesVal);
            return true;
        }
        
        private void UpdateCurrencyVal(int currencyVal)
        {
            Currency.text = Utils.PaddingNum(currencyVal, 4);
        }

        private void UpdateIncomeValAsNotActive()
        {
            Incomes.text = "---";
            Incomes.color = Color.black;
        }
        
        private void UpdateIncomeVal(int incomesVal)
        {
            if (incomesVal > 0)
            {
                Incomes.text = Utils.PaddingNum(incomesVal, 3);
                Incomes.color = Color.green;
            }
            else if (incomesVal == 0)
            {
                Incomes.text = "000";
                Incomes.color = Color.red;
            }
            else
            {
                Incomes.text = "-" + Utils.PaddingNum(Math.Abs(incomesVal), 2);
                Incomes.color = Color.red;
            }
        }

        private void CostChartUpdateHandler(IMessage rMessage)
        {
            if (rMessage is CurrencyUpdatedInfo info)
            {
                CostChartUpdateCore(info.CurrencyVal, info.IncomesVal);
            }
        }

        private void CostChartUpdateCore(int currencyVal, int incomesVal)
        {
            if (currencyVal >= 0)
            {
                UpdateCurrencyVal(currencyVal);
            }

            switch (StageType)
            {
                case StageType.Shop:
                case StageType.Telemetry:
                case StageType.Ending:
                    UpdateIncomeValAsNotActive();
                    break;
                case StageType.Require:
                case StageType.Destoryer:
                    if (incomesVal >= 0)
                    {
                        UpdateIncomeVal(incomesVal);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void RoundTypeChangedHandler(IMessage rmessage)
        {
            base.RoundTypeChangedHandler(rmessage);
            if (rmessage is TimingEventInfo info)
            {
                var message = new CurrencyInquiryInfo {CallBack = RequestCostChartInfoCallBack};
                MessageDispatcher.SendMessage(message);
            }
        }
        
        protected override void Awake()
        {
            base.Awake();
            MessageDispatcher.AddListener(CurrencyUpdatedEvent, CostChartUpdateHandler);
        }
    }
}