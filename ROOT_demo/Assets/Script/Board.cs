using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ROOT;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace ROOT
{
    public sealed class Board : MonoBehaviour
    {
        public readonly int BoardLength = 6;
        private readonly float _boardPhysicalLength = 1.2f;
        private readonly float _boardPhysicalOriginX = -3.1f - 1.75f-2.0f;
        private readonly float _boardPhysicalOriginY = -3.1f;

        public GameObject UnitTemplate; //TODO 应该要改成AssetLoad那种

        public Dictionary<Vector2Int, GameObject> Units { get; private set; }

        [CanBeNull]
        public GameObject FindUnitUnderBoardPos(Vector2Int boardPos)
        {
            Debug.Assert(CheckBoardPosValid(boardPos));
            if (CheckBoardPosValidAndEmpty(boardPos))
            {
                return null;
            }

            return Units.TryGetValue(boardPos, out var go) ? go : null;
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

        [Obsolete]
        public void UpdateUnitBoardPos(Vector2Int oldKey)
        {
            Units.TryGetValue(oldKey,out var unit);
            Units.Remove(oldKey);
            System.Diagnostics.Debug.Assert(unit != null, nameof(unit) + " != null");
            Units.Add(unit.GetComponentInChildren<Unit>().CurrentBoardPosition,unit);
        }

        public void UpdateUnitBoardPosAnimation(Vector2Int oldKey)
        {
            Units.TryGetValue(oldKey, out var unit);//这里get出来和上面拿到的Unit不是一个？？
            Units.Remove(oldKey);
            System.Diagnostics.Debug.Assert(unit != null, nameof(unit) + " != null");
            Units.Add(unit.GetComponentInChildren<Unit>().NextBoardPosition, unit);
        }

        public void UpdateUnitBoardPosAnimation_Touch(Unit unit)
        {
            //这里get出来和上面拿到的Unit不是一个？？
            //TODO 用这个弄了一下，但是不知道为什么。
            Units.Remove(unit.CurrentBoardPosition);
            System.Diagnostics.Debug.Assert(unit != null, nameof(unit) + " != null");
            Units.Add(unit.GetComponentInChildren<Unit>().NextBoardPosition, unit.gameObject);
        }

        public void UpdateBoardInit()
        {
            foreach (var unit in Units)
            {
                if (unit.Value == null) continue;
                var mUnit = unit.Value.GetComponentInChildren<Unit>();
                mUnit.UpdateTransform(GetFloatTransform(mUnit.CurrentBoardPosition));
                mUnit.UpdateWorldRotationTransform();
            }
        }

        public void UpdateBoardRotate()
        {
            foreach (var unit in Units)
            {
                if (unit.Value == null) continue;
                var mUnit = unit.Value.GetComponentInChildren<Unit>();
                mUnit.UpdateWorldRotationTransform();
            }
        }

        public void UpdateBoardAnimation()
        {
            foreach (var unit in Units)
            {
                if (unit.Value == null) continue;
                var mUnit = unit.Value.GetComponentInChildren<Unit>();
                mUnit.UpdateTransform(GetFloatTransformAnimation(mUnit.LerpingBoardPosition));
                mUnit.UpdateWorldRotationTransform();
            }
        }

        public void UpdateBoardPostAnimation()
        {
            foreach (var unit in Units)
            {
                if (unit.Value == null) continue;
                var mUnit = unit.Value.GetComponentInChildren<Unit>();
                mUnit.UpdateNeighboringDataAndSideMesh();
            }
        }

        public GameObject InitUnit(Vector2Int board_pos,CoreType core,SideType[] sides)
        {
            var go = Instantiate(UnitTemplate);
            go.name = "Unit_" + Hash128.Compute(board_pos.ToString());
            var unit = go.GetComponentInChildren<Unit>();
            unit.InitPosWithAnimation(board_pos);
            unit.InitUnit(core, sides);
            return go;
        }

        public void CreateUnitOnBoard(UnitGist unitGist)
        {
            var unitGO = Instantiate(UnitTemplate);
            unitGO.name = "Unit_" + Hash128.Compute(unitGist.Pos.ToString());
            Unit unit = unitGO.GetComponentInChildren<Unit>();
            unit.InitPosWithAnimation(unitGist.Pos);
            Units.Add(unitGist.Pos, unitGO);
            unit.InitUnit(unitGist.Core, unitGist.Sides, this);
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

        [Obsolete]
        public void InitBoardRealStart()
        {
            Vector2Int[] startingArray =
                {
                    new Vector2Int(0, 1),
                    //new Vector2Int(0, 2),
                    //new Vector2Int(0, 3),
                    new Vector2Int(0, 4),

                    new Vector2Int(5, 1),
                    //new Vector2Int(5, 2),
                    //new Vector2Int(5, 3),
                    new Vector2Int(5, 4),

                    new Vector2Int(1, 0),
                    //new Vector2Int(2, 0),
                    //new Vector2Int(3, 0),
                    new Vector2Int(4, 0),

                    new Vector2Int(1, 5),
                    //new Vector2Int(2, 5),
                    //new Vector2Int(3, 5),
                    new Vector2Int(4, 5),
                };
            foreach (var vector2Int in startingArray)
            {              
                var go = Instantiate(UnitTemplate);
                if (go == null)
                {
                    Debug.Assert(true, "Empty Template");
                    continue;
                };
                go.name = "Unit_" + Hash128.Compute(vector2Int.ToString());
                go.GetComponentInChildren<Unit>().InitPosWithAnimation(vector2Int);
                Units.Add(vector2Int, go);
                go.GetComponentInChildren<Unit>().InitUnit(CoreType.PCB,
                    new[] { SideType.NoConnection, SideType.NoConnection, SideType.NoConnection, SideType.NoConnection },this);
            }

            Vector2Int vector2IntA=new Vector2Int(0,0);
            var goA = Instantiate(UnitTemplate);
            goA.name = "Unit_" + Hash128.Compute(vector2IntA.ToString());
            goA.GetComponentInChildren<Unit>().InitPosWithAnimation(vector2IntA);
            Units.Add(vector2IntA, goA);
            goA.GetComponentInChildren<Unit>().InitUnit(CoreType.Processor, new[] { SideType.Connection, SideType.Connection, SideType.Connection, SideType.Connection }, this);

            Vector2Int vector2IntB = new Vector2Int(5, 5);
            var goB = Instantiate(UnitTemplate);
            goB.name = "Unit_" + Hash128.Compute(vector2IntB.ToString());
            goB.GetComponentInChildren<Unit>().InitPosWithAnimation(vector2IntB);
            Units.Add(vector2IntB, goB);
            goB.GetComponentInChildren<Unit>().InitUnit(CoreType.Processor,
                new[] { SideType.Connection, SideType.Connection, SideType.Connection, SideType.Connection }, this);

            Vector2Int vector2IntC = new Vector2Int(0, 5);
            var goC = Instantiate(UnitTemplate);
            goC.name = "Unit_" + Hash128.Compute(vector2IntC.ToString());
            goC.GetComponentInChildren<Unit>().InitPosWithAnimation(vector2IntC);
            Units.Add(vector2IntC, goC);
            goC.GetComponentInChildren<Unit>().InitUnit(CoreType.Server,
                new[] { SideType.Connection, SideType.Connection, SideType.Connection, SideType.Connection }, this);

            Vector2Int vector2IntD = new Vector2Int(5, 0);
            var goD = Instantiate(UnitTemplate);
            goD.name = "Unit_" + Hash128.Compute(vector2IntD.ToString());
            goD.GetComponentInChildren<Unit>().InitPosWithAnimation(vector2IntD);
            //goD.GetComponentInChildren<Unit>().SetupStationUnit();
            Units.Add(vector2IntD, goD);
            goD.GetComponentInChildren<Unit>().InitUnit(CoreType.Server,
                new[] { SideType.Connection, SideType.Connection, SideType.Connection, SideType.Connection }, this);
        }

        public bool CheckBoardPosValid(Vector2Int mVector2Int)
        {
            return (mVector2Int.x >= 0) && (mVector2Int.y >= 0) && (mVector2Int.x < BoardLength) && (mVector2Int.y < BoardLength);
        }

        public bool CheckBoardPosValidAndEmpty(Vector2Int mVector2Int)
        {
            return (!Units.ContainsKey(mVector2Int))&&CheckBoardPosValid(mVector2Int);
        }

        public bool CheckBoardPosValidAndFilled(Vector2Int mVector2Int)
        {
            return (Units.ContainsKey(mVector2Int)) && CheckBoardPosValid(mVector2Int);
        }

        public int GetUnitCount()
        {
            return Units.Count;
        }

        void Awake()
        {
            Units=new Dictionary<Vector2Int, GameObject>();
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

        public bool DeliverUnitAssignedPlace(GameObject unit, Vector2Int AssignedPos)
        {
            if (CheckBoardPosValidAndEmpty(AssignedPos))
            {
                unit.GetComponentInChildren<Unit>().InitPosWithAnimation(AssignedPos);
                unit.GetComponentInChildren<Unit>().GameBoard = this;
                Units.Add(AssignedPos, unit);
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
            Units.Add(randomPlace, unit);          
            UpdateBoardInit();
            deliveringPos = randomPlace;
            return true;
        }

        public void UpdateBoard()
        {
            foreach (var unit in Units)
            {
                if (unit.Value == null) continue;
                var mUnit = unit.Value.GetComponentInChildren<Unit>();
                mUnit.UpdateNeighboringDataAndSideMesh();
            }
        }

        public bool TryDeleteCertainNoStationUnit(Vector2Int pos,out CoreType? destoryedCore)
        {
            if (CheckBoardPosValidAndFilled(pos))
            {
                //Debug.Log("Destorying=" + pos.ToString());
                Units.TryGetValue(pos, out GameObject go);
                if (!go.GetComponentInChildren<Unit>().StationUnit)
                {
                    destoryedCore = go.GetComponentInChildren<Unit>().UnitCore;
                    Destroy(go);
                    Units.Remove(pos);
                    UpdateBoard();
                    return true;
                }
            }
            destoryedCore = null;
            return false;
        }

        public int GetCountByType(CoreType coreType)
        {
            int res = 0;
            foreach (var unit in Units)
            {
                if (unit.Value.GetComponentInChildren<Unit>().UnitCore == coreType)
                {
                    res++;
                }
            }
            return res;
        }

        public void ResetUnitEmission()
        {
            foreach (var unit in Units)
            {
                unit.Value.GetComponentInChildren<Unit>().SetCoreEmissive(Color.black);
            }
        }

        public void DisplayConnectedHDDUnit()
        {
            float time = Time.timeSinceLevelLoad;
            foreach (var unit in Units)
            {
                if (unit.Value.GetComponentInChildren<Unit>().InHddGrid)
                {
                    Color color = (Mathf.Sin(time * 10) + 1.0f) * Color.red;
                    unit.Value.GetComponentInChildren<Unit>().SetCoreEmissive(color);
                }
            }
        }

        public void DisplayConnectedServerUnit()
        {
            float time = Time.timeSinceLevelLoad;
            foreach (var unit in Units)
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

        public void ForceChangeUnitCoreType(TutorialMgr invoker)
        {
            Debug.Assert(invoker,"这个函数只能在教程里面调。");
            foreach (var key in Units.Keys)
            {
                if (CheckBoardPosValidAndFilled(key))
                {
                    Units.TryGetValue(key, out GameObject go);
                    Destroy(go);
                }
            }
            Units=new Dictionary<Vector2Int, GameObject>();
            SideType[] sidesA =
            {
                SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection
            };
            GameObject goA = InitUnit(new Vector2Int(2, 2), CoreType.Server,Utils.Shuffle(sidesA));
            GameObject goB = InitUnit(new Vector2Int(2, 2), CoreType.NetworkCable,Utils.Shuffle(sidesA));
            GameObject goC = InitUnit(new Vector2Int(2, 2), CoreType.NetworkCable, Utils.Shuffle(sidesA));
            GameObject goD = InitUnit(new Vector2Int(2, 2), CoreType.NetworkCable, Utils.Shuffle(sidesA));
            GameObject goE = InitUnit(new Vector2Int(2, 2), CoreType.NetworkCable, Utils.Shuffle(sidesA));
            DeliverUnitRandomPlace(goA);
            DeliverUnitRandomPlace(goB);
            DeliverUnitRandomPlace(goC);
            DeliverUnitRandomPlace(goD);
            DeliverUnitRandomPlace(goE);
        }
    }
}
