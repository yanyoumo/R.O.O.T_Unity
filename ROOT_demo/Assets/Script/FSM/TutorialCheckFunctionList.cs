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
        ContinueWhenOneUnitIsBought = 20,
        Achieve150ProfitOrTimeOut = 21,
        Proceed5TimeTick = 22,
        AtLeastUseFastForwardOneTime = 30,
        UseBackward = 31,
        UseTransferToConnectUnits = 32,
        ConnectAllCusterUnits = 33,
        ConnectAllFirewallUnits = 34,
        Reach16Benefit = 35,
        Reach10Benefit = 36,
        Reach12Benefit = 37,
        Reach14Benefit = 38,
        
        ReachInfoZoneOf18 = 39,
        ReachInfoZoneOf19 = 40,
        ReachInfoZoneOf14 = 41,
        ReachInfoZoneOf21 = 42,

        Achieve750ProfitOrTimeOut = 43,
        
        //250左右的号段我用了、给基本Gameplay内容做一些判断-youmo
        CustomCheckGameplay0 = 250,
        CustomCheckGameplay1 = 251,
        CustomCheckGameplay2 = 252,
        CustomCheckGameplay3 = 253,
        CustomCheckGameplay4 = 254,
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
            return board.Units.Where(unit => unit.CheckType(SignalType.Matrix, HardwareType.Core)).Any(unit =>
                unit.GetConnectedOtherUnit.Any(unit => unit.CheckType(SignalType.Matrix, HardwareType.Field)));
        }

        private static bool ConnectAnyThermalFieldDirectlyWithThermalCore(FSMLevelLogic fsm, Board board)
        {
            // we have one thermo field and one thermo core here
            return board.Units.Where(unit => unit.CheckType(SignalType.Thermo, HardwareType.Core)).Any(unit =>
                unit.GetConnectedOtherUnit.Any(unit => unit.CheckType(SignalType.Thermo, HardwareType.Field)));
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
            return board.Units.Where(unit => unit.CheckType(SignalType.Matrix, HardwareType.Core)).Any(unit =>
                unit.GetConnectedOtherUnit.Any(unit => unit.CheckType(SignalType.Thermo, HardwareType.Field))) && 
                   board.Units.Where(unit => unit.CheckType(SignalType.Thermo, HardwareType.Core)).Any(unit =>
                    unit.GetConnectedOtherUnit.Any(unit => unit.CheckType(SignalType.Matrix, HardwareType.Field)));
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

            return board.GetUnitsConnectedIsland() == 1;
        }

        public static bool MoveOneMatrixToPlace(FSMLevelLogic fsm, Board board)
        {
            var unit = board.FindUnitByPos(new Vector2Int(2,1));
            if (unit == null)
                return false;
            return unit.UnitSignal == SignalType.Matrix && board.GetUnitsConnectedIsland() == 1;
        }

        public static bool ContinueWhenOneUnitIsBought(FSMLevelLogic fsm, Board board)
        {
            return Mathf.RoundToInt(fsm.LevelAsset.GameCurrencyMgr.Currency) < Mathf.RoundToInt(fsm.LevelAsset.GameCurrencyMgr.StartingMoney);
        }

        public static bool Achieve150ProfitOrTimeOut(FSMLevelLogic fsm, Board board)
        {
            var timeOut = false;
            if (fsm is FSMLevelLogic_Career career)
            {
                var gist = career.RoundLibDriver.GetCurrentRoundGist(career.RoundLibDriver.StepCount);
                var truncatedStep = career.RoundLibDriver.GetTruncatedStep(career.RoundLibDriver.StepCount);
                timeOut = truncatedStep - gist.shopLength >= 25;
            }
            return Mathf.RoundToInt(fsm.LevelAsset.GameCurrencyMgr.Currency) >= 150 || timeOut;
        }

        public static bool Proceed5TimeTick(FSMLevelLogic fsm, Board board)
        {
            return fsm.LevelAsset.StepCount > 4;
        }

        public static bool AtLeastUseFastForwardOneTime(FSMLevelLogic fsm, Board board)
        {
            return fsm.LevelAsset.SkillMgr.SkillUsedCountByID(0) + fsm.LevelAsset.SkillMgr.SkillUsedCountByID(1) >= 1;
        }

        public static bool UseBackward(FSMLevelLogic fsm, Board board)
        {
            return fsm.LevelAsset.SkillMgr.SkillUsedCountByID(2) + fsm.LevelAsset.SkillMgr.SkillUsedCountByID(3) >= 1;
        }

        public static bool UseTransferToConnectUnits(FSMLevelLogic fsm, Board board)
        {
            return fsm.LevelAsset.SkillMgr.SkillUsedCountByID(4) + fsm.LevelAsset.SkillMgr.SkillUsedCountByID(5) +
                   fsm.LevelAsset.SkillMgr.SkillUsedCountByID(6) >= 1 &&
                   board.Units.Where(unit => unit.CheckType(SignalType.Matrix, HardwareType.Core)).Any(unit =>
                       unit.GetConnectedOtherUnit.Count(fieldUnit =>
                           fieldUnit.CheckType(SignalType.Matrix, HardwareType.Field)) == 4);
        }

        public static bool ConnectAllCusterUnit(FSMLevelLogic fsm, Board board) => ConnectAllUnitByType(board, SignalType.Cluster);
        public static bool ConnectAllFirewallUnit(FSMLevelLogic fsm, Board board) => ConnectAllUnitByType(board, SignalType.Firewall);
        public static bool ReachBenefitOf16(FSMLevelLogic fsm, Board board) => ReachBenefit(fsm, 16);
        public static bool ReachBenefitOf10(FSMLevelLogic fsm, Board board) => ReachBenefit(fsm, 10);
        public static bool ReachBenefitOf12(FSMLevelLogic fsm, Board board) => ReachBenefit(fsm, 12);
        public static bool ReachBenefitOf14(FSMLevelLogic fsm, Board board) => ReachBenefit(fsm, 14);
        public static bool ReachInfoZoneOf18(FSMLevelLogic fsm, Board board) => InfoZoneCount(fsm, 18);
        public static bool ReachInfoZoneOf19(FSMLevelLogic fsm, Board board) => InfoZoneCount(fsm, 19);
        public static bool ReachInfoZoneOf14(FSMLevelLogic fsm, Board board) => InfoZoneCount(fsm, 14);
        public static bool ReachInfoZoneOf21(FSMLevelLogic fsm, Board board) => InfoZoneCount(fsm, 18);
        
        private static bool InfoZoneCount(FSMLevelLogic fsm, int Target)
        {
            return fsm.LevelAsset.CollectorZone.Distinct().Count() >= Target;
        }

        private static bool ConnectAllUnitByType(Board board, SignalType _signalType)
        {
            return board.Units.Where(u => u.SignalCore.SignalType == _signalType).All(u => u.SignalCore.IsUnitActive);
        }
        
        private static bool ReachBenefit(FSMLevelLogic fsm,int Target)
        {
            return fsm.LevelAsset.DeltaCurrency >= Target;
        }

        public static bool Achieve750ProfitOrTimeOut(FSMLevelLogic fsm, Board board)
        {
            var timeOut = false;
            if (fsm is FSMLevelLogic_Career career)
            {
                timeOut = career.RoundLibDriver.StepCount >= 86;
            }
            return Mathf.RoundToInt(fsm.LevelAsset.GameCurrencyMgr.Currency) >= 750 || timeOut;
        }
    }
}