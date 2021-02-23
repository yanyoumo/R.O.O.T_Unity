using System;
using com.ootii.Messages;
using ROOT.Message;
using TMPro;
using UnityEngine;
using static ROOT.WorldEvent;

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
            MessageDispatcher.AddListener(InGameStatusChangedEvent, RoundTypeChangedHandler);
        }

        protected virtual void OnDestroy()
        {
            MessageDispatcher.RemoveListener(InGameStatusChangedEvent, RoundTypeChangedHandler);
        }
    }

    public class CostChart : RoundRelatedUIBase
    {
        public TextMeshPro Currency;
        public TextMeshPro Incomes;

        private int _cached_currencyVal;
        private int _cached_incomesVal;
            
        private void UpdateCachedData(int currencyVal, int incomesVal)
        {
            if (currencyVal != int.MaxValue) _cached_currencyVal = currencyVal;
            if (incomesVal != int.MaxValue) _cached_incomesVal = incomesVal;
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

        //TODO
        private BossStageType bossType = BossStageType.Telemetry;
        
        private void CostChartUpdateCore(int currencyVal, int incomesVal)
        {
            UpdateCachedData(currencyVal, incomesVal);
                
            UpdateCurrencyVal(_cached_currencyVal);
            switch (StageType)
            {
                case StageType.Boss:
                    if (bossType == BossStageType.Telemetry)
                    {
                        UpdateIncomeValAsNotActive();
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                case StageType.Shop:
                case StageType.Ending:
                    UpdateIncomeValAsNotActive();
                    break;
                case StageType.Require:
                case StageType.Destoryer:
                    UpdateIncomeVal(_cached_incomesVal);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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