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
            return unit.InHddSignalGrid && otherUnit.InHddSignalGrid;
        }

        public static int SignalVal(Unit unit, Unit otherUnit)
        {
            return Math.Min(unit.HardDiskVal, otherUnit.HardDiskVal);
        }

        private float CalculateProcessorScoreSingleDir(Unit unit, Vector2Int hostKey, RotationDirection direction, int depth)
        {
            var score = 0.0f;
            var side = unit.GetWorldSpaceUnitSide(direction);
            if (side == SideType.Connection)
            {
                var nextKey = hostKey + Utils.ConvertDirectionToBoardPosOffset(direction);
                if (Owner.GameBoard.CheckBoardPosValidAndFilled(nextKey))
                {
                    var nextGo = Owner.GameBoard.FindUnitUnderBoardPos(nextKey);
                    Debug.Assert(nextGo != null);
                    var nextUnit = nextGo.GetComponentInChildren<Unit>();
                    var otherSide = nextUnit.GetWorldSpaceUnitSide(Utils.GetInvertDirection(direction));

                    if (otherSide == SideType.Connection)
                    {
                        score = CalculateProcessorScoreCore(nextKey, Utils.GetInvertDirection(direction), depth + 1);
                    }
                }
            }

            return score;
        }

        private float CalculateProcessorScoreCore(Vector2Int hostKey, RotationDirection dir, int depth)
        {
            var score = 0.0f;
            Owner.GameBoard.UnitsGameObjects.TryGetValue(hostKey, out var currentUnit);
            if (currentUnit != null)
            {
                var unit = currentUnit.GetComponentInChildren<Unit>();
                if (!unit.Visited)
                {
                    if (unit.UnitCore == CoreType.HardDrive)
                    {
                        var (scoreMutiplier, item2, item3) = ShopMgr.TierMultiplier(unit.Tier);
                        score += scoreMutiplier;
                        unit.InHddGrid = true;
                    }

                    unit.InHddSignalGrid = true;
                    unit.Visited = true;

                    score += CalculateProcessorScoreSingleDir(unit, hostKey, RotationDirection.North, depth);
                    score += CalculateProcessorScoreSingleDir(unit, hostKey, RotationDirection.East, depth);
                    score += CalculateProcessorScoreSingleDir(unit, hostKey, RotationDirection.South, depth);
                    score += CalculateProcessorScoreSingleDir(unit, hostKey, RotationDirection.West, depth);

                    unit.SignalFromDir = dir;
                    unit.HardDiskVal = (int)score;
                }
            }
            else
            {
                Debug.Assert(true);
            }

            return score;
        }
        
        public override float CalScore(out int driverCountInt)
        {
            driverCountInt = Mathf.RoundToInt(CalculateProcessorScoreCore(Owner.CurrentBoardPosition, RotationDirection.North, 0));
            return driverCountInt;
        }
    }
}