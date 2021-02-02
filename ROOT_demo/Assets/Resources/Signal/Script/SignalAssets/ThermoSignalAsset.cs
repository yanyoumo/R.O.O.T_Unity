using System.Collections;
using System.Collections.Generic;
using ROOT;
using UnityEngine;

namespace ROOT.Signal
{
    public class ThermoSignalAsset : SignalAssetBase
    {
        void Awake()
        {
            UnitSignalCoreType = gameObject.AddComponent<ThermoUnitSignalCore>().GetType();
        }

        public override SignalType Type => SignalType.Thermo;

        public override bool ShowSignal(RotationDirection dir, Unit unit, Unit otherUnit)
        {
            //TODO
            var ShowNetLED = true;
            
            return ShowNetLED;
        }

        public override int SignalVal(RotationDirection dir, Unit unit, Unit otherUnit)
        {
            //TODO
            var showSig = ShowSignal(dir, unit, otherUnit);
            return showSig ? 1 : 0;
        }

        public override float CalAllScore(Board gameBoard, out int thermoItemCountInt)
        {
            //TODO
            thermoItemCountInt = 1;
            return 0;
        }
    }
}
