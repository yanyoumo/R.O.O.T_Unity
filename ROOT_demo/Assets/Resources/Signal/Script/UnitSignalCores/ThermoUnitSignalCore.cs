using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class ThermoUnitSignalCore : UnitSignalCoreBase
    {
        public override SignalType Type => SignalType.Thermo;

        public override List<Vector2Int> SingleInfoCollectorZone
        {
            get
            {
                //TODO
                var res = new List<Vector2Int>();
                return res;
            }
        }

        public override float CalScore(out int thermoItemCount)
        {
            //TODO
            thermoItemCount = 1;
            return 1f;
        }
    }
}
