using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class MatrixUnitSignalCore : UnitSignalCoreBase
    {
        public static bool ShowSignal(Unit unit, Unit otherUnit)
        {
            return unit.SignalCore.InMatrixSignal && otherUnit.SignalCore.InMatrixSignal;
        }

        public static int SignalVal(Unit unit, Unit otherUnit)
        {
            return Math.Min(unit.SignalCore.MatrixVal, otherUnit.SignalCore.MatrixVal);
        }

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
            signalCore.MatrixVal = (int) score;
            return score;
        }

        public override SignalType Type => SignalType.Matrix;

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

        public override float CalScore(out int driverCountInt)
        {
            driverCountInt =
                Mathf.RoundToInt(CalculateProcessorScoreCore(Owner.CurrentBoardPosition, RotationDirection.North, 0));
            return driverCountInt;
        }
    }
}