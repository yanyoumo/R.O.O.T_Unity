using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable PossibleNullReferenceException

namespace ROOT.Signal
{
    public class ScanSignalAsset : SignalAssetBase
    {
        public override Type UnitSignalCoreType => typeof(ScanUnitSignalCore);

        //这里的代码未来还要考虑到这些数据的位置会变。
        public override SignalType SignalType => SignalType.Scan;

        //这个函数名不得不改、要不然调它的时候引用会飘到基类上-youmo
        private List<Unit> CalAllScore_Scan(Board gameBoard)
        {
            var maxScore = Int32.MinValue;
            var minLength = Board.BoardLength * Board.BoardLength;
            var res = new List<Unit>();
            foreach (var signalCore in gameBoard.FindUnitWithCoreType(SignalType, HardwareType.Core).Select(unit => unit.SignalCore as ScanUnitSignalCore))
            {
                var tmp = signalCore.CalScore(ref minLength, ref maxScore);
                if (tmp.Count != 0)
                {
                    res = tmp;
                }
            }
            if (maxScore!=Int32.MinValue)
                SignalMasterMgr.MaxNetworkDepth = maxScore;
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

        public override IEnumerable<SignalPath> FindAllPathSingleLayer(Board board)
        {
            if (tempScanPath.Count < 1) return new List<SignalPath>();
            var res = new SignalPath(tempScanPath);
            return new[] {res};
        }
    }
}