using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

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
        private float TryGetPrice(SideType side)
        {
            if (_priceBySide.TryGetValue(side, out var sidePrice0))
            {
                return sidePrice0;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }
        /// <summary>
        /// 生成新的静态单元Flag的Array。
        /// </summary>
        /// <returns>来提示是否还有新的Array</returns>
        private bool GenerateStationaryArray()
        {
            var srList = currentLevelAsset.ActionAsset.StationaryRateList ?? DefaultStationaryRateList;
            if (StationaryRateListLastIndex>= srList.Length)
            {
                return false;
            }
            var vec = srList[StationaryRateListLastIndex];
            if (vec.x == 0 && vec.y == 0)
            {
                return false;
            }

            stationaryArray = new bool[vec.x];
            for (var i = 0; i < stationaryArray.Length; i++)
            {
                stationaryArray[i] = (i < vec.y);
            }

            Utils.Shuffle(stationaryArray);
            StationaryRateListLastIndex++;
            return true;
        }
        /// <summary>
        /// 根据现有的状态来决定下一个单元是否应该是静态的。
        /// </summary>
        private bool ShouldStationary
        {
            get
            {
                var forceNewArray = false;
                if (nomoreStationary) return false;
                if (stationaryArray != null && localOffset == stationaryArray.Length)
                {
                    countOffset = totalCount;
                    forceNewArray = true;
                }

                if (stationaryArray == null || forceNewArray)
                {
                    if (!GenerateStationaryArray())
                    {
                        nomoreStationary = true;
                        return false;
                    }
                }
                return stationaryArray[localOffset];
            }
        }

        private GameObject InitUnitShop(CoreType core, SideType[] sides, out float hardwarePrice, int ID, int _cost,int tier)
        {
            var go = Instantiate(UnitTemplate);
            go.name = "Unit_" + Hash128.Compute(Utils.LastRandom.ToString());
            var unit = go.GetComponentInChildren<Unit>();
            unit.InitPosWithAnimation(Vector2Int.zero);
            unit.InitUnit(core, sides, tier);
            if (ShouldStationary)
            {
                unit.SetupStationUnit();
                hardwarePrice = StationaryDiscount(sides);
            }
            else
            {
                _priceByCore.TryGetValue(core, out var corePrice);
                hardwarePrice = corePrice + sides.Sum(TryGetPrice);
            }

            unit.SetShop(ID, Mathf.FloorToInt(hardwarePrice), _cost, totalCount % 2 == 0);
            totalCount++;
            return go;
        }
    }

    public partial class Unit : MoveableBase
    {
        public int Cost { get; internal set; } = 0;

        private int _tier = 0;
        public int Tier
        {
            get => _tier;
            internal set
            {
                _tier = value;
                TierTag.text = Utils.PaddingNum2Digit(_tier);
                TierLEDs.Val = _tier;
            }
        }
        private int _retailPrice = -1;
        public int RetailPrice
        {
            get => _retailPrice;
            internal set
            {
                _retailPrice = value;
                PriceTag.text = Utils.PaddingNum2Digit(_retailPrice);
            }
        }

        public SimpleLEDArray TierLEDs;

        public TextMeshPro TierTag;
        public TextMeshPro CostTag;
        public TextMeshPro PriceTag;

        public Transform ShopBackPlane;
        public MeshRenderer BackQuadRenderer;
        public Material BuyingMat;
        public Material ImmovableMat;
        public override bool Immovable
        {
            set
            {
                _immovable = value;
                AdditionalClampMesh.material = ImmovableMat;
                AdditionalClampMesh.enabled = _immovable;
            }
            get => _immovable;
        }
        public MeshRenderer AdditionalClampMesh;
        public bool StationUnit { get; private set; }
        public Dictionary<RotationDirection, Tuple<int, int>> StationRequirement;
        public int ShopID { get; private set; } = -1;

        public void UnsetShop()
        {
            ShopID = -1;
            SetPendingBuying = false;
            ShopBackPlane.gameObject.SetActive(false);
        }

        public void SetShop(int shopID, int price, int _cost, bool? showQuad)
        {
            ShopID = shopID;
            if (price != -1)
            {
                RetailPrice = price;
            }

            ShopBackPlane.gameObject.SetActive(true);
            if (showQuad.HasValue)
            {
                BackQuadRenderer.enabled = showQuad.Value;
            }

            if (_cost != -1)
            {
                Cost = _cost;
                CostTag.text = Utils.PaddingNum2Digit(Cost);
            }
        }

        public bool SetPendingBuying
        {
            set
            {
                if (value)
                {
                    //Debug.Assert(ShopID != -1);
                    AdditionalClampMesh.material = BuyingMat;
                    AdditionalClampMesh.enabled = true;
                }
                else
                {
                    if (!StationUnit)
                    {
                        AdditionalClampMesh.enabled = false;
                    }
                    else
                    {
                        AdditionalClampMesh.material = ImmovableMat;
                    }
                }
            }
        }

        protected string UnitName { get; }

        public CoreType UnitCore { get; protected set; }
        public CoreGenre UnitCoreGenre { get; protected set; }
        public bool IsSource => UnitCoreGenre == CoreGenre.Source;
        public Dictionary<RotationDirection, SideType> UnitSides { get; protected set; }

        private RotationDirection _unitRotation;
        private Transform _rootTransform=>transform.parent;
        private Material _coreMat;

        private MeshRenderer _coreMeshRenderer;
        public Dictionary<RotationDirection, Connector> ConnectorLocalDir;
        public Connector _localNorthConnector;
        public Connector _localEastConnector;
        public Connector _localWestConnector;
        public Connector _localSouthConnector;

        private Transform _sideRootTransform;
        private Transform _coreTransform;

        private Dictionary<CoreType, string> _coreMatNameDic;

        public Mesh DtoDSideMesh;
        public Mesh DtoSSideMesh;

        public Board GameBoard;

        public void SetupStationUnit()
        {
            Immovable = true;
            StationUnit = true;
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
            _coreMatNameDic.Add(CoreType.HQ, GlobalResourcePath.UNIT_HQ_MAT_NAME);//TODO HQ的核心还没有实际材质
        }

        [ReadOnly]
        public RotationDirection SignalFromDir;
        [ReadOnly]
        public bool Visited { get; set; } //for scoring purpose
        [ReadOnly]
        public int HardDiskVal; //for scoring purpose
        [ReadOnly]
        public bool InHddGrid { get; set; } //for scoring purpose
        [ReadOnly]
        public bool InHddSignalGrid; //for scoring purpose

        #region 服务器计分
        /// <summary>
        /// 记录服务器信号深度的变量，和服务器相连的Network该数值应该为1.
        /// 可以作为中间量、即使不处于最长序列该值不必清除。
        /// </summary>
        [ReadOnly]
        public int ServerDepth;//for scoring purpose
        /// <summary>
        /// 标记一次计分后，本单元是否处于必要最长序列中。不处于的需要显式记为false。
        /// </summary>
        [ReadOnly]
        public bool InServerGrid; //for scoring purpose
        /// <summary>
        /// 具体显示LED的field，即，最接近服务器的该数值应为全部深度，最枝端的显示值需要为1。
        /// </summary>
        public int NetworkVal=> BoardDataCollector.MaxNetworkDepth - ServerDepth + 1;
        #endregion

        //Rotation使用的世界方向的。
        public Dictionary<RotationDirection, ConnectionData> WorldNeighboringData { protected set; get; }

        public bool AnyConnection
        {
            get
            {
                return WorldNeighboringData.Where(keyValuePair => keyValuePair.Value.HasConnector).Any(keyValuePair => keyValuePair.Value.Connected);
            }
        }

        [HideInInspector]
        public readonly RotationDirection[] RotationList =
        {
            RotationDirection.East,
            RotationDirection.North,
            RotationDirection.South,
            RotationDirection.West
        };

        [HideInInspector] public Vector2Int LastNetworkPos = Vector2Int.zero;

        private void InitConnector(Connector connector, SideType sideType)
        {
            connector.UseScrVersion = (UnitCoreGenre == CoreGenre.Source);
            connector.gameObject.SetActive(sideType == SideType.Connection);
            connector.NormalSignalVal = 0;
            connector.NetworkSignalVal = 0;
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

            TierLEDs.gameObject.SetActive(!IsSource);
        }

        public void SetCoreEmissive(Color color)
        {
            _coreMeshRenderer.material.EnableKeyword("_EMISSION"); //还是不懂，为什么每次设置前得Enable一下。
            _coreMeshRenderer.material.SetColor("_EmissionColor", color);
        }

        public void InitUnit(CoreType core, SideType[] sides, int tier, Board gameBoard = null)
        {
            Debug.Assert(sides.Length == 4);
            InitUnit(core, sides[0], sides[1], sides[2], sides[3], tier, gameBoard);
        }

        public void InitUnit(CoreType core, SideType lNSide, SideType lSSide, SideType lWSide, SideType lESide, int tier, Board gameBoard = null)
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

            Tier = tier;

            UpdateSideMesh();
        }

        protected virtual void Awake()
        {
            ShopID = -1;
            CurrentBoardPosition = new Vector2Int(0, 0);
            UnitSides = new Dictionary<RotationDirection, SideType>();
            _unitRotation = RotationDirection.North;

            _coreMatNameDic = new Dictionary<CoreType, string>();
            Immovable = false;
            ShopBackPlane.gameObject.SetActive(false);
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
            UnitSides.TryGetValue(_unitRotation, out var res);
            return res;
        }

        public SideType GetWorldSpaceUnitSide(RotationDirection worldDirection)
        {
            var desiredLocalSideDirection = Utils.RotateDirectionBeforeRotation(worldDirection, _unitRotation);
            UnitSides.TryGetValue(desiredLocalSideDirection, out var res);
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
                        GameBoard.UnitsGameObjects.TryGetValue(otherUnitPos, out GameObject value);
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
                        var NormalSignalValtmp = 0;
                        var NetworkSignalValtmp = 0;
                        /*Connector.NormalSignalVal = 0;
                        Connector.NetworkSignalVal = 0;*/
                        if (data.Connected)
                        {
                            if (UnitCoreGenre == CoreGenre.Destination)
                            {
                                Unit otherUnit = data.OtherUnit;
                                if (otherUnit != null)
                                {
                                    bool noHideHDD = InHddSignalGrid && (currentSideDirection == SignalFromDir) && (HardDiskVal != 0);
                                    bool noHideNet = InServerGrid && (ServerDepth >= otherUnit.ServerDepth) && (otherUnit.InServerGrid);

                                    bool otherNoHideHDD = otherUnit.InHddSignalGrid && (Utils.GetInvertDirection(currentSideDirection) == otherUnit.SignalFromDir) && (otherUnit.HardDiskVal != 0);
                                    bool otherNoHideNet = otherUnit.InServerGrid && (otherUnit.ServerDepth >= ServerDepth) && (InServerGrid);

                                    if (otherUnit.HardDiskVal == 0 && UnitCore != CoreType.HardDrive)
                                    {
                                        NormalSignalValtmp = 0;
                                    }
                                    else
                                    {
                                        if (UnitCore != CoreType.HardDrive)
                                        {
                                            NormalSignalValtmp = Math.Min(HardDiskVal, otherUnit.HardDiskVal);
                                        }
                                        else
                                        {
                                            NormalSignalValtmp = HardDiskVal;
                                        }
                                    }

                                    if (ServerDepth == -1 || !InServerGrid)
                                    {
                                        NetworkSignalValtmp = 0;
                                    }
                                    else
                                    {
                                        if (UnitCore != CoreType.NetworkCable)
                                        {
                                            NetworkSignalValtmp = NetworkVal-1;
                                        }
                                        else
                                        {
                                            NetworkSignalValtmp = NetworkVal;
                                        }
                                    }

                                    bool noHide = noHideHDD || noHideNet; //有可能算出来两边都显示（？），的确，常见
                                    bool otherNoHide = otherNoHideHDD || otherNoHideNet; //有可能算出来两边都显示（？），的确，常见
                                    if (noHide && otherNoHide)
                                    {
                                        if (otherUnit.UnitCore == CoreType.Server || otherUnit.UnitCore == CoreType.Processor)
                                        {
                                            noHide = true;
                                        }
                                        else
                                        {
                                            noHide = (PosHash > otherUnit.PosHash);
                                            if (noHide)
                                            {
                                                NetworkSignalValtmp = Math.Min(NetworkVal, otherUnit.NetworkVal);
                                                NormalSignalValtmp = Math.Min(HardDiskVal, otherUnit.HardDiskVal);
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
                        Connector.NormalSignalVal = NormalSignalValtmp;
                        Connector.NetworkSignalVal = NetworkSignalValtmp;
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
