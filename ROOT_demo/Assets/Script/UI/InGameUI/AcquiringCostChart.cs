using com.ootii.Messages;
using ROOT.Common;
using ROOT.Message.Inquiry;
using ROOT.SetupAsset;
using TMPro;

namespace ROOT.UI
{
    public class AcquiringCostChart : CostChart
    {
        //这里现在需要调整一下显示；需要显示出基础收入和额外输入。

        protected override UITag UITag => UITag.Currency_Acquiring;

        public TextMeshPro TgtCurrency;//这个数据一局中不会变，所以原则上应该不用cache
        public TextMeshPro BonusCurrency;
        public TextMeshPro Slash;//那个杠杠，要统一改一下颜色。

        private void UpdateBonusIncomeVal(int bonusIncomesVal)
        {
            if (bonusIncomesVal > 0)
            {
                BonusCurrency.text = "+" + Utils.PaddingNum(bonusIncomesVal, 2);
                BonusCurrency.color = ColorLibManager.Instance.ColorLib.ROOT_UI_HIGHLIGHTING_GREEN;
            }
            else if (bonusIncomesVal == 0)
            {
                BonusCurrency.text = "+00";
                BonusCurrency.color = ColorLibManager.Instance.ColorLib.ROOT_UI_DEFAULT_DARKGRAY;
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
            BonusCurrency.color = ColorLibManager.Instance.ColorLib.ROOT_UI_DEFAULT_BLACK;
        }

        protected override void Awake()
        {
            base.Awake();
            TgtCurrency.color = ColorLibManager.Instance.ColorLib.ROOT_UI_HIGHLIGHTING_GREEN;
            Slash.color = ColorLibManager.Instance.ColorLib.ROOT_UI_HIGHLIGHTING_GREEN;
            MessageDispatcher.SendMessage(new AcquiringCostTargetInquiryData
            {
                AcquiringCostTargetCallBack = tgt => TgtCurrency.text = tgt.ToString("D4")//TODO 怎么和数值位数和面积匹配是个问题。
            });
        }
    }
}