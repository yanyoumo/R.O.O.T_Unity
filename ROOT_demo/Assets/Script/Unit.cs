using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

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
        public Unit OtherUnit;

        public ConnectionData(bool hasConnector = false)
        {
            HasConnector = hasConnector;
            Connected = false;
            ConnectedToGenre = CoreGenre.Other;
            OtherUnit = null;
        }
    }

    public sealed partial class ShopMgr : MonoBehaviour
    {

        private GameObject InitUnitShop(CoreType core, SideType[] sides, out float price, int ID)
        {
            var go = Instantiate(UnitTemplate);
            go.name = "Unit_" + Hash128.Compute(Utils.LastRandom.ToString());
            var unit = go.GetComponentInChildren<Unit>();
            unit.InitPosWithAnimation(Vector2Int.zero);
            unit.InitUnit(core, sides);
            _priceByCore.TryGetValue(core, out float corePrice);
            _priceBySide.TryGetValue(sides[0], out float sidePrice0);
            _priceBySide.TryGetValue(sides[1], out float sidePrice1);
            _priceBySide.TryGetValue(sides[2], out float sidePrice2);
            _priceBySide.TryGetValue(sides[3], out float sidePrice3);
            price = corePrice + sidePrice0 + sidePrice1 + sidePrice2 + sidePrice3;
            unit.ShopID = ID;
            return go;
        }
    }

    public partial class Unit : MoveableBase
    {
        public int ShopID { get; internal set; } = -1;
        protected string UnitName { get; }

        public CoreType UnitCore { get; protected set; }
        public CoreGenre UnitCoreGenre { get; protected set; }
        public Dictionary<RotationDirection, SideType> UnitSides { get; protected set; }

        private RotationDirection _unitRotation;
        private Transform _rootTransform;
        private Material _coreMat;

        private MeshRenderer _coreMeshRenderer;
        /*private MeshRenderer _localNorthSideMeshRenderer;
        private MeshRenderer _localEastSideMeshRenderer;
        private MeshRenderer _localSouthSideMeshRenderer;
        private MeshRenderer _localWestSideMeshRenderer;*/
        public Dictionary<RotationDirection, Connector> ConnectorLocalDir;
        public Connector _localNorthConnector;
        public Connector _localEastConnector;
        public Connector _localWestConnector;
        public Connector _localSouthConnector;

        private Transform _sideRootTransform;
        private Transform _coreTransform;

        private Dictionary<CoreType, string> _coreMatNameDic;
        //private Dictionary<SideType, Color> _sideMatColorDic;

        public Mesh DtoDSideMesh;
        public Mesh DtoSSideMesh;

        public Board GameBoard;

        protected Unit()
        {
            UnitName = "";
        }

        public void InitDic()
        {
            _coreMatNameDic.Add(CoreType.PCB, GlobalResourcePath.UNIT_PCB_MAT_NAME);
            _coreMatNameDic.Add(CoreType.BackPlate, "");
            _coreMatNameDic.Add(CoreType.Bridge, "");
            _coreMatNameDic.Add(CoreType.Cooler, "");
            _coreMatNameDic.Add(CoreType.HardDrive, GlobalResourcePath.UNIT_HDD_MAT_NAME);
            _coreMatNameDic.Add(CoreType.NetworkCable, GlobalResourcePath.UNIT_NETCABLE_MAT_NAME);
            _coreMatNameDic.Add(CoreType.Processor, GlobalResourcePath.UNIT_PROCESSOR_MAT_NAME);
            _coreMatNameDic.Add(CoreType.Server, GlobalResourcePath.UNIT_SERVER_MAT_NAME);
        }

        public RotationDirection SignalFromDir;
        public bool Visited { get; set; } //for scoring purpose
        public int ServerDepth;//for scoring purpose
        public int HardDiskVal; //for scoring purpose
        public bool InHddGrid { get; set; } //for scoring purpose
        public bool InHddSignalGrid; //for scoring purpose
        public bool InServerGrid; //for scoring purpose
        public bool InServerSignalGrid { get; set; } //for scoring purpose

        //Rotation使用的世界方向的。
        public Dictionary<RotationDirection, ConnectionData> WorldNeighboringData { protected set; get; }

        [HideInInspector]
        public readonly RotationDirection[] RotationList =
        {
            RotationDirection.East,
            RotationDirection.North,
            RotationDirection.South,
            RotationDirection.West
        };

        /*[HideInInspector]
        public Dictionary<RotationDirection, MeshRenderer> MeshRendererLocalDir;*/

        [HideInInspector] public Vector2Int LastNetworkPos = Vector2Int.zero;

        private void InitConnector(Connector connector, SideType sideType)
        {
            connector.UseScrVersion = (UnitCoreGenre == CoreGenre.Source);
            //connector.UseScrVersion = true;
            connector.gameObject.SetActive(sideType == SideType.Connection);
            connector.NormalSignalVal = 3;
            connector.NetworkSignalVal = 5;
        }

        //North,South,West,East
        protected void InitSide(MeshRenderer meshRenderer, SideType sideType)
        {
            if (sideType == SideType.Connection)
            {
                meshRenderer.material.SetColor("_Color", Color.green * 0.55f);
            }
            else if (sideType == SideType.NoConnection)
            {
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

            Transform[] trans = _rootTransform.GetComponentsInChildren<Transform>();
            foreach (var tran in trans)
            {
                if (tran.parent)
                {
                    if (tran.parent.name == "UnitRoot")
                    {
                        if (tran.name == connectorMasterNodeName)
                        {
                            _sideRootTransform = tran;
                        }
                        else if (tran.name == coreMeshNodeName)
                        {
                            _coreTransform = tran;
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

            _coreMeshRenderer = _coreTransform.GetComponentInChildren<MeshRenderer>();
            ConnectorLocalDir = new Dictionary<RotationDirection, Connector>()
            {
                {RotationDirection.East, _localEastConnector},
                {RotationDirection.North, _localNorthConnector},
                {RotationDirection.South, _localSouthConnector},
                {RotationDirection.West, _localWestConnector},
            };
        }

        public void SetCoreEmissive(Color color)
        {
            _coreMeshRenderer.material.EnableKeyword("_EMISSION"); //还是不懂，为什么每次设置前得Enable一下。
            _coreMeshRenderer.material.SetColor("_EmissionColor", color);
        }

        public void InitUnit(CoreType core, SideType[] sides, Board gameBoard = null)
        {
            Debug.Assert(sides.Length == 4);
            InitUnit(core, sides[0], sides[1], sides[2], sides[3], gameBoard);
        }

        public void InitUnit(CoreType core, SideType lNSide, SideType lSSide, SideType lWSide, SideType lESide, Board gameBoard = null)
        {
            this.UnitCore = core;
            InitUnitMeshByCore(core);

            UnitSides.Add(RotationDirection.North, lNSide);
            UnitSides.Add(RotationDirection.South, lSSide);
            UnitSides.Add(RotationDirection.West, lWSide);
            UnitSides.Add(RotationDirection.East, lESide);

            _coreMatNameDic.TryGetValue(UnitCore, out string val);
            _coreMeshRenderer.material = Resources.Load<Material>(GlobalResourcePath.UNIT_MAT_PATH_PREFIX + val);
            Debug.Assert(_coreMeshRenderer.material);

            InitConnector(_localNorthConnector,lNSide);
            InitConnector(_localEastConnector,lESide);
            InitConnector(_localWestConnector,lWSide);
            InitConnector(_localSouthConnector,lSSide);

            Visited = false;
            InServerGrid = false;
            InHddGrid = false;
            GameBoard = gameBoard;

            UpdateSideMesh();
        }

        protected virtual void Awake()
        {
            ShopID = -1;

            _rootTransform = transform.parent;
            Debug.Assert(_rootTransform != null, "Unit should use as prefab");
            CurrentBoardPosition = new Vector2Int(0, 0);
            UnitSides = new Dictionary<RotationDirection, SideType>();
            _unitRotation = RotationDirection.North;

            _coreMatNameDic = new Dictionary<CoreType, string>();
            //_sideMatColorDic = new Dictionary<SideType, Color>();
            InitDic();
        }

        public override void UpdateTransform(Vector3 pos)
        {
            _rootTransform.position = pos;
        }

        public void UnitRotateCw()
        {
            _unitRotation = Utils.GetCWDirection(_unitRotation);
        }

        public void UnitRotateCcw()
        {
            _unitRotation = Utils.GetCCWDirection(_unitRotation);
        }

        public SideType GetLocalSpaceUnitSide(RotationDirection localDirection)
        {
            SideType res = SideType.SIDETYPECOUNT;
            UnitSides.TryGetValue(_unitRotation, out res);
            return res;
        }

        public SideType GetWorldSpaceUnitSide(RotationDirection worldDirection)
        {
            SideType res = SideType.SIDETYPECOUNT;
            var desiredLocalSideDirection = Utils.RotateDirectionBeforeRotation(worldDirection, _unitRotation);
            UnitSides.TryGetValue(desiredLocalSideDirection, out res);
            return res;
        }

        public void UpdateWorldRotationTransform()
        {
            switch (_unitRotation)
            {
                case RotationDirection.North:
                    _rootTransform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case RotationDirection.East:
                    _rootTransform.rotation = Quaternion.Euler(0, 90, 0);
                    break;
                case RotationDirection.West:
                    _rootTransform.rotation = Quaternion.Euler(0, 270, 0);
                    break;
                case RotationDirection.South:
                    _rootTransform.rotation = Quaternion.Euler(0, 180, 0);
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
                            connectionData.OtherUnit = otherUnit;
                            connectionData.Connected = (otherUnit.GetWorldSpaceUnitSide(Utils.GetInvertDirection(currentSideDirection)) ==
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

        private void UpdateDestConnectionSide(ConnectionMeshType connectionMeshType, ref Connector connector)
        {
            if (connectionMeshType == ConnectionMeshType.NoChange) return;
            switch (connectionMeshType)
            {
                case ConnectionMeshType.NoConnectionMesh:
                    connector.Connected = false;
                    break;
                case ConnectionMeshType.DtoDConnectedMesh:
                case ConnectionMeshType.DtSConnectedMesh:
                case ConnectionMeshType.StDConnectedMesh:
                    connector.Connected = true;
                    break;
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
                        var localRotation = Utils.RotateDirectionBeforeRotation(currentSideDirection, _unitRotation);
                        ConnectorLocalDir.TryGetValue(localRotation, out Connector Connector);
                        Connector.Connected = data.Connected;
                        Connector.NormalSignalVal = 0;
                        Connector.NetworkSignalVal = 0;
                        if (data.Connected)
                        {
                            if (UnitCoreGenre == CoreGenre.Destination)
                            {
                                if (data.OtherUnit != null)
                                {
                                    bool noHideHDD = InHddSignalGrid && (currentSideDirection == SignalFromDir) && (HardDiskVal != 0);
                                    bool noHideNet = InServerGrid && (ServerDepth >= data.OtherUnit.ServerDepth) && (data.OtherUnit.InServerGrid);

                                    bool otherNoHideHDD = data.OtherUnit.InHddSignalGrid && (Utils.GetInvertDirection(currentSideDirection) == data.OtherUnit.SignalFromDir) && (data.OtherUnit.HardDiskVal != 0);
                                    bool otherNoHideNet = data.OtherUnit.InServerGrid && (data.OtherUnit.ServerDepth >= ServerDepth) && (InServerGrid);

                                    if (data.OtherUnit.HardDiskVal == 0 && UnitCore != CoreType.HardDrive)
                                    {
                                        Connector.NormalSignalVal = 0;
                                    }
                                    else
                                    {
                                        if (UnitCore != CoreType.HardDrive)
                                        {
                                            Connector.NormalSignalVal = Math.Min(HardDiskVal, data.OtherUnit.HardDiskVal);
                                        }
                                        else
                                        {
                                            Connector.NormalSignalVal = HardDiskVal;
                                        }
                                    }

                                    if (ServerDepth == -1 || !InServerGrid)
                                    {
                                        Connector.NetworkSignalVal = 0;
                                    }
                                    else
                                    {
                                        int SV = BoardDataCollector.MaxNetworkDepth - ServerDepth + 1;
                                        if (UnitCore != CoreType.NetworkCable)
                                        {
                                            SV--;
                                        }
                                        Connector.NetworkSignalVal = SV;
                                    }

                                    bool noHide = noHideHDD || noHideNet; //有可能算出来两边都显示（？），的确，常见
                                    bool otherNoHide = otherNoHideHDD || otherNoHideNet; //有可能算出来两边都显示（？），的确，常见
                                    if (noHide && otherNoHide)
                                    {
                                        int myData = (CurrentBoardPosition.x * 10 + CurrentBoardPosition.y);
                                        int otherData = (data.OtherUnit.CurrentBoardPosition.x * 10 + data.OtherUnit.CurrentBoardPosition.y);

                                        if (data.OtherUnit.UnitCore == CoreType.Server || data.OtherUnit.UnitCore == CoreType.Processor)
                                        {
                                            noHide = true;
                                        }
                                        else
                                        {
                                            noHide = (myData > otherData);
                                            if (noHide)
                                            {
                                                int SV = BoardDataCollector.MaxNetworkDepth - ServerDepth + 1;
                                                int otherSV = BoardDataCollector.MaxNetworkDepth - data.OtherUnit.ServerDepth + 1;
                                                Connector.NetworkSignalVal = Math.Min(SV,otherSV);
                                                Connector.NormalSignalVal = Math.Min(HardDiskVal, data.OtherUnit.HardDiskVal);
                                            }
                                        }
                                    }

                                    Connector.Hided = !noHide;
                                }
                            }
                            else
                            {
                                //Source不显示
                                Connector.Hided = true;
                            }
                        }
                    }
                }
            }
            else
            {
                //ShopCase
                foreach (var keyValuePair in ConnectorLocalDir)
                {
                    keyValuePair.Value.Connected = false;
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
