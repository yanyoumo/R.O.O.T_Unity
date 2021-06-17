using System;

namespace ROOT.Signal
{
    public class FirewallSignalAsset : SignalAssetBase
    {
        public override Type UnitSignalCoreType => typeof(FirewallUnitSignalCore);
        public override SignalType SignalType => SignalType.Firewall;
    }
}
