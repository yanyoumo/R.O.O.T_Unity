using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace ROOT
{
    public enum TutorialCheckType
    {
        MoveCursorToTarget55 = 0,
        MoveMatrixUnitsToSameYIndex = 1,
        MoveThreeMatrixUnitsToOneLink = 2,
        ConnectOneMatrixCoreWithOneMatrixField = 10,
        ConnectOneMatrixCoreWithTwoMatrixField = 11,
        ConnectOneMatrixCoreWithFiveMatrixField = 12,
        ConnectOneThermalCoreWithOneThermalField = 13,
        ConnectOneThermalCoreWithFourThermalField = 14,
        ConnectDifferentCoreAndField = 15,
        MoveThreeMatrixAndTwoThermalToPlace = 16,
        MoveOneMatrixToPlace = 17,
        Buy3UnitsOrNotEnoughMoney = 99,
        FourWarningGridOneHeatSink = 100,
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
            return board.GetUnitsConnectedIsland() == 1;
        }

        private static bool ConnectAnyMatrixFieldDirectlyWithMatrixCore(FSMLevelLogic fsm, Board board)
        {
            // we have one matrix field and one matrix core here
            return board.Units.Where(unit => unit.JudgeType(SignalType.Matrix, HardwareType.Core)).Any(unit =>
                unit.GetConnectedOtherUnit.Any(unit => unit.JudgeType(SignalType.Matrix, HardwareType.Field)));
        }

        private static bool ConnectAnyThermalFieldDirectlyWithThermalCore(FSMLevelLogic fsm, Board board)
        {
            // we have one thermo field and one thermo core here
            return board.Units.Where(unit => unit.JudgeType(SignalType.Thermo, HardwareType.Core)).Any(unit =>
                unit.GetConnectedOtherUnit.Any(unit => unit.JudgeType(SignalType.Thermo, HardwareType.Field)));
        }

        public static bool ConnectOneMatrixCoreWithOneMatrixField(FSMLevelLogic fsm, Board board)
        {
            return board.CheckAllActive();
        }

        public static bool ConnectOneMatrixCoreWithTwoMatrixField(FSMLevelLogic fsm, Board board)
        {
            return board.CheckAllActive();
        }

        public static bool ConnectOneMatrixCoreWithFiveMatrixField(FSMLevelLogic fsm, Board board)
        {
            return board.CheckAllActive();
        }

        public static bool ConnectOneThermalCoreWithOneThermalField(FSMLevelLogic fsm, Board board)
        {
            return board.CheckAllActive();
        }

        public static bool ConnectOneThermalCoreWithFourThermalField(FSMLevelLogic fsm, Board board)
        {
            return board.CheckAllActive();
        }

        public static bool ConnectDifferentCoreAndField(FSMLevelLogic fsm, Board board)
        {
            // we have one thermo field and one thermo core here
            return !ConnectAnyMatrixFieldDirectlyWithMatrixCore(fsm, board) &&
                   !ConnectAnyThermalFieldDirectlyWithThermalCore(fsm, board) && board.CheckAllActive();
        }

        public static bool MoveThreeMatrixAndTwoThermalToPlace(FSMLevelLogic fsm, Board board)
        {
            var dic = new Dictionary<Vector2Int, SignalType>
            {
                {new Vector2Int(1,5), SignalType.Matrix},
                {new Vector2Int(2,5), SignalType.Matrix},
                {new Vector2Int(2,4), SignalType.Matrix},
                {new Vector2Int(2,2), SignalType.Thermo},
                {new Vector2Int(2,3), SignalType.Thermo}
            };
            foreach(var i in dic)
            {
                var unit = board.FindUnitByPos(i.Key);
                if (unit == null)
                    return false;
                if (unit.UnitSignal != i.Value)
                    return false;
            }
            return  board.CheckAllActive();
        }

        public static bool MoveOneMatrixToPlace(FSMLevelLogic fsm, Board board)
        {
            var unit = board.FindUnitByPos(new Vector2Int(2,1));
            if (unit == null)
                return false;
            return unit.UnitSignal == SignalType.Matrix && board.CheckAllActive();
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