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
            //Redundant
            thermoItemCount = 1;
            return 1f;
        }

        public float CalScore()
        {
            //TODO
            return 1f;
        }
    }
}
