using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT.Signal
{
    public class FirewallUnitSignalCore : UnitSignalCoreBase
    {
        private float[] scoreMultiplier = {0.3f, 0.65f, 1.0f, 0.9f, 0.8f, 0.7f, 0.5f, 0.3f, 0.1f};
        
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
        
        private List<Vector2Int> SearchingPatternList => Utils.GetPixelateCircle_Tier(2).CenteredPatternList.Select(s => s + Owner.CurrentBoardPosition).ToList();
        private int NeighbouringFirewallUnit => SearchingPatternList.Select(p => GameBoard.FindUnitByPos(p)).Count(u => u != null && u.UnitSignal == SignalType.Firewall);
        
        private const int perFirewallFieldUnitPrice = 1;

        public override float SingleUnitScore => (IsUnitActive && Owner.UnitHardware == HardwareType.Field) ? perFirewallFieldUnitPrice * scoreMultiplier[NeighbouringFirewallUnit] * Owner.Tier : 0.0f;
    }
}