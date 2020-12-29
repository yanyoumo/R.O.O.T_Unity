using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public override CoreType CoreUnitType => CoreType.Server;
        public override CoreType FieldUnitType => CoreType.NetworkCable;

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

        public override float CalAllScore(Board gameBoard, out int hardwareCount)
        {
            int maxCount = Board.BoardLength * Board.BoardLength;
            var maxScore = Int32.MinValue;
            var maxLength = maxCount;
            float res = 0;
            foreach (var signalCore in gameBoard.Units.Where(unit => unit.UnitCore == CoreUnitType).Select(unit => unit.SignalCore))
            {
                ((ScanUnitSignalCore)signalCore).MaxCount = maxCount;
                ((ScanUnitSignalCore)signalCore).MaxScore = maxScore;
                ((ScanUnitSignalCore)signalCore).MaxLength = maxLength;
                res = signalCore.CalScore(out var count);
                maxCount = ((ScanUnitSignalCore)signalCore).MaxCount;
                maxScore = ((ScanUnitSignalCore)signalCore).MaxScore;
                maxLength = ((ScanUnitSignalCore)signalCore).MaxLength;
            }
            BoardDataCollector.MaxNetworkDepth = hardwareCount = maxScore;
            return res;
        }

    }
}