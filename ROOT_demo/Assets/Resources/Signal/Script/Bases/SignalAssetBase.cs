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
        public abstract SignalType Type { get; }
        public abstract CoreType CoreUnitType { get; }
        public abstract CoreType FieldUnitType { get; }
        public abstract bool ShowSignal(RotationDirection dir,Unit unit,Unit otherUnit);
        public abstract int SignalVal(RotationDirection dir, Unit unit,Unit otherUnit);
        
        //从这里看，就是将会有一个地方可以对某类信号统一调用结果；
        //但是现在相当于要把视角放在单元上；
        //然后将这个函数统一放在这里。这是最费劲的事儿，这里需要Board的引用，先留个口吧。
        //哦对，这里还需要一个Unit和SignalType的映射。
        public virtual float CalAllScore(Board gameBoard, out int hardwareCount)
        {
            float res=0;
            int reshdCount = 0;
            foreach (var signalCore in gameBoard.Units.Where(unit => unit.UnitCore == CoreUnitType).Select(unit=>unit.SignalCore))
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