using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ROOT
{
    public enum RotationDirection
    {
        //即所为当前状态的表示，也作为旋转动作的标识
        //顺时针为正：±0，+90，-90，±180
        North,
        East,
        West,
        South
    }

    public struct ConnectionData
    {
        public bool HasConnector;
        public bool Connected;
        public CoreGenre ConnectedToGenre;

        public ConnectionData(bool hasConnector = false)
        {
            HasConnector = hasConnector;
            Connected = false;
            ConnectedToGenre = CoreGenre.Other;
        }
    }

    public abstract partial class UnitBase : MoveableBase
    {
        protected string UnitName { get; }

        public CoreType UnitCore { get; protected set; }
        public CoreGenre UnitCoreGenre { get; protected set; }
        public Dictionary<RotationDirection, SideType> UnitSides { get; protected set; }

        protected RotationDirection UnitRotation;

        //public RotationDirection UnitRotation => unitRotation;

        protected Transform RootTransform;

        protected Material CoreMat;

        protected MeshRenderer CoreMeshRenderer;
        protected MeshRenderer LocalNorthSideMeshRenderer;
        protected MeshRenderer LocalEastSideMeshRenderer;
        protected MeshRenderer LocalSouthSideMeshRenderer;
        protected MeshRenderer LocalWestSideMeshRenderer;

        protected Transform SideRootTransform;
        protected Transform CoreTransform;

        protected Dictionary<CoreType, string> CoreMatNameDic;
        protected Dictionary<SideType, Color> SideMatColorDic;

        public Mesh DtoDSideMesh;
        public Mesh DtoSSideMesh;

        public Board GameBoard;

        protected UnitBase()
        {
            UnitName = "";
        }

        public void InitDic()
        {
            CoreMatNameDic.Add(CoreType.PCB, GlobalResourcePath.UNIT_PCB_MAT_NAME);
            CoreMatNameDic.Add(CoreType.BackPlate, "");
            CoreMatNameDic.Add(CoreType.Bridge, "");
            CoreMatNameDic.Add(CoreType.Cooler, "");
            CoreMatNameDic.Add(CoreType.HardDrive, GlobalResourcePath.UNIT_HDD_MAT_NAME);
            CoreMatNameDic.Add(CoreType.NetworkCable, GlobalResourcePath.UNIT_NETCABLE_MAT_NAME);
            CoreMatNameDic.Add(CoreType.Processor, GlobalResourcePath.UNIT_PROCESSOR_MAT_NAME);
            CoreMatNameDic.Add(CoreType.Server, GlobalResourcePath.UNIT_SERVER_MAT_NAME);

            SideMatColorDic.Add(SideType.NoConnection, Color.red * 0.75f); //先干脆改成红的
            //SideMatColorDic.Add(SideType.Firewall, new Color(0.6f, 0.1f, 0.1f));
            SideMatColorDic.Add(SideType.Connection, Color.blue);
            //SideMatColorDic.Add(SideType.SerialConnector, new Color(0.1f, 0.6f, 0.6f));
        }

        public bool Visited { get; set; } //for scoring purpose
        public int IntA { get; set; } //for scoring purpose
        public int IntB { get; set; } //for scoring purpose
        public bool InHddGrid { get; set; } //for scoring purpose
        public bool InServerGrid { get; set; } //for scoring purpose

        //Rotation使用的世界方向的。
        public Dictionary<RotationDirection, ConnectionData> WorldNeighboringData { protected set; get; }

        public readonly RotationDirection[] RotationList =
        {
            RotationDirection.East,
            RotationDirection.North,
            RotationDirection.South,
            RotationDirection.West
        };

        protected Dictionary<RotationDirection, MeshRenderer> meshRendererLocalDir;

        [HideInInspector] public Vector2Int LastNetworkPos = Vector2Int.zero;

        //public GlobalAssetLib _globalAssetLib;

        //North,South,West,East
        protected void InitSide(MeshRenderer meshRenderer, SideType sideType)
        {
            if (sideType == SideType.Connection)
            {
                meshRenderer.material.SetColor("_Color", Color.green * 0.55f);
            }
            else if (sideType == SideType.NoConnection)
            {
                //感觉还是有个红的比较靠谱
                //meshRenderer.material.SetColor("_Color", Color.red * 0.25f);
                meshRenderer.enabled = false;
            }
        }

        protected void InitUnitMeshByCore(CoreType core)
        {
            UnitCoreGenre = GetCoreGenreByCoreType(core);
            string connectorMasterNodeName = "";
            string coreMeshNodeName = "";

            if (UnitCoreGenre == CoreGenre.Source)
            {
                connectorMasterNodeName = StaticName.SOURCE_CONNECTOR_MASTER_NODE_NAME;
                coreMeshNodeName = StaticName.SOURCE_CORE_NODE_NAME;
            }
            else
            {
                connectorMasterNodeName = StaticName.DEST_CONNECTOR_MASTER_NODE_NAME;
                coreMeshNodeName = StaticName.DEST_CORE_NODE_NAME;
            }

            Transform[] trans = RootTransform.GetComponentsInChildren<Transform>();
            foreach (var tran in trans)
            {
                if (tran.parent)
                {
                    if (tran.parent.name == "UnitRoot")
                    {
                        if (tran.name == connectorMasterNodeName)
                        {
                            SideRootTransform = tran;
                        }
                        else if (tran.name == coreMeshNodeName)
                        {
                            CoreTransform = tran;
                        }
                        else
                        {
                            MeshRenderer[] meshRenderers = tran.GetComponentsInChildren<MeshRenderer>();
                            foreach (var meshRenderer in meshRenderers)
                            {
                                meshRenderer.enabled = false;
                            }
                        }
                    }
                }
            }

            CoreMeshRenderer = CoreTransform.GetComponentInChildren<MeshRenderer>();

            MeshRenderer[] sideMeshRenderers = SideRootTransform.GetComponentsInChildren<MeshRenderer>();
            foreach (var meshRenderer in sideMeshRenderers)
            {
                if (meshRenderer.name == StaticName.LOCAL_EAST_SIDE_MESH_RENDERER_NAME)
                {
                    LocalEastSideMeshRenderer = meshRenderer;
                }

                if (meshRenderer.name == StaticName.LOCAL_NORTH_SIDE_MESH_RENDERER_NAME)
                {
                    LocalNorthSideMeshRenderer = meshRenderer;
                }

                if (meshRenderer.name == StaticName.LOCAL_SOUTH_SIDE_MESH_RENDERER_NAME)
                {
                    LocalSouthSideMeshRenderer = meshRenderer;
                }

                if (meshRenderer.name == StaticName.LOCAL_WEST_SIDE_MESH_RENDERER_NAME)
                {
                    LocalWestSideMeshRenderer = meshRenderer;
                }
            }

            meshRendererLocalDir = new Dictionary<RotationDirection, MeshRenderer>()
            {
                {RotationDirection.East, LocalEastSideMeshRenderer},
                {RotationDirection.North, LocalNorthSideMeshRenderer},
                {RotationDirection.South, LocalSouthSideMeshRenderer},
                {RotationDirection.West, LocalWestSideMeshRenderer},
            };
        }

        public void SetCoreEmissive(Color color)
        {
            CoreMeshRenderer.material.EnableKeyword("_EMISSION"); //还是不懂，为什么每次设置前得Enable一下。
            CoreMeshRenderer.material.SetColor("_EmissionColor", color);
        }

        public void InitUnit(CoreType core, SideType[] sides, Board gameBoard = null)
        {
            Debug.Assert(sides.Length == 4);
            InitUnit(core, sides[0], sides[1], sides[2], sides[3], gameBoard);
        }

        public void InitUnit(CoreType core, SideType lNSide, SideType lSSide, SideType lWSide, SideType lESide,
            Board gameBoard = null)
        {
            //Debug.Assert(side.Length == 4);
            this.UnitCore = core;
            InitUnitMeshByCore(core);

            UnitSides.Add(RotationDirection.North, lNSide);
            UnitSides.Add(RotationDirection.South, lSSide);
            UnitSides.Add(RotationDirection.West, lWSide);
            UnitSides.Add(RotationDirection.East, lESide);

            CoreMatNameDic.TryGetValue(UnitCore, out string val);
            CoreMeshRenderer.material = Resources.Load<Material>(GlobalResourcePath.UNIT_MAT_PATH_PREFIX + val);
            Debug.Assert(CoreMeshRenderer.material);

            InitSide(LocalNorthSideMeshRenderer, lNSide);
            InitSide(LocalEastSideMeshRenderer, lESide);
            InitSide(LocalSouthSideMeshRenderer, lSSide);
            InitSide(LocalWestSideMeshRenderer, lWSide);

            Visited = false;
            InServerGrid = false;
            InHddGrid = false;
            GameBoard = gameBoard;
        }

        protected virtual void Awake()
        {
            RootTransform = transform.parent;
            Debug.Assert(RootTransform != null, "Unit should use as prefab");
            CurrentBoardPosition = new Vector2Int(0, 0);
            UnitSides = new Dictionary<RotationDirection, SideType>();
            UnitRotation = RotationDirection.North;

            CoreMatNameDic = new Dictionary<CoreType, string>();
            SideMatColorDic = new Dictionary<SideType, Color>();
            InitDic();
        }

        public override void UpdateTransform(Vector3 pos)
        {
            RootTransform.position = pos;
        }

        public void UnitRotateCw()
        {
            UnitRotation = Utils.GetCWDirection(UnitRotation);
        }

        public void UnitRotateCcw()
        {
            UnitRotation = Utils.GetCCWDirection(UnitRotation);
        }

        public SideType GetLocalSpaceUnitSide(RotationDirection localDirection)
        {
            SideType res = SideType.SIDETYPECOUNT;
            UnitSides.TryGetValue(UnitRotation, out res);
            return res;
        }

        public SideType GetWorldSpaceUnitSide(RotationDirection worldDirection)
        {
            SideType res = SideType.SIDETYPECOUNT;
            var desiredLocalSideDirection = Utils.RotateDirectionBeforeRotation(worldDirection, UnitRotation);
            UnitSides.TryGetValue(desiredLocalSideDirection, out res);
            return res;
        }

        public void UpdateWorldRotationTransform()
        {
            switch (UnitRotation)
            {
                case RotationDirection.North:
                    RootTransform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case RotationDirection.East:
                    RootTransform.rotation = Quaternion.Euler(0, 90, 0);
                    break;
                case RotationDirection.West:
                    RootTransform.rotation = Quaternion.Euler(0, 270, 0);
                    break;
                case RotationDirection.South:
                    RootTransform.rotation = Quaternion.Euler(0, 180, 0);
                    break;
            }

            UpdateNeighboringDataAndSideMesh();
        }

        public void UpdateNeighboringData()
        {
            WorldNeighboringData = new Dictionary<RotationDirection, ConnectionData>();
            if (GameBoard != null)
            {
                foreach (var currentSideDirection in RotationList)
                {
                    ConnectionData connectionData = new ConnectionData();
                    if (GetWorldSpaceUnitSide(currentSideDirection) == SideType.Connection)
                    {
                        connectionData.HasConnector = true;
                        Vector2Int otherUnitPos = GetNeigbourCoord(currentSideDirection);
                        GameBoard.Units.TryGetValue(otherUnitPos, out GameObject value);
                        if (value != null)
                        {
                            Unit otherUnit = value.GetComponentInChildren<Unit>();
                            connectionData.Connected =
                                (otherUnit.GetWorldSpaceUnitSide(Utils.GetInvertDirection(currentSideDirection)) ==
                                 SideType.Connection);
                            if (connectionData.Connected)
                            {
                                connectionData.ConnectedToGenre = otherUnit.UnitCoreGenre;
                            }
                        }
                    }
                    WorldNeighboringData.Add(currentSideDirection, connectionData);
                }
            }
        }

        private void UpdateDestConnectionSide(ConnectionMeshType connectionMeshType,ref MeshRenderer localMeshRenderer)
        {
            if (connectionMeshType == ConnectionMeshType.NoChange) return;
            switch (connectionMeshType)
            {
                case ConnectionMeshType.NoConnectionMesh:
                case ConnectionMeshType.DtoDConnectedMesh:
                    localMeshRenderer.gameObject.GetComponent<MeshFilter>().mesh = DtoDSideMesh;
                    break;
                case ConnectionMeshType.DtSConnectedMesh:
                    localMeshRenderer.gameObject.GetComponent<MeshFilter>().mesh = DtoSSideMesh;
                    break;
                case ConnectionMeshType.StDConnectedMesh:
                case ConnectionMeshType.StSConnectedMesh:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void UpdateSideMesh()
        {
            if (WorldNeighboringData != null)
            {
                foreach (var currentSideDirection in RotationList)
                {
                    WorldNeighboringData.TryGetValue(currentSideDirection, out ConnectionData data);
                    if (data.HasConnector)
                    {
                        var localRotation = Utils.RotateDirectionBeforeRotation(currentSideDirection, UnitRotation);
                        meshRendererLocalDir.TryGetValue(localRotation, out MeshRenderer localMeshRenderer);
#if UNITY_EDITOR
                        System.Diagnostics.Debug.Assert(localMeshRenderer != null,
                            nameof(localMeshRenderer) + " != null");
#endif
                        ConnectionMeshType connectionMeshType = data.Connected ? Utils.GetRelationBetweenGenre(UnitCoreGenre, data.ConnectedToGenre) : Utils.GetRelationNoConnection(UnitCoreGenre);
                        UpdateDestConnectionSide(connectionMeshType, ref localMeshRenderer);
                    }
                }
            }
        }

        public void UpdateNeighboringDataAndSideMesh()
        {
            UpdateNeighboringData();
            UpdateSideMesh();
        }
    }
}