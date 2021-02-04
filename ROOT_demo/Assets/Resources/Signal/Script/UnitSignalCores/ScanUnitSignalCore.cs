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
                if (unit.UnitSignal == SignalType.Scan&&unit.UnitHardware == HardwareType.Field)
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
            float[] incomeArrayDel = {1.0f, 2.0f, 3.0f, 4.0f, 5.0f};
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

                    foreach (var unitConnectedToLastNode in networkCable.GetConnectedOtherUnit.Where(unit =>
                        lastLevelDict.ContainsKey(unit)))
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

            SignalMasterMgr.MaxNetworkDepth = networkCount = maxScore;
            return GetServerIncomeByLength(maxScore);
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

        [ShowInInspector]
        public override bool GetActivationStatusPerSignal
        {
            get
            {
                //TODO
                return false;
            }
        }

        public override bool IsUnitActive
        {
            get
            {
                //TODO 
                //首先、想利用已有的数据地图计算扫描的算法绝对有：
                //利用图像处理的思路；只考虑扫描信号深度场的本地峰值的最小值。那个值就是"必要最短路径的末端单元（激活单元）"。
                //知道了激活单元后、在自激活单元的位置、做全部考虑的单元的、在扫描信号硬件深度场上做梯度下降就可以了。
                //细一想有点OverHype了
                return false;
            }
        }

        //TODO
        public override float SingleUnitScore => 0.0f;
    }
}