using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ROOT;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT.Signal
{
    //现在几点比较严重的问题、就是现在几个新的系统结合起来不匹配。
    //1、Thermo单元是鼓励玩家将这些单元放在末端。
    //2、融资单元的玩法鼓励玩家平衡两种信号。
    //3、末端单元的数量一定比中段单元要少。
    
    //讲道理、玩家此时就是要选择是平衡还是单方面高效；从纸上谈兵的角度上看、是比较“有趣”的。但是不知道为什么我玩的很烦躁。
    //有一点可能是：每次商店开启时，单元的等级都是差不多的；也就是说、没有特别好的方法调整“平衡”。
    //叮铃铃！有没有可能让商店允许玩家自己选择想购买的等级？
    public class ThermoSignalAsset : SignalAssetBase
    {
        void Awake()
        {
            UnitSignalCoreType = gameObject.AddComponent<ThermoUnitSignalCore>().GetType();
        }

        public override SignalType SignalType => SignalType.Thermo;
    }
}
