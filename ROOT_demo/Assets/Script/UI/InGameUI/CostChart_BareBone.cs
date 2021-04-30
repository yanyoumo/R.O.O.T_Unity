using System;
using com.ootii.Messages;
using ROOT.Common;
using ROOT.Message;
using TMPro;
using UnityEngine;
using static ROOT.WorldEvent;

namespace ROOT.UI
{
    public class CostChart_BareBone : HideableUI
    {
        protected override UITag UITag => UITag.Currency_BareBone;
        
        public TextMeshPro Currency;
        public TextMeshPro Incomes;

        private int _cached_currencyVal;
        private int _cached_incomesVal;

        private void UpdateCachedData(int currencyVal, int incomesVal, int baseincomesVal, int bonusincomesVal)
        {
            if (currencyVal != int.MaxValue) _cached_currencyVal = currencyVal;
            if (incomesVal != int.MaxValue) _cached_incomesVal = incomesVal;
        }
        
        private void UpdateCurrencyVal(int currencyVal)
        {
            Currency.text = Utils.PaddingNum(currencyVal, 4);
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
                CostChartUpdateCore(info.CurrencyVal, info.TotalIncomesVal, info.BaseIncomesVal, info.BonusIncomesVal);
            }
        }

        protected virtual void UpdateIncomeUI() => UpdateIncomeVal(_cached_incomesVal);
        
        private void CostChartUpdateCore(int currencyVal, int incomesVal, int baseincomesVal, int bonusincomesVal)
        {
            UpdateCachedData(currencyVal, incomesVal, baseincomesVal, bonusincomesVal);
            UpdateCurrencyVal(_cached_currencyVal);
            UpdateIncomeVal(_cached_incomesVal);
        }

        protected override void Awake()
        {
            base.Awake();
            MessageDispatcher.AddListener(CurrencyUpdatedEvent, CostChartUpdateHandler);
        }

        protected override void OnDestroy()
        {
            MessageDispatcher.RemoveListener(CurrencyUpdatedEvent, CostChartUpdateHandler);
            base.OnDestroy();
        }
    }
}