using System;

namespace ROOT.Signal
{
    public class BasicSignalAsset : SignalAssetBase
    {
        public override Type UnitSignalCoreType => typeof(BasicUnitSignalCore);
        public override SignalType SignalType => SignalType.Matrix;
    }
}