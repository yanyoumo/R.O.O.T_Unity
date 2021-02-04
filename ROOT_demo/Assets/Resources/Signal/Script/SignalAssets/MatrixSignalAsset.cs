using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT.Signal
{
    public class MatrixSignalAsset : SignalAssetBase
    {
        void Awake()
        {
            UnitSignalCoreType = gameObject.AddComponent<MatrixUnitSignalCore>().GetType();
        }

        public override SignalType SignalType => SignalType.Matrix;

        public override bool ShowSignal(RotationDirection dir, Unit unit, Unit otherUnit)
        {
            var showScan = unit.SignalCore.HasCertainSignal(SignalType.Matrix) && otherUnit.SignalCore.HasCertainSignal(SignalType.Matrix);
            var solidScanSignal = (unit.SignalCore.SignalFromDir == dir);
            solidScanSignal |= (Utils.GetInvertDirection(otherUnit.SignalCore.SignalFromDir) == dir);
            showScan &= solidScanSignal;
            return showScan;
        }
        public override int SignalVal(RotationDirection dir, Unit unit, Unit otherUnit)
        {
            var showSig = ShowSignal(dir, unit, otherUnit);
            var ValA = unit.SignalCore.SignalStrengthComplex[SignalType.Matrix].Item1;
            var ValB = otherUnit.SignalCore.SignalStrengthComplex[SignalType.Matrix].Item1;
            return showSig ? Math.Min(ValA, ValB) : 0;
        }
    }
}