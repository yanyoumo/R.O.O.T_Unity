using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT.Signal
{
    public class FirewallUnitSignalCore : UnitSignalCoreBase
    {
        [ShowInInspector] public override SignalType SignalType => SignalType.Firewall;

        [Obsolete]
        public override List<Vector2Int> SingleInfoCollectorZone
        {
            get
            {
                var zone = Utils.GetPixelateCircle_Tier(Owner.Tier - 1);
                var res = new List<Vector2Int>();
                zone.PatternList.ForEach(vec => res.Add(vec + Owner.CurrentBoardPosition - new Vector2Int(zone.CircleRadius, zone.CircleRadius)));
                return res;
            }
        }
        
        [Obsolete]
        private const int perMatrixFieldUnitPrice = 1;

        [Obsolete]
        public override float SingleUnitScore => (IsUnitActive && Owner.UnitHardware == HardwareType.Field) ? perMatrixFieldUnitPrice * Owner.Tier : 0.0f;
    }
}