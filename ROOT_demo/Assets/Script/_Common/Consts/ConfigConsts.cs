using System;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT.Configs
{
    //这里到是可以把老的Shop里面一些配置函数拉过来。但其实也没用了
    public static class ConfigCommons
    {
        public static Dictionary<SideType, float> PriceBySide = new Dictionary<SideType, float>
        {
            {SideType.NoConnection, 0.0f},
            {SideType.Connection, 2.0f},
        };

        /// <summary>
        /// 从Tier获取单元各种数据的倍率。
        /// </summary>
        /// <param name="Tier">位于哪个Tier</param>
        /// <returns>依次为（分数、购买价格、Cost）的float Tuple</returns>
        public static Tuple<float, float, float> TierMultiplier(int Tier)
        {
            //目前对Tier进行设定：
            //先确定需要由Tier影响的内容：
            //分数、购买价格、Cost。
            var SignalMultipler = (float) Tier;
            var PriceMultipler = 1.0f + 1.75f * (Tier - 1);//这个数据现在看起来太温柔、斜率绝对不能小于1.
            var CostMultipler = 1.0f + 0.5f * Tier;
            return new Tuple<float, float, float>(SignalMultipler, PriceMultipler, CostMultipler);
        }
        
        public static int TierProgress(float gameProgress)
        {
            var fluctuationRate = 0.25f;
            var fluctuation = 1.0f;
            var baseTier = Mathf.Lerp(1, 6, gameProgress);
            return Mathf.Clamp(Mathf.RoundToInt(baseTier), 1, 5);
        }
    }
}