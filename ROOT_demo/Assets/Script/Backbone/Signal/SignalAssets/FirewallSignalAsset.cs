namespace ROOT.Signal
{
    public class FirewallSignalAsset : SignalAssetBase
    {
        void Awake()
        {
            UnitSignalCoreType = gameObject.AddComponent<ThermoUnitSignalCore>().GetType();
        }
        
        public override SignalType SignalType => SignalType.Firewall;
    }
}
