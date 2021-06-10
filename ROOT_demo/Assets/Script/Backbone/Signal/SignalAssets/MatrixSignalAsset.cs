using System;

namespace ROOT.Signal
{
    public class MatrixSignalAsset : SignalAssetBase
    {
        public override Type UnitSignalCoreType => typeof(MatrixUnitSignalCore);
        public override SignalType SignalType => SignalType.Matrix;
    }
}