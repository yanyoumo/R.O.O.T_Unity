using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;
using Random = UnityEngine.Random;

namespace ROOT
{

    public partial class BoardDataCollector : MonoBehaviour
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
    }

    public sealed partial class ShopMgr:MonoBehaviour
    {
        public Transform ShopCoverRoot;
        private bool _shopOpening;
        public bool ShopOpening
        {
            set
            {
                _shopOpening = value;
                ShopCoverRoot.gameObject.SetActive(!_shopOpening);
            }
            get => _shopOpening;
        }
        private int totalCount = 0;
        private int countOffset = 0;
        private int localOffset => (totalCount - countOffset);
        private bool[] stationaryArray;
        private bool nomoreStationary = false;
        public GameObject UnitTemplate;
        private GameAssets currentLevelAsset;

        public Board GameBoard;
        public GameStateMgr CurrentGameStateMgr;

        private GameObject[] _items;

        [CanBeNull]
        private Unit[] _itemUnit => _items.Select(unit => unit ? unit.GetComponentInChildren<Unit>() : null).ToArray();

        private float[] _hardwarePrices;

        private Vector3[] currentPosS;
        private Vector3[] nextPosS;

        public Transform PlacementPosA;
        public Transform PlacementPosB;

        private Vector3 _posA => PlacementPosA.position;
        private float _posDisplace => Vector3.Distance(PlacementPosA.position, PlacementPosB.position);

        private Dictionary<CoreType, float> _priceByCore;
        private Dictionary<SideType, float> _priceBySide;

        private Dictionary<SideType, float> _defaultSideWeight;
        private Dictionary<SideType, float> _processorSideWeight;
        private Dictionary<SideType, float> _serverSideWeight;
        private Dictionary<SideType, float> _hddSideWeight;
        private Dictionary<SideType, float> _netCableSideWeight;

        private Dictionary<CoreType, float> _defaultCoreWeight;
        private Dictionary<CoreType, float> _noServerCoreWeight;
        private Dictionary<CoreType, float> _noProcessorCoreWeight;
        private Dictionary<CoreType, float> _nandServerProcessorCoreWeight;
        private Dictionary<CoreType, Dictionary<SideType, float>> _sideWeightLib;

        //KeySide minCount
        private Dictionary<CoreType, Tuple<SideType, int>> _keySideLib;

        public List<CoreType> excludedTypes=new List<CoreType>();

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

        private CoreType GenerateRandomCore()
        {
            //这里不会生成HQ核心。
            Dictionary<CoreType, float> lib= _defaultCoreWeight;
            if (GameBoard.GetCountByType(CoreType.Server)<1)
            {
                lib = _noServerCoreWeight;
            }
            if (GameBoard.GetCountByType(CoreType.Processor) < 1)
            {
                lib = _noProcessorCoreWeight;
            }
            if ((GameBoard.GetCountByType(CoreType.Server) < 1)&& (GameBoard.GetCountByType(CoreType.Processor) < 1))
            {
                lib = _nandServerProcessorCoreWeight;
            }
            if (excludedTypes.Count>0)
            {
                foreach (var excludedType in excludedTypes)
                {
                    if (lib.TryGetValue(excludedType,out float value))
                    {
                        lib.Remove(excludedType);
                    }
                }
            }
            NormalizeDicVal(ref lib);
            return Utils.GenerateWeightedRandom(lib);
        }

        private bool CheckConformKeySide(CoreType core, SideType[] sides)
        {

            if (!_keySideLib.TryGetValue(core, out var data))
            {
                //no constrain always ok
                return true;
            }

            return Utils.GetSideCount(data.Item1, sides) >= data.Item2;
        }

        private SideType[] GenerateRandomSideArray(CoreType core=CoreType.PCB)
        {
            if (!_sideWeightLib.TryGetValue(core, out var lib))
            {
                Debug.Assert(false);
                lib = _defaultSideWeight;
            }
            SideType[] res=new SideType[4];
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
        }

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
                    for (int j = 0; j < _items.Length; j++)
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

        public void ShopInit(GameAssets _currentLevelAsset)
        {
            _items = new GameObject[4];
            _hardwarePrices = new float[_items.Length];
            //ItemPriceTexts_TMP = new TextMeshPro[_items.Length];
            currentPosS = new Vector3[_items.Length];
            nextPosS = new Vector3[_items.Length];
            currentLevelAsset = _currentLevelAsset;
        }

        public void ShopStart()
        {
            InitPrice();
            InitSideCoreWeight();

            totalCount = 0;

            ShopPreAnimationUpdate();
            ShopPostAnimationUpdate();
            ShopOpening = false;
        }

        public void ShopPreAnimationUpdate()
        {
            ShopUpdateStack();

            for (int i = 0; i < _items.Length; i++)
            {
                nextPosS[i] = _posA + new Vector3(_posDisplace * i, 0, 0);
            }
        }

        private Vector3 lerpVec3(Vector3 vecA,Vector3 vecB,float lerp)
        {
            return new Vector3(Mathf.Lerp(vecA.x, vecB.x, lerp), Mathf.Lerp(vecA.y, vecB.y, lerp),
                Mathf.Lerp(vecA.z, vecB.z, lerp));
        }

        public void ShopUpdateAnimation(float lerp)
        {
            for (int i = 0; i < _items.Length; i++)
            {
                if (_items[i])
                {
                    _items[i].gameObject.transform.position = lerpVec3(currentPosS[i], nextPosS[i], lerp);
                    _items[i].gameObject.GetComponentInChildren<Unit>().RetailPrice = Mathf.FloorToInt(Random.value * 100.0f);
                }
            }
        }

        public void ShopPostAnimationUpdate()
        {
            for (int i = 0; i < _items.Length; i++)
            {
                currentPosS[i] = new Vector3(nextPosS[i].x, nextPosS[i].y, nextPosS[i].z);
            }

            for (int i = 0; i < _items.Length; i++)
            {
                int _cost = currentLevelAsset.BoardDataCollector.NthUnitCost(GameBoard.GetUnitCount);
                if (!_items[i])
                {
                    CoreType core = GenerateRandomCore();
                    int tier = TierProgress(currentLevelAsset.LevelProgress);
                    //Tier的计分部分，Server迪公去接.
                    //硬盘已经接上去了。
                    var (item1, item2, CostMultiplier) = TierMultiplier(tier);
                    CostMultiplier = 0.0f;
                    _cost = Mathf.RoundToInt(_cost * CostMultiplier);
                    _items[i] = InitUnitShop(core, GenerateRandomSideArray(core), out _hardwarePrices[i], i, _cost, tier);
                    _itemUnit[i].SetShop(i, UnitRetailPrice(i), _cost, totalCount % 2 == 0);
                    currentPosS[i] = _posA + new Vector3(_posDisplace * i, 0, 0);
                    nextPosS[i] = _posA + new Vector3(_posDisplace * i, 0, 0);
                    _items[i].gameObject.transform.position = currentPosS[i];
                }
                else
                {
                    _items[i].gameObject.GetComponentInChildren<Unit>().SetShop(i, UnitRetailPrice(i), -1,  null);
                }
            }
        }

        public void ResetPendingBuy()
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

        public bool RequestBuy(int idx, out int postalPrice)
        {
            postalPrice = -1;
            if (_items[idx])
            {
                var totalPrice = UnitRetailPrice(idx);
                CalculatePostalPrice(totalPrice,currentLevelAsset.LevelProgress, out postalPrice);
                if (CurrentGameStateMgr.GetCurrency() >= totalPrice)
                {
                    _items[idx].GetComponentInChildren<Unit>().SetPendingBuying = true;
                    return true;
                }
            }
            return false;
        }

        public bool BuyToPos(int idx, Vector2Int pos,bool crash=false)
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

        public bool BuyToRandom(int idx)
        {
            if (_items[idx])
            {
                if (CurrentGameStateMgr.SpendShopCurrency(_hardwarePrices[idx]*_priceShopDiscount[idx]))
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
