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
            {TutorialCheckType.Buy3UnitsOrNotEnoughMoney, TutorialCheckFunctionList.Buy3UnitsOrNotEnoughMoney},
            {TutorialCheckType.FourWarningGridOneHeatSink, TutorialCheckFunctionList.FourWarningGridOneHeatSink}
        };
    }
}