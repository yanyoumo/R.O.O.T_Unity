using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ROOT
{
    public abstract class SignalAssetBase : MonoBehaviour
    {
        public Type UnitSignalCoreType { protected set; get; }
        //返回对应的信号的enum
        public abstract SignalType Type { get; }
        //public CoreType CoreUnitType => CoreUnitAsset.UnitType;
        //public CoreType FieldUnitType => FieldUnitAsset.UnitType;
        public UnitAsset CoreUnitAsset;
        public UnitAsset FieldUnitAsset;
        //在LED屏幕上是否显示本信号的的逻辑。
        public abstract bool ShowSignal(RotationDirection dir,Unit unit,Unit otherUnit);
        //在LED屏幕上显示本信号的具体数值的逻辑。（此数据为绝对值、不要归一化）
        public abstract int SignalVal(RotationDirection dir, Unit unit,Unit otherUnit);
        
        //从这里看，就是将会有一个地方可以对某类信号统一调用结果；
        //但是现在相当于要把视角放在单元上；
        //然后将这个函数统一放在这里。这是最费劲的事儿，这里需要Board的引用，先留个口吧。
        //哦对，这里还需要一个Unit和SignalType的映射。
        //从整个信号视角计算信号总和。
        public virtual float CalAllScore(Board gameBoard, out int hardwareCount)
        {
            float res = 0;
            int reshdCount = 0;
            foreach (var signalCore in gameBoard.Units
                .Where(unit => unit.UnitCore == Type && unit.UnitCoreGenre == CoreGenre.Core).Select(unit => unit.SignalCore))
            {
                res += signalCore.CalScore(out var count);
                reshdCount += count;
            }

            hardwareCount = reshdCount;
            return res;
        }

        public float CalAllScore(Board gameBoard)
        {
            return CalAllScore(gameBoard, out var A);
        }
    }
}