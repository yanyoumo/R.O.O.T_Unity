using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class ScanSignalAsset : SignalAssetBase
    {
        void Awake()
        {
            UnitSignalCoreType = gameObject.AddComponent<ScanUnitSignalCore>().GetType();
        }

        //这里的代码未来还要考虑到这些数据的位置会变。
        public override SignalType Type => SignalType.Scan;
        public override bool ShowSignal(Unit unit, Unit otherUnit)
        {
            var ShowNetLED = unit.InServerGrid && otherUnit.InServerGrid;
            ShowNetLED &= Math.Abs(unit.ServerDepth - otherUnit.ServerDepth) <= 1;
            return ShowNetLED;
        }
        public override int SignalVal(Unit unit, Unit otherUnit)
        {
            return Math.Min(unit.ServerDepth, otherUnit.NetworkVal);
        }
    }
}