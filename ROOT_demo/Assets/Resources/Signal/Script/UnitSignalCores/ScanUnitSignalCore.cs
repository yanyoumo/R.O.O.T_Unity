using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;


namespace ROOT
{
    using networkCableStatus = Tuple<Unit, int, int, ulong>;
    public class ScanUnitSignalCore : UnitSignalCoreBase
    {
        private List<Unit> GeneratePath(Unit start, ulong vis)
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
                now.SignalCore.InServerGrid = true;
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
                unit.SignalCore.ServerDepth = length--;
                unit.SignalCore.ServerSignalDepth = cnt;
                if (unit.UnitCore == CoreType.NetworkCable)
                {
                    cnt -= unit.Tier;
                }
            }
            return unitPathList;
        }

        private ulong UnitToBit64(Unit now)
        {
            return 1ul << Utils.UnrollVector2Int(now.CurrentBoardPosition, Board.BoardLength);
        }

        private bool IsVis(Unit now, ulong vis)
        {
            return (vis & UnitToBit64(now)) != 0ul;
        }

        private ulong AddPath(Unit now, ulong vis)
        {
            return vis ^ UnitToBit64(now);
        }

        private ulong RemovePath(Unit now, ulong vis)
        {
            return AddPath(now, vis);
        }

        private static bool PathContains(ulong a, ulong b)
        {
            return (a & b) == b;
        }

        private bool FindNextLevelNetworkCable(Queue<networkCableStatus> networkCableQueue,
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
                        unitConnectedToHardDrive.SignalCore.Visited == false)
                    {
                        isLast = false;
                        if (unitConnectedToHardDrive.SignalCore.Visiting == false)
                        {
                            unitConnectedToHardDrive.SignalCore.Visiting = true;
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
        public float GetServerIncomeByLength(int length)
        {
            float[] incomeArrayDel = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };
            float incomeArrayBase = 0.0f;
            float income = incomeArrayBase;
            for (int i = 0; i < length; i++)
            {
                int idx = Mathf.Min(i, incomeArrayDel.Length - 1);
                income += incomeArrayDel[idx];
            }

            return Mathf.Floor(income);
        }

public float CalScore(out int networkCount, ref int maxCount, ref int maxScore, ref int maxLength)
        {
            //这里调一下你那个有更多的输出变量CalScore函数就行了。（函数名想变都能变。
            //return CalScore(out networkCount, out var A, out var B, out var C);
            var thisLevelDict = new Dictionary<Unit, networkCableStatus>();
            var lastLevelDict = new Dictionary<Unit, networkCableStatus>();
            Owner.GameBoard.Units.ForEach(unit => unit.SignalCore.Visited = unit.SignalCore.Visiting = false);
            Owner.SignalCore.Visiting = true;
            var networkCableQueue = new Queue<networkCableStatus>();
            networkCableQueue.Enqueue(new networkCableStatus(Owner, 0, 0, AddPath(Owner, 0ul)));
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
                networkCable.SignalCore.Visited = true;
                networkCable.SignalCore.Visiting = false;
                var hardDriveQueue = new Queue<Tuple<Unit, ulong>>();
                hardDriveQueue.Enqueue(new Tuple<Unit, ulong>(networkCable, vis));
                if (FindNextLevelNetworkCable(networkCableQueue, hardDriveQueue, length, score) &&
                    ((length < maxLength && length != 0) || length == maxLength))
                {
                    if (length < maxLength || (length == maxLength && score > maxScore))
                    {
                        maxScore = score;
                        maxLength = length;
                        Owner.GameBoard.Units.ForEach(unit => unit.SignalCore.InServerGrid = false);
                        GeneratePath(Owner, vis);
                    }

                    foreach (var unitConnectedToLastNode in networkCable.GetConnectedOtherUnit.Where(unit => lastLevelDict.ContainsKey(unit)))
                    {
                        var lastNodeButOne = lastLevelDict[unitConnectedToLastNode];
                        if (PathContains(vis, lastNodeButOne.Item4) == false &&
                            lastNodeButOne.Item3 + Utils.GetUnitTierInt(networkCable) > maxScore)
                        {
                            maxScore = lastNodeButOne.Item3 + Utils.GetUnitTierInt(networkCable);
                            Owner.GameBoard.Units.ForEach(unit => unit.SignalCore.InServerGrid = false);
                            GeneratePath(Owner, AddPath(networkCable, lastNodeButOne.Item4));
                        }
                    }
                }
            }
            if (maxLength == maxCount)
            {
                Owner.GameBoard.Units.ForEach(unit => unit.SignalCore.InServerGrid = false);
                maxScore = 0;
            }
            BoardDataCollector.MaxNetworkDepth = networkCount = maxScore;
            return GetServerIncomeByLength(maxScore);
        }

        public override SignalType Type => SignalType.Scan;

        public override List<Vector2Int> SingleInfoCollectorZone
        {
            get
            {
                const float networkA = 1.45f;
                const float networkB = 1.74f;
                var circleTier = Math.Max(Mathf.RoundToInt(Mathf.Pow(BoardDataCollector.MaxNetworkDepth / networkB, networkA)), 0);
                var zone = Utils.GetPixelateCircle_Tier(circleTier);
                var res = new List<Vector2Int>();
                zone.PatternList.ForEach(vec => res.Add(vec + Owner.CurrentBoardPosition - new Vector2Int(zone.CircleRadius, zone.CircleRadius)));
                return res;
            }
        }

        public override float CalScore(out int networkCount)
        {
            throw new NotImplementedException();
        }
    }
}