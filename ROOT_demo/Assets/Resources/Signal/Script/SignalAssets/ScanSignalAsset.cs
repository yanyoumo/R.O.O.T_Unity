using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// ReSharper disable PossibleNullReferenceException

namespace ROOT.Signal
{
    public class ScanSignalAsset : SignalAssetBase
    {
        void Awake()
        {
            UnitSignalCoreType = gameObject.AddComponent<ScanUnitSignalCore>().GetType();
        }

        //这里的代码未来还要考虑到这些数据的位置会变。
        public override SignalType SignalType => SignalType.Scan;

        /*public override bool ShowSignal(RotationDirection dir, Unit unit, Unit otherUnit)
        {
            //这快儿是有问题的，主要是因为之前为了避免绕近道，强制一次一步、但是这么设计没法根据Tier调整数据。
            //可能有需要ServerDepth和HardwareDepth两个平行数据。再否则就是类似阵列信号那边，有一个FromDir。
            var ShowNetLED = unit.SignalCore.InServerGrid && otherUnit.SignalCore.InServerGrid;
            ShowNetLED &= Math.Abs(unit.SignalCore.ScanSignalPathDepth - otherUnit.SignalCore.ScanSignalPathDepth) <= 1;
            return ShowNetLED;
        }
        public override int SignalVal(RotationDirection dir, Unit unit, Unit otherUnit)
        {
            var showSig = ShowSignal(dir, unit, otherUnit);
            return showSig ? Math.Min(unit.SignalCore.ServerSignalDepth, otherUnit.SignalCore.ServerSignalDepth) : 0;
        }*/

        [Obsolete]
        public override float CalAllScore(Board gameBoard, out int hardwareCount)
        {
            //TODO SingleUnitScore实现后、这个函数就要被干掉了。
            int maxCount = Board.BoardLength * Board.BoardLength;
            var maxScore = Int32.MinValue;
            var maxLength = maxCount;
            float res = 0;
            foreach (var signalCore in gameBoard.FindUnitWithCoreType(SignalType, HardwareType.Core).Select(unit => unit.SignalCore as ScanUnitSignalCore))
            {
                //懂了，主要是CalScore代码之间需要交互这三个数据。
                //这个可以搞的，写一个CalScore的重写，里面写上需要的out变量就行了。
                //给你在ScanUnitSignalCore.cs的129行左右留了更多的内容。
                res = signalCore.CalScore(out var count, ref maxCount, ref maxScore, ref maxLength);
            }
            if (maxScore == Int32.MinValue)
            {
                maxScore = 0;
            }
            SignalMasterMgr.MaxNetworkDepth = hardwareCount = maxScore;
            return res;
        }
        public List<Unit> CalAllScore(Board gameBoard)
        {
            int maxCount = Board.BoardLength * Board.BoardLength;
            var maxScore = Int32.MinValue;
            var maxLength = maxCount;
            var res = new List<Unit>();
            foreach (var signalCore in gameBoard.FindUnitWithCoreType(SignalType, HardwareType.Core).Select(unit => unit.SignalCore as ScanUnitSignalCore))
            {
                var tmp = signalCore.CalScore(ref maxCount, ref maxScore, ref maxLength);
                if (tmp.Count != 0)
                    res = tmp;
            }
            return res;
        }
    }
}