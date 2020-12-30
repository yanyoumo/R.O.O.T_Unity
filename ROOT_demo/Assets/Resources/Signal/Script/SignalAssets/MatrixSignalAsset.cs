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

        public override bool ShowSignal(Unit unit, Unit otherUnit)
        {
            throw new System.NotImplementedException();
        }
        public override int SignalVal(Unit unit, Unit otherUnit)
        {
            throw new System.NotImplementedException();
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