using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT.Signal
{
    public class MatrixUnitSignalCore : UnitSignalCoreBase
    {
        [ShowInInspector] public override SignalType SignalType => SignalType.Matrix;
        
        private const int perMatrixFieldUnitPrice = 1;
        private float[] scoreMultiplier = {0.075f, 0.25f, 0.5f, 0.75f, 1.0f};

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
        private List<Vector2Int> SearchingPatternList => Utils.GetPixelateCircle_Tier(1).CenteredPatternList.Select(s => s + Owner.CurrentBoardPosition).ToList();
        private int NeighbouringMatrixUnitCount => SearchingPatternList.Select(p => GameBoard.FindUnitByPos(p)).Count(u => u != null && u != Owner && u.UnitSignal == SignalType.Matrix);

        public override float SingleUnitScore => (IsUnitActive && Owner.UnitHardware == HardwareType.Field) ? perMatrixFieldUnitPrice * scoreMultiplier[NeighbouringMatrixUnitCount] * Owner.Tier : 0.0f;
    }
}