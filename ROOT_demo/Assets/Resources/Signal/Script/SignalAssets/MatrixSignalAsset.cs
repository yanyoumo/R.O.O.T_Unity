using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT.Signal
{
    public class MatrixSignalAsset : SignalAssetBase
    {
       
        void Awake()
        {
            UnitSignalCoreType = gameObject.AddComponent<MatrixUnitSignalCore>().GetType();
        }

        public override SignalType Type => SignalType.Matrix;

        public override bool ShowSignal(RotationDirection dir, Unit unit, Unit otherUnit)
        {
            var showScan = unit.SignalCore.InMatrixSignal && otherUnit.SignalCore.InMatrixSignal;
            var solidScanSignal = (unit.SignalCore.SignalFromDir == dir);
            solidScanSignal |= (Utils.GetInvertDirection(otherUnit.SignalCore.SignalFromDir) == dir);
            showScan &= solidScanSignal;
            return showScan;
        }
        public override int SignalVal(RotationDirection dir, Unit unit, Unit otherUnit)
        {
            var showSig = ShowSignal(dir, unit, otherUnit);
            return showSig ? Math.Min(unit.SignalCore.SignalStrength[SignalType.Matrix], otherUnit.SignalCore.SignalStrength[SignalType.Matrix]) : 0;
        }

        private void initCounting(Unit unit)
        {
            unit.SignalCore.Visited = false;
            unit.SignalCore.InMatrix = (unit.UnitSignal == SignalType.Matrix && unit.UnitHardware == HardwareType.Core);
            unit.SignalCore.InMatrixSignal = (unit.UnitSignal == SignalType.Matrix && unit.UnitHardware == HardwareType.Core);
        }

        public override float CalAllScore(Board gameBoard, out int driverCountInt)
        {
            var driverCount = 0.0f;
            driverCountInt = 0;

            gameBoard.Units.ForEach(initCounting);

            if (gameBoard.GetCountByType(Type,HardwareType.Core) == 0) return 0.0f;

            foreach (var unit in gameBoard.FindUnitWithCoreType(Type, HardwareType.Core))
            {
                if (unit.SignalCore.Visited) continue;
                unit.SignalCore.CalScore(out var hardwareCount);
                driverCount += hardwareCount;
            }

            driverCountInt = Mathf.FloorToInt(driverCount);
            return Mathf.FloorToInt(driverCount * SignalMasterMgr.GetPerDriverIncome);
        }
    }
}