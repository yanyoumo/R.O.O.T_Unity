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
    }
}