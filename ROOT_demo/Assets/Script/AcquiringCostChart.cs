using System;
using com.ootii.Messages;
using ROOT.Message;
using TMPro;
using UnityEngine;

namespace ROOT
{
    public class AcquiringCostTargetInquiry : RootMessageBase
    {
        public Func<int,bool> AcquiringCostTargetCallBack;
        public override string Type => WorldEvent.AcquiringCostTargetInquiry;
    }
    
    public class AcquiringCostChart : CostChart
    {
        //这里现在需要调整一下显示；需要显示出基础收入和额外输入。
        
        public TextMeshPro TgtCurrency;//这个数据一局中不会变，所以原则上应该不用cache
        public TextMeshPro BonusCurrency;//这个数据一局中不会变，所以原则上应该不用cache

        private void UpdateBonusIncomeVal(int bonusIncomesVal)
        {
            if (bonusIncomesVal > 0)
            {
                BonusCurrency.text = "+" + Utils.PaddingNum(bonusIncomesVal, 2);
                BonusCurrency.color = Color.green;
            }
            else if (bonusIncomesVal == 0)
            {
                BonusCurrency.text = "+00";
                BonusCurrency.color = Color.grey;
            }
        }
        
        private void UpdateIncomeValWBonus()
        {
            UpdateIncomeVal(_cached_baseincomesVal);
            UpdateBonusIncomeVal(_cached_bonusincomesVal);
        }

        protected override void UpdateIncomeUI() => UpdateIncomeValWBonus();
        
        protected override void UpdateIncomeValAsNotActive()
        {
            base.UpdateIncomeValAsNotActive();
            BonusCurrency.text = "---";
            BonusCurrency.color = Color.black;
        }
        
        private bool UpdateTgtCurrencyText(int TgtCurrencyVal)
        {
            //TODO 怎么和数值位数和面积匹配是个问题。
            TgtCurrency.text = TgtCurrencyVal.ToString("D4");
            return true;
        }

        protected override void Awake()
        {
            base.Awake();
            MessageDispatcher.SendMessage(new AcquiringCostTargetInquiry {AcquiringCostTargetCallBack = UpdateTgtCurrencyText});
        }
    }
}