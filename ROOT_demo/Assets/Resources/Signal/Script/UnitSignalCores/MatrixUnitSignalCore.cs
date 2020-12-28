using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class MatrixUnitSignalCore : UnitSignalCoreBase
    {
        public static bool ShowSignal(Unit unit, Unit otherUnit)
        {
            return unit.InHddSignalGrid && otherUnit.InHddSignalGrid;
        }

        public static int SignalVal(Unit unit, Unit otherUnit)
        {
            return Math.Min(unit.HardDiskVal, otherUnit.HardDiskVal);
        }
        
        public override float CalScore(out int driverCountInt)
        {
            Debug.LogWarning("MatrixUnitLogicCore_CalScore");
            driverCountInt = 0;
            return -1.0f;
            //throw new System.NotImplementedException("MatrixUnitLogicCore_CalScore");
        }
    }
}