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
        private readonly float _boardPhysicalOriginX = -3.1f - 1.75f;
        private readonly float _boardPhysicalOriginY = -3.1f;
        //public GlobalAssetLib _globalAssetLib;

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

        public Vector3 GetFloatTransform(Vector2Int boardPos)
        {
            return new Vector3(this._boardPhysicalOriginX + boardPos.x * this._boardPhysicalLength, 0,
                this._boardPhysicalOriginY + boardPos.y * this._boardPhysicalLength);
        }

        public void UpdateUnitBoardPos(Vector2Int oldKey)
        {
            Units.TryGetValue(oldKey,out var unit);
            Units.Remove(oldKey);
            System.Diagnostics.Debug.Assert(unit != null, nameof(unit) + " != null");
            Units.Add(unit.GetComponentInChildren<Unit>().board_position,unit);
        }

        public void UpdateBoard()
        {
            foreach (var unit in Units)
            {
                if (unit.Value == null) continue;
                var mUnit = unit.Value.GetComponentInChildren<Unit>();
                mUnit.UpdateTransform(GetFloatTransform(mUnit.board_position));
                mUnit.UpdateWorldRotationTransform();
            }
        }

        public GameObject InitUnit(Vector2Int board_pos,CoreType core,SideType[] sides)
        {
            var go = Instantiate(UnitTemplate);
            go.name = "Unit_" + Hash128.Compute(board_pos.ToString());
            var unit = go.GetComponentInChildren<Unit>();
            unit.board_position = board_pos;
            //unit._globalAssetLib = _globalAssetLib;
            unit.InitUnit(core, sides);
            return go;
        }

#if UNITY_EDITOR

        /*public void InitBoardRandom()
        {
            SideType[] sides = { SideType.SerialConnector, SideType.Connection, SideType.Connection, SideType.SerialConnector };
            //SideType[] sides2 = { SideType.NoConnection, SideType.SerialConnector, SideType.SerialConnector, SideType.NoConnection };
            Units.Add(new Vector2Int(2, 2), InitUnit(new Vector2Int(2, 2), CoreType.Server, Utils.Shuffle(sides)));
            Units.Add(new Vector2Int(2, 3), InitUnit(new Vector2Int(2, 3), CoreType.NetworkCable, Utils.Shuffle(sides)));
            Units.Add(new Vector2Int(3, 3), InitUnit(new Vector2Int(3, 3), CoreType.NetworkCable, Utils.Shuffle(sides)));
            Units.Add(new Vector2Int(3, 2), InitUnit(new Vector2Int(3, 2), CoreType.NetworkCable, Utils.Shuffle(sides)));

            Units.Add(new Vector2Int(4, 2), InitUnit(new Vector2Int(4, 2), CoreType.Processor, Utils.Shuffle(sides)));
            Units.Add(new Vector2Int(4, 3), InitUnit(new Vector2Int(4, 3), CoreType.HardDrive, Utils.Shuffle(sides)));
            Units.Add(new Vector2Int(4, 4), InitUnit(new Vector2Int(4, 4), CoreType.HardDrive, Utils.Shuffle(sides)));
            Units.Add(new Vector2Int(4, 5), InitUnit(new Vector2Int(4, 5), CoreType.HardDrive, Utils.Shuffle(sides)));
        }

        public void InitBoardTest()
        {
            SideType[] sides = { SideType.NoConnection,  SideType.Connection , SideType.NoConnection, SideType.Connection };
            SideType[] sides2 = { SideType.NoConnection,  SideType.SerialConnector , SideType.SerialConnector, SideType.NoConnection };
            Units.Add(new Vector2Int(2, 2), InitUnit(new Vector2Int(2, 2),CoreType.Processor, sides));
            Units.Add(new Vector2Int(2, 3), InitUnit(new Vector2Int(2, 3),CoreType.HardDrive, sides));
            Units.Add(new Vector2Int(3, 3), InitUnit(new Vector2Int(3, 3),CoreType.HardDrive, sides));
            Units.Add(new Vector2Int(3, 2), InitUnit(new Vector2Int(3, 2),CoreType.HardDrive, sides));

            Units.Add(new Vector2Int(4, 2), InitUnit(new Vector2Int(4, 2),CoreType.Server, sides2));
            Units.Add(new Vector2Int(4, 3), InitUnit(new Vector2Int(4, 3),CoreType.NetworkCable, sides2));
            Units.Add(new Vector2Int(4, 4), InitUnit(new Vector2Int(4, 4),CoreType.NetworkCable, sides2));
            Units.Add(new Vector2Int(4, 5), InitUnit(new Vector2Int(4, 5),CoreType.NetworkCable, sides2));
        }

        public void InitBoardSimpleStart()
        {
            Vector2Int[] startingArray =
                {new Vector2Int(2, 2), new Vector2Int(2, 3), new Vector2Int(3, 2), new Vector2Int(3, 3)};
            foreach (var vector2Int in startingArray)
            {
                var go = PrefabUtility.InstantiatePrefab(UnitTemplate) as GameObject;
                if (go == null)
                {
                    Debug.Assert(true,"Empty Template");
                    continue;
                };
                go.name = "Unit_" + Hash128.Compute(vector2Int.ToString());
                go.GetComponentInChildren<Unit>().board_position = vector2Int;
                Units.Add(vector2Int, go);
                go.GetComponentInChildren<Unit>().InitUnit(CoreType.NoConnection,
                    new[] {SideType.NoConnection, SideType.NoConnection, SideType.NoConnection, SideType.NoConnection});
            }
        }

        public void InitBoardSimple()
        {
            for (int i = 0; i < BoardLength * BoardLength; i++)
            {
                Vector2Int bVector2Int = new Vector2Int((i / BoardLength), i % BoardLength);
                var go = PrefabUtility.InstantiatePrefab(UnitTemplate) as GameObject;
                Debug.Assert(go != null);
                go.name = "Unit_" + Hash128.Compute(i.ToString());
                go.GetComponentInChildren<Unit>().board_position = bVector2Int;
                Units.Add(bVector2Int, go);
            }
        }*/
#endif

        public void InitBoardRealStart()
        {
            Vector2Int[] startingArray =
                {
                    new Vector2Int(0, 1),
                    new Vector2Int(0, 2),
                    new Vector2Int(0, 3),
                    new Vector2Int(0, 4),

                    new Vector2Int(5, 1),
                    new Vector2Int(5, 2),
                    new Vector2Int(5, 3),
                    new Vector2Int(5, 4),

                    new Vector2Int(1, 0),
                    new Vector2Int(2, 0),
                    new Vector2Int(3, 0),
                    new Vector2Int(4, 0),

                    new Vector2Int(1, 5),
                    new Vector2Int(2, 5),
                    new Vector2Int(3, 5),
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
                go.GetComponentInChildren<Unit>().board_position = vector2Int;
                Units.Add(vector2Int, go);
                go.GetComponentInChildren<Unit>().InitUnit(CoreType.PCB,
                    new[] { SideType.NoConnection, SideType.NoConnection, SideType.NoConnection, SideType.NoConnection });
            }

            Vector2Int vector2IntA=new Vector2Int(0,0);
            var goA = Instantiate(UnitTemplate);
            goA.name = "Unit_" + Hash128.Compute(vector2IntA.ToString());
            goA.GetComponentInChildren<Unit>().board_position = vector2IntA;
            Units.Add(vector2IntA, goA);
            goA.GetComponentInChildren<Unit>().InitUnit(CoreType.Processor,
                new[] { SideType.Connection, SideType.Connection, SideType.Connection, SideType.Connection });

            Vector2Int vector2IntB = new Vector2Int(5, 5);
            var goB = Instantiate(UnitTemplate);
            goB.name = "Unit_" + Hash128.Compute(vector2IntB.ToString());
            goB.GetComponentInChildren<Unit>().board_position = vector2IntB;
            Units.Add(vector2IntB, goB);
            goB.GetComponentInChildren<Unit>().InitUnit(CoreType.Processor,
                new[] { SideType.Connection, SideType.Connection, SideType.Connection, SideType.Connection });

            Vector2Int vector2IntC = new Vector2Int(0, 5);
            var goC = Instantiate(UnitTemplate);
            goC.name = "Unit_" + Hash128.Compute(vector2IntC.ToString());
            goC.GetComponentInChildren<Unit>().board_position = vector2IntC;
            Units.Add(vector2IntC, goC);
            goC.GetComponentInChildren<Unit>().InitUnit(CoreType.Server,
                new[] { SideType.Connection, SideType.Connection, SideType.Connection, SideType.Connection });

            Vector2Int vector2IntD = new Vector2Int(5, 0);
            var goD = Instantiate(UnitTemplate);
            goD.name = "Unit_" + Hash128.Compute(vector2IntD.ToString());
            goD.GetComponentInChildren<Unit>().board_position = vector2IntD;
            Units.Add(vector2IntD, goD);
            goD.GetComponentInChildren<Unit>().InitUnit(CoreType.Server,
                new[] { SideType.Connection, SideType.Connection, SideType.Connection, SideType.Connection });
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

        public bool DeliverUnitRandomPlace(GameObject unit)
        {
            Vector2Int[] emptyPlace = GetAllEmptySpace();
            if (emptyPlace.Length==0)
            {
                return false;
            }
            Vector2Int randomPlace = Utils.GenerateWeightedRandom(emptyPlace);
            unit.GetComponentInChildren<Unit>().board_position = randomPlace;
            Units.Add(randomPlace, unit);          
            UpdateBoard();
            return true;
        }

        public bool TryDeleteCertainUnit(Vector2Int pos)
        {
            if (CheckBoardPosValidAndFilled(pos))
            {
                //Debug.Log("Destorying=" + pos.ToString());
                Units.TryGetValue(pos, out GameObject go);
                Destroy(go);
                Units.Remove(pos);
                return true;
            }
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
                Material mt = unit.Value.GetComponentInChildren<Unit>().CoreMeshRenderer.material;
                mt.EnableKeyword("_EMISSION");//就不是很懂为什么?照理说新建的模板材质打开这个了啊。
                mt.SetColor("_EmissionColor", Color.black);              
            }
        }

        public void DisplayConnectedHDDUnit()
        {
            float time = Time.timeSinceLevelLoad;
            foreach (var unit in Units)
            {
                if (unit.Value.GetComponentInChildren<Unit>().InHDDGrid)
                {
                    Color color = (Mathf.Sin(time * 10) + 1.0f) * Color.red;
                    Material mt = unit.Value.GetComponentInChildren<Unit>().CoreMeshRenderer.material;
                    mt.SetColor("_EmissionColor", color);
                }
            }
        }

        public void DisplayConnectedServerUnit()
        {
            float time = Time.timeSinceLevelLoad;
            foreach (var unit in Units)
            {
                if (unit.Value.GetComponentInChildren<Unit>().InServerGrid)
                {
                    Color color = (Mathf.Sin(time * 10) + 1.0f) * Color.blue;
                    Material mt = unit.Value.GetComponentInChildren<Unit>().CoreMeshRenderer.material;
                    mt.SetColor("_EmissionColor", color);
                }
            }
        }
    }
}
