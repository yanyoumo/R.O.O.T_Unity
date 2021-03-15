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
    }
}
