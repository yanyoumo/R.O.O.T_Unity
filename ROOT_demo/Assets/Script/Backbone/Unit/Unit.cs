using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ROOT.Consts;
using ROOT.SetupAsset;
using ROOT.Signal;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;

namespace ROOT
{
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

    public partial class Unit : MoveableBase, IClickable
    {
        public TextMeshPro TierTag;
        public TextMeshPro CostTag;
        public TextMeshPro PriceTag;
        public TextMeshPro DiscountedPriceTag;
        public TextMeshPro BillBoardText;
        public TextMeshPro ShopIconSprite;

        public MeshRenderer UnitActivationLEDMat;
        public MeshRenderer BackQuadRenderer;
        public MeshRenderer AdditionalClampMesh;
        public Material BuyingMat;
        public Material ImmovableMat;
        public SpriteRenderer CoreSpriteRenderer;
        public SpriteRenderer FieldSpriteRenderer;

        public Transform ShopBackPlane;
        public Transform ShopDiscountRoot;
        public Transform ThermoRangeIndicatorRoot;
        public Transform NonrotatingRoot;

        public SpriteRenderer ThermoRangeIndicatorRenderer;
        public SpriteMask NorthThermoRangeIndicatorMask;
        public SpriteMask SouthThermoRangeIndicatorMask;
        public SpriteMask EastThermoRangeIndicatorMask;
        public SpriteMask WestThermoRangeIndicatorMask;

        public UnitNeighbouring UnitNeighbouringRendererRoot;
        
        public Connector _localNorthConnector;
        public Connector _localEastConnector;
        public Connector _localWestConnector;
        public Connector _localSouthConnector;
        public List<Unit> GetConnectedOtherUnit => WorldNeighboringData.Values.Where(data => data.Connected).Select(data => data.OtherUnit).ToList();

        public Mesh DtoDSideMesh;
        public Mesh DtoSSideMesh;

        public SimpleLEDArray TierLEDs;

        [ReadOnly]public Board GameBoard;
        
        [ReadOnly]public UnitSignalCoreBase SignalCore;
        public Dictionary<SignalType, SignalData> SignalDataPackList => SignalCore.SignalDataPackList;
        
        public int Tier
        {
            get => _tier;
            private set
            {
                _tier = value;
                TierTag.text = Common.Utils.PaddingNum2Digit(_tier);
                TierLEDs.Val = _tier;
            }
        }

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
        public bool StationUnit { get; private set; }
        public int ShopID { get; private set; } = -1;
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
                    SetClampMesh();
                }
            }
        }

        [ReadOnly] public UnitTag UnitTag = UnitTag.NoTag;
        [ReadOnly] [ShowInInspector] public SignalType UnitSignal { get; private set; }
        [ReadOnly] [ShowInInspector] public HardwareType UnitHardware { get; private set; }
        public Dictionary<RotationDirection, SideType> UnitSides { get; private set; }
        
        private Color signalColorA => ColorLibManager.Instance.GetColorBySignalType(Board.PlayingSignalA);
        private Color signalColorB => ColorLibManager.Instance.GetColorBySignalType(Board.PlayingSignalB);
        
        private RotationDirection _unitRotation
        {
            get => CurrentRotationDirection;
            set => CurrentRotationDirection = value;
        }
        private int _cost = 0;
        private int _tier = 0;
        private int _hardwarePrice = -1;
        private int _retailPrice = -1;
        private int RetailPrice
        {
            get => _retailPrice;
            set
            {
                _retailPrice = value;
                PriceTag.text = _retailPrice <= 99
                    ? Common.Utils.PaddingNum2Digit(_retailPrice)
                    : Common.Utils.PaddingNum3Digit(_retailPrice);
            }
        }
        private bool _hasDiscount = false;
        private MeshRenderer _coreMeshRenderer;
        public override Transform AnimatingRoot => _rootTransform;
        private Transform _rootTransform => transform.parent;
        private Material _coreMat;
        private Transform _sideRootTransform;
        private Transform _coreTransform;
        private Color[] UnitActivationLEDMat_Colors => SignalMasterMgr.Instance.UnitActivationLED_Colors;
        private Dictionary<RotationDirection, ConnectionData> WorldNeighboringData { set; get; }//Rotation使用的世界方向的。
        private Dictionary<RotationDirection, Connector> ConnectorLocalDir;
        private RotationDirection WorldDir(RotationDirection direction) => Common.Utils.RotateDirectionBeforeRotation(direction, _unitRotation);
        private bool HasDiscount
        {
            set
            {
                _hasDiscount = ShopID != -1 && value;
                ShopDiscountRoot.gameObject.SetActive(_hasDiscount);
            }
            get => _hasDiscount;
        }

        public SpriteMask FindThermoRangeIndicatorByDirection(RotationDirection dir)
        {
            switch (dir)
            {
                case RotationDirection.North:
                    return NorthThermoRangeIndicatorMask;
                case RotationDirection.East:
                    return EastThermoRangeIndicatorMask;
                case RotationDirection.West:
                    return WestThermoRangeIndicatorMask;
                default:
                    return SouthThermoRangeIndicatorMask;
            }
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
                this._cost = _cost;
                CostTag.text = Common.Utils.PaddingNum2Digit(this._cost);
            }

            //这个discountRate写以百分比的数据，比如八折就是写20。（-20%）
            if (discountRate > 0)
            {
                discountRate = Mathf.Min(discountRate, 99);
                HasDiscount = true;
                var discountedPrice = Mathf.FloorToInt(retailPrice * (1.0f - discountRate * 0.01f));
                DiscountedPriceTag.text = "<color=#00A62E>" + Common.Utils.PaddingNum2Digit(discountedPrice) + "</color>";
            }
            else
            {
                HasDiscount = false;
            }

            ShopIconSprite.text = "<sprite=" + (ShopID + 1) % 10 + ">";
        }
        public void UnsetShop()
        {
            HasDiscount = false;
            SetPendingBuying = false;
            ShopBackPlane.gameObject.SetActive(false);
            ShopID = -1;
        }
        public void SetupStationUnit()
        {
            Immovable = true;
            StationUnit = true;
        }
       
        public SideType GetLocalSpaceUnitSide(RotationDirection localDirection)
        {
            UnitSides.TryGetValue(_unitRotation, out var res);
            return res;
        }
        public SideType GetWorldSpaceUnitSide(RotationDirection worldDirection)
        {
            var desiredLocalSideDirection = Common.Utils.RotateDirectionBeforeRotation(worldDirection, _unitRotation);
            UnitSides.TryGetValue(desiredLocalSideDirection, out var res);
            return res;
        }

        public SignalPath FindSignalPath_Iter(SignalType targetSignalType)
        {
            var res = new SignalPath { this };
            var unit = SignalCore.SignalDataPackList[targetSignalType].UpstreamUnit;
            if (unit != null)
            {
                //这样做出来的路径是终叶到Core的。
                res.AddRange(unit.FindSignalPath_Iter(targetSignalType));
            }
            return res;
        }
        public bool CheckNotBeingSignallyReferenced(SignalType targetSignalType)
        {
            var otherUnits = GetConnectedOtherUnit;
            if (otherUnits.Count == 0) return false;
            return otherUnits.All(u => u.GetSignalDataVal(targetSignalType).UpstreamUnit != this);
        }
        public bool CheckType(SignalType signalType, HardwareType hardwareType)
        {
            return UnitSignal == signalType && UnitHardware == hardwareType;
        }
        public bool CheckAnyConnection
        {
            get
            {
                return WorldNeighboringData.Where(keyValuePair => keyValuePair.Value.HasConnector)
                    .Any(keyValuePair => keyValuePair.Value.Connected);
            }
        }

        private void SetClampMesh()
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
        private SignalData GetSignalDataVal(SignalType signalType)
        {
            return SignalDataPackList[signalType];
        }
        
        private void InitConnector(Connector connector, SideType sideType,Color SignalColorA,Color SignalColorB)
        {
            connector.UseScrVersion = (UnitHardware == HardwareType.Core);
            connector.gameObject.SetActive(sideType == SideType.Connection);
            connector.Signal_A_Val = 0;
            connector.Signal_B_Val = 0;
            connector.Signal_A_Col = SignalColorA;
            connector.Signal_B_Col = SignalColorB;
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

            TierLEDs.gameObject.SetActive(UnitHardware == HardwareType.Field);
        }
        public void UpdateUnitTier(int tier)
        {
            Tier = tier;
        }
        public override void UpdateTransform(Vector3 pos)
        {
            _rootTransform.position = pos;
        }

        public void UnitRotateCw()
        {
            NextRotationDirection = Common.Utils.GetCWDirection(_unitRotation);
        }

        public void UpdateWorldRotationTransform()
        {
            _rootTransform.rotation = Common.Utils.RotationToQuaternion(_unitRotation);
        }

        private bool SideFilter(RotationDirection dir)
        {
            return dir == RotationDirection.West || dir == RotationDirection.South;
        }
        private void ResetConnector(RotationDirection dir)
        {
            WorldNeighboringData.TryGetValue(dir, out ConnectionData data);
            if (!data.HasConnector) return;
            ConnectorLocalDir.TryGetValue(Common.Utils.RotateDirectionBeforeRotation(dir, _unitRotation), out Connector Connector);
            if (Connector == null) return;
            Connector.Signal_A_Val = 0;
            Connector.Signal_B_Val = 0;
            Connector.Hided = true;
        }
        private void ConnectorSignalMux(ref Connector connector, SignalType type, int val, bool ignoreVal = false)
        {
            //这个函数是将信号配置到具体的接口LED上面、这个东西只能手动配置；相当于信号和硬件儿的映射。
            //这个相当于硬件软件间的一个接口、这个具体放在哪就还好。
            if (type == Board.PlayingSignalA)
                connector.Signal_A_Val = ignoreVal ? 0 : val;
            else if (type ==  Board.PlayingSignalB)
                connector.Signal_B_Val = ignoreVal ? 0 : val;
            else
                Debug.LogWarning(type + " of signal is not processed.");
        }
        private void SetConnector(RotationDirection crtDir, bool ignoreVal = false)
        {
            WorldNeighboringData.TryGetValue(crtDir, out ConnectionData data);
            ConnectorLocalDir.TryGetValue(Common.Utils.RotateDirectionBeforeRotation(crtDir, _unitRotation), out var Connector);
            Connector.Connected = data.Connected;

            var otherUnit = data.OtherUnit;

            var shouldShow = false;

            var signal = new[] { SignalType.Thermo };

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
        
        private IEnumerator blinkUpInterval(Unit otherUnit)
        {
            yield return new WaitForSeconds(StaticNumericData.BlinkTransferInterval);
            otherUnit.Blink();
        }
        //TODO 这个玩意儿技术上搞定了，调一下Blink就能正常的闪到核心，但是还有一些周边问题要继续处理：
        //  1、常规信号和Blink流程的切换和管理流程。
        //  2、Blink本身的颜色调整。DONE
        public void Blink()
        {
            var upStreamUnit = SignalDataPackList[UnitSignal].UpstreamUnit;
            if (upStreamUnit == null) return;
            var dir = WorldNeighboringData.Where(v => v.Value.OtherUnit == upStreamUnit).Select(v => v.Key).First();
            var cctor = FilterConnector(dir) ? ConnectorLocalDir[WorldDir(dir)] : upStreamUnit.ConnectorLocalDir[WorldDir(Common.Utils.GetInvertDirection(dir))];
            cctor.Blink(StaticNumericData.BlinkSingleDuration, !FilterConnector(dir));
            StartCoroutine(blinkUpInterval(upStreamUnit));
        }
        
        public void InitUnit(SignalType signal, HardwareType genre, SideType[] sides, int tier, Board gameBoard = null, UnitTag unitTag = UnitTag.NoTag)
        {
            Debug.Assert(sides.Length == 4);
            InitUnit(signal, genre, sides[0], sides[1], sides[2], sides[3], tier, gameBoard, unitTag);
        }

        private void InitUnit(SignalType signal, HardwareType genre,
            SideType lNSide, SideType lSSide, SideType lWSide, SideType lESide,
            int tier, Board gameBoard, UnitTag _unitTag)
        {
            UnitTag = _unitTag;
            UnitSignal = signal;
            InitUnitMeshByCore(signal, genre);

            UnitSides.Add(RotationDirection.North, lNSide);
            UnitSides.Add(RotationDirection.South, lSSide);
            UnitSides.Add(RotationDirection.West, lWSide);
            UnitSides.Add(RotationDirection.East, lESide);

            //_coreMeshRenderer.material = SignalMasterMgr.Instance.GetMatByUnitType(signal, genre);
            CoreSpriteRenderer.sprite = SignalMasterMgr.Instance.GetSpriteIconByUnitType(signal, genre);
            FieldSpriteRenderer.sprite = SignalMasterMgr.Instance.GetSpriteIconByUnitType(signal, genre);
            CoreSpriteRenderer.enabled = genre == HardwareType.Core;
            FieldSpriteRenderer.enabled = genre == HardwareType.Field;

            InitConnector(_localNorthConnector, lNSide, signalColorA, signalColorB);
            InitConnector(_localEastConnector, lESide, signalColorA, signalColorB);
            InitConnector(_localWestConnector, lWSide, signalColorA, signalColorB);
            InitConnector(_localSouthConnector, lSSide, signalColorA, signalColorB);

            GameBoard = gameBoard;

            Tier = tier;

            UpdateSideMesh();

            if (UnitHardware == HardwareType.Core)
            {
                UnitActivationLEDMat.enabled = false;
            }
            else
            {
                UnitActivationLEDMat.material.color = UnitActivationLEDMat_Colors[(int) UnitActivationLEDColor.Deactivated];
            }

            if (SignalCore == null)
            {
                var signalType = signal;
                var signalCoreType = SignalMasterMgr.Instance.SignalUnitCore(signalType);
                SignalCore = gameObject.AddComponent(signalCoreType) as UnitSignalCoreBase;
                // ReSharper disable once PossibleNullReferenceException
                SignalCore.Owner = this;
                SignalCore.SignalCoreInit();
            }
        }

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

        private void Update()
        {
            //RISK 这个流程太费了，但是这个玩意儿一时半会儿改不了。
            if (NonrotatingRoot.rotation != Quaternion.identity)
            {
                NonrotatingRoot.rotation = Quaternion.identity;
            }
        }

        public override void PingPongRotationDirection()
        {
            CurrentRotationDirection = NextRotationDirection;
        }
    }
}