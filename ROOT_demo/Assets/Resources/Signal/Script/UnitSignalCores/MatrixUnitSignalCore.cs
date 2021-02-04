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
        [ReadOnly]
        [ShowInInspector]
        public int MatrixVal=> SignalStrength[SignalType.Matrix];
        /*public static bool ShowSignal(Unit unit, Unit otherUnit)
        {
            return unit.SignalCore.InMatrixSignal && otherUnit.SignalCore.InMatrixSignal;
        }

        public static int SignalVal(Unit unit, Unit otherUnit)
        {
            return Math.Min(unit.SignalCore.SignalStrength[SignalType.Matrix], otherUnit.SignalCore.SignalStrength[SignalType.Matrix]);
        }*/

        /*[Obsolete]
        private float CalculateProcessorScoreSingleDir(Unit unit, Vector2Int hostKey, RotationDirection direction, int depth)
        {
            if (unit.GetWorldSpaceUnitSide(direction) != SideType.Connection) return 0.0f;
            var nextKey = hostKey + Utils.ConvertDirectionToBoardPosOffset(direction);
            if (!Owner.GameBoard.CheckBoardPosValidAndFilled(nextKey)) return 0.0f;

            var nextUnit = Owner.GameBoard.FindUnitUnderBoardPos(nextKey)?.GetComponentInChildren<Unit>();
            if (nextUnit == null) return 0.0f;

            var invDir=Utils.GetInvertDirection(direction);
            if (nextUnit.GetWorldSpaceUnitSide(invDir) != SideType.Connection) return 0.0f;
            return CalculateProcessorScoreCore(nextKey, invDir, depth + 1);
        }

        private float CalculateProcessorScoreCore(Vector2Int hostKey, RotationDirection dir, int depth)
        {
            var score = 0.0f;
            var unit = GameBoard.FindUnitUnderBoardPos(hostKey)?.GetComponentInChildren<Unit>();
            var signalCore = unit?.SignalCore;
            if (unit == null || signalCore.Visited) return score;

            if (unit.UnitSignal == Type && unit.UnitHardware == HardwareType.Field)
            {
                var (scoreMultiplier, item2, item3) = ShopBase.TierMultiplier(unit.Tier);
                score += scoreMultiplier;
                signalCore.InMatrix = true;
            }

            signalCore.InMatrixSignal = true;
            signalCore.Visited = true;

            score += CalculateProcessorScoreSingleDir(unit, hostKey, RotationDirection.North, depth);
            score += CalculateProcessorScoreSingleDir(unit, hostKey, RotationDirection.East, depth);
            score += CalculateProcessorScoreSingleDir(unit, hostKey, RotationDirection.West, depth);
            score += CalculateProcessorScoreSingleDir(unit, hostKey, RotationDirection.South, depth);

            signalCore.SignalFromDir = dir;
            signalCore.SignalStrength[SignalType.Matrix] = (int) score;
            return score;
        }*/

        public override SignalType SignalType => SignalType.Matrix;

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

        //public override bool IsSignalUnitCoreActive => SignalStrengthComplex[SignalType].Item1 > 0;

        //这个数据也不对、还没有减去HeatSink的流程。不在这里不减了。
        public override float SingleUnitScore => IsSignalUnitCoreActive ? perMatrixFieldUnitPrice : 0.0f;

        /*public override float CalScore(out int hardwareCount)
        {
            hardwareCount = GameBoard.Units.Where(u => u.SignalCore is MatrixUnitSignalCore).Count(u => u.SignalCore.SignalStrengthComplex[SignalType.Matrix].Item1 > 0);
            return Mathf.RoundToInt(hardwareCount * perMatrixFieldUnitPrice);
        }*/

        /*[Obsolete]
        public float CalScore_Old(out int driverCountInt)
        {
            driverCountInt = Mathf.RoundToInt(CalculateProcessorScoreCore(Owner.CurrentBoardPosition, RotationDirection.North, 0));
            return driverCountInt * perMatrixFieldUnitPrice;
        }*/
    }
}