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
                        var (scoreMutiplier,item2,item3)=ShopMgr.TierMultiplier(unit.Tier);
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
        //TODO Digong 学习并优化服务器计分部分代码。
        //积分原则：只统计从任意一服务器延伸出的必要最长信号长度。
        //积分结果：1、参考CalculateServerScore函数中约270行左右的return
        //        将之前得出必要最长信号长度(int)输入GetServerIncomeByLength函数后返回。
        //        2、必要最长信号长度填充入networkCount及MaxNetworkDepth变量。
        //        3、正确填充Unit类中“服务器计分”相关变量节。
        //备注：1、请注意处于必要最长信号上但是并不是Network的unit的深度，要保证其最后LED显示正确。
        //     2、请将定义于本节外的所有函数视为黑盒。
        //     3、在本节内可以定义任何新函数，但请补充xml-summary。
        private void CalculateServerScoreSingleDir(Unit unit, Vector2Int hostKey, RotationDirection direction, int depth)
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
            m_Board.UnitsGameObjects.TryGetValue(hostKey, out var currentUnit);
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

        public List<Unit> GeneratePath(Unit start, Unit end, ulong vis)
        {
            var res = new List<Unit>();
            var now = start;
            vis = AddPath(end, vis);
            while (vis != 0ul)
            {
                res.Add(now);
                now.InServerGrid = true;
                vis = RemovePath(now, vis);
                foreach (var keyValuePair in now.WorldNeighboringData)
                {
                    var otherUnit = keyValuePair.Value.OtherUnit;
                    if (otherUnit != null && IsVis(otherUnit, vis))
                    {
                        now = otherUnit;
                        break;
                    }
                }
            }
            //Debug.Log("START " + start.CurrentBoardPosition + "END " + end.CurrentBoardPosition + "Len " + res.Count);
            var length = 0;
            for (int i = res.Count - 1; i >= 0; --i)
            {
                res[i].ServerDepth = ++length;
                //Debug.Log(res[i].CurrentBoardPosition.ToString());
            }
            return res;
        }

        public bool IsVis(Unit now, ulong vis)
        {
            return (vis & (1ul << Utils.UnrollVector2Int(now.CurrentBoardPosition,m_Board.BoardLength))) != 0ul;
        }

        public ulong AddPath(Unit now, ulong vis)
        {
            return vis ^ (1ul << Utils.UnrollVector2Int(now.CurrentBoardPosition, m_Board.BoardLength));
        }
        public ulong RemovePath(Unit now, ulong vis)
        {
            return AddPath(now, vis);
        }

        /*public float CalculateServerScore(out int networkCount)
        {
            int maxCount = m_Board.BoardLength * m_Board.BoardLength;
            var maxLength = maxCount;
            var resPath = new List<Unit>();
            foreach (var startPoint in m_Board.FindUnitWithCoreType(CoreType.Server))
            {
                m_Board.Units.ForEach(unit => unit.InServerGrid = unit.Visited = false);
                startPoint.Visited = true;
                var networkCableQueue = new Queue<Tuple<Unit, int, ulong>>();
                networkCableQueue.Enqueue(new Tuple<Unit, int, ulong>(startPoint, 0, AddPath(startPoint, 0ul)));
                //Debug.Log("ENQUE " + startPoint.CurrentBoardPosition.ToString());
                while (networkCableQueue.Count != 0)
                {
                    var (networkCable, length, vis) = networkCableQueue.Dequeue();
                    var hardDriveQueue = new Queue<Tuple<Unit, ulong>>();
                    hardDriveQueue.Enqueue(new Tuple<Unit, ulong>(networkCable, vis));
                    while (hardDriveQueue.Count != 0)
                    {
                        var (hardDrive, vis2) = hardDriveQueue.Dequeue();
                        foreach (var hardDriveNeighbor in hardDrive.WorldNeighboringData)
                        {
                            var unitConnectedToHardDrive = hardDriveNeighbor.Value.OtherUnit;
                            if (unitConnectedToHardDrive != null && IsVis(unitConnectedToHardDrive, vis2) == false)
                            {
                                if (unitConnectedToHardDrive.UnitCore == CoreType.NetworkCable && unitConnectedToHardDrive.Visited == false)
                                {
                                    bool flag = false;
                                    foreach (var networkCableNeighbor in unitConnectedToHardDrive.WorldNeighboringData)
                                    {
                                        var unitConnectedToNetworkCable = networkCableNeighbor.Value.OtherUnit;
                                        if (unitConnectedToNetworkCable != null && IsVis(unitConnectedToNetworkCable, vis2) == false && unitConnectedToNetworkCable.UnitCore == CoreType.NetworkCable)
                                        {
                                            flag = true;
                                            break;
                                        }
                                    }

                                    int val = 1;
                                    if (flag == false)
                                    {
                                        if (length + val < maxLength)
                                        {
                                            maxLength = length + val;
                                            resPath = GeneratePath(startPoint, unitConnectedToHardDrive, vis2);
                                        }
                                        goto END_SPOT;
                                    }

                                    unitConnectedToHardDrive.Visited = true;
                                    networkCableQueue.Enqueue(new Tuple<Unit, int, ulong>(unitConnectedToHardDrive, length + val,
                                        AddPath(unitConnectedToHardDrive, vis2)));
                                    //Debug.Log("ENQUE " + unitConnectedToHardDrive.CurrentBoardPosition.ToString());
                                }
                                else
                                {
                                    hardDriveQueue.Enqueue(new Tuple<Unit, ulong>(unitConnectedToHardDrive, AddPath(unitConnectedToHardDrive, vis2)));
                                }
                            }
                        }
                    }
                }
                END_SPOT:;
            }

            if (maxLength == maxCount) maxLength = 0;

            MaxNetworkDepth = networkCount = maxLength;
            return GetServerIncomeByLength(maxLength);
        }*/

        [Obsolete]
        public float CalculateServerScore(out int networkCount)
        {
            var maxLength = 0.0f;
            var farthestUnitPos = Vector2Int.zero;
            var serverKeys = new List<Vector2Int>();

            foreach (var keyValuePair in m_Board.UnitsGameObjects)
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
                networkCount = 0;
                return 0.0f;
            }
            else
            {
                //score += 0.0f;//只有服务器没分儿
                foreach (var key in serverKeys)
                {
                    m_Board.UnitsGameObjects.TryGetValue(key, out var currentServerUnit);
                    currentServerUnit.GetComponentInChildren<Unit>().ServerDepth = -1;

                    CalculateServerScoreCore(key, 0, key); //这是在服务器本身上面调的，没有Srckey，或者说就是它本身。
                }

                foreach (var keyValuePair in m_Board.UnitsGameObjects)
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
                        m_Board.UnitsGameObjects.TryGetValue(farthestUnitPos, out var currentUnit);
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

                foreach (var unit1 in m_Board.Units.Where(unit=> (unit.UnitCore == CoreType.Server)))
                {
                    unit1.InServerGrid = true;
                }

                networkCount = MaxNetworkDepth = (int)maxLength;
                return GetServerIncomeByLength((int) maxLength);
            }
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
