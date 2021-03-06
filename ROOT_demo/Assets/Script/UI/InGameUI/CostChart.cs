﻿using System;
using com.ootii.Messages;
using ROOT.Common;
using ROOT.Message;
using ROOT.Message.Inquiry;
using ROOT.SetupAsset;
using TMPro;
using UnityEngine;
using static ROOT.WorldEvent;

namespace ROOT.UI
{
    public class CostChart : RoundRelatedUIBase
    {
        protected override UITag UITag => UITag.Currency_Career;

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
            Currency.text = Utils.PaddingNum(currencyVal, 4);
        }

        private Color TextRed => ColorLibManager.Instance.ColorLib.ROOT_UI_HIGHLIGHTING_RED;
        private Color TextGreen => ColorLibManager.Instance.ColorLib.ROOT_UI_HIGHLIGHTING_GREEN;
        private Color TextBlack => ColorLibManager.Instance.ColorLib.ROOT_UI_DEFAULT_BLACK;
        
        protected virtual void UpdateIncomeValAsNotActive()
        {
            Incomes.text = "---";
            Incomes.color = TextBlack;
        }
        
        protected void UpdateIncomeVal(int incomesVal)
        {
            if (incomesVal > 0)
            {
                Incomes.text = Utils.PaddingNum(incomesVal, 3);
                Incomes.color = TextGreen;
            }
            else if (incomesVal == 0)
            {
                Incomes.text = "000";
                Incomes.color = TextRed;
            }
            else
            {
                Incomes.text = "-" + Utils.PaddingNum(Math.Abs(incomesVal), 2);
                Incomes.color = TextRed;
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

        private void CostChartUpdateCore(float currencyVal, float incomesVal, float baseincomesVal, float bonusincomesVal)
        {
            CostChartUpdateCore(Mathf.RoundToInt(currencyVal), Mathf.RoundToInt(incomesVal), Mathf.RoundToInt(baseincomesVal), Mathf.RoundToInt(bonusincomesVal));
        }

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
            Currency.color = TextGreen;
            Incomes.color = TextGreen;
        }

        protected override void OnDestroy()
        {
            MessageDispatcher.RemoveListener(CurrencyUpdatedEvent, CostChartUpdateHandler);
            base.OnDestroy();
        }
    }
}