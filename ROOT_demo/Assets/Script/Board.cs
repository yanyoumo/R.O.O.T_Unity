﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ROOT
{
    public enum LightUpBoardGirdMode
    {
        REPLACE,
        ADD,
        CLEAR,
    }

    public enum LightUpBoardColor
    {
        Clicked,
        Hovered,
        Unhovered,
    }

    public sealed class Board : MonoBehaviour
    {
        public InfoAirdrop AirDrop;
        public Transform LFLocator;
        public Transform URLocator;

        public static Transform LFLocatorStatic;
        public static Transform URLocatorStatic;
        public static Vector2Int? WorldPosToXZGrid(Vector3 worldPos)
        {
            return WorldPosToXZGrid(new Vector2(worldPos.x, worldPos.z));
        }
        public static Vector2Int? WorldPosToXZGrid(Vector2 worldPosXZ)
        {
            var xN = Utils.SignalChannelSplit(LFLocatorStatic.transform.position.x, URLocatorStatic.transform.position.x, BoardLength, worldPosXZ.x);
            var yN = Utils.SignalChannelSplit(LFLocatorStatic.transform.position.z, URLocatorStatic.transform.position.z, BoardLength, worldPosXZ.y);
            var res= new Vector2Int(xN, yN);
            return CheckBoardPosValidStatic(res) ? (Vector2Int?) res : null;
        }
        
        public Unit FindNearestUnit(Vector2Int Pos)
        {
            var distance = float.MaxValue;
            var nearestPos = Vector2Int.zero;
            foreach (var vector2Int in UnitsGameObjects.Keys)
            {
                if (vector2Int == Pos)
                {
                    distance = 0;
                    nearestPos = vector2Int;
                    break;
                }
                else
                {
                    var tmpDist=Vector2.Distance(Pos, vector2Int);
                    if (tmpDist<distance)
                    {
                        distance = tmpDist;
                        nearestPos = vector2Int;
                    }
                }
            }

            return UnitsGameObjects[nearestPos].GetComponentInChildren<Unit>();
        }
        public Unit FindNearestSignalAP(Vector2Int Pos)
        {
            var distance = float.MaxValue;
            var nearestPos = Vector2Int.zero;
            foreach (var vector2Int in UnitsGameObjects.Keys)
            {
                var unit = UnitsGameObjects[vector2Int].GetComponentInChildren<Unit>();

                if (unit.UnitCore == CoreType.HardDrive ||
                    (unit.UnitCore == CoreType.NetworkCable && unit.SignalCore.InServerGrid && unit.SignalCore.ServerDepth == 1))
                {
                    if (vector2Int == Pos)
                    {
                        distance = 0;
                        nearestPos = vector2Int;
                        break;
                    }
                    else
                    {
                        var tmpDist = Vector2.Distance(Pos, vector2Int);
                        if (tmpDist < distance)
                        {
                            distance = tmpDist;
                            nearestPos = vector2Int;
                        }
                    }
                }
            }

            return UnitsGameObjects[nearestPos].GetComponentInChildren<Unit>();
        }

        public void SomeGridHasCollectedInfo(BoardGirdCell girdCell)
        {
            //逻辑到这里居然是好用的，只是需要去调整Extend的内容。
            //从逻辑上讲，只要把接收的分数加上加上就好了。剩下都是表现侧的。
            //girdCell.Blink();//TODO 这里要处理接收了Info的内容。
            var girdPos = girdCell.OnboardPos;
            var collectingUnit = FindNearestSignalAP(girdPos);
            collectingUnit.Blink(null); //接收了就先闪亮一下
        }

        public List<Vector2Int> GetInfoCollectorZone()
        {
            //这里保证前面调过一次计分函数，实在不行在这儿再调一遍。
            var res = new List<Vector2Int>();
            Units.Where(u => u.IsActiveMatrixFieldUnit || u.IsEndingScanFieldUnit).ForEach(u => res.AddRange(u.SignalCore.SingleInfoCollectorZone));
            return res.Where(CheckBoardPosValid).Distinct().ToList();
        }

        public void UpdateInfoZone(GameAssets levelAssets)
        {
            levelAssets.CollectorZone = GetInfoCollectorZone();
            BoardGirds.Values.ForEach(grid => grid.ClearEdge());
            BoardGirds.Values.ForEach(grid => grid.UpdateEdge(levelAssets.CollectorZone));
        }

        #region 热力系统

        public HeatSinkPatternLib HeatSinkPatterns;
        private PatternPermutation _HeatSinkPermutation = PatternPermutation.None;
        private int _currentHeatSinkPatternsID = 0;
        private int _currentHeatSinkDiminishingID = 0;

        public int MinHeatSinkCount=> ActualHeatSinkPos.Length;
        //private Vector2Int[] RawHeatSinkPos => HeatSinkPatterns.Lib[_currentHeatSinkPatternsID].Lib.ToArray();
        private Vector2Int[] RawHeatSinkPos => new Vector2Int[0];//现在使初始pattern都是空的。
        private Vector2Int[] ActualHeatSinkPos => GetActualHeatSinkUpward().Select(vec => Utils.PermutateV2I(vec, BoardLength-1, _HeatSinkPermutation)).ToArray();

        public int DiminishingStep { get; private set; }

        private void TryDeleteIfFilledCertainUnit(Vector2Int pos)
        {
            if (CheckBoardPosValidAndFilled(pos))
            {
                TryDeleteCertainUnit(pos);
            }
        }

        public void DestoryHeatsinkOverlappedUnit()
        {
            ActualHeatSinkPos.ForEach(TryDeleteIfFilledCertainUnit);
        }

        private Vector2Int[] GetActualHeatSinkUpward()
        {
            //RISK 现在出来一个问题，就是基础图样加上这一轮添加后，有可能就堆满了。
            //TODO 有办法，就是想办法在pattern里面储存“不能被填充”这种数据。
            //现在Diminishing里面加了一个Maxout。hmmmm先这样，并没有解决上面的核心问题。
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
            _HeatSinkPermutation = (PatternPermutation) Random.Range(0, 6);
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

            DiminishingStep = 0;
        }

        private void InitBoardGird()
        {
            BoardGirds=new Dictionary<Vector2Int, BoardGirdCell>();
            for (var i = 0; i < BoardLength; i++)
            {
                for (var j = 0; j < BoardLength; j++)
                {
                    var go = Instantiate(BoardGridTemplate, BoardGridRoot);
                    var gridLength = _boardPhysicalLength;
                    var offset = new Vector3(i * gridLength, 0.0f, j * gridLength);
                    go.transform.localPosition = BoardGridZeroing.position + offset;
                    var key = new Vector2Int(i, j);
                    BoardGirds.Add(key, go.GetComponent<BoardGirdCell>());
                    go.GetComponent<BoardGirdCell>().OnboardPos = key;
                    go.GetComponent<BoardGirdCell>().owner = this;
                }
            }
            UpdatePatternID();
        }

        private Vector2Int? FindFurthestHeatSink(in Vector2Int[] existingHeatSink)
        {
            Vector2 center = new Vector2((BoardLength - 1) / 2.0f, (BoardLength - 1) / 2.0f);
            float distance = -1.0f;
            Vector2Int? FurthestHeatSink=null;

            for (int i = 0; i < BoardLength; i++)
            {
                for (int j = 0; j < BoardLength; j++)
                {
                    Vector2Int key = new Vector2Int(i, j);
                    if (CheckBoardPosValidAndEmpty(key))
                    {
                        if (!existingHeatSink.Contains(key))
                        {
                            float tmpDistance = Vector2.Distance(key, center);
                            if (tmpDistance>distance)
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
            for (var i = 0; i < BoardLength; i++)
            {
                for (var j = 0; j < BoardLength; j++)
                {
                    var key = new Vector2Int(i, j);
                    if (CheckBoardPosValidAndEmpty(key)&&(!existingHeatSink.Contains(key)))
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
        public Unit[] OverlapHeatSinkUnit => CheckHeatSink(StageType.Shop) != 0 ? Units.Where(unit => ActualHeatSinkPos.Contains(unit.CurrentBoardPosition)).ToArray() : null;

        /// <summary>
        /// 这里是检查时候又fix的HeatSink被占用。
        /// </summary>
        /// <returns>返回有多少个HeatSink格没有被满足，返回0即均满足。</returns>
        public int CheckHeatSink(StageType type)
        {
            //这里需要把status接进来，然后判是什么阶段的。
            CellStatus targetingStatus;
            switch (type)
            {
                case StageType.Shop:
                    targetingStatus = CellStatus.Normal;
                    break;
                case StageType.Require:
                case StageType.Telemetry: //TODO Boss的Sink状态还要在这儿决定。
                    targetingStatus = CellStatus.Warning;
                    break;
                case StageType.Destoryer:
                case StageType.Ending:
                    targetingStatus = CellStatus.Sink;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            BoardGirds.Values.ForEach(grid => grid.CellStatus = CellStatus.Normal);
            if (type == StageType.Require)
            {
                var waringTile = new List<Vector2Int>();
                RotationDirection[] RotationList =
                {
                    RotationDirection.East,
                    RotationDirection.North,
                    RotationDirection.South,
                    RotationDirection.West
                };

                foreach (var actualHeatSinkPo in ActualHeatSinkPos)
                {
                    foreach (var rotationDirection in RotationList)
                    {
                        var offset = Utils.ConvertDirectionToBoardPosOffset(rotationDirection);
                        var tmpPos = actualHeatSinkPo + offset;
                        if (!waringTile.Contains(tmpPos))
                        {
                            waringTile.Add(tmpPos);
                        }
                    }
                }

                BoardGirds.Where(val => waringTile.Contains(val.Key)).ForEach(val => val.Value.CellStatus = CellStatus.PreWarning);
            }
            BoardGirds.Where(val => ActualHeatSinkPos.Contains(val.Key)).ForEach(val => val.Value.CellStatus = targetingStatus);
            
            return ActualHeatSinkPos.Count(pos => CheckBoardPosValidAndEmpty(pos) == false);
        }

        /// <summary>
        /// 游戏板扫描格点查看是否有可服务的HeatSink格
        /// </summary>
        /// <returns>返回有多少个HeatSink格没有被满足，返回0即均满足。</returns>
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

        public Dictionary<Vector2Int, BoardGirdCell> BoardGirds { get; private set; }

        public void LightUpBoardGird(
            Vector2Int pos,
            LightUpBoardGirdMode mode=LightUpBoardGirdMode.REPLACE, 
            LightUpBoardColor color= LightUpBoardColor.Hovered)
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

        #endregion

        public static Vector2Int ClampPosInBoard(Vector2Int pos)
        {
            var newPos = pos;
            newPos.x = Mathf.Clamp(newPos.x, 0, Board.BoardLength - 1);
            newPos.y = Mathf.Clamp(newPos.y, 0, Board.BoardLength - 1);
            return newPos;
        }

        public const int BoardLength = 6;
        public int TotalBoardCount => BoardLength * BoardLength;
        private readonly float _boardPhysicalLength = 1.2f;
        private readonly float _boardPhysicalOriginX = -3.1f - 1.75f-2.0f;
        private readonly float _boardPhysicalOriginY = -3.1f;

        public GameObject UnitTemplate;
        public GameObject BoardGridTemplate;
        public Transform BoardGridRoot;
        public Transform BoardGridZeroing;

        public Dictionary<Vector2Int, GameObject> UnitsGameObjects { get; private set; }

        public int GetTotalTierCountByCoreType(CoreType coreType)
        {
            return Units.Where(unit => unit.UnitCore == coreType).Sum(unit => unit.Tier);
        }
        public int GetUnitCount => UnitsGameObjects.Count;
        public int GetNonPCBUnitCount => Units.Count(unit => unit.UnitCore != CoreType.PCB);

        public Unit[] Units => UnitsGameObjects.Values.Select(unitsValue => unitsValue.GetComponentInChildren<Unit>()).ToArray();
        
        public int GetBoardID(Vector2Int pos)
        {
            return pos.y * BoardLength + pos.x;
        }

        public Unit[] FindUnitWithCoreType(CoreType type)
        {
            return Units.Where(u => u.UnitCore == type).ToArray();
        }

        [CanBeNull]
        public Unit RandomUnit => GetUnitCount == 0 ? null : Units[Mathf.FloorToInt(UnityEngine.Random.value * Units.Length)];

        [CanBeNull]
        public GameObject FindUnitUnderBoardPos(Vector2Int boardPos)
        {
            Debug.Assert(CheckBoardPosValid(boardPos));
            if (CheckBoardPosValidAndEmpty(boardPos))
            {
                return null;
            }

            return UnitsGameObjects.TryGetValue(boardPos, out var go) ? go : null;
        }

        [Obsolete]
        public Vector3 GetFloatTransform(Vector2Int boardPos)
        {
            return new Vector3(this._boardPhysicalOriginX + boardPos.x * this._boardPhysicalLength, 0,
                this._boardPhysicalOriginY + boardPos.y * this._boardPhysicalLength);
        }

        public Vector3 GetFloatTransformAnimation(Vector2 boardPos)
        {
            return new Vector3(this._boardPhysicalOriginX + boardPos.x * this._boardPhysicalLength, 0,
                this._boardPhysicalOriginY + boardPos.y * this._boardPhysicalLength);
        }

        public void UpdateUnitBoardPosAnimation(Vector2Int oldKey)
        {
            UnitsGameObjects.TryGetValue(oldKey, out var unit);//这里get出来和上面拿到的Unit不是一个？？
            UnitsGameObjects.Remove(oldKey);
            System.Diagnostics.Debug.Assert(unit != null, nameof(unit) + " != null");
            UnitsGameObjects.Add(unit.GetComponentInChildren<Unit>().NextBoardPosition, unit);
        }

        public void UpdateUnitBoardPosAnimation_Touch(Unit unit)
        {
            //这里get出来和上面拿到的Unit不是一个？？
            //RISK 用这个弄了一下，但是不知道为什么。
            UnitsGameObjects.Remove(unit.CurrentBoardPosition);
            System.Diagnostics.Debug.Assert(unit != null, nameof(unit) + " != null");
            UnitsGameObjects.Add(unit.GetComponentInChildren<Unit>().NextBoardPosition, unit.gameObject);
        }

        public void UpdateBoardInit()
        {
            foreach (var unit in Units)
            {
                unit.UpdateTransform(GetFloatTransform(unit.CurrentBoardPosition));
                unit.UpdateWorldRotationTransform();
            }
        }

        public void UpdateBoardRotate()
        {
            foreach (var unit in Units)
            {
                unit.UpdateWorldRotationTransform();
            }
        }

        public void UpdateBoardAnimation()
        {
            foreach (var unit in Units)
            {
                unit.UpdateTransform(GetFloatTransformAnimation(unit.LerpingBoardPosition));
                unit.UpdateWorldRotationTransform();
            }
        }

        public void UpdateBoardPostAnimation()
        {
            foreach (var unit in Units)
            {
                unit.UpdateNeighboringDataAndSideMesh();
            }
        }

        public GameObject InitUnit(Vector2Int board_pos,CoreType core,SideType[] sides,int Tier)
        {
            var go = Instantiate(UnitTemplate);
            go.name = "Unit_" + Hash128.Compute(board_pos.ToString());
            var unit = go.GetComponentInChildren<Unit>();
            unit.InitPosWithAnimation(board_pos);
            unit.InitUnit(core, sides, Tier);
            return go;
        }

        CoreType UnitCoreFromSignalAndGenre(SignalType signalType,CoreGenre coreGenre)
        {
            SignalMasterMgr.Instance.UnitTypeFromSignal(signalType, out var coreUnit, out var fieldUnit);
            if (coreGenre==CoreGenre.Source)
            {
                return coreUnit;
            }
            if (coreGenre==CoreGenre.Destination)
            {
                return fieldUnit;
            }

            return CoreType.PCB;
        }
        SignalType SignalTypeFromAdditionalGameSetup(AdditionalGameSetup additionalGameSetup,PlayingSignalSelector selector)
        {
            if (selector == PlayingSignalSelector.TypeA)
            {
                return additionalGameSetup.PlayingSignalTypeA;
            }

            if (selector == PlayingSignalSelector.TypeB)
            {
                return additionalGameSetup.PlayingSignalTypeB;
            }

            return SignalType.Matrix;
        }
        
        private void CreateUnitOnBoard(UnitGist unitGist,AdditionalGameSetup additionalGameSetup)
        {
            var unitGO = Instantiate(UnitTemplate);
            unitGO.name = "Unit_" + Hash128.Compute(unitGist.Pos.ToString());
            Unit unit = unitGO.GetComponentInChildren<Unit>();
            unit.InitPosWithAnimation(unitGist.Pos);
            UnitsGameObjects.Add(unitGist.Pos, unitGO);
            var signalType = SignalTypeFromAdditionalGameSetup(additionalGameSetup, unitGist.PlayingSignalSelector);
            var unitCore = UnitCoreFromSignalAndGenre(signalType,unitGist.CoreGenre);
            unit.InitUnit(unitCore, unitGist.Sides, unitGist.Tier, this);
            if (unitGist.IsStation)
            {
                unit.SetupStationUnit();
            }
        }

        public Unit GetUnitWithPosAndDir(Vector2Int center, RotationDirection offsetDirection)
        {
            var nextPos = center + Utils.ConvertDirectionToBoardPosOffset(offsetDirection);
            return CheckBoardPosValidAndFilled(nextPos) ? UnitsGameObjects[nextPos].GetComponentInChildren<Unit>() : null;
        }

        public void InitBoardWAsset(LevelActionAsset actionAsset)
        {
            foreach (var unitGist in actionAsset.InitalBoard)
            {
                CreateUnitOnBoard(unitGist, actionAsset.AdditionalGameSetup);
            }
        }

        public bool CheckBoardPosValid(Vector2Int mVector2Int)
        {
            return CheckBoardPosValidStatic(mVector2Int);
        }

        public static bool CheckBoardPosValidStatic(Vector2Int mVector2Int)
        {
            return (mVector2Int.x >= 0) && (mVector2Int.y >= 0) && (mVector2Int.x < BoardLength) && (mVector2Int.y < BoardLength);
        }

        public bool CheckBoardPosValidAndEmpty(Vector2Int mVector2Int)
        {
            return (!UnitsGameObjects.ContainsKey(mVector2Int))&&CheckBoardPosValid(mVector2Int);
        }

        public bool CheckBoardPosValidAndFilled(Vector2Int mVector2Int)
        {
            return (UnitsGameObjects.ContainsKey(mVector2Int)) && CheckBoardPosValid(mVector2Int);
        }

        void Awake()
        {
            UnitsGameObjects = new Dictionary<Vector2Int, GameObject>();
            InitBoardGird();
            CheckHeatSink(StageType.Shop);
            LFLocatorStatic = LFLocator;
            URLocatorStatic = URLocator;
        }

        public Vector2Int[] GetAllEmptySpace()
        {
            List<Vector2Int> res = new List<Vector2Int>();
            for (int i = 0; i < BoardLength; i++)
            {
                for (int j = 0; j < BoardLength; j++)
                {
                    if(CheckBoardPosValidAndEmpty(new Vector2Int(i, j)))
                    {
                        res.Add(new Vector2Int(i, j));
                    }
                }
            }
            return res.ToArray();
        }

        public bool DeliverUnitAssignedPlaceCrash(GameObject unit, Vector2Int AssignedPos)
        {
            if (!CheckBoardPosValidAndEmpty(AssignedPos))
            {
                TryDeleteCertainUnit(AssignedPos);
            }

            return DeliverUnitAssignedPlace(unit, AssignedPos);
        }

        public bool DeliverUnitAssignedPlace(GameObject unit, Vector2Int AssignedPos)
        {
            if (CheckBoardPosValidAndEmpty(AssignedPos))
            {
                unit.GetComponentInChildren<Unit>().InitPosWithAnimation(AssignedPos);
                unit.GetComponentInChildren<Unit>().GameBoard = this;
                UnitsGameObjects.Add(AssignedPos, unit);
                UpdateBoardInit();
                return true;
            }
            return false;
        }

        public bool TransferUnitAssignedPlace(Vector2Int From, Vector2Int To)
        {
            if (!CheckBoardPosValidAndFilled(From)) return false;
            UnitsGameObjects.TryGetValue(From, out GameObject go);
            UnitsGameObjects.Remove(From);
            UpdateBoard();
            return DeliverUnitAssignedPlace(go, To);
        }

        public bool DeliverUnitRandomPlace(GameObject unit)
        {
            return DeliverUnitRandomPlace(unit, out Vector2Int vector2Int);
        }

        public bool DeliverUnitRandomPlace(GameObject unit,out Vector2Int deliveringPos)
        {
            Vector2Int[] emptyPlace = GetAllEmptySpace();
            if (emptyPlace.Length==0)
            {
                deliveringPos = Vector2Int.zero;
                return false;
            }
            Vector2Int randomPlace = Utils.GenerateWeightedRandom(emptyPlace);
            unit.GetComponentInChildren<Unit>().InitPosWithAnimation(randomPlace);
            unit.GetComponentInChildren<Unit>().GameBoard = this;
            UnitsGameObjects.Add(randomPlace, unit);          
            UpdateBoardInit();
            deliveringPos = randomPlace;
            return true;
        }

        public void UpdateBoard()
        {
            foreach (var unit in UnitsGameObjects)
            {
                if (unit.Value == null) continue;
                var mUnit = unit.Value.GetComponentInChildren<Unit>();
                mUnit.UpdateNeighboringDataAndSideMesh();
            }
        }

        public bool SwapUnit(Vector2Int posA,Vector2Int posB)
        {
            Debug.Assert(CheckBoardPosValid(posA));
            Debug.Assert(CheckBoardPosValid(posB));
            var unitA = FindUnitUnderBoardPos(posA);
            var unitB = FindUnitUnderBoardPos(posB);
            if (unitA==null&&unitB==null)
            {
                return false;
            }
            else if(unitA != null && unitB != null)
            {
                UnitsGameObjects.TryGetValue(posA, out GameObject goA);
                UnitsGameObjects.TryGetValue(posB, out GameObject goB);
                UnitsGameObjects.Remove(posA);
                UnitsGameObjects.Remove(posB);
                UpdateBoard();
                var resA=DeliverUnitAssignedPlace(goA, posB);
                var resB=DeliverUnitAssignedPlace(goB, posA);
                UpdateBoard();
                return resA && resB;
            }
            else
            {
                var fromPos = unitA == null ? posB : posA;
                var toPos = unitA == null ? posA : posB;
                return TransferUnitAssignedPlace(fromPos, toPos);
            }
        }

        public bool TryDeleteCertainUnit(Vector2Int pos)
        {
            return TryDeleteCertainUnit(pos, out var destoryedCore);
        }

        public bool TryDeleteCertainUnit(Vector2Int pos, out CoreType? destoryedCore)
        {
            if (CheckBoardPosValidAndFilled(pos))
            {
                UnitsGameObjects.TryGetValue(pos, out GameObject go);
                destoryedCore = go.GetComponentInChildren<Unit>().UnitCore;
                Destroy(go);
                UnitsGameObjects.Remove(pos);
                //想办法需要在这儿调双个计分函数。

                UpdateBoard();
                return true;
            }
            destoryedCore = null;
            return false;
        }

        public bool TryDeleteCertainNoStationUnit(Vector2Int pos,out CoreType? destoryedCore)
        {
            if (CheckBoardPosValidAndFilled(pos))
            {
                UnitsGameObjects.TryGetValue(pos, out GameObject go);
                if (!go.GetComponentInChildren<Unit>().StationUnit)
                {
                    destoryedCore = go.GetComponentInChildren<Unit>().UnitCore;
                    Destroy(go);
                    UnitsGameObjects.Remove(pos);
                    UpdateBoard();
                    return true;
                }
            }
            destoryedCore = null;
            return false;
        }

        public int GetCountByType(CoreType coreType)
        {
            return FindUnitWithCoreType(coreType).Length;
        }

        public void ResetUnitEmission()
        {
            foreach (var unit in UnitsGameObjects)
            {
                unit.Value.GetComponentInChildren<Unit>().SetCoreEmissive(Color.black);
            }
        }
        
        [Obsolete]
        public void DisplayConnectedHDDUnit()
        {
            float time = Time.timeSinceLevelLoad;
            foreach (var unit in UnitsGameObjects)
            {
                if (unit.Value.GetComponentInChildren<Unit>().SignalCore.InMatrixSignal)
                {
                    Color color = (Mathf.Sin(time * 10) + 1.0f) * Color.red;
                    unit.Value.GetComponentInChildren<Unit>().SetCoreEmissive(color);
                }
            }
        }

        [Obsolete]
        public void DisplayConnectedServerUnit()
        {
            float time = Time.timeSinceLevelLoad;
            foreach (var unit in UnitsGameObjects)
            {
                Unit unitComp = unit.Value.GetComponentInChildren<Unit>();
                if (unitComp.UnitCore==CoreType.NetworkCable||unitComp.UnitCore==CoreType.Server)
                {                  
                    //现在网络只显示网线和服务器，不会有错，但是有可能有更好的解决方案？
                    if (unitComp.SignalCore.InServerGrid)
                    {
                        Color color = (Mathf.Sin(time * 10) + 1.0f) * Color.blue;
                        unit.Value.GetComponentInChildren<Unit>().SetCoreEmissive(color);
                    }
                }
            }
        }

        public float CalculateTotalIncomes()
        {
            return Units.Where(unit => unit.IsSource).Sum(unit => unit.SignalCore.CalScore());
        }
    }
}
