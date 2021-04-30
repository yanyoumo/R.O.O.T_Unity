using System;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Messages;
using JetBrains.Annotations;
using ROOT.Consts;
using ROOT.SetupAsset;
using ROOT.Signal;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using static ROOT.Utils;
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

    public partial class BoardGirdCell
    {
        public int HeatSinkID { get; internal set; }//Always Positive.
        public int HeatSinkCost { get; internal set; }//Always Positive.
    }

    public partial class Unit
    {
        //Board Only function(s)
        internal void UpdateNeighboringData()
        {
            WorldNeighboringData = new Dictionary<RotationDirection, ConnectionData>();
            if (GameBoard == null) return;
            foreach (var currentSideDirection in Common.Utils.ROTATION_LIST)
            {
                var connectionData = new ConnectionData();

                if (GetWorldSpaceUnitSide(currentSideDirection) == SideType.Connection)
                {
                    connectionData.HasConnector = true;
                    var otherUnitPos = GetNeigbourCoord(currentSideDirection);
                    var otherUnit = GameBoard.FindUnitByPos(otherUnitPos);//这个还是要处理、但是千万要注意；这里的代码太核心了。
                    if (otherUnit != null)
                    {
                        connectionData.OtherUnit = otherUnit;
                        connectionData.Connected =
                            (otherUnit.GetWorldSpaceUnitSide(Common.Utils.GetInvertDirection(currentSideDirection)) ==
                             SideType.Connection);
                        if (connectionData.Connected)
                        {
                            connectionData.ConnectedToGenre = otherUnit.UnitHardware;
                        }
                    }
                }

                WorldNeighboringData.Add(currentSideDirection, connectionData);
            }
        }

        internal void UpdateSideMesh()
        {
            if (WorldNeighboringData == null) return;
            Common.Utils.ROTATION_LIST.ForEach(ResetConnector);
            ConnectorLocalDir.Values.ForEach(val => val.Connected = false);
            var ignoreVal = WorldCycler.TelemetryStage && !WorldCycler.TelemetryPause;
            Common.Utils.ROTATION_LIST.Where(FilterConnector).ForEach(dir => SetConnector(dir, ignoreVal));
        }

        internal void UpdateActivationLED() => UnitActivationLEDMat.material.color = UnitActivationLEDMat_Colors[(int) SignalCore.GetLEDLightingStatus];
    }
    
    public sealed class Board : MonoBehaviour
    {
        public static SignalType PlayingSignalA;
        public static SignalType PlayingSignalB;

        public BoardGirdDriver BoardGirdDriver;
        public HeatSinkPatternLib HeatSinkPatterns;

        public InfoAirdrop AirDrop;
        public Transform LFLocator;
        public Transform URLocator;

        public static Transform LFLocatorStatic;
        public static Transform URLocatorStatic;

        public static int BoardLength => StaticNumericData.BoardLength;
        public readonly float _boardPhysicalLength = 1.2f;
        private readonly float _boardPhysicalOriginX = -3.1f - 1.75f - 2.0f;
        private readonly float _boardPhysicalOriginY = -3.1f;

        public GameObject UnitTemplate;
        public GameObject BoardGridTemplate;
        public Transform BoardGridRoot;
        public Transform BoardGridZeroing;

        private Dictionary<Vector2Int, GameObject> _unitsGameObjects;
        
        public int GetUnitCount => _unitsGameObjects.Count;
        public int GetCountByType(SignalType signal, HardwareType genre) => FindUnitWithCoreType(signal, genre).Length;
        public int GetTotalTierCountByType(SignalType signal, HardwareType genre) => Units.Where(unit => unit.UnitSignal == signal && unit.UnitHardware == genre).Sum(unit => unit.Tier);
        public List<Vector2Int> GetInfoCollectorZone()
        {
            //这里保证前面调过一次计分函数，实在不行在这儿再调一遍。
            var res = new List<Vector2Int>();
            Units.Select(u => u.SignalCore).Where(s => s.IsUnitActive).ForEach(s => res.AddRange(s.SingleInfoCollectorZone));
            return res.Where(CheckBoardPosValid).Distinct().ToList();
        }

        public Unit[] Units => _unitsGameObjects.Values.Select(u => u.GetComponentInChildren<Unit>()).ToArray();
        public Unit FindNearestUnit(Vector2Int Pos)
        {
            var distance = float.MaxValue;
            var nearestPos = Vector2Int.zero;
            foreach (var vector2Int in _unitsGameObjects.Keys)
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

            return _unitsGameObjects[nearestPos].GetComponentInChildren<Unit>();
        }
        public Unit[] FindUnitsByUnitTag(UnitTag tag) => Units.Where(u => u.UnitTag == tag).ToArray();
        public Unit FindNearestSignalAP(Vector2Int Pos)
        {
            var distance = float.MaxValue;
            var nearestPos = Vector2Int.zero;
            foreach (var vector2Int in _unitsGameObjects.Keys)
            {
                var unit = _unitsGameObjects[vector2Int].GetComponentInChildren<Unit>();

                if ((unit.UnitSignal == SignalType.Matrix && unit.UnitHardware == HardwareType.Field) ||
                    ((unit.UnitSignal == SignalType.Scan && unit.UnitHardware == HardwareType.Field) &&
                     unit.SignalCore.InServerGrid && unit.SignalCore.ScanSignalPathDepth == 1))
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

            return _unitsGameObjects[nearestPos].GetComponentInChildren<Unit>();
        }
        [CanBeNull] public Unit FindRandomUnit => GetUnitCount == 0 ? null : Units[Mathf.FloorToInt(UnityEngine.Random.value * Units.Length)];
        [CanBeNull] public Unit FindUnitByPos(Vector2Int boardPos)
        {
            if (!CheckBoardPosValid(boardPos))
            {
                Debug.LogError("Requesting a not valid board position!!");
                return null;
            }
            if (CheckBoardPosValidAndEmpty(boardPos))
            {
                return null;
            }

            return _unitsGameObjects.TryGetValue(boardPos, out var go) ? go.GetComponentInChildren<Unit>() : null;
        }
        public Unit[] FindUnitWithCoreType(SignalType signal, HardwareType genre) => Units.Where(u => u.UnitSignal == signal && u.UnitHardware == genre).ToArray();

        public void SomeGridHasCollectedInfo(BoardGirdCell girdCell)
        {
            //逻辑到这里居然是好用的，只是需要去调整Extend的内容。
            //从逻辑上讲，只要把接收的分数加上加上就好了。剩下都是表现侧的。
            //girdCell.Blink();//TODO 这里要处理接收了Info的内容。
            var girdPos = girdCell.OnboardPos;
            var collectingUnit = FindNearestSignalAP(girdPos);
            collectingUnit.Blink(); //接收了就先闪亮一下
        }
        
        public bool CheckBoardPosValid(Vector2Int mVector2Int) => CheckBoardPosValidStatic(mVector2Int);
        public static bool CheckBoardPosValidStatic(Vector2Int mVector2Int) => (mVector2Int.x >= 0) && (mVector2Int.y >= 0) && (mVector2Int.x < BoardLength) && (mVector2Int.y < BoardLength);
        public bool CheckBoardPosValidAndEmpty(Vector2Int mVector2Int) => (!_unitsGameObjects.ContainsKey(mVector2Int)) && CheckBoardPosValid(mVector2Int);
        public bool CheckBoardPosValidAndFilled(Vector2Int mVector2Int) => (_unitsGameObjects.ContainsKey(mVector2Int)) && CheckBoardPosValid(mVector2Int);

        public bool CheckAllActive()
        {
            return Units.All(unit => unit.SignalCore.IsUnitActive);
        }
        public int GetUnitsConnectedIsland()
        {
            var res = 0;
            var vis = new Dictionary<Unit, bool>();
            Units.ForEach(unit => vis[unit] = false);
            foreach (var unit in Units)
            {
                if (vis[unit]) continue;
                ++res;
                var queue = new Queue<Unit>();
                vis[unit] = true;
                queue.Enqueue(unit);
                while (queue.Count > 0)
                {
                    var now = queue.Dequeue();
                    foreach (var otherUnit in now.GetConnectedOtherUnit.Where(otherUnit => vis[otherUnit] == false))
                    {
                        vis[otherUnit] = true;
                        queue.Enqueue(otherUnit);
                    }
                }
            }

            return res;
        }
        public Vector2Int[] GetAllEmptySpace()
        {
            List<Vector2Int> res = new List<Vector2Int>();
            for (int i = 0; i < BoardLength; i++)
            {
                for (int j = 0; j < BoardLength; j++)
                {
                    if (CheckBoardPosValidAndEmpty(new Vector2Int(i, j)))
                    {
                        res.Add(new Vector2Int(i, j));
                    }
                }
            }
            return res.ToArray();
        }
        public Vector2Int FindRandomEmptyPlace()
        {
            var res = Vector2Int.zero;
            do
            {
                res = new Vector2Int(StaticNumericData.RandomBoardRowIndex, StaticNumericData.RandomBoardRowIndex);
            } while (_unitsGameObjects.ContainsKey(res));
            return res;
        }

        public Vector3 GetFloatTransform(Vector2Int boardPos)
        {
            return new Vector3(_boardPhysicalOriginX + boardPos.x * this._boardPhysicalLength, 0,
                this._boardPhysicalOriginY + boardPos.y * this._boardPhysicalLength);
        }
        public Vector3 GetFloatTransformAnimation(Vector2 boardPos)
        {
            return new Vector3(_boardPhysicalOriginX + boardPos.x * _boardPhysicalLength, 0, _boardPhysicalOriginY + boardPos.y * _boardPhysicalLength);
        }

        public void UpdateBoardUnit()
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
            /*foreach (var unit in Units)
            {
                unit.UpdateNeighboringDataAndSideMesh();
            }*/
        }
        public void UpdateUnitBoardPosAnimation(Vector2Int oldKey)
        {
            _unitsGameObjects.TryGetValue(oldKey, out var unit);//这里get出来和上面拿到的Unit不是一个？？
            _unitsGameObjects.Remove(oldKey);
            System.Diagnostics.Debug.Assert(unit != null, nameof(unit) + " != null");
            _unitsGameObjects.Add(unit.GetComponentInChildren<Unit>().NextBoardPosition, unit);
        }
        public void UpdateUnitBoardPosAnimation_Touch(Unit unit)
        {
            //这里get出来和上面拿到的Unit不是一个？？
            //RISK 用这个弄了一下，但是不知道为什么。
            _unitsGameObjects.Remove(unit.CurrentBoardPosition);
            System.Diagnostics.Debug.Assert(unit != null, nameof(unit) + " != null");
            _unitsGameObjects.Add(unit.GetComponentInChildren<Unit>().NextBoardPosition, unit.gameObject);
        }
        
        SignalType SignalTypeFromAdditionalGameSetup(AdditionalGameSetup additionalGameSetup, PlayingSignalSelector selector)
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

        private void CreateUnitByGist(UnitGist unitGist, AdditionalGameSetup additionalGameSetup)
        {
            var signalType = SignalTypeFromAdditionalGameSetup(additionalGameSetup, unitGist.PlayingSignalSelector);
            CreateUnit(unitGist.Pos, signalType, unitGist.CoreGenre, unitGist.Sides, unitGist.Tier, unitGist.IsStation);
        }
        public void CreateUnit(Vector2Int board_pos, SignalType signal, HardwareType genre, SideType[] sides, int Tier, bool IsStationary = false, UnitTag unitTag = UnitTag.NoTag)
        {
            var go = Instantiate(UnitTemplate);
            go.name = "Unit_" + Hash128.Compute(board_pos.ToString());
            var unit = go.GetComponentInChildren<Unit>();
            unit.InitPosWithAnimation(board_pos);
            _unitsGameObjects.Add(board_pos, go);
            unit.InitUnit(signal, genre, sides, Tier, this, unitTag);
            if (IsStationary)
            {
                unit.SetupStationUnit();
            }
        }

        public bool DeliverUnitRandomPlace(GameObject unit) => DeliverUnitRandomPlace(unit, out Vector2Int vector2Int);
        private bool DeliverUnitRandomPlace(GameObject unit, out Vector2Int deliveringPos)
        {
            Vector2Int[] emptyPlace = GetAllEmptySpace();
            if (emptyPlace.Length == 0)
            {
                deliveringPos = -Vector2Int.one;
                return false;
            }
            deliveringPos = GenerateWeightedRandom(emptyPlace);
            return DeliverUnitAssignedPlace(unit, deliveringPos);
        }
        public bool DeliverUnitAssignedPlaceCrash(GameObject unit, Vector2Int AssignedPos)
        {
            if (!CheckBoardPosValidAndEmpty(AssignedPos))
            {
                TryDeleteCertainUnit(AssignedPos);
            }
            return DeliverUnitAssignedPlace(unit, AssignedPos);
        }
        private bool DeliverUnitAssignedPlace(GameObject unit, Vector2Int AssignedPos)
        {
            if (CheckBoardPosValidAndEmpty(AssignedPos))
            {
                unit.GetComponentInChildren<Unit>().InitPosWithAnimation(AssignedPos);
                unit.GetComponentInChildren<Unit>().GameBoard = this;
                _unitsGameObjects.Add(AssignedPos, unit);
                UpdateBoardUnit();
                return true;
            }
            return false;
        }

        public bool SwapUnit(Vector2Int posA, Vector2Int posB)
        {
            Debug.Assert(CheckBoardPosValid(posA));
            Debug.Assert(CheckBoardPosValid(posB));
            var unitA = FindUnitByPos(posA);
            var unitB = FindUnitByPos(posB);
            if (unitA == null && unitB == null)
            {
                return false;
            }
            else if (unitA != null && unitB != null)
            {
                _unitsGameObjects.TryGetValue(posA, out GameObject goA);
                _unitsGameObjects.TryGetValue(posB, out GameObject goB);
                _unitsGameObjects.Remove(posA);
                _unitsGameObjects.Remove(posB);
                var resA = DeliverUnitAssignedPlace(goA, posB);
                var resB = DeliverUnitAssignedPlace(goB, posA);
                return resA && resB;
            }
            else
            {
                var fromPos = unitA == null ? posB : posA;
                var toPos = unitA == null ? posA : posB;
                return TransferUnitAssignedPlace(fromPos, toPos);
            }
        }
        public bool TransferUnitAssignedPlace(Vector2Int From, Vector2Int To)
        {
            if (!CheckBoardPosValidAndFilled(From)) return false;
            _unitsGameObjects.TryGetValue(From, out GameObject go);
            _unitsGameObjects.Remove(From);
            //UpdateBoard();
            return DeliverUnitAssignedPlace(go, To);
        }

        public bool TryDeleteCertainUnit(Vector2Int pos)
        {
            return TryDeleteCertainUnit(pos, out var destoryedCore);
        }
        public bool TryDeleteCertainUnit(Vector2Int pos, out SignalType? destoryedCore)
        {
            return TryDeleteCertainUnit(pos, true, out destoryedCore);
        }
        public bool TryDeleteCertainUnit(Vector2Int pos, bool bypassStation, out SignalType? destoryedCore)
        {
            if (CheckBoardPosValidAndFilled(pos))
            {
                var unit = FindUnitByPos(pos);
                if (unit != null && (bypassStation || !unit.StationUnit))
                {
                    destoryedCore = unit.UnitSignal;
                    Destroy(unit.transform.parent.gameObject);
                    _unitsGameObjects.Remove(pos);
                    return true;
                }
            }
            destoryedCore = null;
            return false;
        }
        public void TryDeleteAllUnits()
        {
            var oldKeys = _unitsGameObjects.Keys.ToArray();//必须得存出来、因为删除那里会动Key，所以不能轮换删。
            oldKeys.ForEach(k => TryDeleteCertainUnit(k));
        }
        public void TryDeleteUnitByTags(UnitTag targetTag)
        {
            var targetingUnit = FindUnitsByUnitTag(targetTag);
            if (targetingUnit.Length==0) return;
            var oldKeys = targetingUnit.Select(u => u.CurrentBoardPosition);
            oldKeys.ForEach(k => TryDeleteCertainUnit(k));
        }

        public bool IsDataReady { get; private set; } = false;
        
        private int lastUnitsHashCode = 0;
        private int UnitsHashCode => Units.Select(u => u.GetHashCode()).Aggregate(0, (current, result) => current ^ result);
        private int GridsHashCode => BoardGirdDriver.BoardGirds.Aggregate(0, (current, val) => current ^ (val.Key.GetHashCode() ^ val.Value.CellStatus.GetHashCode()));
        
        private void FullyUpdateBoardData(IMessage rMessage)
        {
            IsDataReady = false;
            //现在要假设所有场景内容全是错的，准备更新。
            Units.ForEach(u => u.UpdateNeighboringData());
            //至此所有Unit边界数据设置完成。
            SignalMasterMgr.Instance.RefreshBoardAllSignalStrength(this);
            //至此所有信号路径设置完成。
            Units.ForEach(u => u.UpdateSideMesh());
            Units.ForEach(u => u.UpdateActivationLED());
            //至此所有单元提示灯具设置完成。
            MessageDispatcher.SendMessage(WorldEvent.BoardUpdatedEvent);
            IsDataReady = true;
        }

        public IEnumerable<Unit> FindEndingUnit(SignalType signalType)
        {
            return Units.Where(u => u.CheckNotBeingSignallyReferenced(signalType));
        }

        public void InitBoardWAsset(LevelActionAsset actionAsset)
        {
            PlayingSignalA = actionAsset.AdditionalGameSetup.PlayingSignalTypeA;
            PlayingSignalB = actionAsset.AdditionalGameSetup.PlayingSignalTypeB;
            foreach (var unitGist in actionAsset.InitalBoard)
            {
                CreateUnitByGist(unitGist, actionAsset.AdditionalGameSetup);
            }
        }

        void Awake()
        {
            _unitsGameObjects = new Dictionary<Vector2Int, GameObject>();
            BoardGirdDriver = new BoardGirdDriver { owner = this };
            BoardGirdDriver.InitBoardGird();
            BoardGirdDriver.UpkeepHeatSink(StageType.Shop);
            LFLocatorStatic = LFLocator;
            URLocatorStatic = URLocator;
            MessageDispatcher.AddListener(WorldEvent.BoardShouldUpdateEvent, FullyUpdateBoardData);
            //BoardShouldUpdateEvent += FullyUpdateBoardData;
        }
        
        private void Update()
        {
            //这里的实现现在有点儿“鲁”，但是这里只有一个目的，就是需要让让派生数据随着锚点数据更新而更新。
            //这里面需要搞一个BoardUnitsHash的那个东西、如果改了那么就触发RefreshSignalStrength
            var hashCode = UnitsHashCode ^ GridsHashCode;
            if (lastUnitsHashCode != hashCode)
            {
                lastUnitsHashCode = hashCode;
                RootDebug.Log("RefreshBoardAllSignalStrength:" + lastUnitsHashCode, NameID.YanYoumo_Log);
                FullyUpdateBoardData(new com.ootii.Messages.Message());
            }
        }
        
        private void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.BoardShouldUpdateEvent, FullyUpdateBoardData);
        }

        #region Static Func

        public static Vector2Int? WorldPosToXZGrid(Vector3 worldPos)
        {
            return WorldPosToXZGrid(new Vector2(worldPos.x, worldPos.z));
        }
        public static Vector2Int? WorldPosToXZGrid(Vector2 worldPosXZ)
        {
            var xN = SignalChannelSplit(LFLocatorStatic.transform.position.x, URLocatorStatic.transform.position.x, BoardLength, worldPosXZ.x);
            var yN = SignalChannelSplit(LFLocatorStatic.transform.position.z, URLocatorStatic.transform.position.z, BoardLength, worldPosXZ.y);
            var res = new Vector2Int(xN, yN);
            return CheckBoardPosValidStatic(res) ? (Vector2Int?)res : null;
        }
        public static Vector2Int ClampPosInBoard(Vector2Int pos)
        {
            var newPos = pos;
            newPos.x = Mathf.Clamp(newPos.x, 0, BoardLength - 1);
            newPos.y = Mathf.Clamp(newPos.y, 0, BoardLength - 1);
            return newPos;
        }

        #endregion
    }
}
