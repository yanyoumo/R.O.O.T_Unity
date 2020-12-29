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
        public int MaxCount;
        public int MaxScore;
        public int MaxLength;
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
        public override float CalScore(out int networkCount)
        {
            var lastLevelDict = new Dictionary<Unit, networkCableStatus>();
            var thisLevelDict = new Dictionary<Unit, networkCableStatus>();
            Owner.GameBoard.Units.ForEach(unit => unit.Visited = unit.Visiting = false);
            Owner.Visiting = true;
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
                if (length > MaxLength)
                    break;
                networkCable.Visited = true;
                networkCable.Visiting = false;
                var hardDriveQueue = new Queue<Tuple<Unit, ulong>>();
                hardDriveQueue.Enqueue(new Tuple<Unit, ulong>(networkCable, vis));
                if (FindNextLevelNetworkCable(networkCableQueue, hardDriveQueue, length, score) &&
                    ((length < MaxLength && length != 0) || length == MaxLength))
                {
                    if (length < MaxLength || (length == MaxLength && score > MaxScore))
                    {
                        MaxScore = score;
                        MaxLength = length;
                        Owner.GameBoard.Units.ForEach(unit => unit.InServerGrid = false);
                        GeneratePath(Owner, vis);
                    }

                    foreach (var unitConnectedToLastNode in networkCable.GetConnectedOtherUnit.Where(unit => lastLevelDict.ContainsKey(unit)))
                    {
                        var lastNodeButOne = lastLevelDict[unitConnectedToLastNode];
                        if (PathContains(vis, lastNodeButOne.Item4) == false &&
                            lastNodeButOne.Item3 + Utils.GetUnitTierInt(networkCable) > MaxScore)
                        {
                            MaxScore = lastNodeButOne.Item3 + Utils.GetUnitTierInt(networkCable);
                            Owner.GameBoard.Units.ForEach(unit => unit.InServerGrid = false);
                            GeneratePath(Owner, AddPath(networkCable, lastNodeButOne.Item4));
                        }
                    }
                }
            }
            if (MaxLength == MaxCount)
            {
                Owner.GameBoard.Units.ForEach(unit => unit.InServerGrid = false);
                MaxScore = 0;
            }
            BoardDataCollector.MaxNetworkDepth = networkCount = MaxScore;
            return GetServerIncomeByLength(MaxScore);
        }
    }
}