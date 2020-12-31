using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT
{
    public class MatrixSignalAsset : SignalAssetBase
    {
        void Awake()
        {
            UnitSignalCoreType = gameObject.AddComponent<MatrixUnitSignalCore>().GetType();
        }

        public override SignalType Type => SignalType.Matrix;
        public override CoreType CoreUnitType => CoreType.Processor;
        public override CoreType FieldUnitType => CoreType.HardDrive;

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
            return showSig ? Math.Min(unit.SignalCore.MatrixVal, otherUnit.SignalCore.MatrixVal) : 0;
        }

        private void initCounting(Unit unit)
        {
            unit.SignalCore.Visited = false;
            unit.SignalCore.InMatrix = (unit.UnitCore == CoreType.Processor);
            unit.SignalCore.InMatrixSignal = (unit.UnitCore == CoreType.Processor);
        }

        public override float CalAllScore(Board gameBoard, out int driverCountInt)
        {
            var driverCount = 0.0f;
            driverCountInt = 0;

            gameBoard.Units.ForEach(initCounting);

            if (gameBoard.GetCountByType(CoreUnitType) == 0) return 0.0f;

            foreach (var unit in gameBoard.FindUnitWithCoreType(CoreUnitType))
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