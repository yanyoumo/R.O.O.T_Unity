using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

namespace ROOT
{
    public partial class HintMaster : MonoBehaviour
    {
        public ShopPostalHint shopPostalHint;
    }

    //TODO Yuxuan为文本方向工作。
    //1、参考项目中的文本文件：2020_9_3_Textfile.txt
    //将其从新排版和转换内容，填充到：
    //      GamePlayAdditiveVisualScene => HintTextA (1)
    //的文本框中。
    //备注：需要高度重写和重排版文本内容，仅保留其含义即可。
    //2、参考静态类ScriptLocalization，创建颜色库（一个新的静态类）。将使用的到的颜色使用HEX string储存起来。
    //3（优先级下移）、现在如下信息进入：Localization.xlsx中，
    //的BasicControl_KM和BasicControl_Touch节中（包括英文内容）。
    //信息：
    //{点击左Ctrl可强制演进一步。（键鼠版）
    // 按住屏幕中空白位置可强制演进一步。（触摸版）}
    //备注：参考已有内容重排版文本内容，仅保留其含义即可。
    //录入完毕后，另存为基于UTF-8的CSV文件（本地覆盖集合）
    //之后整合进项目中的i2Localizer部分。

    public class ShopPostalHint : MonoBehaviour
    {
        public Localize AdvShopTextLocalizer;
        public Localize AdvShopTextTwoLocalizer;
        public LocalizationParamsManager LocalizerMessageParam;
        public int PostalPrice
        {
            set => LocalizerMessageParam.SetParameterValue("VALUE", value + "");
        }

        void Start()
        {
            if (StartGameMgr.UseTouchScreen)
            {
                AdvShopTextLocalizer.SetTerm(ScriptTerms.AdvShop_Touch);
                AdvShopTextTwoLocalizer.SetTerm(ScriptTerms.AdvShop_2_Touch);
            }
            else
            {
                AdvShopTextLocalizer.SetTerm(ScriptTerms.AdvShop_KM);
                AdvShopTextTwoLocalizer.SetTerm(ScriptTerms.AdvShop_2_KM);
            }
        }
    }
}