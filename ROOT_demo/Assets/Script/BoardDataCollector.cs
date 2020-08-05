using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public struct ScoreContext
    {
        public SideType ConnectionType;
        public Func<Unit, CoreType, float, bool> ActionOnUnitUponVisit;
        public Func<Unit, RotationDirection, SideType, bool> ConnectionRequirement;
    }

    public partial class BoardDataCollector : MonoBehaviour
    {
        public static int MaxNormalDepth;
        public static int MaxNetworkDepth;

        public Board m_Board;

        public void Awake()
        {
            InitIncomeCost();
        }

        private float CalculateProcessorScoreSingleDir(Unit unit, Vector2Int hostKey, RotationDirection direction, int depth)
        {
            var score = 0.0f;
            var side = unit.GetWorldSpaceUnitSide(direction);
            if (side == SideType.Connection)
            {
                var nextKey = hostKey + Utils.ConvertDirectionToBoardPosOffset(direction);
                if (m_Board.CheckBoardPosValidAndFilled(nextKey))
                {
                    var nextGo = m_Board.FindUnitUnderBoardPos(nextKey);
                    Debug.Assert(nextGo != null);
                    var nextUnit = nextGo.GetComponentInChildren<Unit>();
                    var otherSide = nextUnit.GetWorldSpaceUnitSide(Utils.GetInvertDirection(direction));

                    if (otherSide == SideType.Connection)
                    {
                        score = CalculateProcessorScoreCore(nextKey, Utils.GetInvertDirection(direction), depth+1);
                    }
                }
            }

            return score;
        }

        private float CalculateProcessorScoreCore(Vector2Int hostKey,RotationDirection dir,int depth)
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
                        unit.InHddGrid = true;
                    }

                    unit.InHddSignalGrid = true;
                    unit.Visited = true;

                    score += CalculateProcessorScoreSingleDir(unit, hostKey, RotationDirection.North, depth);
                    score += CalculateProcessorScoreSingleDir(unit, hostKey, RotationDirection.East, depth);
                    score += CalculateProcessorScoreSingleDir(unit, hostKey, RotationDirection.South, depth);
                    score += CalculateProcessorScoreSingleDir(unit, hostKey, RotationDirection.West, depth);

                    unit.SignalFromDir = dir;
                    unit.HardDiskVal = (int) score;
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
                unit.InHddGrid = false;
                unit.InHddSignalGrid = false;
                if (unit.UnitCore == CoreType.Processor)
                {
                    unit.InHddGrid = true;
                    unit.InHddSignalGrid = true;
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
                        //CPU的这个方位用不着。
                        driverCount += CalculateProcessorScoreCore(processorKey,RotationDirection.North,0);
                    }
                }

                MaxNormalDepth = (int)driverCount;
                return driverCount * GetPerDriverIncome();
            }
        }

        private void CalculateServerScoreSingleDir(Unit unit, Vector2Int hostKey, RotationDirection direction,int depth)
        {
            var side = unit.GetWorldSpaceUnitSide(direction);
            if (side == SideType.Connection)
            {
                var nextKey = hostKey + Utils.ConvertDirectionToBoardPosOffset(direction);
                if (m_Board.CheckBoardPosValidAndFilled(nextKey))
                {
                    var nextGo = m_Board.FindUnitUnderBoardPos(nextKey);
                    Debug.Assert(nextGo != null);
                    var nextUnit = nextGo.GetComponentInChildren<Unit>();
                    var otherSide = nextUnit.GetWorldSpaceUnitSide(Utils.GetInvertDirection(direction));

                    if (otherSide == SideType.Connection)
                    {
                        CalculateServerScoreCore(nextKey, depth, hostKey);
                    }
                }
            }
        }

        private void CalculateServerScoreCore(Vector2Int hostKey, int currentDepth, Vector2Int srcPos)
        {
            var depth = currentDepth;
            m_Board.Units.TryGetValue(hostKey, out var currentUnit);
            if (currentUnit != null)
            {
                var unit = currentUnit.GetComponentInChildren<Unit>();
                if (unit.ServerDepth > depth || unit.ServerDepth == -1)
                {
                    unit.Visited = true;
                    unit.LastNetworkPos = srcPos;
                    if (unit.UnitCore == CoreType.NetworkCable)
                    {
                        depth++;
                        unit.ServerDepth = depth;
                    }
                    else
                    {
                        unit.ServerDepth = depth;
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
            var farthestUnitPos = Vector2Int.zero;
            var serverKeys = new List<Vector2Int>();

            foreach (var keyValuePair in m_Board.Units)
            {
                var unit = keyValuePair.Value.GetComponentInChildren<Unit>();
                unit.Visited = false;
                unit.InServerGrid = false;
                unit.ServerDepth = -1;
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
                    m_Board.Units.TryGetValue(key, out var currentServerUnit);
                    currentServerUnit.GetComponentInChildren<Unit>().ServerDepth = -1;

                    CalculateServerScoreCore(key, 0, key); //这是在服务器本身上面调的，没有Srckey，或者说就是它本身。
                }

                foreach (var keyValuePair in m_Board.Units)
                {
                    //之所以第二个Server没有再计算是因为他的IntA被写了，所以就不再计算了。
                    //两个Server被串在一起后，肯定有一个（随机）会被完全剔出计算流程。
                    //讲道理，如果两个Server被串在一起，就是应该只有半个的距离：最远Unit应该是任何一个Server中必要最长的。
                    var unit = keyValuePair.Value.GetComponentInChildren<Unit>();
                    //这个max被顶起来了下不去了，之前没有考虑另一个Server会给这个数据降下来的可能。
                    if (unit.ServerDepth > maxLength)
                    {
                        maxLength = unit.ServerDepth;
                        farthestUnitPos = keyValuePair.Key;
                    }

                    //unit.ServerDepth = -1;？？为什么？
                }

                int maxCount = 1000;
                int counter = 0;

                Vector2Int tmp = new Vector2Int(-1, -1);

                if (maxLength > 0)
                {
                    Unit farthestUnit;
                    do
                    {
                        m_Board.Units.TryGetValue(farthestUnitPos, out var currentUnit);
                        Debug.Assert(currentUnit);
                        farthestUnit = currentUnit.GetComponentInChildren<Unit>();
                        farthestUnit.InServerGrid = true;
                        tmp = farthestUnit.NextBoardPosition;
                        farthestUnitPos = farthestUnit.LastNetworkPos;
                        counter++;
                        if (counter >= maxCount)
                        {
                            Debug.Assert(false, "ERROR");
                            break;
                        }
                    } while (tmp != farthestUnit.LastNetworkPos);
                }

                foreach (var keyValuePair in m_Board.Units)
                {
                    var unit = keyValuePair.Value.GetComponentInChildren<Unit>();
                    if (unit.UnitCore == CoreType.Server)
                    {
                        unit.InServerGrid = true;
                    }
                }

                MaxNetworkDepth = (int)maxLength;
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
