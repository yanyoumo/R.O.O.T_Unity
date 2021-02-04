using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT.Signal
{
    public class MatrixUnitSignalCore : UnitSignalCoreBase
    {
        [ShowInInspector] public int MatrixVal=> SignalStrengthComplex[SignalType.Matrix].Item1;
        [ShowInInspector] public override SignalType SignalType => SignalType.Matrix;

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
        
        private const int perMatrixFieldUnitPrice = 1;
        public override float SingleUnitScore => IsUnitActive ? perMatrixFieldUnitPrice : 0.0f;
    }
}