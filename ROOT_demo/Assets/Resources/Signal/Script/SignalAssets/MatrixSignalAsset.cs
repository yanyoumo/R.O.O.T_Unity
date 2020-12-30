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
            var ShowHDDLED = unit.InHddSignalGrid && otherUnit.InHddSignalGrid;
            var HasSolidHDDSigal = (unit.SignalFromDir == dir);
            HasSolidHDDSigal |= (Utils.GetInvertDirection(otherUnit.SignalFromDir) == dir);
            ShowHDDLED &= HasSolidHDDSigal;
            return ShowHDDLED;
        }
        public override int SignalVal(RotationDirection dir, Unit unit, Unit otherUnit)
        {
            var showSig = ShowSignal(dir, unit, otherUnit);
            return showSig ? Math.Min(unit.HardDiskVal, otherUnit.HardDiskVal) : 0;
        }

        public int MaxNormalDepth;

        private void initCounting(Unit unit)
        {
            unit.Visited = false;
            unit.InHddGrid = (unit.UnitCore == CoreType.Processor);
            unit.InHddSignalGrid = (unit.UnitCore == CoreType.Processor);
        }

        public override float CalAllScore(Board gameBoard, out int driverCountInt)
        {
            var driverCount = 0.0f;
            driverCountInt = 0;

            gameBoard.Units.ForEach(initCounting);

            if (gameBoard.GetCountByType(CoreUnitType) == 0) return 0.0f;

            foreach (var unit in gameBoard.FindUnitWithCoreType(CoreUnitType))
            {
                if (unit.Visited) continue;
                unit.SignalCore.CalScore(out var hardwareCount);
                driverCount += hardwareCount;
            }

            driverCountInt = MaxNormalDepth = Mathf.FloorToInt(driverCount);
            return Mathf.FloorToInt(driverCount * BoardDataCollector.GetPerDriverIncome);
        }
    }
}