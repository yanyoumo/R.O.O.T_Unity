using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
// ReSharper disable PossibleMultipleEnumeration

namespace ROOT.Signal
{
    public class ThermoUnitSignalCore : UnitSignalCoreBase
    {
        public override SignalType SignalType => SignalType.Thermo;

        public List<Vector2Int> ExpellingPatternList
        {
            get
            {
                var zone = Utils.GetPixelateCircle_Tier(Owner.Tier - 1);
                var res = new List<Vector2Int>();
                zone.PatternList.ForEach(vec => res.Add(vec + Owner.CurrentBoardPosition - new Vector2Int(zone.CircleRadius, zone.CircleRadius)));
                return res;
            }
        }


        public override List<Vector2Int> SingleInfoCollectorZone => ExpellingPatternList;

        private float getScoreFromPercentage(float x)
        {
            float k = 1f, b = 1f;
            return k * x + b;
        }

        public override bool IsUnitActive => HasCertainSignal(SignalType);

        public override float SingleUnitScore
        {
            get
            {
                var validPattern = ExpellingPatternList.Where(Board.CheckBoardPosValidStatic);
                var emptyPos = validPattern.Where(p => Owner.GameBoard.CheckBoardPosValidAndEmpty(p + Owner.CurrentBoardPosition));
                return getScoreFromPercentage((float) emptyPos.Count() / validPattern.Count());
            }
        }
    }
}
