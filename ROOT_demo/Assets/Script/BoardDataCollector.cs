using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEditorInternal.VR;
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
        //TODO Digong 服务器计分分数需要读取Unit.Tier参数。
        //其提供信号深度为调用Shop.TierMultiplier中得出的SignalMultipler数值。
        //此时就出现了物理距离和信号距离两种距离。
        //一个Tier为3的单元目前提供1物理距离，提供3信号距离。
        //具体的计分逻辑要进行微调：
        //  1、查看必要性的时候使用物理距离。
        //  2、寻找最长必要距离时使用信号距离。
        //  3、寻找最长距离时，如果信号距离相等，则选择其中物理距离较短的（平均信号/物理密度较高的那个）。
        //  4、若信号距离相等、且密度相等，则随便选择一条。
        //并且对本系列函数补充部分注释。
        public List<Unit> GeneratePath(Unit start, Unit end, ulong vis)
        {
            var res = new List<Unit>();
            var now = start;
            end.InServerGrid = true;
            vis = AddPath(end, vis);
            while (vis != 0ul)
            {
                res.Add(now);
                now.InServerGrid = true;
                vis = RemovePath(now, vis);
                foreach (var keyValuePair in now.WorldNeighboringData)
                {
                    var otherUnit = keyValuePair.Value.OtherUnit;
                    if (keyValuePair.Value.Connected && otherUnit != null && IsVis(otherUnit, vis))
                    {
                        now = otherUnit;
                        break;
                    }
                }
            }
            Debug.Log("START " + start.CurrentBoardPosition + "END " + end.CurrentBoardPosition + "Len " + res.Count);
            var length = 0;
            for (int i = res.Count - 1; i >= 0; --i)
            {
                res[i].ServerDepth = ++length;
                Debug.Log(res[i].CurrentBoardPosition.ToString());
            }
            return res;
        }

        public bool IsVis(Unit now, ulong vis)
        {
            return (vis & (1ul << Utils.UnrollVector2Int(now.CurrentBoardPosition, m_Board.BoardLength))) != 0ul;
        }

        public ulong AddPath(Unit now, ulong vis)
        {
            return vis ^ (1ul << Utils.UnrollVector2Int(now.CurrentBoardPosition, m_Board.BoardLength));
        }

        public ulong RemovePath(Unit now, ulong vis)
        {
            return AddPath(now, vis);
        }

        public float CalculateServerScore(out int networkCount)
        {
            int maxCount = m_Board.BoardLength * m_Board.BoardLength;
            var maxLength = maxCount;
            var resPath = new List<Unit>();
            foreach (var startPoint in m_Board.FindUnitWithCoreType(CoreType.Server))
            {
                m_Board.Units.ForEach(unit => unit.InServerGrid = unit.Visited = unit.Visiting = false);
                startPoint.Visiting = true;
                var networkCableQueue = new Queue<Tuple<Unit, int, ulong>>();
                networkCableQueue.Enqueue(new Tuple<Unit, int, ulong>(startPoint, 0, AddPath(startPoint, 0ul)));
                //Debug.Log("start ENQUE1 " + startPoint.CurrentBoardPosition);
                while (networkCableQueue.Count != 0)
                {
                    var (networkCable, length, vis) = networkCableQueue.Dequeue();
                    networkCable.Visited = true;
                    //Debug.Log("DEQUE1 " + networkCable.CurrentBoardPosition);
                    var hardDriveQueue = new Queue<Tuple<Unit, ulong>>();
                    hardDriveQueue.Enqueue(new Tuple<Unit, ulong>(networkCable, vis));
                    //Debug.Log("start ENQUE2 " + networkCable.CurrentBoardPosition);
                    var isLast = true;
                    while (hardDriveQueue.Count != 0)
                    {
                        var (hardDrive, vis2) = hardDriveQueue.Dequeue();
                        //Debug.Log("DEQUE2 " + hardDrive.CurrentBoardPosition);
                        foreach (var hardDriveNeighbor in hardDrive.WorldNeighboringData)
                        {
                            var unitConnectedToHardDrive = hardDriveNeighbor.Value.OtherUnit;
                            if (hardDriveNeighbor.Value.Connected && unitConnectedToHardDrive != null && IsVis(unitConnectedToHardDrive, vis2) == false)
                            {
                                if (unitConnectedToHardDrive.UnitCore == CoreType.NetworkCable && unitConnectedToHardDrive.Visited == false)
                                {
                                    isLast = false;
                                    if (unitConnectedToHardDrive.Visiting == false)
                                    {
                                        unitConnectedToHardDrive.Visiting = true;
                                        int val = 1;
                                        networkCableQueue.Enqueue(new Tuple<Unit, int, ulong>(unitConnectedToHardDrive, length + val,
                                            AddPath(unitConnectedToHardDrive, vis2)));
                                        //Debug.Log("ENQUE1 " + unitConnectedToHardDrive.CurrentBoardPosition + unitConnectedToHardDrive.UnitCore);
                                    }
                                }
                                else
                                {
                                    hardDriveQueue.Enqueue(new Tuple<Unit, ulong>(unitConnectedToHardDrive, AddPath(unitConnectedToHardDrive, vis2)));
                                    //Debug.Log("ENQUE2 " + unitConnectedToHardDrive.CurrentBoardPosition + unitConnectedToHardDrive.UnitCore);
                                }
                            }
                        }
                    }
                    if (isLast == true && length < maxLength && length != 0)
                    {
                        maxLength = length;
                        resPath = GeneratePath(startPoint, networkCable, vis);
                        break;
                    }
                }
            }

            if (maxLength == maxCount) maxLength = 0;
            //Debug.Log("ANS " + maxLength);
            MaxNetworkDepth = networkCount = maxLength;
            return GetServerIncomeByLength(maxLength);
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
