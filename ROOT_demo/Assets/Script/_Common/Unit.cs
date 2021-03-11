using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ROOT.Signal;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

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
        public HardwareType ConnectedToGenre;
        private Unit _otherUnit;

        public Unit OtherUnit
        {
            get => HasConnector && Connected ? _otherUnit : null;
            set => _otherUnit = value;
        }

        public ConnectionData(bool hasConnector = false)
        {
            HasConnector = hasConnector;
            Connected = false;
            ConnectedToGenre = HardwareType.Other;
            _otherUnit = null;
        }
    }

    public enum PlayingSignalSelector
    {
        TypeA,
        TypeB,
    }

    [Serializable]
    public struct UnitGist
    {
        [Header("Basic")] public PlayingSignalSelector PlayingSignalSelector;
        public HardwareType CoreGenre;
        public SideType[] Sides;
        [Range(1, 5)] public int Tier;

        [Header("OnBoardInfo")] public Vector2Int Pos;
        public bool IsStation;
    }

    public sealed partial class ShopMgr : ShopBase
    {
        /// <summary>
        /// 生成新的静态单元Flag的Array。
        /// </summary>
        /// <returns>来提示是否还有新的Array</returns>
        private bool GenerateStationaryArray()
        {
            var srList = currentLevelAsset.ActionAsset.StationaryRateList ?? DefaultStationaryRateList;
            if (StationaryRateListLastIndex >= srList.Length)
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
                return false;
                var forceNewArray = false;
                if (nomoreStationary) return false;
                if (stationaryArray != null && localOffset == stationaryArray.Length)
                {
                    countOffset = TotalCount;
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

        private GameObject InitUnitShop(SignalType signalType, HardwareType genre, SideType[] sides,
            out float hardwarePrice, int ID, int _cost,
            int tier)
        {
            var go = InitUnitShopCore(signalType, genre, sides, ID, _cost, tier);
            var unit = go.GetComponentInChildren<Unit>();
            if (ShouldStationary)
            {
                unit.SetupStationUnit();
                hardwarePrice = StationaryDiscount(sides);
            }
            else
            {
                var corePrice = SignalMasterMgr.Instance.PriceFromUnit(signalType, genre);
                hardwarePrice = corePrice + sides.Sum(TryGetPrice);
            }

            TotalCount++;
            return go;
        }

        public override void OpenShop(bool Opening, int discount)
        {
            throw new NotImplementedException();
        }
    }

    public partial class Unit : MoveableBase,IClickable
    {
        public UnitSignalCoreBase SignalCore;
        public TextMeshPro BillBoardText;

        public Dictionary<SignalType, SignalData> SignalDataPackList => SignalCore.SignalDataPackList;

        public SignalData SignalDataVal(SignalType signalType)
        {
            return SignalDataPackList[signalType];
        }
        
        public MeshRenderer UnitActivationLEDMat;
        public Color[] UnitActivationLEDMat_Colors => SignalMasterMgr.Instance.UnitActivationLED_Colors;

        private int Cost = 0;
        private int _tier = 0;

        public int Tier
        {
            get => _tier;
            private set
            {
                _tier = value;
                TierTag.text = Utils.PaddingNum2Digit(_tier);
                TierLEDs.Val = _tier;
            }
        }

        private int _hardwarePrice = -1;
        private int _retailPrice = -1;

        public int RetailPrice
        {
            get => _retailPrice;
            internal set
            {
                _retailPrice = value;
                PriceTag.text = _retailPrice <= 99
                    ? Utils.PaddingNum2Digit(_retailPrice)
                    : Utils.PaddingNum3Digit(_retailPrice);
            }
        }

        public SimpleLEDArray TierLEDs;

        public TextMeshPro TierTag;
        public TextMeshPro CostTag;
        public TextMeshPro PriceTag;
        public TextMeshPro DiscountedPriceTag;

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

        public Transform ShopBackPlane;
        public Transform ShopDiscountRoot;

        public bool IsActiveThermoFieldUnit;

        private bool _hasDiscount = false;

        private bool HasDiscount
        {
            set
            {
                _hasDiscount = ShopID != -1 && value;
                ShopDiscountRoot.gameObject.SetActive(_hasDiscount);
            }
            get => _hasDiscount;
        }

        public int ShopID { get; private set; } = -1;

        public void UnsetShop()
        {
            HasDiscount = false;
            SetPendingBuying = false;
            ShopBackPlane.gameObject.SetActive(false);
            ShopID = -1;
        }


        public void SetShop(int shopID, int retailPrice, int discountRate, int _cost, bool? showQuad)
        {
            ShopID = shopID;
            if (retailPrice != -1)
            {
                RetailPrice = retailPrice;
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

            //这个discountRate写以百分比的数据，比如八折就是写20。（-20%）
            if (discountRate > 0)
            {
                discountRate = Mathf.Min(discountRate, 99);
                HasDiscount = true;
                var discountedPrice = Mathf.FloorToInt(retailPrice * (1.0f - discountRate * 0.01f));
                DiscountedPriceTag.text = "<color=#00A62E>" + Utils.PaddingNum2Digit(discountedPrice) + "</color>";
            }
            else
            {
                HasDiscount = false;
            }
        }

        public bool SetPendingBuying
        {
            set
            {
                if (value)
                {
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

        [ReadOnly] [ShowInInspector] public SignalType UnitSignal { get; private set; }
        [ReadOnly] [ShowInInspector] public HardwareType UnitHardware { get; private set; }
        public bool IsCore => UnitHardware == HardwareType.Core;
        public Dictionary<RotationDirection, SideType> UnitSides { get; private set; }

        private RotationDirection _unitRotation
        {
            get => ApparentRotationDirection;
            set => ApparentRotationDirection = value;
        }

        private Transform _rootTransform => transform.parent;
        private Material _coreMat;

        private MeshRenderer _coreMeshRenderer;
        public Dictionary<RotationDirection, Connector> ConnectorLocalDir;
        public Connector _localNorthConnector;
        public Connector _localEastConnector;
        public Connector _localWestConnector;
        public Connector _localSouthConnector;

        private Transform _sideRootTransform;
        private Transform _coreTransform;

        public Mesh DtoDSideMesh;
        public Mesh DtoSSideMesh;

        public Board GameBoard;

        public void SetupStationUnit()
        {
            Immovable = true;
            StationUnit = true;
        }

        //Rotation使用的世界方向的。
        public Dictionary<RotationDirection, ConnectionData> WorldNeighboringData { protected set; get; }

        public bool AnyConnection
        {
            get
            {
                return WorldNeighboringData.Where(keyValuePair => keyValuePair.Value.HasConnector)
                    .Any(keyValuePair => keyValuePair.Value.Connected);
            }
        }

        private readonly RotationDirection[] RotationList =
        {
            RotationDirection.East,
            RotationDirection.North,
            RotationDirection.South,
            RotationDirection.West
        };

        [HideInInspector] public Vector2Int LastNetworkPos = Vector2Int.zero;

        private void InitConnector(Connector connector, SideType sideType)
        {
            connector.UseScrVersion = (UnitHardware == HardwareType.Core);
            connector.gameObject.SetActive(sideType == SideType.Connection);
            connector.Signal_A_Val = 0;
            connector.Signal_B_Val = 0;
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

        private void InitUnitMeshByCore(SignalType signal, HardwareType genre)
        {
            UnitHardware = genre;
            var connectorMasterNodeName = "";
            var coreMeshNodeName = "";

            if (UnitHardware == HardwareType.Core)
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

            TierLEDs.gameObject.SetActive(!IsCore);
        }

        public void SetCoreEmissive(Color color)
        {
            _coreMeshRenderer.material.EnableKeyword("_EMISSION"); //还是不懂，为什么每次设置前得Enable一下。
            _coreMeshRenderer.material.SetColor("_EmissionColor", color);
        }

        public void UpdateUnitTier(int tier)
        {
            Tier = tier;
        }

        public void InitUnit(SignalType signal, HardwareType genre, SideType[] sides,
            int tier, Board gameBoard = null)
        {
            Debug.Assert(sides.Length == 4);
            InitUnit(signal, genre, sides[0], sides[1], sides[2], sides[3],
                tier, gameBoard);
        }

        public static SignalType PlayingSignalA;
        public static SignalType PlayingSignalB;


        public void InitUnit(SignalType signal, HardwareType genre,
            SideType lNSide, SideType lSSide, SideType lWSide, SideType lESide,
            int tier, Board gameBoard = null)
        {
            UnitSignal = signal;
            InitUnitMeshByCore(signal, genre);

            UnitSides.Add(RotationDirection.North, lNSide);
            UnitSides.Add(RotationDirection.South, lSSide);
            UnitSides.Add(RotationDirection.West, lWSide);
            UnitSides.Add(RotationDirection.East, lESide);

            _coreMeshRenderer.material = SignalMasterMgr.Instance.GetMatByUnitType(signal, genre);
            //Debug.Assert(_coreMeshRenderer.material);

            InitConnector(_localNorthConnector, lNSide);
            InitConnector(_localEastConnector, lESide);
            InitConnector(_localWestConnector, lWSide);
            InitConnector(_localSouthConnector, lSSide);

            GameBoard = gameBoard;

            Tier = tier;

            UpdateSideMesh();

            UnitActivationLEDMat.material.color = UnitHardware == HardwareType.Core
                ? UnitActivationLEDMat_Colors[1]
                : UnitActivationLEDMat_Colors[0];

            if (SignalCore == null)
            {
                var signalType = signal;
                var signalCoreType = SignalMasterMgr.Instance.SignalUnitCore(signalType);
                SignalCore = gameObject.AddComponent(signalCoreType) as UnitSignalCoreBase;
                // ReSharper disable once PossibleNullReferenceException
                SignalCore.Owner = this;
            }
        }

        protected void Awake()
        {
            ShopID = -1;
            CurrentBoardPosition = new Vector2Int(0, 0);
            UnitSides = new Dictionary<RotationDirection, SideType>();
            _unitRotation = RotationDirection.North;

            Immovable = false;
            ShopBackPlane.gameObject.SetActive(false);
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
            _rootTransform.rotation = RotationToQuaternion(_unitRotation);
        }



        [Obsolete]
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

        // 获得相连的所有Unit
        public List<Unit> GetConnectedOtherUnit => WorldNeighboringData.Values.Where(data => data.Connected)
            .Select(data => data.OtherUnit).ToList();

        private bool SideFilter(RotationDirection dir)
        {
            return dir == RotationDirection.West || dir == RotationDirection.South;
        }

        private void ResetConnector(RotationDirection dir)
        {
            WorldNeighboringData.TryGetValue(dir, out ConnectionData data);
            if (!data.HasConnector) return;
            ConnectorLocalDir.TryGetValue(Utils.RotateDirectionBeforeRotation(dir, _unitRotation),
                out Connector Connector);
            if (Connector == null) return;
            Connector.Signal_A_Val = 0;
            Connector.Signal_B_Val = 0;
            Connector.Hided = true;
        }

        //对，仔细想下，如果需要搞，这里的东西也需要抽象出来。
        //包括Connector本身的设计也需要抽象。
        //现在Connector不能是：有一个network灯、一个hardware灯。
        //而需要是：有两个通用的灯。
        //其实为了添加新的Unit，除了Asset部分、所有现有矩阵和扫描相关的代码都得抽象掉。

        //这块儿的代码还可以进行抽象出来、理论上可以：
        //这里的代码需要遍历所有的信号类型（因为所有接口应该可以传送所有信号）。
        //可以把所有的LED具体显示逻辑（661~2行）可以挂在UnitLogicBase上面变成静态函数。
        //利用lib系统的设计，这里需要能够遍历UnitLogicBase中所有对应静态函数来计分。
        //当然、LED上面能不能显示那么多是另一回事、抽象框架设计出来后，都会好一些。

        private void ConnectorSignalMux(ref Connector connector, SignalType type, int val, bool ignoreVal = false)
        {
            //这个函数是将信号配置到具体的接口LED上面、这个东西只能手动配置；相当于信号和硬件儿的映射。
            //这个相当于硬件软件间的一个接口、这个具体放在哪就还好。
            if (type == PlayingSignalA)
                connector.Signal_A_Val = ignoreVal ? 0 : val;
            else if (type == PlayingSignalB)
                connector.Signal_B_Val = ignoreVal ? 0 : val;
            else
                Debug.LogWarning(type + " of signal is not processed.");
        }

        private void SetConnector(RotationDirection crtDir, bool ignoreVal = false)
        {
            WorldNeighboringData.TryGetValue(crtDir, out ConnectionData data);
            ConnectorLocalDir.TryGetValue(Utils.RotateDirectionBeforeRotation(crtDir, _unitRotation), out var Connector);
            Connector.Connected = data.Connected;

            var otherUnit = data.OtherUnit;

            var shouldShow = false;

            var signal = new[] {SignalType.Thermo};
            
            foreach (var signalType in SignalMasterMgr.Instance.SignalLib)
            {
                var tmpShouldShow = SignalMasterMgr.Instance.ShowSignal(signalType, crtDir, this, otherUnit);
                var tmpVal = SignalMasterMgr.Instance.SignalVal(signalType, crtDir, this, otherUnit);
                ConnectorSignalMux(ref Connector, signalType, tmpVal, ignoreVal);
                shouldShow |= tmpShouldShow;
            }

            Connector.Hided = !shouldShow;
        }

        private bool FilterConnector(RotationDirection dir)
        {
            WorldNeighboringData.TryGetValue(dir, out ConnectionData data);
            return data.HasConnector && data.Connected && SideFilter(dir) && data.OtherUnit != null;
        }
        
        #region Blink

        //这里需要完全重做、但是优先级没有多高。

        private readonly float BlinkDuration = 0.075f;

        private RotationDirection nextBlinkDir;

        private IEnumerator NextBlinkGap(float duration)
        {
            yield return new WaitForSeconds(duration);
            NextBlink(nextBlinkDir);
        }

        internal void SimpleBlink(RotationDirection requiredDirection)
        {
            if (requiredDirection == RotationDirection.West || requiredDirection == RotationDirection.South)
            {
                var localRotation = Utils.RotateDirectionBeforeRotation(requiredDirection, _unitRotation);
                ConnectorLocalDir.TryGetValue(localRotation, out Connector Connector);
                if (Connector == null) return;
                Connector.Blink(BlinkDuration);
                return;
            }

            throw new ArgumentException();
        }

        public void Blink(RotationDirection? fromDirection)
        {
            //Server那里用不用迪公帮忙把路径捋出来？目前看不用
            if (UnitSignal == SignalType.Matrix && UnitHardware == HardwareType.Field)
            {
                if (SignalCore.SignalFromDir == RotationDirection.West ||
                    SignalCore.SignalFromDir == RotationDirection.South)
                {
                    var localRotation = Utils.RotateDirectionBeforeRotation(SignalCore.SignalFromDir, _unitRotation);
                    ConnectorLocalDir.TryGetValue(localRotation, out Connector Connector);
                    if (Connector != null) Connector.Blink(BlinkDuration);
                }
                else
                {
                    var nextUnit = GameBoard.GetUnitWithPosAndDir(CurrentBoardPosition, SignalCore.SignalFromDir);
                    if (nextUnit != null) nextUnit.SimpleBlink(Utils.GetInvertDirection(SignalCore.SignalFromDir));
                }

                StartCoroutine(NextBlinkGap(BlinkDuration));
            }
            else if (UnitSignal == SignalType.Scan && UnitHardware == HardwareType.Field)
            {
                foreach (var currentSideDirection in RotationList)
                {
                    if (!fromDirection.HasValue || (currentSideDirection != fromDirection.Value))
                    {
                        var localRotation =
                            Utils.RotateDirectionBeforeRotation(currentSideDirection, _unitRotation);
                        ConnectorLocalDir.TryGetValue(localRotation, out Connector Connector);

                        if (Connector == null) continue;

                        WorldNeighboringData.TryGetValue(currentSideDirection, out ConnectionData data);
                        var otherUnit = data.OtherUnit;

                        if (otherUnit == null) continue;

                        var showNetLed = SignalCore.InServerGrid && otherUnit.SignalCore.InServerGrid;
                        showNetLed &=
                            Math.Abs(SignalCore.ScanSignalPathDepth - otherUnit.SignalCore.ScanSignalPathDepth) <= 1;

                        if (showNetLed)
                        {
                            nextBlinkDir = currentSideDirection;
                            if (nextBlinkDir == RotationDirection.West || nextBlinkDir == RotationDirection.South)
                            {
                                Connector.Blink(BlinkDuration);
                            }
                            else
                            {
                                var nextUnit = GameBoard.GetUnitWithPosAndDir(CurrentBoardPosition, nextBlinkDir);
                                if (nextUnit != null) nextUnit.SimpleBlink(Utils.GetInvertDirection(nextBlinkDir));
                            }

                            StartCoroutine(NextBlinkGap(BlinkDuration));
                            break;
                        }
                    }
                }
            }
        }

        private void NextBlink(RotationDirection? nextDirection)
        {
            if (UnitSignal == SignalType.Matrix && UnitHardware == HardwareType.Field)
            {
                var nextUnit = GameBoard.GetUnitWithPosAndDir(CurrentBoardPosition, SignalCore.SignalFromDir);
                if (nextUnit != null) nextUnit.Blink(null);
            }
            else if ((UnitSignal == SignalType.Scan && UnitHardware == HardwareType.Field) && nextDirection.HasValue)
            {
                var nextUnit = GameBoard.GetUnitWithPosAndDir(CurrentBoardPosition, nextDirection.Value);
                if (nextUnit != null) nextUnit.Blink(Utils.GetInvertDirection(nextDirection.Value));
            }
        }

        #endregion

        public override int GetHashCode()
        {
            var A = CurrentBoardPosition.GetHashCode();
            var B = _unitRotation.GetHashCode();
            var C = SignalCore.GetHashCode();
            var D = UnitHardware.GetHashCode();
            var E = UnitSides.GetHashCode();
            var F = _tier.GetHashCode();
            return A ^ B ^ C ^ D ^ E ^ F;
        }

        public void Clicked()
        {
            Debug.Log("I'm clicked");
            //throw new NotImplementedException();
        }

        public SignalPath FindSignalPath_Iter(SignalType targetSignalType)
        {
            var res = new SignalPath {this};
            var unit = SignalCore.SignalDataPackList[targetSignalType].UpstreamUnit;
            if (unit != null)
            {
                //这样做出来的路径是终叶到Core的。
                res.AddRange(unit.FindSignalPath_Iter(targetSignalType));
            }
            return res;
        }

        public bool NotBeingSignallyReferenced(SignalType targetSignalType)
        {
            var otherUnits = GetConnectedOtherUnit;
            if (otherUnits.Count==0) return false;
            return otherUnits.All(u => u.SignalDataVal(targetSignalType).UpstreamUnit != this);
        }
    }
}