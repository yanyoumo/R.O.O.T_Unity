using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT
{
    using networkCableStatus = Tuple<Unit, int, int, ulong>;
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
                        score = CalculateProcessorScoreCore(nextKey, Utils.GetInvertDirection(direction), depth + 1);
                    }
                }
            }

            return score;
        }

        private float CalculateProcessorScoreCore(Vector2Int hostKey, RotationDirection dir, int depth)
        {
            var score = 0.0f;
            m_Board.UnitsGameObjects.TryGetValue(hostKey, out var currentUnit);
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

        List<Unit> FindEndLeafPoint()
        {
            var res = new List<Unit>();
            //vidited: dequeued
            //visiting: in queue
            m_Board.Units.ForEach(unit => unit.InServerGrid = unit.Visited = unit.Visiting = false);
            foreach (var startPoint in m_Board.FindUnitWithCoreType(CoreType.Server))
            {
                //enumerate unvisited server
                if (!startPoint.Visited)
                {
                    var networkCableQueue = new Queue<Tuple<Unit, ulong>>();
                    startPoint.Visited = true;
                    networkCableQueue.Enqueue(new Tuple<Unit, ulong>(startPoint, AddPath(startPoint, 0ul)));
                    startPoint.Visiting = true;
                    //BFS
                    while (networkCableQueue.Count != 0)
                    {
                        var (networkCable, vis) = networkCableQueue.Dequeue();
                        networkCable.Visiting = false;
                        networkCable.Visited = true;
                        var hardDriveQueue = new Queue<Tuple<Unit, ulong>>();
                        hardDriveQueue.Enqueue(new Tuple<Unit, ulong>(networkCable, vis));
                        var isEnd = true;
                        //BFS again, to find next level network cable
                        //if no unit found, it is end leaf point
                        while (hardDriveQueue.Count != 0)
                        {
                            var (hardDrive, vis2) = hardDriveQueue.Dequeue();
                            //enumerate the other unvisited units connected with this unit
                            foreach (var unitConnectedToHardDrive in hardDrive.GetConnectedOtherUnit.Where(
                                unit => IsVis(unit, vis2) == false && unit.Visited == false))
                            {
                                //network cable: mark as visited; push into network cable BFS queue
                                if (unitConnectedToHardDrive.UnitCore == CoreType.NetworkCable)
                                {
                                    unitConnectedToHardDrive.Visited = true;
                                    networkCableQueue.Enqueue(new Tuple<Unit, ulong>(unitConnectedToHardDrive,
                                        AddPath(unitConnectedToHardDrive, vis2)));
                                    isEnd = false;
                                }
                                //other unit: mark as vivited; push into haed drive cable BFS queue
                                else
                                {
                                    unitConnectedToHardDrive.Visited = true;
                                    hardDriveQueue.Enqueue(new Tuple<Unit, ulong>(unitConnectedToHardDrive,
                                        AddPath(unitConnectedToHardDrive, vis2)));
                                }
                            }
                        }
                        if (isEnd == true && networkCable.UnitCore != CoreType.Server)
                        {
                            res.Add(networkCable);
                        }
                    }
                }
            }
            return res;
        }
        public List<List<Unit>> CalculateProcessorScoreFindSetA()
        {
            int maxCount = Board.BoardLength * Board.BoardLength;
            var res = new List<List<Unit>>();
            var endLeavePoint = FindEndLeafPoint();
            RootDebug.Log(endLeavePoint.Count.ToString(), NameID.JiangDigong_Log);
            foreach (var i in endLeavePoint)
            {
                RootDebug.Log(i.name.ToString() + i.CurrentBoardPosition.ToString(), NameID.JiangDigong_Log);
            }
            foreach (var startPoint in endLeavePoint)
            {
                var maxLength = maxCount;
                m_Board.Units.ForEach(unit => unit.InServerGrid = unit.Visited = unit.Visiting = false);
                startPoint.Visiting = true;
                var networkCableQueue = new Queue<Tuple<Unit, int, ulong>>();
                networkCableQueue.Enqueue(new Tuple<Unit, int, ulong>(startPoint, 0, 0ul));
                while (networkCableQueue.Count != 0)
                {
                    var (networkCable, length, vis) = networkCableQueue.Dequeue();
                    if (length > maxLength)
                        break;
                    networkCable.Visited = true;
                    var hardDriveQueue = new Queue<Tuple<Unit, ulong>>();
                    hardDriveQueue.Enqueue(new Tuple<Unit, ulong>(networkCable, vis));
                    while (hardDriveQueue.Count != 0)
                    {
                        var (hardDrive, vis2) = hardDriveQueue.Dequeue();
                        foreach (var unitConnectedToHardDrive in hardDrive.GetConnectedOtherUnit.Where(unit => IsVis(unit, vis2) == false))
                        {
                            if (unitConnectedToHardDrive.UnitCore == CoreType.NetworkCable &&
                                unitConnectedToHardDrive.Visited == false &&
                                unitConnectedToHardDrive.Visiting == false)
                            {
                                unitConnectedToHardDrive.Visiting = true;
                                networkCableQueue.Enqueue(new Tuple<Unit, int, ulong>(unitConnectedToHardDrive,
                                    length + 1,
                                    AddPath(unitConnectedToHardDrive, vis2)));
                            }
                            else if (unitConnectedToHardDrive.UnitCore == CoreType.HardDrive)
                                hardDriveQueue.Enqueue(new Tuple<Unit, ulong>(unitConnectedToHardDrive, AddPath(unitConnectedToHardDrive, vis2)));
                            else if (unitConnectedToHardDrive.UnitCore == CoreType.Server)
                            {
                                maxLength = length + 1;
                                res.Add(GeneratePath(startPoint, AddPath(unitConnectedToHardDrive, vis2)));
                            }
                        }
                    }
                }
            }
            return res;
        }

        public float CalculateProcessorScore(out int driverCountInt)
        {
            var driverCount = 0.0f;
            var processorKeys = new List<Vector2Int>();

            foreach (var keyValuePair in m_Board.UnitsGameObjects)
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
                driverCountInt = 0;
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
                        driverCount += CalculateProcessorScoreCore(processorKey, RotationDirection.North, 0);
                    }
                }

                MaxNormalDepth = (int)driverCount;
                driverCountInt = (int)driverCount;
                return Mathf.FloorToInt(driverCount * GetPerDriverIncome);
            }
        }

        #region 服务器计分标准
        //Digong 服务器计分分数需要读取Unit.Tier参数。
        //其提供信号深度为调用Shop.TierMultiplier中得出的SignalMultipler数值。
        //此时就出现了物理距离和信号距离两种距离。
        //一个Tier为3的单元目前提供1物理距离，提供3信号距离。
        //具体的计分逻辑要进行微调：
        //  1、查看必要性的时候使用物理距离。
        //  2、寻找最长必要距离时使用信号距离。
        //  3、寻找最长距离时，如果信号距离相等，则选择其中物理距离较短的（平均信号/物理密度较高的那个）。
        //  4、若信号距离相等、且密度相等，则随便选择一条。
        //并且对本系列函数补充部分注释。
        public List<Unit> GeneratePath(Unit start, ulong vis)
        {
            var unitPathList = new List<Unit>();
            var now = start;
            int cnt = 0;
            while (vis != 0ul)
            {
                if (now.UnitCore == CoreType.NetworkCable)
                {
                    cnt += now.Tier;
                }
                unitPathList.Add(now);
                now.InServerGrid = true;
                vis = RemovePath(now, vis);
                foreach (var otherUnit in now.GetConnectedOtherUnit)
                {
                    if (IsVis(otherUnit, vis))
                    {
                        now = otherUnit;
                        break;
                    }
                }
            }
            var length = unitPathList.Count;
            foreach (var unit in unitPathList)
            {
                unit.ServerDepth = cnt;
                if (unit.UnitCore == CoreType.NetworkCable)
                {
                    cnt -= unit.Tier;
                }
            }
            return unitPathList;
        }

        public ulong UnitToBit64(Unit now)
        {
            return 1ul << Utils.UnrollVector2Int(now.CurrentBoardPosition, Board.BoardLength);
        }

        public bool IsVis(Unit now, ulong vis)
        {
            return (vis & UnitToBit64(now)) != 0ul;
        }

        public ulong AddPath(Unit now, ulong vis)
        {
            return vis ^ UnitToBit64(now);
        }

        public ulong RemovePath(Unit now, ulong vis)
        {
            return AddPath(now, vis);
        }

        public static bool PathContains(ulong a, ulong b)
        {
            return (a & b) == b;
        }

        public bool FindNextLevelNetworkCable(Queue<networkCableStatus> networkCableQueue,
                                            Queue<Tuple<Unit, ulong>> hardDriveQueue,
                                            int length,
                                            int score)
        {
            bool isLast = true;
            while (hardDriveQueue.Count != 0)
            {
                var (hardDrive, vis) = hardDriveQueue.Dequeue();
                foreach (var unitConnectedToHardDrive in hardDrive.GetConnectedOtherUnit.Where(unit => IsVis(unit, vis) == false))
                {
                    if (unitConnectedToHardDrive.UnitCore == CoreType.NetworkCable &&
                        unitConnectedToHardDrive.Visited == false)
                    {
                        isLast = false;
                        if (unitConnectedToHardDrive.Visiting == false)
                        {
                            unitConnectedToHardDrive.Visiting = true;
                            networkCableQueue.Enqueue(new networkCableStatus(unitConnectedToHardDrive,
                                length + 1,
                                score + Utils.GetUnitTierInt(unitConnectedToHardDrive),
                                AddPath(unitConnectedToHardDrive, vis)));
                        }
                    }
                    else
                        hardDriveQueue.Enqueue(new Tuple<Unit, ulong>(unitConnectedToHardDrive, AddPath(unitConnectedToHardDrive, vis)));
                }
            }
            return isLast;
        }
        public float CalculateServerScore(out int networkCount)
        {
            int maxCount = Board.BoardLength * Board.BoardLength;
            var maxScore = Int32.MinValue;
            var maxLength = maxCount;
            foreach (var startPoint in m_Board.FindUnitWithCoreType(CoreType.Server))
            {
                var lastLevelDict = new Dictionary<Unit, networkCableStatus>();
                var thisLevelDict = new Dictionary<Unit, networkCableStatus>();
                m_Board.Units.ForEach(unit => unit.Visited = unit.Visiting = false);
                startPoint.Visiting = true;
                var networkCableQueue = new Queue<networkCableStatus>();
                networkCableQueue.Enqueue(new networkCableStatus(startPoint, 0, 0, AddPath(startPoint, 0ul)));
                while (networkCableQueue.Count != 0)
                {
                    var (networkCable, length, score, vis) = networkCableQueue.Dequeue();
                    if (thisLevelDict.Count > 0 &&
                        length > thisLevelDict.First().Value.Item2)
                    {
                        lastLevelDict = thisLevelDict;
                        thisLevelDict = new Dictionary<Unit, networkCableStatus>
                        {
                            {networkCable, new networkCableStatus(networkCable, length, score, vis)}
                        };
                    }
                    else
                        thisLevelDict.Add(networkCable, new networkCableStatus(networkCable, length, score, vis));
                    if (length > maxLength)
                        break;
                    networkCable.Visited = true;
                    networkCable.Visiting = false;
                    var hardDriveQueue = new Queue<Tuple<Unit, ulong>>();
                    hardDriveQueue.Enqueue(new Tuple<Unit, ulong>(networkCable, vis));
                    if (FindNextLevelNetworkCable(networkCableQueue, hardDriveQueue, length, score) &&
                        ((length < maxLength && length != 0) || length == maxLength))
                    {
                        if (length < maxLength || (length == maxLength && score > maxScore))
                        {
                            maxScore = score;
                            maxLength = length;
                            m_Board.Units.ForEach(unit => unit.InServerGrid = false);
                            GeneratePath(startPoint, vis);
                        }

                        foreach (var unitConnectedToLastNode in networkCable.GetConnectedOtherUnit.Where(unit => lastLevelDict.ContainsKey(unit)))
                        {
                            var lastNodeButOne = lastLevelDict[unitConnectedToLastNode];
                            if (PathContains(vis, lastNodeButOne.Item4) == false &&
                                lastNodeButOne.Item3 + Utils.GetUnitTierInt(networkCable) > maxScore)
                            {
                                maxScore = lastNodeButOne.Item3 + Utils.GetUnitTierInt(networkCable);
                                m_Board.Units.ForEach(unit => unit.InServerGrid = false);
                                GeneratePath(startPoint, AddPath(networkCable, lastNodeButOne.Item4));
                            }
                        }
                    }
                }
            }
            if (maxLength == maxCount)
            {
                m_Board.Units.ForEach(unit => unit.InServerGrid = false);
                maxScore = 0;
            }
            MaxNetworkDepth = networkCount = maxScore;
            return GetServerIncomeByLength(maxScore);
        }
        #endregion
        private float CalculateBasicCost()
        {
            return m_Board.Units.Sum(unit => GetCostByCore(unit.UnitCore));
        }

        private float CalculateTieredCost()
        {
            return m_Board.Units.Sum(unit => unit.Cost);
        }

        //这个返回的也是正数。
        //Tokenize后，这个逻辑要换。
        public float CalculateCost()
        {
            return CalculateTieredCost();
            //return CalculateBasicCost();
        }
    }
}
