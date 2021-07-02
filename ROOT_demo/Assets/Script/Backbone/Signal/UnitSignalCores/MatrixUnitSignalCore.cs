using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT.Signal
{
    public class MatrixUnitSignalCore : UnitSignalCoreBase
    {
        [ShowInInspector] public override SignalType SignalType => SignalType.Matrix;

        public override List<Vector2Int> SingleInfoCollectorZone
        {
            get
            {
                if (IsUnitActive && Owner.UnitHardware == HardwareType.Field)
                {
                    var zone = Utils.GetPixelateCircle_Tier(Owner.Tier - 1); //TODO 这里要加一个Cap
                    var res = new List<Vector2Int>();
                    zone.PatternList.ForEach(vec => res.Add(vec + Owner.CurrentBoardPosition - new Vector2Int(zone.CircleRadius, zone.CircleRadius)));
                    return res;
                }
                return new List<Vector2Int>();
            }
        }

        //TODO 这里再放一些Cap
        private const float perMatrixFieldUnitPrice = 1.0f;
        public override float SingleUnitScore => IsActiveFieldUnitThisSignal(Owner) ? perMatrixFieldUnitPrice * Owner.Tier : 0.0f;
    }
}