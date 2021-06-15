using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ROOT
{
    public sealed class BoardGirdDriver
    {
        [ReadOnly] public Board owner;
        [ReadOnly] public Dictionary<Vector2Int, BoardGirdCell> BoardGirds;

        private HeatSinkPatternLib HeatSinkPatterns => owner.HeatSinkPatterns;

        //public bool HasInfoZone { private set; get; }

        public void ClearAllEdges(EdgeStatus _edgeStatus)
        {
            BoardGirds.Values.ForEach(grid => grid.ClearEdge(_edgeStatus));
        }

        public void UpdateInfoZone(List<Vector2Int> CollectorZone)
        {
            BoardGirds.Values.ForEach(grid => grid.ClearEdge(EdgeStatus.InfoZone));
            BoardGirds.Values.ForEach(grid => grid.SetEdge(CollectorZone, EdgeStatus.InfoZone));
        }

        public void UpdateSingleInfoZone(List<Vector2Int> CollectorZone)
        {
            //Debug.Log("CollectorZone.Count=" + CollectorZone.Count);
            BoardGirds.Values.ForEach(grid => grid.ClearEdge(EdgeStatus.SingleInfoZone));
            BoardGirds.Values.ForEach(grid => grid.SetEdge(CollectorZone, EdgeStatus.SingleInfoZone));
        }
        
        public List<Vector2Int> ExtractCachedZone(EdgeStatus edgeStatus) => BoardGirds.Keys.Where(keys =>BoardGirds[keys].LayeringEdgeStatus.HaveFlag(edgeStatus)).ToList();

        private PatternPermutation _HeatSinkPermutation = PatternPermutation.None;
        private int _currentHeatSinkPatternsID;
        private int _currentHeatSinkDiminishingID;

        public int MinHeatSinkCount => ActualHeatSinkPos.Length;
        private Vector2Int[] RawHeatSinkPos => new Vector2Int[0]; //现在使初始pattern都是空的。

        private Vector2Int[] ActualHeatSinkPos => GetActualHeatSinkUpward().ForEach(vec => Utils.PermutateV2I(vec, Board.BoardLength - 1, _HeatSinkPermutation)).ToArray();

        public int DiminishingStep { get; private set; }

        public void DestoryHeatsinkOverlappedUnit()
        {
            foreach (var actualHeatSinkPo in ActualHeatSinkPos)
            {
                owner.TryDeleteCertainUnit(actualHeatSinkPo);
            }
        }

        private Vector2Int[] GetActualHeatSinkUpward()
        {
            //现在出来一个问题，就是基础图样加上这一轮添加后，有可能就堆满了。
            //有办法，就是想办法在pattern里面储存“不能被填充”这种数据。
            //现在Diminishing里面加了一个Maxout。hmmmm先这样，并没有解决上面的核心问题。
            //Debug.Log("DiminishingStep="+DiminishingStep);
            if (DiminishingStep == -1) return RawHeatSinkPos;

            var res = RawHeatSinkPos.ToList();
            var HeatSinkPattern = HeatSinkPatterns.DiminishingList[_currentHeatSinkDiminishingID];
            var dimList = HeatSinkPattern.DiminishingList;
            //RISK 这个算法还是考虑写道HeatSinkPattern的算法里面。
            var maxOut = HeatSinkPattern.CutOffCount;

            var TaperedDiminishingStep = Mathf.Min(DiminishingStep, maxOut);

            for (var i = 0; i < TaperedDiminishingStep; i++)
            {
                if (i < dimList.Count && !res.Contains(dimList[i]))
                {
                    res.Add(dimList[i]);
                }
            }

            return res.ToArray();
        }

        /// <summary>
        /// 这个函数时往下减少HeatSink数量。
        /// </summary>
        /// <returns>计算完毕后的HeatSinkPattern</returns>
        [Obsolete]
        private Vector2Int[] GetActualHeatSinkDownward()
        {
            if (DiminishingStep == -1) return RawHeatSinkPos;

            var res = RawHeatSinkPos.ToList();
            var dimList = HeatSinkPatterns.DiminishingList[_currentHeatSinkDiminishingID].DiminishingList;
            for (var i = 0; i < DiminishingStep; i++)
            {
                if (i < dimList.Count && res.Contains(dimList[i]))
                {
                    res.Remove(dimList[i]);
                }
            }

            return res.ToArray();
        }

        public void UpdatePatternDiminishing()
        {
            DiminishingStep++;
        }

        public void UpdatePatternID()
        {
            _HeatSinkPermutation = (PatternPermutation)Random.Range(0, 6);
            var oldID = _currentHeatSinkPatternsID;
            var oldDimID = _currentHeatSinkDiminishingID;
            const int max = 100;
            var counter = 0;
            do
            {
                counter++;
                _currentHeatSinkPatternsID = Random.Range(0, HeatSinkPatterns.Lib.Count);
                _currentHeatSinkDiminishingID = Random.Range(0, HeatSinkPatterns.DiminishingList.Count);
            } while ((_currentHeatSinkDiminishingID == oldDimID || _currentHeatSinkPatternsID == oldID) && counter <= max);

            //ActualHeatSinkPos = new Vector2Int[];
            DiminishingStep = 0;
            foreach (var boardGirdsValue in BoardGirds.Values)
            {
                //刷一下格子的消耗。
                boardGirdsValue.HeatSinkCost = 0;
                boardGirdsValue.CellStatus = CellStatus.Normal;
            }
        }

        public void InitBoardGird()
        {
            var BoardLength = Board.BoardLength;
            BoardGirds = new Dictionary<Vector2Int, BoardGirdCell>();
            for (var i = 0; i < BoardLength; i++)
            {
                for (var j = 0; j < BoardLength; j++)
                {
                    var go = MonoBehaviour.Instantiate(owner.BoardGridTemplate, owner.BoardGridRoot);
                    var gridLength = owner._boardPhysicalLength;
                    var offset = new Vector3(i * gridLength, 0.0f, j * gridLength);
                    go.transform.localPosition = owner.BoardGridZeroing.position + offset;
                    var key = new Vector2Int(i, j);
                    BoardGirds.Add(key, go.GetComponent<BoardGirdCell>());
                    go.GetComponent<BoardGirdCell>().OnboardPos = key;
                    go.GetComponent<BoardGirdCell>().owner = owner;
                }
            }
            UpdatePatternID();
        }

        private Vector2Int? FindFurthestHeatSink(in Vector2Int[] existingHeatSink)
        {
            var BoardLength = Board.BoardLength;
            Vector2 center = new Vector2((BoardLength - 1) / 2.0f, (BoardLength - 1) / 2.0f);
            float distance = -1.0f;
            Vector2Int? FurthestHeatSink = null;

            for (int i = 0; i < BoardLength; i++)
            {
                for (int j = 0; j < BoardLength; j++)
                {
                    Vector2Int key = new Vector2Int(i, j);
                    if (owner.CheckBoardPosValidAndEmpty(key))
                    {
                        if (!existingHeatSink.Contains(key))
                        {
                            float tmpDistance = Vector2.Distance(key, center);
                            if (tmpDistance > distance)
                            {
                                distance = tmpDistance;
                                FurthestHeatSink = key;
                            }
                        }
                    }
                }
            }

            return FurthestHeatSink;
        }

        private Vector2Int? FindAHeatSink(in Vector2Int[] existingHeatSink)
        {
            for (var i = 0; i < Board.BoardLength; i++)
            {
                for (var j = 0; j < Board.BoardLength; j++)
                {
                    var key = new Vector2Int(i, j);
                    if (owner.CheckBoardPosValidAndEmpty(key) && (!existingHeatSink.Contains(key)))
                    {
                        return key;
                    }
                }
            }

            return null;
        }

        public void ResetHeatSink()
        {
            Debug.Log("HeatSink Reseted");
            DiminishingStep = 0;
        }

        [CanBeNull]
        public Unit[] OverlapHeatSinkUnit
        {
            get
            {
                CheckOverlappedHeatSinkCount(out var lappedHeatSinkCount);
                return lappedHeatSinkCount != 0 ? owner.Units.Where(unit => ActualHeatSinkPos.Contains(unit.CurrentBoardPosition)).ToArray() : null;
            }
        }

        private CellStatus GettargetingStatus(StageType type, BossStageType bossType = BossStageType.Telemetry)
        {
            CellStatus targetingStatus = CellStatus.Normal;
            switch (type)
            {
                case StageType.Shop:
                    targetingStatus = CellStatus.Normal;
                    break;
                case StageType.Boss: //TODO Boss的Sink状态还要在这儿决定。
                    if (bossType == BossStageType.Telemetry)
                    {
                        targetingStatus = CellStatus.Warning;
                    }
                    else if (bossType == BossStageType.Acquiring)
                    {
                        throw new NotImplementedException();
                    }
                    break;
                case StageType.Require:
                    targetingStatus = CellStatus.Warning;
                    break;
                case StageType.Destoryer:
                case StageType.Ending:
                    targetingStatus = CellStatus.Sink;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return targetingStatus;
        }

        private void ResetGridStatus()
        {
            BoardGirds.Values.ForEach(grid => grid.CellStatus = CellStatus.Normal);
        }

        private int CalcTotalHeatSinkCost(int totalCount)
        {
            const float pow = 1.25f;
            return Mathf.CeilToInt(Mathf.Pow(pow, totalCount) - 1);
        }

        private int CalcPerHeatSinkCost(int index)
        {
            Debug.Assert(index >= 0);
            int x1 = index + 1;
            int x0 = index;
            return (CalcTotalHeatSinkCost(x1) - CalcTotalHeatSinkCost(x0)) + 1;
        }

        public int CheckOverlappedHeatSinkCount(out int heatSinkCost)
        {
            var overlappedHeatSink = 0;
            heatSinkCost = 0;
            foreach (var pos in ActualHeatSinkPos)
            {
                if (owner.CheckBoardPosValidAndFilled(pos))
                {
                    //有很大问题！这个PreHeatSink的顺序和Grid本身存进去的数据不匹配。
                    //是个比较急转弯的事情、主要是本应该是捡Gird上面的数据、而不是从头开始算。
                    heatSinkCost += BoardGirds[pos].HeatSinkCost;
                    overlappedHeatSink++;
                }
            }
            return overlappedHeatSink;
        }

        public int HeatSinkCost
        {
            get
            {
                CheckOverlappedHeatSinkCount(out var res);
                return res;
            }
        }

        private int LastActualHeatSinkPos = -1;
        
        public void UpkeepHeatSink(StageType type)
        {
            if (ActualHeatSinkPos.Length == LastActualHeatSinkPos) return;
            //在这里计算每个HeatSink的价值。
            ResetGridStatus();
            //这里需要把status接进来，然后判是什么阶段的。
            if (type == StageType.Require)
            {
                var waringTile = new List<Vector2Int>();
                foreach (var actualHeatSinkPo in ActualHeatSinkPos)
                {
                    Common.Utils.ROTATION_LIST
                        .Select(o => Common.Utils.ConvertDirectionToBoardPosOffset(o) + actualHeatSinkPo)
                        .Where(s => !waringTile.Contains(s))
                        .ForEach(waringTile.Add);
                }

                BoardGirds.Where(val => waringTile.Contains(val.Key))
                    .ForEach(val => val.Value.CellStatus = CellStatus.PreWarning);
            }

            for (var i = 0; i < ActualHeatSinkPos.Length; i++)
            {
                var key = ActualHeatSinkPos[i];
                BoardGirds[key].HeatSinkID = i;
                BoardGirds[key].HeatSinkCost = CalcPerHeatSinkCost(i);
                BoardGirds[key].CellStatus = GettargetingStatus(type);
            }

            LastActualHeatSinkPos = ActualHeatSinkPos.Length;
        }

        /// <summary>
        /// 游戏板扫描格点查看是否有可服务的HeatSink格
        /// </summary>
        /// <returns>返回有多少个HeatSink格没有被满足，返回0即均满足。</returns>
        [Obsolete]
        public int ScanHeatSink()
        {
            //RISK 这个是O(n2)的函数，千万不能每帧调，就是Per-move才调。
            //购买抵达的时候应该也要算一次？
            Vector2Int[] heatSinkPos = new Vector2Int[MinHeatSinkCount];
            for (var i = 0; i < heatSinkPos.Length; i++)
            {
                heatSinkPos[i] = new Vector2Int(-1, -1);
            }

            int noHeatSinkCount = 0;
            BoardGirds.Values.ForEach(grid => grid.CellStatus = CellStatus.Normal);

            for (var i = 0; i < heatSinkPos.Length; i++)
            {
                var val = FindFurthestHeatSink(in heatSinkPos);
                if (val.HasValue)
                {
                    heatSinkPos[i] = val.Value;
                    BoardGirds[val.Value].CellStatus = CellStatus.Sink;
                }
                else
                {
                    heatSinkPos[i] = new Vector2Int(-1, -1);
                    noHeatSinkCount++;
                }
            }

            return noHeatSinkCount;
        }

        public void LightUpBoardGird(
            Vector2Int pos,
            LightUpBoardGirdMode mode = LightUpBoardGirdMode.REPLACE,
            LightUpBoardColor color = LightUpBoardColor.Hovered)
        {
            switch (mode)
            {
                case LightUpBoardGirdMode.REPLACE:
                    BoardGirds.Values.ForEach(val => val.ChangeStrokeMode(LightUpBoardColor.Unhovered));
                    BoardGirds.TryGetValue(pos, out var cell);
                    if (cell != null)
                    {
                        cell.ChangeStrokeMode(color);
                    }

                    break;
                case LightUpBoardGirdMode.ADD:
                    BoardGirds.TryGetValue(pos, out var cell1);
                    if (cell1 != null)
                    {
                        cell1.ChangeStrokeMode(color);
                    }

                    break;
                case LightUpBoardGirdMode.CLEAR:
                    BoardGirds.Values.ForEach(val => val.ChangeStrokeMode(LightUpBoardColor.Unhovered));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }

}