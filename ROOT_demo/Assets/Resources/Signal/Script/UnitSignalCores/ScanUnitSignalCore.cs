using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;


namespace ROOT.Signal
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
                if (now.UnitSignal == SignalType.Scan && now.UnitHardware == HardwareType.Field)
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
                unit.SignalCore.ScanSignalPathDepth = length--;
                unit.SignalCore.ServerSignalDepth = cnt;
                if (unit.UnitSignal == SignalType.Scan && unit.UnitHardware == HardwareType.Field)
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
                foreach (var unitConnectedToHardDrive in hardDrive.GetConnectedOtherUnit.Where(unit =>
                    IsVis(unit, vis) == false))
                {
                    if ((unitConnectedToHardDrive.UnitSignal == SignalType.Scan &&
                         unitConnectedToHardDrive.UnitHardware == HardwareType.Field) &&
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
                        hardDriveQueue.Enqueue(new Tuple<Unit, ulong>(unitConnectedToHardDrive,
                            AddPath(unitConnectedToHardDrive, vis)));
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

        public List<Unit> GetPath(Unit dest, Dictionary<Unit, Unit> pathTable, Unit source)
        {
            var res = new List<Unit>();
            while (dest != source)
            {
                res.Add(dest);
                dest = pathTable[dest];
            }
            //remove source scan field unit 
            res.Reverse();
            return res;
        }

        public List<List<Unit>> GetConnectUnit(Unit now)
        {
            //Looking for the next scan field unit that is one signal distance away
            var res = new List<List<Unit>>();
            Owner.GameBoard.Units.ForEach(unit => unit.SignalCore.Visiting = false);
            var pre = new Dictionary<Unit, Unit>();
            var queue = new Queue<Unit>();
            now.SignalCore.Visiting = true;
            queue.Enqueue(now);
            while (queue.Count > 0)
            {
                var tmp = queue.Dequeue();
                foreach (var otherUnit in tmp.GetConnectedOtherUnit.Where(unit =>
                    unit.SignalCore.Visiting == false && unit.SignalCore.Visited == false))
                {
                    otherUnit.SignalCore.Visiting = true;
                    if (otherUnit.UnitSignal == SignalType.Scan && otherUnit.UnitHardware == HardwareType.Core)
                    {
                        continue;
                    }
                    pre[otherUnit] = tmp;
                    if (otherUnit.UnitSignal == SignalType.Scan && otherUnit.UnitHardware == HardwareType.Field)
                    {
                        res.Add(GetPath(otherUnit, pre, now));
                    }
                    else
                    {
                        queue.Enqueue(otherUnit);
                    }
                }
            }
            return res;
        }

        public int GetScore(List<Unit> path)
        {
            return path.Where(unit => unit.UnitSignal == SignalType.Scan && unit.UnitHardware == HardwareType.Field).Sum(unit => unit.Tier);
        }

        public void dfs(ref List<Unit> path, ref List<Unit> ans, int localSignalLength, ref int minLength, ref int maxScore)
        {
            if (localSignalLength > minLength)
                return;
            var connectedUnit = GetConnectUnit(path.Last());
            if (connectedUnit.Count == 0)
            {
                var localSignalScore = GetScore(path);
                if (localSignalLength == minLength && localSignalScore > maxScore)
                {
                    maxScore = localSignalScore;
                    ans = new List<Unit>(path);
                }

                if (localSignalLength!=0 && localSignalLength < minLength)
                {
                    minLength = localSignalLength;
                    maxScore = localSignalScore;
                    ans = new List<Unit>(path);
                }

                return;
            }

            foreach (var subPath in connectedUnit)
            {
                foreach (var unit in subPath)
                {
                    unit.SignalCore.Visited = true;
                    path.Add(unit);
                }
                dfs(ref path, ref ans, localSignalLength + 1, ref minLength, ref maxScore);
                for (var i = subPath.Count - 1; i >= 0; --i)
                {
                    var unit = subPath[i];
                    unit.SignalCore.Visited = false;
                    path.RemoveAt(path.Count - 1);
                }
            }
        }

        public List<Unit> CalScore(ref int minLength, ref int maxScore)
        {
            var ans = new List<Unit>();
            var path = new List<Unit>();
            Owner.GameBoard.Units.ForEach(unit => unit.SignalCore.Visited = false);
            Owner.SignalCore.Visited = true;
            path.Add(Owner);
            dfs(ref path, ref ans, 0, ref minLength, ref maxScore);
            return ans;
        }

        public List<Unit> CalScore(ref int maxCount, ref int maxScore, ref int maxLength)
        {
            var res = new List<Unit>();
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
                        res = GeneratePath(Owner, vis);
                    }

                    foreach (var unitConnectedToLastNode in networkCable.GetConnectedOtherUnit.Where(unit =>
                        lastLevelDict.ContainsKey(unit)))
                    {
                        var lastNodeButOne = lastLevelDict[unitConnectedToLastNode];
                        if (PathContains(vis, lastNodeButOne.Item4) == false &&
                            lastNodeButOne.Item3 + Utils.GetUnitTierInt(networkCable) > maxScore)
                        {
                            maxScore = lastNodeButOne.Item3 + Utils.GetUnitTierInt(networkCable);
                            Owner.GameBoard.Units.ForEach(unit => unit.SignalCore.InServerGrid = false);
                            res = GeneratePath(Owner, AddPath(networkCable, lastNodeButOne.Item4));
                        }
                    }
                }
            }

            if (maxLength == maxCount)
            {
                Owner.GameBoard.Units.ForEach(unit => unit.SignalCore.InServerGrid = false);
                maxScore = 0;
            }

            return res;
        }

        public override SignalType SignalType => SignalType.Scan;

        public override List<Vector2Int> SingleInfoCollectorZone
        {
            get
            {
                const float networkA = 1.45f;
                const float networkB = 1.74f;
                var circleTier =
                    Math.Max(Mathf.RoundToInt(Mathf.Pow(SignalMasterMgr.MaxNetworkDepth / networkB, networkA)), 0);
                var zone = Utils.GetPixelateCircle_Tier(circleTier);
                var res = new List<Vector2Int>();
                zone.PatternList.ForEach(vec =>
                    res.Add(vec + Owner.CurrentBoardPosition - new Vector2Int(zone.CircleRadius, zone.CircleRadius)));
                return res;
            }
        }

        internal bool IsUnitVeryActive
        {
            get
            {
                if (SignalMasterMgr.Instance.Paths == null || !SignalMasterMgr.Instance.HasAnyPath(SignalType)) return false;
                return Owner == SignalMasterMgr.Instance.Paths[SignalType][0].Last();
            }
        }

        public override bool IsUnitActive
        {
            get
            {
                if (!SignalMasterMgr.Instance.HasAnyPath(SignalType)) return false;
                var normalActive = SignalMasterMgr.Instance.WithinCertainSignalPath(Owner, SignalType);
                return normalActive || IsUnitVeryActive;
            }
        }

        public override float SingleUnitScore => IsUnitVeryActive ? GetServerIncomeByLength(Owner.SignalCore.SignalDataPackList[SignalType.Scan].SignalDepth) : 0.0f;
    }
}