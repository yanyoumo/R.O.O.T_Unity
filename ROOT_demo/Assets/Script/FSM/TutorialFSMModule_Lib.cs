using System;
using System.Collections.Generic;

namespace ROOT.FSM
{
    using CheckingLib = Dictionary<TutorialCheckType, Func<FSMLevelLogic, Board, bool>>;
    public partial class TutorialFSMModule
    {
        private readonly CheckingLib CheckLib = new CheckingLib
        {
            {TutorialCheckType.MoveCursorToTarget55, TutorialCheckFunctionList.MoveCursorToTarget55},
            {TutorialCheckType.MoveMatrixUnitsToSameYIndex, TutorialCheckFunctionList.MoveMatrixUnitsToSameYIndex},
            {TutorialCheckType.MoveThreeMatrixUnitsToOneLink, TutorialCheckFunctionList.MoveThreeMatrixUnitsToOneLink},
            {TutorialCheckType.ConnectOneMatrixCoreWithOneMatrixField, TutorialCheckFunctionList.ConnectOneMatrixCoreWithOneMatrixField},
            {TutorialCheckType.ConnectOneMatrixCoreWithTwoMatrixField, TutorialCheckFunctionList.ConnectOneMatrixCoreWithTwoMatrixField},
            {TutorialCheckType.ConnectOneMatrixCoreWithFiveMatrixField, TutorialCheckFunctionList.ConnectOneMatrixCoreWithFiveMatrixField},
            {TutorialCheckType.ConnectOneThermalCoreWithOneThermalField, TutorialCheckFunctionList.ConnectOneThermalCoreWithOneThermalField},
            {TutorialCheckType.ConnectOneThermalCoreWithFourThermalField, TutorialCheckFunctionList.ConnectOneThermalCoreWithFourThermalField},
            {TutorialCheckType.ConnectDifferentCoreAndField, TutorialCheckFunctionList.ConnectDifferentCoreAndField},
            {TutorialCheckType.MoveThreeMatrixAndTwoThermalToPlace, TutorialCheckFunctionList.MoveThreeMatrixAndTwoThermalToPlace},
            {TutorialCheckType.MoveOneMatrixToPlace, TutorialCheckFunctionList.MoveOneMatrixToPlace},
            {TutorialCheckType.ContinueWhenOneUnitIsBought, TutorialCheckFunctionList.ContinueWhenOneUnitIsBought},
            {TutorialCheckType.Achieve150ProfitOrTimeOut, TutorialCheckFunctionList.Achieve150ProfitOrTimeOut},
            {TutorialCheckType.Proceed5TimeTick, TutorialCheckFunctionList.Proceed5TimeTick},
            {TutorialCheckType.AtLeastUseFastForwardOneTime, TutorialCheckFunctionList.AtLeastUseFastForwardOneTime},
            {TutorialCheckType.UseBackward, TutorialCheckFunctionList.UseBackward},
            {TutorialCheckType.UseTransferToConnectUnits, TutorialCheckFunctionList.UseTransferToConnectUnits},
            {TutorialCheckType.ConnectAllCusterUnits, TutorialCheckFunctionList.ConnectAllCusterUnit},
            {TutorialCheckType.ConnectAllFirewallUnits, TutorialCheckFunctionList.ConnectAllFirewallUnit},
            {TutorialCheckType.Reach16Benefit, TutorialCheckFunctionList.ReachBenefitOf16},
            {TutorialCheckType.Reach10Benefit, TutorialCheckFunctionList.ReachBenefitOf10},
            {TutorialCheckType.Reach12Benefit, TutorialCheckFunctionList.ReachBenefitOf12},
            {TutorialCheckType.Reach14Benefit, TutorialCheckFunctionList.ReachBenefitOf14},
            {TutorialCheckType.ReachInfoZoneOf18, TutorialCheckFunctionList.ReachInfoZoneOf18},
            {TutorialCheckType.ReachInfoZoneOf19, TutorialCheckFunctionList.ReachInfoZoneOf19},
            {TutorialCheckType.ReachInfoZoneOf20, TutorialCheckFunctionList.ReachInfoZoneOf20},
            {TutorialCheckType.ReachInfoZoneOf21, TutorialCheckFunctionList.ReachInfoZoneOf21},
            {TutorialCheckType.CustomCheckGameplay0, GameplayCheckFunctionList.GameplayCheck0},
            {TutorialCheckType.CustomCheckGameplay1, GameplayCheckFunctionList.GameplayCheck1},
            {TutorialCheckType.CustomCheckGameplay2, GameplayCheckFunctionList.GameplayCheck2},
            {TutorialCheckType.CustomCheckGameplay3, GameplayCheckFunctionList.GameplayCheck3},
            {TutorialCheckType.CustomCheckGameplay4, GameplayCheckFunctionList.GameplayCheck4},
        };
    }
}