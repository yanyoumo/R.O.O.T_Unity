using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
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

        //这个函数名不得不改、要不然调它的时候引用会飘到基类上-youmo
        private List<Unit> CalAllScore_Scan(Board gameBoard)
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


        private List<Unit> tempScanPath;
        public override void RefreshBoardSignalStrength(Board board)
        {
            base.RefreshBoardSignalStrength(board);
            tempScanPath = CalAllScore_Scan(board);
            //clear path
            foreach (var unit in board.Units)
                unit.SignalCore.SignalDataPackList[SignalType].UpstreamUnit = null;
            for (var i = tempScanPath.Count - 1; i >= 1; --i)
            {
                tempScanPath[i].SignalCore.SignalDataPackList[SignalType].UpstreamUnit = tempScanPath[i - 1];
            }
        }
    }
}