using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class MatrixSignalAsset : SignalAssetBase
    {
        void Awake()
        {
            UnitSignalCoreType = gameObject.AddComponent<MatrixUnitSignalCore>().GetType();
        }

        public override SignalType Type => SignalType.Matrix;
        public override bool ShowSignal(Unit unit, Unit otherUnit)
        {
            throw new System.NotImplementedException();
        }
        public override int SignalVal(Unit unit, Unit otherUnit)
        {
            throw new System.NotImplementedException();
        }
    }
}