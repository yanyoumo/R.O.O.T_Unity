using com.ootii.Messages;
using ROOT.Message.Inquiry;
using TMPro;
using UnityEngine;

namespace ROOT.UI
{
    public class AcquiringCostChart : CostChart
    {
        //这里现在需要调整一下显示；需要显示出基础收入和额外输入。
        
        public TextMeshPro TgtCurrency;//这个数据一局中不会变，所以原则上应该不用cache
        public TextMeshPro BonusCurrency;//这个数据一局中不会变，所以原则上应该不用cache

        private void UpdateBonusIncomeVal(int bonusIncomesVal)
        {
            if (bonusIncomesVal > 0)
            {
                BonusCurrency.text = "+" + Common.Utils.PaddingNum(bonusIncomesVal, 2);
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

        protected override void Awake()
        {
            base.Awake();
            MessageDispatcher.SendMessage(new AcquiringCostTargetInquiry
            {
                AcquiringCostTargetCallBack = (tgt) => TgtCurrency.text = tgt.ToString("D4")//TODO 怎么和数值位数和面积匹配是个问题。
            });
        }
    }
}