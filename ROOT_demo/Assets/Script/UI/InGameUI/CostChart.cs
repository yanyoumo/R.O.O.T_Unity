using System;
using com.ootii.Messages;
using ROOT.Message;
using TMPro;
using UnityEngine;
using static ROOT.WorldEvent;

namespace ROOT.UI
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
            MessageDispatcher.AddListener(InGameStageChangedEvent, RoundTypeChangedHandler);
        }

        protected virtual void OnDestroy()
        {
            MessageDispatcher.RemoveListener(InGameStageChangedEvent, RoundTypeChangedHandler);
        }
    }

    public class CostChart : RoundRelatedUIBase
    {
        public TextMeshPro Currency;
        public TextMeshPro Incomes;

        private int _cached_currencyVal;
        protected int _cached_incomesVal;
        protected int _cached_baseincomesVal;
        protected int _cached_bonusincomesVal;

        protected override void RoundTypeChangedHandler(IMessage rmessage)
        {
            base.RoundTypeChangedHandler(rmessage);
            if (rmessage is TimingEventInfo info)
            {
                CostChartUpdateCore(int.MaxValue,int.MaxValue,int.MaxValue,int.MaxValue);
            }
        }

        private void UpdateCachedData(int currencyVal, int incomesVal, int baseincomesVal, int bonusincomesVal)
        {
            if (currencyVal != int.MaxValue) _cached_currencyVal = currencyVal;
            if (incomesVal != int.MaxValue) _cached_incomesVal = incomesVal;
            if (baseincomesVal != int.MaxValue) _cached_baseincomesVal = baseincomesVal;
            if (bonusincomesVal != int.MaxValue) _cached_bonusincomesVal = bonusincomesVal;
        }
        
        private void UpdateCurrencyVal(int currencyVal)
        {
            Currency.text = Common.Utils.PaddingNum(currencyVal, 4);
        }

        protected virtual void UpdateIncomeValAsNotActive()
        {
            Incomes.text = "---";
            Incomes.color = Color.black;
        }
        
        protected void UpdateIncomeVal(int incomesVal)
        {
            if (incomesVal > 0)
            {
                Incomes.text = Common.Utils.PaddingNum(incomesVal, 3);
                Incomes.color = Color.green;
            }
            else if (incomesVal == 0)
            {
                Incomes.text = "000";
                Incomes.color = Color.red;
            }
            else
            {
                Incomes.text = "-" + Common.Utils.PaddingNum(Math.Abs(incomesVal), 2);
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

        //TODO
        private BossStageType bossType = BossStageType.Telemetry;

        protected virtual void UpdateIncomeUI() => UpdateIncomeVal(_cached_incomesVal);
        
        private void CostChartUpdateCore(int currencyVal, int incomesVal, int baseincomesVal, int bonusincomesVal)
        {
            //Debug.Log("CostChartUpdateCore");
            UpdateCachedData(currencyVal, incomesVal, baseincomesVal, bonusincomesVal);
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
                        UpdateIncomeValAsNotActive();
                        //throw new NotImplementedException();
                    }
                    break;
                case StageType.Shop:
                case StageType.Ending:
                    //到这里有个问题、就是它是显示出来了，但是Cache的数据还是原来的。
                    //本来没问题、但是Cache的这个数据、是之前“没有收入”的时候拿到的；所以是0。
                    UpdateIncomeValAsNotActive();
                    break;
                case StageType.Require:
                case StageType.Destoryer:
                    UpdateIncomeUI();
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