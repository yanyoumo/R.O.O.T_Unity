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
        
        public override int CalScore(out int driverCountInt)
        {
            throw new System.NotImplementedException("MatrixUnitLogicCore");
        }
    }
}