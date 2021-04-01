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