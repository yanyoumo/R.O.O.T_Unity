using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using JetBrains.Annotations;
using ROOT;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace ROOT
{
    using Direction= RotationDirection;

    public sealed class Board : MonoBehaviour
    {
        #region 热力系统

        public HeatSinkPatternLib HeatSinkPatterns;
        private int _currentHeatSinkPatternsID = 0;

        public int MinHeatSinkCount=> FixedHeatSinkPos.Length;
        private Vector2Int[] FixedHeatSinkPos => HeatSinkPatterns.Lib[_currentHeatSinkPatternsID].Lib.ToArray();

        public void UpdatePatternID()
        {
            var oldID = _currentHeatSinkPatternsID;
            do
            {
                _currentHeatSinkPatternsID = Random.Range(0, HeatSinkPatterns.Count - 1);
            } while (_currentHeatSinkPatternsID == oldID);
        }

        private void InitHeatInfo()
        {
            BoardGirds=new Dictionary<Vector2Int, BoardGirdCell>();
            for (var i = 0; i < BoardLength; i++)
            {
                for (var j = 0; j < BoardLength; j++)
                {
                    var go = Instantiate(BoardGridTemplate, BoardGridRoot);
                    float GridLength = _boardPhysicalLength;
                    Vector3 offset = new Vector3(i * GridLength, 0.0f, j * GridLength);
                    go.transform.localPosition = BoardGridZeroing.position + offset;
                    Vector2Int key = new Vector2Int(i, j);
                    BoardGirds.Add(key, go.GetComponent<BoardGirdCell>());
                }
            }
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
            for (int i = 0; i < BoardLength; i++)
            {
                for (int j = 0; j < BoardLength; j++)
                {
                    Vector2Int key = new Vector2Int(i, j);
                    if (CheckBoardPosValidAndEmpty(key))
                    {
                        if (!existingHeatSink.Contains(key))
                        {
                            return key;
                        }
                    }
                }
            }

            return null;
        }

        [CanBeNull]
        public Unit[] OverlapHeatSinkUnit => CheckHeatSink() != 0 ? Units.Where(unit => FixedHeatSinkPos.Contains(unit.CurrentBoardPosition)).ToArray() : null;

        /// <summary>
        /// 这里是检查时候又fix的HeatSink被占用。
        /// </summary>
        /// <returns>返回有多少个HeatSink格没有被满足，返回0即均满足。</returns>
        public int CheckHeatSink()
        {
            BoardGirds.Values.ForEach(grid => grid.NormalOrHeatSink = false);
            BoardGirds.ForEach(val => val.Value.NormalOrHeatSink = FixedHeatSinkPos.Contains(val.Key));
            return FixedHeatSinkPos.Count(pos => CheckBoardPosValidAndEmpty(pos) == false);
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
            BoardGirds.Values.ForEach(grid => grid.NormalOrHeatSink = false);

            for (var i = 0; i < heatSinkPos.Length; i++)
            {
                var val = FindFurthestHeatSink(in heatSinkPos);
                if (val.HasValue)
                {
                    heatSinkPos[i] = val.Value;
                    BoardGirds[val.Value].NormalOrHeatSink = true;
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

        #endregion

        public readonly int BoardLength = 6;
        public int TotalBoardCount => BoardLength * BoardLength;
        private readonly float _boardPhysicalLength = 1.2f;
        private readonly float _boardPhysicalOriginX = -3.1f - 1.75f-2.0f;
        private readonly float _boardPhysicalOriginY = -3.1f;

        public GameObject UnitTemplate;
        public GameObject BoardGridTemplate;
        public Transform BoardGridRoot;
        public Transform BoardGridZeroing;

        public Dictionary<Vector2Int, GameObject> UnitsGameObjects { get; private set; }

        public int GetUnitCount => UnitsGameObjects.Count;
        public int GetNonPCBUnitCount => Units.Count(unit => unit.UnitCore != CoreType.PCB);

        public Unit[] Units => UnitsGameObjects.Values.Select(unitsValue => unitsValue.GetComponentInChildren<Unit>()).ToArray();
        
        public int GetBoardID(Vector2Int pos)
        {
            return pos.y * BoardLength + pos.x;
        }

        #region ConnectionID部分

        /// <summary>
        /// 这段是一部分试图重构的数据结构，其目的是将棋盘上全部可能的链接编号并管理。
        /// 但是在再次讨论后，发现毫无意义，于是就此弃之。
        ///
        /// 核心思路是从棋盘左下角开始将每个格点编号，并且将所有可能的格点之间的链接进行编号。
        /// 编号逻辑是：
        /// 单元南侧的链接编号是单元ID的二倍。（2n）
        /// 单元东侧的链接编号是单元ID的二倍加一。(2n+1)
        ///
        /// 所有单元不去管理自己北侧和西侧的接口。（防止重复计算）
        /// 这样的问题就是棋盘上Connection的链接是不连续的。
        /// </summary>

        public int ConnectionCount => 2 * BoardLength * (BoardLength - 1);
        public Dictionary<int,bool> ConnectionShowing;//之所以要用Dic是因为Key不是连续的。
        public int[] GetAllConnectionID(int boardID)
        {
            //这里不做ConnectionID合法性的判断，需要自行判断。
            int A = GetConnectionID(boardID, Direction.North);
            int B = GetConnectionID(boardID, Direction.West);
            int C = GetConnectionID(boardID, Direction.South);
            int D = GetConnectionID(boardID, Direction.East);
            return new[] {A, B, C, D};
        }
        public int GetConnectionID(Vector2Int pos, Direction desiredWorldDirection)
        {
            return GetConnectionID(GetBoardID(pos), desiredWorldDirection);
        }
        public int GetConnectionID(int boardID, Direction desiredWorldDirection)
        {
            switch (desiredWorldDirection)
            {
                case RotationDirection.North:
                    return 2 * (boardID + BoardLength);
                case RotationDirection.East:
                    return 2 * (boardID - 1) + 1;
                case RotationDirection.West:
                    return 2 * boardID + 1;
                case RotationDirection.South:
                    return 2 * boardID;
                default:
                    throw new ArgumentOutOfRangeException(nameof(desiredWorldDirection), desiredWorldDirection, null);
            }
        }
        /// <summary>
        /// 验证一个单元位置上链接ID是否合法
        /// </summary>
        /// <param name="boardID">单元ID</param>
        /// <param name="connectionID">欲验证的ConnectionID</param>
        /// <returns>此connection在本棋盘下是否合法</returns>
        public bool CheckConnectionIDVaild(int boardID, int connectionID)
        {
            //这个函数有问题，如果之后要用的话，需要重写。
            if (connectionID<0) return false;//0位置的左侧算出来就是负数（-2）。

            if (connectionID>=2*BoardLength*BoardLength)return false;//除掉顶层的。

            if (connectionID%2==0)
            {
                if ((connectionID / 2)<= BoardLength)
                {
                    return false;//除掉底层的。
                }
            }

            var (IDA, IDB) = GetBoardIDFromConnectionID(connectionID);
            return (IDA / BoardLength == IDB / BoardLength);//除掉两侧的。
        }
        public Tuple<int, int> GetBoardIDFromConnectionID(int connectionID)
        {
            //保证这个函数获得的ConnectionID是对的，需要让Unit去判断Connection是假的。
            if (connectionID%2==0)
            {
                //EVEN
                return new Tuple<int, int>(connectionID / 2, connectionID / 2 - BoardLength);
            }
            else
            {
                //ODD
                int val = (connectionID - 1) / 2;
                return new Tuple<int, int>(val, val + 1);
            }
        }

        #endregion

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

        public void CreateUnitOnBoard(UnitGist unitGist)
        {
            var unitGO = Instantiate(UnitTemplate);
            unitGO.name = "Unit_" + Hash128.Compute(unitGist.Pos.ToString());
            Unit unit = unitGO.GetComponentInChildren<Unit>();
            unit.InitPosWithAnimation(unitGist.Pos);
            UnitsGameObjects.Add(unitGist.Pos, unitGO);
            unit.InitUnit(unitGist.Core, unitGist.Sides, unitGist.Tier, this);
            if (unitGist.IsStation)
            {
                unit.SetupStationUnit();
            }
        }

        public void InitBoardWAsset(LevelActionAsset actionAsset)
        {
            foreach (var unitGist in actionAsset.InitalBoard)
            {
                CreateUnitOnBoard(unitGist);
            }
        }

        public bool CheckBoardPosValid(Vector2Int mVector2Int)
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
            InitHeatInfo();
            CheckHeatSink();
            //ScanHeatSink();
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
                if (unit.Value.GetComponentInChildren<Unit>().InHddGrid)
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
                    if (unitComp.InServerGrid)
                    {
                        Color color = (Mathf.Sin(time * 10) + 1.0f) * Color.blue;
                        unit.Value.GetComponentInChildren<Unit>().SetCoreEmissive(color);
                    }
                }
            }
        }
    }
}
