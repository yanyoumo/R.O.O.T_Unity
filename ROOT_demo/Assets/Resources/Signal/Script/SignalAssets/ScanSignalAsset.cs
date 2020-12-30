using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// ReSharper disable PossibleNullReferenceException

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

        public override bool ShowSignal(RotationDirection dir, Unit unit, Unit otherUnit)
        {
            //这快儿是有问题的，主要是因为之前为了避免绕近道，强制一次一步、但是这么设计没法根据Tier调整数据。
            //可能有需要ServerDepth和HardwareDepth两个平行数据。再否则就是类似阵列信号那边，有一个FromDir。
            var ShowNetLED = unit.InServerGrid && otherUnit.InServerGrid;
            ShowNetLED &= Math.Abs(unit.ServerDepth - otherUnit.ServerDepth) <= 1;
            return ShowNetLED;
        }
        public override int SignalVal(RotationDirection dir, Unit unit, Unit otherUnit)
        {
            var showSig = ShowSignal(dir, unit, otherUnit);
            return showSig ? Math.Min(unit.ServerDepth, otherUnit.NetworkVal) : 0;
        }

        public override float CalAllScore(Board gameBoard, out int hardwareCount)
        {
            int maxCount = Board.BoardLength * Board.BoardLength;
            var maxScore = Int32.MinValue;
            var maxLength = maxCount;
            float res = 0;
            foreach (var signalCore in gameBoard.FindUnitWithCoreType(CoreUnitType).Select(unit => unit.SignalCore as ScanUnitSignalCore))
            {
                //懂了，主要是CalScore代码之间需要交互这三个数据。
                //这个可以搞的，写一个CalScore的重写，里面写上需要的out变量就行了。
                //给你在ScanUnitSignalCore.cs的129行左右留了更多的内容。
                signalCore.MaxCount = maxCount;
                signalCore.MaxScore = maxScore;
                signalCore.MaxLength = maxLength;
                res = signalCore.CalScore(out var count);
                maxCount = signalCore.MaxCount;
                maxScore = signalCore.MaxScore;
                maxLength = signalCore.MaxLength;
            }
            BoardDataCollector.MaxNetworkDepth = hardwareCount = maxScore;
            return res;
        }

    }
}