using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT.Signal
{
    public class MatrixUnitSignalCore : UnitSignalCoreBase
    {
        [ShowInInspector] public override SignalType SignalType => SignalType.Matrix;

        private const float perMatrixFieldUnitPrice = 1.0f;
        private int[] matrixTierSignalZoneMapping = {0, 1, 2, 2, 3};
        
        public override List<Vector2Int> SingleInfoCollectorZone
        {
            get
            {
                if (IsUnitActive && Owner.UnitHardware == HardwareType.Field)
                {
                    var zoneIndex = matrixTierSignalZoneMapping[Owner.Tier - 1];
                    var zone = Utils.GetPixelateCircle_Tier(zoneIndex);
                    var res = new List<Vector2Int>();
                    zone.PatternList.ForEach(vec => res.Add(vec + Owner.CurrentBoardPosition - new Vector2Int(zone.CircleRadius, zone.CircleRadius)));
                    return res;
                }
                return new List<Vector2Int>();
            }
        }

        public override float SingleUnitScore => IsActiveFieldUnitThisSignal(Owner) ? perMatrixFieldUnitPrice * Owner.Tier : 0.0f;
    }
}