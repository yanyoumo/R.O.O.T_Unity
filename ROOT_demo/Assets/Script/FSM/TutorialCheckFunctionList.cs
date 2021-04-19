using System.Linq;
using UnityEngine;

namespace ROOT
{
    public enum TutorialCheckType
    {
        MoveCursorToTarget55,
        MoveMatrixUnitsToSameYIndex,
        MoveThreeMatrixUnitsToOneLink,
        ConnectOneMatrixUnitWithMatrixCore,
        ConnectAllMatrixUnitsWithMatrixCore,
        ConnectThermalUnitWithThermalCore,
        ConnectMatrixLinksWithThermalLinks,
        ConnectNewAddedThermalUnitsIntoLinks,
        Buy3UnitsOrNotEnoughMoney,
        FourWarningGridOneHeatSink,
    }

    public static class TutorialCheckFunctionList
    {
        public static bool MoveCursorToTarget55(FSMLevelLogic fsm, Board board)
        {
            return fsm.LevelAsset.Cursor.CurrentBoardPosition.Equals(new Vector2Int(5, 5));
        }

        public static bool MoveMatrixUnitsToSameYIndex(FSMLevelLogic fsm, Board board)
        {
            var y = -1;
            foreach (var unit in board.Units)
            {
                if (y == -1)
                    y = unit.CurrentBoardPosition.y;
                else if (y != unit.CurrentBoardPosition.y)
                    return false;
            }

            return true;
        }

        public static bool MoveThreeMatrixUnitsToOneLink(FSMLevelLogic fsm, Board board)
        {
            // to get connectivity
            while (!board.IsDataReady) { }
            return board.GetConnectComponent() == 1;
        }

        public static bool ConnectOneMatrixUnitWithMatrixCore(FSMLevelLogic fsm, Board board)
        {
            // to get connectivity
            while (!board.IsDataReady) { }
            // we have one matrix field and one matrix core here
            return board.Units.Where(unit => unit.JudgeType(SignalType.Matrix, HardwareType.Core)).Any(unit =>
                unit.GetConnectedOtherUnit.Any(unit => unit.JudgeType(SignalType.Matrix, HardwareType.Field)));
        }

        public static bool ConnectAllMatrixUnitsWithMatrixCore(FSMLevelLogic fsm, Board board)
        {
            // to get connectivity
            while (!board.IsDataReady) { }
            // link all the units together
            return board.GetConnectComponent() == 1;
        }

        public static bool ConnectThermalUnitWithThermalCore(FSMLevelLogic fsm, Board board)
        {
            // to get connectivity
            while (!board.IsDataReady) { }
            // we have one thermo field and one thermo core here
            return board.Units.Where(unit => unit.JudgeType(SignalType.Thermo, HardwareType.Core)).Any(unit =>
                unit.GetConnectedOtherUnit.Any(unit => unit.JudgeType(SignalType.Thermo, HardwareType.Field)));
        }

        public static bool Buy3UnitsOrNotEnoughMoney(FSMLevelLogic fsm, Board board)
        {
            return board.Units.Length >= 3 || Mathf.RoundToInt(fsm.LevelAsset.GameCurrencyMgr.Currency) < 4;
        }

        public static bool FourWarningGridOneHeatSink(FSMLevelLogic fsm, Board board)
        {
            return board.BoardGirdDriver.BoardGirds.Values.Count(cell => cell.CellStatus == CellStatus.Warning) >= 4 &&
                   board.BoardGirdDriver.BoardGirds.Values.Any(cell => cell.CellStatus == CellStatus.Sink);
        }
    }
}