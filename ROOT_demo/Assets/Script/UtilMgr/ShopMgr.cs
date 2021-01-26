using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;


namespace ROOT
{
    //using CoreWeight = Dictionary<CoreType, float>;
    using UnitTypeCombo=Tuple<SignalType,CoreGenre>;

    /*public partial class BoardDataCollector : MonoBehaviour
    {
        internal int NthUnitCost(int N)
        {
            return 0;
            Dictionary<int, int> tokenizedVal = new Dictionary<int, int>()
            {
                {0, 0},
                {1, 1},
                {5, 1},
                {10, 1},
                {18, 2},
                {24, 3},
                {36, 6},
            };

            if (!tokenizedVal.Keys.All(i => i <= N))
            {

                var minKey = tokenizedVal.Keys.Where(i => i <= N).Max();
                var maxKey = tokenizedVal.Keys.Where(i => i >= N).Min();
                if (minKey == maxKey)
                {
                    return tokenizedVal[minKey];
                }
                else
                {
                    var minVal = tokenizedVal[minKey];
                    var maxVal = tokenizedVal[maxKey];
                    var normalizedCount = (N - minVal) / (float)(maxVal - minVal);
                    return Mathf.RoundToInt(Mathf.Lerp(minVal, maxVal, normalizedCount));
                }
            }
            else
            {
                return tokenizedVal[tokenizedVal.Keys.Max()];
            }
        }
    }*/

    public interface IAnimatableShop
    {
        void ShopPreAnimationUpdate();
        void ShopUpdateAnimation(float lerp);
        void ShopPostAnimationUpdate();
    }

    public interface IRequirableShop
    {
        void SetRequire(int dur, int normal, int network);
        void ResetRequire();
    }

    
    public abstract class ShopBase : MonoBehaviour
    {
        //目前这两个是写死的；之后商店这个种类也改成可配置的；DONE
        protected SignalType SignalTypeA = SignalType.Matrix;
        protected SignalType SignalTypeB = SignalType.Scan;

        protected ((UnitTypeCombo, UnitTypeCombo), (UnitTypeCombo, UnitTypeCombo)) UnitType => (UnitTypeA,UnitTypeB);
        private (UnitTypeCombo, UnitTypeCombo) UnitTypeA=> (new UnitTypeCombo(SignalTypeA, CoreGenre.Core), new UnitTypeCombo(SignalTypeA, CoreGenre.Field));
        private (UnitTypeCombo, UnitTypeCombo) UnitTypeB => (new UnitTypeCombo(SignalTypeB, CoreGenre.Core), new UnitTypeCombo(SignalTypeB, CoreGenre.Field));

        public GameObject UnitTemplate;
        protected GameAssets currentLevelAsset;
        public Board GameBoard;
        public GameStateMgr CurrentGameStateMgr;
        public List<SignalType> excludedTypes = new List<SignalType>();

        public abstract void ShopInit(GameAssets _currentLevelAsset);
        public abstract void ShopStart();

        protected bool _shopOpening;
        public abstract bool ShopOpening { get; protected set; }
        public abstract void OpenShop(bool Opening, int discount);


        public abstract bool BuyToRandom(int idx);

        protected int TotalCount = 0;

        protected GameObject[] _items;

        [CanBeNull]
        protected Unit[] _itemUnit =>
            _items.Select(unit => unit ? unit.GetComponentInChildren<Unit>() : null).ToArray();

        protected float[] _hardwarePrices;

        protected GameObject InitUnitShopCore(SignalType signal,CoreGenre genre, SideType[] sides, int ID, int _cost, int tier)
        {
            var go = Instantiate(UnitTemplate);
            go.name = "Unit_" + Hash128.Compute(Utils.LastRandom.ToString());
            var unit = go.GetComponentInChildren<Unit>();
            unit.InitPosWithAnimation(Vector2Int.zero);
            unit.InitUnit(signal,genre, sides, tier);
            return go;
        }

        protected int NthUnitCost(int N)
        {
            return 0;
            Dictionary<int, int> tokenizedVal = new Dictionary<int, int>()
            {
                {0, 0},
                {1, 1},
                {5, 1},
                {10, 1},
                {18, 2},
                {24, 3},
                {36, 6},
            };

            if (!tokenizedVal.Keys.All(i => i <= N))
            {

                var minKey = tokenizedVal.Keys.Where(i => i <= N).Max();
                var maxKey = tokenizedVal.Keys.Where(i => i >= N).Min();
                if (minKey == maxKey)
                {
                    return tokenizedVal[minKey];
                }
                else
                {
                    var minVal = tokenizedVal[minKey];
                    var maxVal = tokenizedVal[maxKey];
                    var normalizedCount = (N - minVal) / (float) (maxVal - minVal);
                    return Mathf.RoundToInt(Mathf.Lerp(minVal, maxVal, normalizedCount));
                }
            }
            else
            {
                return tokenizedVal[tokenizedVal.Keys.Max()];
            }
        }

        /// <summary>
        /// 从Tier获取单元各种数据的倍率。
        /// </summary>
        /// <param name="Tier">位于哪个Tier</param>
        /// <returns>依次为（分数、购买价格、Cost）的float Tuple</returns>
        public static Tuple<float, float, float> TierMultiplier(int Tier)
        {
            //目前对Tier进行设定：
            //先确定需要由Tier影响的内容：
            //分数、购买价格、Cost。
            var SignalMultipler = (float) Tier;
            var PriceMultipler = 1.0f + 1.55f * (Tier - 1);
            var CostMultipler = 1.0f + 0.5f * Tier;
            return new Tuple<float, float, float>(SignalMultipler, PriceMultipler, CostMultipler);
        }

        protected int TierProgress(float gameProgress)
        {
            var fluctuationRate = 0.25f;
            var fluctuation = 1.0f;
            var baseTier = Mathf.Lerp(1, 6, gameProgress);
            if (Random.value <= fluctuationRate)
            {
                if (Random.value <= 0.5)
                {
                    baseTier += fluctuation;
                }
                else
                {
                    baseTier -= fluctuation;
                }
            }

            return Mathf.Clamp(Mathf.RoundToInt(baseTier), 1, 5);
        }

        protected Dictionary<SideType, float> _priceBySide { private set; get; }

        public void InitPrice()
        {
            _priceBySide = new Dictionary<SideType, float>()
            {
                {SideType.NoConnection, 0.0f},
                {SideType.Connection, 2.0f},
            };
        }

        protected float TryGetPrice(SideType side)
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

        #region UnitSideWeight

        /*protected Dictionary<SideType, float> _defaultSideWeight { private set; get; }
        protected Dictionary<SideType, float> _processorSideWeight { private set; get; }
        protected Dictionary<SideType, float> _serverSideWeight { private set; get; }
        protected Dictionary<SideType, float> _hddSideWeight { private set; get; }
        protected Dictionary<SideType, float> _netCableSideWeight { private set; get; }

        protected Dictionary<CoreType, float> _defaultCoreWeight { private set; get; }
        protected Dictionary<CoreType, float> _noServerCoreWeight { private set; get; }
        protected Dictionary<CoreType, float> _noProcessorCoreWeight { private set; get; }
        protected Dictionary<CoreType, float> _nandServerProcessorCoreWeight { private set; get; }
        protected Dictionary<CoreType, Dictionary<SideType, float>> _sideWeightLib { private set; get; }*/

        private SignalType[] TotalSignals => SignalMasterMgr.Instance.SignalLib;

        /*private void AppendSignalWeight(SignalType type, ref CoreWeight dict)
        {
            //现在没有根据现有单元动态调整的逻辑。
            SignalMasterMgr.Instance.UnitTypeFromSignal(type,out var coreUnit,out var fieldUnit);
            var coreUnitAsset=SignalMasterMgr.Instance.GetUnitAssetByUnitType(coreUnit);
            var fieldUnitAsset=SignalMasterMgr.Instance.GetUnitAssetByUnitType(fieldUnit);
            dict.Add(coreUnit, coreUnitAsset.BaseRate);
            dict.Add(fieldUnit, fieldUnitAsset.BaseRate);
        }*/

        protected UnitTypeCombo GenerateRandomCore()
        {
            //TODO 如果真这么写，随即生成流程还真得大改；但是早晚要改。
            return new UnitTypeCombo(SignalType.Scan, CoreGenre.Field);
            /*var lib = new CoreWeight();
            foreach (var totalSignal in TotalSignals)
            {
                AppendSignalWeight(totalSignal, ref lib);
            }

            if (excludedTypes.Count > 0)
            {
                foreach (var excludedType in excludedTypes)
                {
                    if (lib.TryGetValue(excludedType, out float value))
                    {
                        lib.Remove(excludedType);
                    }
                }
            }

            NormalizeDicVal(ref lib);
            return Utils.GenerateWeightedRandom(lib);*/
        }

        /*protected SideType[] GenerateRandomSideArray(UnitTypeCombo combo)
        {
            var unitAsset = SignalMasterMgr.Instance.GetUnitAssetByUnitType(core);
            var ratio = unitAsset.AdditionalPortRate;
            var lib = new Dictionary<SideType, float>
            {
                [SideType.NoConnection] = 100 - ratio,
                [SideType.Connection] = ratio
            };

            NormalizeDicVal(ref lib);

            SideType[] res = new SideType[4];
            const int cutoff = 1000;
            int cutoffCounter = 0;
            do
            {
                res[0] = Utils.GenerateWeightedRandom(lib);
                res[1] = Utils.GenerateWeightedRandom(lib);
                res[2] = Utils.GenerateWeightedRandom(lib);
                res[3] = Utils.GenerateWeightedRandom(lib);
                cutoffCounter++;
                if (cutoffCounter >= cutoff)
                {
                    Debug.Assert(false, "Can't generate good Unit");
                    break;
                }
            } while (!CheckConformKeySide(core, res));

            return res;
        }*/


        //KeySide minCount
        /*protected Dictionary<CoreType, Tuple<SideType, int>> _keySideLib { private set; get; }

        //按照某套概率生成一个随机单元的需求肯定有、但是现在的优先级没有那么高。
        [Obsolete]
        public void InitSideCoreWeight()
        {
            _keySideLib = new Dictionary<CoreType, Tuple<SideType, int>>()
            {
                {CoreType.PCB, new Tuple<SideType, int>(SideType.NoConnection, 4)},
                {CoreType.ScanField, new Tuple<SideType, int>(SideType.Connection, 2)},
                {CoreType.ScanCore, new Tuple<SideType, int>(SideType.Connection, 1)},
                {CoreType.MatrixField, new Tuple<SideType, int>(SideType.Connection, 1)},
                {CoreType.MatrixCore, new Tuple<SideType, int>(SideType.Connection, 1)},
            };
        }*/

        #endregion

        private bool CheckConformKeySide(UnitTypeCombo type, SideType[] sides)
        {
            return true;
            /*if (!_keySideLib.TryGetValue(core, out var data))
            {
                //no constrain always ok
                return true;
            }

            return Utils.GetSideCount(data.Item1, sides) >= data.Item2;*/
        }

        public static void NormalizeDicVal<T>(ref Dictionary<T, float> lib)
        {
            float totalWeight = 0;
            foreach (var weight in lib.Values)
            {
                totalWeight += weight;
            }

            if (!(Mathf.Abs(totalWeight - 1) < 1e-3))
            {
                var keys = lib.Keys.ToArray().Clone() as T[];
                foreach (var coreType in keys)
                {
                    lib[coreType] /= totalWeight;
                }
            }
        }

        public abstract void ResetPendingBuy();
        public abstract bool RequestBuy(int shopID, out int postalPrice);
        public abstract bool BuyToPos(int idx, Vector2Int pos, bool crash);
    }

    public sealed partial class ShopMgr : ShopBase, IAnimatableShop, IRequirableShop
    {
        public override bool ShopOpening
        {
            protected set
            {
                _shopOpening = value;
                ShopCoverRoot.gameObject.SetActive(!_shopOpening);
            }
            get => _shopOpening;
        }

        private int _madateUnitCount = -1;
        private int _normalMinRequire = -1;
        private int _networkMinRequire = -1;
        private int[] normalMadateArray = new[] {-1};
        private int[] networkMadateArray = new[] {-1};
        private int madateBaseTier = -1;
        private int unitCountOffset = -1;

        private int[] createMadateArray(int minCount, int dur)
        {
            var baseTier = TierProgress(currentLevelAsset.LevelProgress);
            var minUnit = (minCount / baseTier) + ((minCount % baseTier) == 0 ? 0 : 1);
            Utils.SpreadOutLaying(minUnit, dur, out var res);
            return res;
        }

        public void SetRequire(int dur, int normal, int network)
        {
            _madateUnitCount = dur * 4;
            _normalMinRequire = normal;
            _networkMinRequire = network;

            if (_normalMinRequire != -1)
            {
                normalMadateArray = createMadateArray(_normalMinRequire, _madateUnitCount);
            }

            if (_networkMinRequire != -1)
            {
                networkMadateArray = createMadateArray(_networkMinRequire, _madateUnitCount);
            }

            madateBaseTier = TierProgress(currentLevelAsset.LevelProgress);
            unitCountOffset = TotalCount;
        }

        public void ResetRequire()
        {
            _madateUnitCount = -1;
            _normalMinRequire = -1;
            _networkMinRequire = -1;
            normalMadateArray = new[] {-1};
            networkMadateArray = new[] {-1};
            madateBaseTier = -1;
            unitCountOffset = -1;
        }

        public Transform ShopCoverRoot;
        private int countOffset = 0;
        private int localOffset => (TotalCount - countOffset);
        private bool[] stationaryArray;
        private bool nomoreStationary = false;

        private Vector3[] currentPosS;
        private Vector3[] nextPosS;

        public Transform PlacementPosA;
        public Transform PlacementPosB;

        private Vector3 _posA => PlacementPosA.position;
        private float _posDisplace => Vector3.Distance(PlacementPosA.position, PlacementPosB.position);

        public void ShopUpdateStack()
        {
            if (_items[0])
            {
                Destroy(_items[0].gameObject);
            }

            _items[0] = null;
            for (int i = 0; i < _items.Length; i++)
            {
                if (_items[i])
                {
                    for (var j = 0; j < _items.Length; j++)
                    {
                        if (!_items[j])
                        {
                            _items[j] = _items[i];
                            _items[i] = null;
                            _hardwarePrices[j] = _hardwarePrices[i];
                            _hardwarePrices[i] = -1;
                            currentPosS[j] = currentPosS[i];
                            break;
                        }
                    }
                }
            }
        }

        public override void ShopInit(GameAssets _currentLevelAsset)
        {
            _items = new GameObject[4];
            _hardwarePrices = new float[_items.Length];
            //ItemPriceTexts_TMP = new TextMeshPro[_items.Length];
            currentPosS = new Vector3[_items.Length];
            nextPosS = new Vector3[_items.Length];
            currentLevelAsset = _currentLevelAsset;
        }

        public override void ShopStart()
        {
            InitPrice();
            //InitSideCoreWeight();

            TotalCount = 0;

            ShopPreAnimationUpdate();
            ShopPostAnimationUpdate();
            ShopOpening = false;
        }

        public void ShopPreAnimationUpdate()
        {
            ShopUpdateStack();

            for (var i = 0; i < _items.Length; i++)
            {
                nextPosS[i] = _posA + new Vector3(_posDisplace * i, 0, 0);
            }
        }

        private Vector3 lerpVec3(Vector3 vecA, Vector3 vecB, float lerp)
        {
            return new Vector3(Mathf.Lerp(vecA.x, vecB.x, lerp), Mathf.Lerp(vecA.y, vecB.y, lerp),
                Mathf.Lerp(vecA.z, vecB.z, lerp));
        }

        public void ShopUpdateAnimation(float lerp)
        {
            for (var i = 0; i < _items.Length; i++)
            {
                if (_items[i])
                {
                    _items[i].gameObject.transform.position = lerpVec3(currentPosS[i], nextPosS[i], lerp);
                    _items[i].gameObject.GetComponentInChildren<Unit>().RetailPrice =
                        Mathf.FloorToInt(Random.value * 100.0f);
                }
            }
        }

        private UnitTypeCombo GenerateCoreAndTier(out int tier)
        {
            var offsetedUnitCount = TotalCount - unitCountOffset;
            if (normalMadateArray.Contains(offsetedUnitCount))
            {
                tier = madateBaseTier;
                return new UnitTypeCombo(SignalType.Matrix, CoreGenre.Field);
            }
            else if (networkMadateArray.Contains(offsetedUnitCount))
            {
                tier = madateBaseTier;
                return new UnitTypeCombo(SignalType.Scan, CoreGenre.Field);
            }
            else
            {
                tier = TierProgress(currentLevelAsset.LevelProgress);
                return GenerateRandomCore();
            }
        }

        public void ShopPostAnimationUpdate()
        {
            for (var i = 0; i < _items.Length; i++)
            {
                currentPosS[i] = new Vector3(nextPosS[i].x, nextPosS[i].y, nextPosS[i].z);
            }

            for (var i = 0; i < _items.Length; i++)
            {
                var _cost = NthUnitCost(GameBoard.GetUnitCount);
                if (!_items[i])
                {
                    var core = GenerateCoreAndTier(out var tier);
                    var (item1, item2, CostMultiplier) = TierMultiplier(tier);
                    CostMultiplier = 0.0f;
                    _cost = Mathf.RoundToInt(_cost * CostMultiplier);
                    _items[i] = InitUnitShop(core.Item1,core.Item2, new [] {SideType.Connection,SideType.Connection,SideType.NoConnection,SideType.NoConnection}, out _hardwarePrices[i], i, _cost, tier);
                    _itemUnit[i].SetShop(i, UnitRetailPrice(i), 0, _cost, TotalCount % 2 == 0);
                    currentPosS[i] = _posA + new Vector3(_posDisplace * i, 0, 0);
                    nextPosS[i] = _posA + new Vector3(_posDisplace * i, 0, 0);
                    _items[i].gameObject.transform.position = currentPosS[i];
                }
                else
                {
                    _items[i].gameObject.GetComponentInChildren<Unit>().SetShop(i, UnitRetailPrice(i), 0, -1, null);
                }
            }
        }

        public override void ResetPendingBuy()
        {
            foreach (var unit in _itemUnit)
            {
                unit.SetPendingBuying = false;
            }
        }

        /// <summary>
        /// 计算单位本身的价格，不含邮费。
        /// </summary>
        /// <param name="idx">商店ID</param>
        /// <returns>除了邮费的总价</returns>
        private int UnitRetailPrice(int idx)
        {

            var ( item1, priceMutilpier, item3) = TierMultiplier(_itemUnit[idx].Tier);

            //现在使用时间节奏调整价格。
            var val = Mathf.FloorToInt(_hardwarePrices[idx] *
                                       _priceShopDiscount[idx] *
                                       PriceMultiplier(currentLevelAsset.LevelProgress) *
                                       priceMutilpier);
            //在基价已经比较便宜的时候，这个算完后可能为0.
            return Math.Max(val, 1);
        }

        public override bool RequestBuy(int shopID, out int postalPrice)
        {
            postalPrice = -1;
            if (_items[shopID])
            {
                var totalPrice = UnitRetailPrice(shopID);
                CalculatePostalPrice(totalPrice, currentLevelAsset.LevelProgress, out postalPrice);
                if (CurrentGameStateMgr.GetCurrency() >= totalPrice)
                {
                    _items[shopID].GetComponentInChildren<Unit>().SetPendingBuying = true;
                    return true;
                }
            }

            return false;
        }

        public override bool BuyToPos(int idx, Vector2Int pos, bool crash = false)
        {
            if (_items[idx])
            {
                var unitPrice = UnitRetailPrice(idx);
                var totalPrice = CalculatePostalPrice(unitPrice, currentLevelAsset.LevelProgress, out int postalPrice);

                if (CurrentGameStateMgr.SpendShopCurrency(totalPrice))
                {
                    _items[idx].gameObject.GetComponentInChildren<Unit>().UnsetShop();
                    if (crash)
                    {
                        GameBoard.DeliverUnitAssignedPlaceCrash(_items[idx], pos);
                    }
                    else
                    {
                        GameBoard.DeliverUnitAssignedPlace(_items[idx], pos);
                    }

                    _items[idx] = null;
                    return true;
                }
            }

            return false;
        }

        public override bool BuyToRandom(int idx)
        {
            if (_items[idx])
            {
                if (CurrentGameStateMgr.SpendShopCurrency(_hardwarePrices[idx] * _priceShopDiscount[idx]))
                {
                    _items[idx].gameObject.GetComponentInChildren<Unit>().UnsetShop();
                    GameBoard.DeliverUnitRandomPlace(_items[idx]);
                    _items[idx] = null;
                    return true;
                }
            }

            return false;
        }
    }
}
