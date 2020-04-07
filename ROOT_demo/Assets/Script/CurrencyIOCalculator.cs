using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public struct ScoreContext
    {
        public SideType ConnectionType;
        public Func<Unit, CoreType,float, bool> ActionOnUnitUponVisit;
        public Func<Unit, RotationDirection, SideType, bool> ConnectionRequirement;
    }

    public partial class CurrencyIOCalculator : MonoBehaviour
    {
        public Board m_Board;

        public void Awake()
        {
            InitIncomeCost();
        }

        private float CalculateProcessorScoreSingleDir(UnitBase unit, Vector2Int hostKey, RotationDirection direction)
        {
            var score = 0.0f;
            var side = unit.GetWorldSpaceUnitSide(direction);
            if (side == SideType.ParallelConnector)
            {
                var nextKey = hostKey + Utils.ConvertDirectionToBoardPosOffset(direction);
                if (m_Board.CheckBoardPosValidAndFilled(nextKey))
                {
                    var nextGo = m_Board.FindUnitUnderBoardPos(nextKey);
                    Debug.Assert(nextGo != null);
                    var nextUnit = nextGo.GetComponentInChildren<Unit>();
                    var otherSide =
                        nextUnit.GetWorldSpaceUnitSide(Utils.GetInvertDirection(direction));

                    if (otherSide == SideType.ParallelConnector)
                    {
                        score = CalculateProcessorScoreCore(nextKey);
                    }
                }
            }

            return score;
        }

        private float CalculateProcessorScoreCore(Vector2Int hostKey)
        {
            var score = 0.0f;
            m_Board.Units.TryGetValue(hostKey, out var currentUnit);
            if (currentUnit != null)
            {
                var unit = currentUnit.GetComponentInChildren<Unit>();
                if (!unit.Visited)
                {
                    if (unit.UnitCore == CoreType.HardDrive)
                    {
                        score += 1.0f;
                    }

                    unit.Visited = true;

                    score += CalculateProcessorScoreSingleDir(unit, hostKey, RotationDirection.North);
                    score += CalculateProcessorScoreSingleDir(unit, hostKey, RotationDirection.East);
                    score += CalculateProcessorScoreSingleDir(unit, hostKey, RotationDirection.South);
                    score += CalculateProcessorScoreSingleDir(unit, hostKey, RotationDirection.West);
                }
            }
            else
            {
                Debug.Assert(true);
            }

            return score;
        }

        public float CalculateProcessorScore()
        {
            var driverCount = 0.0f;
            var processorKeys = new List<Vector2Int>();

            foreach (var keyValuePair in m_Board.Units)
            {
                var unit = keyValuePair.Value.GetComponentInChildren<Unit>();
                unit.Visited = false;
                if (unit.UnitCore == CoreType.Processor)
                {
                    processorKeys.Add(keyValuePair.Key);
                    //现在是设计为任何接续到任何一个CPU上的硬盘都算分。但是只能算一次，就是一个集群中有两个CPU也只能算一次分。
                }
            }

            if (processorKeys.Count == 0)
            {
                return 0.0f;
            }
            else
            {
                //score += 1.0f;//基础处理器还是不算分儿了
                foreach (var processorKey in processorKeys)
                {
                    var go = m_Board.FindUnitUnderBoardPos(processorKey);
                    var unit = go.GetComponentInChildren<Unit>();
                    if (!unit.Visited)
                    {
                        driverCount += CalculateProcessorScoreCore(processorKey);
                    }
                }

                return driverCount * GetPerDriverIncome();
            }
        }

        private void CalculateServerScoreSingleDir(UnitBase unit, Vector2Int hostKey, RotationDirection direction,
            int depth)
        {
            var side = unit.GetWorldSpaceUnitSide(direction);
            if (side == SideType.SerialConnector)
            {
                var nextKey = hostKey + Utils.ConvertDirectionToBoardPosOffset(direction);
                if (m_Board.CheckBoardPosValidAndFilled(nextKey))
                {
                    var nextGo = m_Board.FindUnitUnderBoardPos(nextKey);
                    Debug.Assert(nextGo != null);
                    var nextUnit = nextGo.GetComponentInChildren<Unit>();
                    var otherSide =
                        nextUnit.GetWorldSpaceUnitSide(Utils.GetInvertDirection(direction));

                    if (otherSide == SideType.SerialConnector)
                    {
                        CalculateServerScoreCore(nextKey, depth);
                    }
                }
            }
        }

        private void CalculateServerScoreCore(Vector2Int hostKey, int currentDepth)
        {
            var depth = currentDepth;
            m_Board.Units.TryGetValue(hostKey, out var currentUnit);
            if (currentUnit != null)
            {
                var unit = currentUnit.GetComponentInChildren<Unit>();
                if (unit.IntA > depth || unit.IntA == -1)
                {
                    unit.Visited = true;
                    if (unit.UnitCore == CoreType.NetworkCable)
                    {
                        depth++;
                        unit.IntA = depth;
                    }
                    else
                    {
                        unit.IntA = depth;
                    }

                    CalculateServerScoreSingleDir(unit, hostKey, RotationDirection.North, depth);
                    CalculateServerScoreSingleDir(unit, hostKey, RotationDirection.East, depth);
                    CalculateServerScoreSingleDir(unit, hostKey, RotationDirection.South, depth);
                    CalculateServerScoreSingleDir(unit, hostKey, RotationDirection.West, depth);
                }
            }
            else
            {
                Debug.Assert(true);
            }
        }

        public float CalculateServerScore()
        {
            var maxLength = 0.0f;
            var serverKeys = new List<Vector2Int>();

            foreach (var keyValuePair in m_Board.Units)
            {
                var unit = keyValuePair.Value.GetComponentInChildren<Unit>();
                unit.Visited = false;
                unit.IntA = -1;
                if (unit.UnitCore == CoreType.Server)
                {
                    serverKeys.Add(keyValuePair.Key); //现在处理了，取其中一个，是对于任意一个Server中必要最长的。
                }
            }

            if (serverKeys.Count == 0)
            {
                return 0.0f;
            }
            else
            {
                //score += 0.0f;//只有服务器没分儿
                foreach (var key in serverKeys)
                {
                    CalculateServerScoreCore(key, 0);
                    foreach (var value in m_Board.Units.Values)
                    {
                        var unit = value.GetComponentInChildren<Unit>();
                        maxLength = Mathf.Max(maxLength, unit.IntA);
                        unit.IntA = -1;
                    }
                }

                return GetServerIncomeByLength((int) maxLength);
            }
        }

        //这个返回的也是正数。
        public float CalculateCost()
        {
            float cost = 0.0f;
            foreach (var value in m_Board.Units.Values)
            {
                var unit = value.GetComponentInChildren<Unit>();
                cost += GetCostByCore(unit.UnitCore);
            }
            return cost;
        }
    }
}
