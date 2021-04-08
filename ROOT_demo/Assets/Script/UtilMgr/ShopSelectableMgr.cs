using System;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Messages;
using JetBrains.Annotations;
using ROOT;
using ROOT.Message;
using ROOT.Signal;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ROOT
{
    using UnitTypeCombo=Tuple<SignalType,HardwareType>;

    public interface IAnimatableShop
    {
        void ShopPreAnimationUpdate();
        void ShopUpdateAnimation(float lerp);
        void ShopPostAnimationUpdate();
    }
    
    public class ShopSelectableMgr : MonoBehaviour
    {
        //目前这两个是写死的；之后商店这个种类也改成可配置的；DONE
        private SignalType _signalTypeA = SignalType.Matrix;
        private SignalType _signalTypeB = SignalType.Scan;

        private ((UnitTypeCombo, UnitTypeCombo), (UnitTypeCombo, UnitTypeCombo)) UnitType => (UnitTypeA,UnitTypeB);
        private (UnitTypeCombo, UnitTypeCombo) UnitTypeA=> (new UnitTypeCombo(_signalTypeA, HardwareType.Core), new UnitTypeCombo(_signalTypeA, HardwareType.Field));
        private (UnitTypeCombo, UnitTypeCombo) UnitTypeB => (new UnitTypeCombo(_signalTypeB, HardwareType.Core), new UnitTypeCombo(_signalTypeB, HardwareType.Field));

        public GameObject UnitTemplate;
        private GameAssets currentLevelAsset;
        public Board GameBoard;
        public GameCurrencyMgr CurrentGameCurrencyMgr;
        public List<SignalType> excludedTypes = new List<SignalType>();
        
        private bool _shopOpening;
        private int TotalCount = 0;
        
        [CanBeNull] private Unit[] _itemUnit => _items.Select(unit => unit ? unit.GetComponentInChildren<Unit>() : null).ToArray();

        private float[] _hardwarePrices;

        private GameObject InitUnitShopCore(SignalType signal, HardwareType genre, SideType[] sides, int ID,
            int _cost, int tier)
        {
            var go = Instantiate(UnitTemplate);
            go.name = "Unit_" + Hash128.Compute(Utils.LastRandom.ToString());
            var unit = go.GetComponentInChildren<Unit>();
            unit.InitPosWithAnimation(Vector2Int.zero);
            unit.InitUnit(signal, genre, sides,  tier);
            return go;
        }

        private int TierProgress(float gameProgress)
        {
            var fluctuationRate = 0.25f;
            var fluctuation = 1.0f;
            var baseTier = Mathf.Lerp(1, 6, gameProgress);
            return Mathf.Clamp(Mathf.RoundToInt(baseTier), 1, 5);
        }

        private Dictionary<SideType, float> _priceBySide { set; get; }

        private void InitPrice()
        {
            _priceBySide = new Dictionary<SideType, float>()
            {
                {SideType.NoConnection, 0.0f},
                {SideType.Connection, 2.0f},
            };
        }

        private GameObject[] _items;

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
            var PriceMultipler = 1.0f + 1.75f * (Tier - 1);//这个数据现在看起来太温柔、斜率绝对不能小于1.
            var CostMultipler = 1.0f + 0.5f * Tier;
            return new Tuple<float, float, float>(SignalMultipler, PriceMultipler, CostMultipler);
        }
        
        private void UpdateShopSelf(int discount)
        {
            //主要是要把打折的相关数据放进来。
            for (var i = 0; i < _items.Length; i++)
            {
                var unit = _itemUnit[i];
                if (i >= _items.Length - PremiumCount)
                {
                    Destroy(unit.transform.parent.gameObject);
                    // ReSharper disable once PossibleNullReferenceException
                    var (i0, j0) = IDtoIJ(i);
                    CreatePremiumUnit(i0, j0, discount);
                }
                else
                {
                    unit.UpdateUnitTier(ShopTierMultiplier);
                    unit.SetShop(i, UnitRetailPrice(i, _itemUnit[i].Tier), discount, 0, true);
                }
            }
        }

        public bool ShopOpening
        {
            private set
            {
                DisplayRoot.gameObject.SetActive(value);
                _shopOpening = value;
            }
            get => _shopOpening;
        }

        /// <summary>
        /// 根据商店的基本设置从2D转换为线性的ID。
        /// </summary>
        /// <param name="i">输入 i</param>
        /// <param name="j">输入 j</param>
        /// <returns>如果此处不应该放置单位则返回-1</returns>
        private int IJtoID(int i, int j)
        {
            var rawRes = j + i * HorizontalCount;
            if (i < VerticalCount - 1)
            {
                return rawRes;
            }
            else
            {
                return j < EmptyCount ? -1 : rawRes - EmptyCount;
            }
        }

        /// <summary>
        /// 根据现有设置，从1D的ID转换为二维ID。
        /// </summary>
        /// <param name="ID">输入ID</param>
        /// <returns>如果输入非法ID则返回null</returns>
        [CanBeNull]
        private Tuple<int, int> IDtoIJ(int ID)
        {
            if (ID >= TotalCount) return null;

            var modID = (ID >= RetailCount) ? ID + EmptyCount : ID;

            //if (ID >= RetailCount) modID += EmptyCount;

            return new Tuple<int, int>(modID / HorizontalCount, modID % HorizontalCount);
        }

        public Transform DisplayRoot;
        public Transform StaticContent;
        private readonly float Offset = 1.589f;
        private readonly float OffsetX = -1.926f;
        private readonly int HorizontalCount = 4;
        private readonly int VerticalCount = 3;
        private readonly int PremiumCount = 2;

        public TextMeshPro ShopTierMultiplierText;
        
        private int EmptyCount => HorizontalCount - PremiumCount;
        private int RetailCount => HorizontalCount * (VerticalCount - 1);
        private int MaxDisplayCount => (VerticalCount - 1) * HorizontalCount + PremiumCount;

        private readonly float YOffset = 0.05f;

        private int _discountRate = 0;

        private int UnitRetailPrice(int idx, int tier)
        {
            var val = _hardwarePrices[idx] * TierMultiplier(tier).Item2;
            val = tier <= 2 ? Mathf.Round(val) : Mathf.Round(val / 5.0f) * 5.0f;//在这里对数据进行一个规范化。
            val *= 1.0f - _discountRate * 0.01f;
            return Mathf.RoundToInt(Math.Max(val, 1));
        }

        //TEMP 这个还是要统一管理起来。
        private int UnitHardwarePrice(SignalType signal,HardwareType genre, SideType[] sides)
        {
            var corePrice = SignalMasterMgr.Instance.PriceFromUnit(signal, genre);
            var sideCount = sides.Count(side => side == SideType.Connection);
            var sidePrice = Mathf.Pow(2.0f, sideCount);
            var hardwarePrice = corePrice + sidePrice;
            return Mathf.RoundToInt(hardwarePrice);
        }

        private GameObject InitUnitShop(SignalType signal,HardwareType genre, SideType[] sides, out int hardwarePrice, int ID, int _cost,
            int tier, int discount)
        {
            var go = InitUnitShopCore(signal,genre, sides, ID, _cost, tier);
            go.transform.parent = DisplayRoot;
            var unit = go.GetComponentInChildren<Unit>();
            hardwarePrice = UnitHardwarePrice(signal,genre, sides);
            _hardwarePrices[ID] = hardwarePrice;
            //TODO
            unit.SetShop(ID, UnitRetailPrice(ID, tier), discount, _cost, true);
            _items[ID] = go;
            return go;
        }

        private UnitTypeCombo GenerateSelfCoreAndTier(in int i, out int tier)
        {
            tier =ShopTierMultiplier;
            switch (i)
            {
                case 0:
                    return UnitType.Item1.Item2;
                case 1:
                default:
                    return UnitType.Item2.Item2;
            }
        }

        private SideType[] GenerateSide(in int i)
        {
            switch (i)
            {
                case 0:
                    return new[]
                        {SideType.Connection, SideType.NoConnection, SideType.NoConnection, SideType.NoConnection};
                case 1:
                    return new[]
                        {SideType.Connection, SideType.Connection, SideType.NoConnection, SideType.NoConnection};
                case 2:
                    return new[]
                        {SideType.Connection, SideType.NoConnection, SideType.Connection, SideType.NoConnection};
                case 3:
                default:
                    return new[] {SideType.Connection, SideType.Connection, SideType.Connection, SideType.NoConnection};
            }
        }

        private void CreateSelfUnit(int i, int j, int discount)
        {
            var ID = IJtoID(i, j);
            var core = GenerateSelfCoreAndTier(in i, out var tier);
            var go = InitUnitShop(core.Item1,core.Item2, GenerateSide(j), out var hardwarePrice, ID, 0, tier, discount);
            go.transform.localPosition = new Vector3(j * Offset, YOffset, i * OffsetX);
        }

        private bool CoreUnitTypeBOnBoard => GameBoard.GetCountByType(_signalTypeB,HardwareType.Core) > 0;
        private bool CoreUnitTypeAOnBoard => GameBoard.GetCountByType(_signalTypeA,HardwareType.Core) > 0;

        protected UnitTypeCombo GenerateRandomCore()
        {
            return Random.value > 0.5f ? UnitType.Item1.Item2 : UnitType.Item2.Item2;
        }

        private int _rawShopTierMultiplier => TierProgress(currentLevelAsset.LevelProgress);
        private int _ShopTierMultiplierOffset = 0;

        private int ShopTierMultiplier => _rawShopTierMultiplier + _ShopTierMultiplierOffset;

        private void CreatePremiumUnit(int i, int j, int discount)
        {
            //ShopTierMultiplier = TierProgress(currentLevelAsset.LevelProgress);
            var ID = IJtoID(i, j);
            UnitTypeCombo core;
            if (CoreUnitTypeBOnBoard && CoreUnitTypeAOnBoard)
            {
                //RISK 这里的生成有问题。还是要确认一下。
                //这个函数是基类提供的；那个配置代码没有和UnitType解耦干净。
                core = GenerateRandomCore();
            }
            else if (CoreUnitTypeBOnBoard)
            {
                core = UnitType.Item1.Item1;
            }
            else if (CoreUnitTypeAOnBoard)
            {
                core = UnitType.Item2.Item1;
            }
            else
            {
                //hmmmmm这里先这样吧…………
                core = Random.value > 0.5 ? UnitType.Item1.Item1 : UnitType.Item2.Item1;
            }

            //TEMP 这个Tier到时候还是统一管理一下。
            var sides = Utils.Shuffle(new[] {SideType.Connection, SideType.Connection, SideType.Connection, SideType.NoConnection});
            if (ShopTierMultiplier > 4) sides = new[] {SideType.Connection, SideType.Connection, SideType.Connection, SideType.Connection};

            var go = InitUnitShop(core.Item1, core.Item2, sides, out var hardwarePrice, ID, 0, ShopTierMultiplier, discount);
            go.transform.localPosition = new Vector3(j * Offset, YOffset, i * OffsetX);
        }

        private void CreateSelfUnit()
        {
            for (var i = 0; i < VerticalCount; i++)
            {
                for (var j = 0; j < HorizontalCount; j++)
                {
                    if (i == VerticalCount - 1)
                    {
                        if (j < HorizontalCount - 2) continue;
                        CreatePremiumUnit(i, j, 0);
                        TotalCount++;
                    }
                    else
                    {
                        CreateSelfUnit(i, j, 0);
                        TotalCount++;
                    }
                }
            }
        }

        public void ShopInit(GameAssets _currentLevelAsset)
        {
            currentLevelAsset = _currentLevelAsset;
            _items = new GameObject[MaxDisplayCount];
            _hardwarePrices = new float[MaxDisplayCount];
            _signalTypeA = _currentLevelAsset.ActionAsset.AdditionalGameSetup.PlayingSignalTypeA;
            _signalTypeB = _currentLevelAsset.ActionAsset.AdditionalGameSetup.PlayingSignalTypeB;
        }

        //Init和Start还是在这个层级拆开吧、一个是设置数据、一个是实际实现数据。
        public void ShopStart()
        {
            InitPrice();
            CreateSelfUnit();
            ShopTierMultiplierText.text = "1";
        }

        public void OpenShop(bool Opening, int discount)
        {
            if (Opening && !_shopOpening)
            {
                UpdateShopSelf(discount);
                _discountRate = discount;
            }
            ShopOpening = Opening;
        }
        
        private GameObject InitUnitShop(Unit SelfUnit)
        {
            var go = Instantiate(UnitTemplate);
            go.name = "Unit_" + Hash128.Compute(Utils.LastRandom.ToString());
            var unit = go.GetComponentInChildren<Unit>();
            unit.InitPosWithAnimation(Vector2Int.zero);
            unit.InitUnit(SelfUnit.UnitSignal, SelfUnit.UnitHardware, SelfUnit.UnitSides.Values.ToArray(), SelfUnit.Tier);
            return go;
        }

        public bool BuyToRandom(int shopID)
        {
            var itemID = ItemIDFromShopID(shopID);
            if (!_items[itemID]) return false;

            if (!CurrentGameCurrencyMgr.SpendShopCurrency(UnitRetailPrice(itemID, _itemUnit[itemID].Tier))) return false;

            if (_itemUnit != null)
            {
                GameBoard.DeliverUnitRandomPlace(InitUnitShop(_itemUnit[itemID]));
                return true;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        private static int ItemIDFromShopID(in int shopID)
        {
            //Key:   1234567890
            //ShopID:0123456789
            //SelfID:1234567890
            //ItemID:0123456789
            //很蛋疼，但是为了操作和管理，这个还得弄。
            return shopID;
        }

        public bool RequestBuy(int shopID, out int postalPrice)
        {
            postalPrice = 0;
            var itemID = ItemIDFromShopID(shopID);
            if (!_items[itemID]) return false;

            var totalPrice = UnitRetailPrice(itemID, _itemUnit[itemID].Tier);
            if (CurrentGameCurrencyMgr.Currency >= totalPrice)
            {
                _items[itemID].GetComponentInChildren<Unit>().SetPendingBuying = true;
                return true;
            }

            return false;
        }

        public void ResetPendingBuy()
        {
            //throw new NotImplementedException();
        }

        public bool BuyToPos(int idx, Vector2Int pos, bool crash)
        {
            return false;
            //throw new NotImplementedException();
        }

        private void ShopTierOffsetChangedEventHandler(IMessage rMessage)
        {
            //技术上这个东西搞定了、但是Premium单元的设计就废了；
            //讲道理、其实这个（Premium单元）本来也是个废设计。
            //从面积上到是省出来一些空间。
            
            //还有另外一个问题、根据现在的设计、Tier的提高除了贵没有问题；那么现在玩家肯定会“努着”也会去买高级单元；
            //随着Tier的价格增加一定是要比线性要快的多。
            
            //之前卡住Tier这个东西掩盖住了一个问题：就是Tier的提高所带来的本质价值。
            //在游戏设计中、每个Tier所能提供的收益加成都是线性的；也就是和线性的成本本身抵消了。
            //但是除了收益这个东西、还有一个需要进行提供价值加成的还有高Tier本身的灵活性，但是也有对于Heatsink摧毁的敏感性。
            //将这个价格需要引导玩家进行选择，进行这种选择的前提是让玩家可以有一个围绕此思考的抓手、也就是让玩家明确的意识到这件事儿。
            //说白了、可能需要价格对Tier的增长在合理的范围下越快越好。
            
            //现在的价格有一定的不确定性、人类对数据的体验是十分微妙的、指数谬误、倍率谬误这些都要考虑。
            //现在解决方案就是较高价格round到5这个思路、这样让玩家可以更清楚地思考价格倍率。

            if (!ShopOpening) return;

            if (rMessage is ShopTierOffsetChangedData data)
            {
                var tmpOffset = _ShopTierMultiplierOffset + (data.UpwardOrDownward ? 1 : -1);
                var tmpTier = _rawShopTierMultiplier + tmpOffset;
                if (tmpTier > 0 && tmpTier < 6)
                {
                    _ShopTierMultiplierOffset = tmpOffset;
                    UpdateShopSelf(_discountRate);
                }
                ShopTierMultiplierText.text = ShopTierMultiplier.ToString();
            }
        }
        
        private void Awake()
        {
            MessageDispatcher.AddListener(WorldEvent.ShopTierOffsetChangedEvent,ShopTierOffsetChangedEventHandler);
        }

        private void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.ShopTierOffsetChangedEvent,ShopTierOffsetChangedEventHandler);
        }
    }
}