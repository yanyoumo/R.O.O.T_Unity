using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

namespace ROOT
{
    /*public partial class HintMaster : MonoBehaviour
    {
        [Obsolete]
        public ShopPostalHint shopPostalHint;
    }*/
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
            /*if (StartGameMgr.UseTouchScreen)
            {
                AdvShopTextLocalizer.SetTerm(ScriptTerms.AdvShop_Touch);
                AdvShopTextTwoLocalizer.SetTerm(ScriptTerms.AdvShop_2_Touch);
            }
            else
            {
                AdvShopTextLocalizer.SetTerm(ScriptTerms.AdvShop_KM);
                AdvShopTextTwoLocalizer.SetTerm(ScriptTerms.AdvShop_2_KM);
            }*/
            AdvShopTextLocalizer.SetTerm(ScriptTerms.AdvShop_KM);
            AdvShopTextTwoLocalizer.SetTerm(ScriptTerms.AdvShop_2_KM);
        }
    }
}