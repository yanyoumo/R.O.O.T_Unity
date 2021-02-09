using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ROOT;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT.Signal
{
    public class ThermoSignalAsset : SignalAssetBase
    {
        void Awake()
        {
            UnitSignalCoreType = gameObject.AddComponent<ThermoUnitSignalCore>().GetType();
        }

        public override SignalType SignalType => SignalType.Thermo;

        /*public override bool ShowSignal(RotationDirection dir, Unit unit, Unit otherUnit)
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
        }*/
    }
}
