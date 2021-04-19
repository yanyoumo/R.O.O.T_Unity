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
            {TutorialCheckType.ConnectOneMatrixUnitWithMatrixCore, TutorialCheckFunctionList.ConnectOneMatrixUnitWithMatrixCore},
            {TutorialCheckType.ConnectAllMatrixUnitsWithMatrixCore, TutorialCheckFunctionList.ConnectAllMatrixUnitsWithMatrixCore},
            {TutorialCheckType.ConnectThermalUnitWithThermalCore, TutorialCheckFunctionList.ConnectThermalUnitWithThermalCore},
            //{TutorialCheckType.ConnectMatrixLinksWithThermalLinks, TutorialCheckFunctionList.ConnectMatrixLinksWithThermalLinks},
            //{TutorialCheckType.ConnectNewAddedThermalUnitsIntoLinks, TutorialCheckFunctionList.ConnectNewAddedThermalUnitsIntoLinks},
            {TutorialCheckType.Buy3UnitsOrNotEnoughMoney, TutorialCheckFunctionList.Buy3UnitsOrNotEnoughMoney},
            {TutorialCheckType.FourWarningGridOneHeatSink, TutorialCheckFunctionList.FourWarningGridOneHeatSink}
        };
    }
}