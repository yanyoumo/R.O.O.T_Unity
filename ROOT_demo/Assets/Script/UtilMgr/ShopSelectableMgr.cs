using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ROOT
{
    public class ShopSelectableMgr : ShopBase
    {
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
                    unit.UpdateUnitTier(TierProgress(currentLevelAsset.LevelProgress));
                    unit.SetShop(i, UnitRetailPrice(i, _itemUnit[i].Tier), discount, 0, true);
                }
            }
        }

        public override void OpenShop(bool Opening, int discount)
        {
            if (Opening && !_shopOpening)
            {
                UpdateShopSelf(discount);
                discountRate = discount;
            }

            ShopOpening = Opening;
        }

        public override bool ShopOpening
        {
            protected set
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

        private int EmptyCount => HorizontalCount - PremiumCount;
        private int RetailCount => HorizontalCount * (VerticalCount - 1);
        private int MaxDisplayCount => (VerticalCount - 1) * HorizontalCount + PremiumCount;

        private readonly float YOffset = 0.05f;

        private int discountRate = 0;

        private int UnitRetailPrice(int idx, int tier)
        {
            var (item1, priceMutilpier, item3) = TierMultiplier(tier);
            var val = Mathf.FloorToInt(_hardwarePrices[idx] * priceMutilpier);
            val = Mathf.FloorToInt(val * (1.0f - discountRate * 0.01f));
            return Math.Max(val, 1);
        }

        //TEMP 这个还是要统一管理起来。
        private int UnitHardwarePrice(CoreType core, SideType[] sides)
        {
            var corePrice = SignalMasterMgr.Instance.PriceFromUnit(core);
            var sideCount = sides.Count(side => side == SideType.Connection);
            var sidePrice = Mathf.Pow(2.0f, sideCount);
            var hardwarePrice = corePrice + sidePrice;
            return Mathf.RoundToInt(hardwarePrice);
        }

        private GameObject InitUnitShop(CoreType core, SideType[] sides, out int hardwarePrice, int ID, int _cost,
            int tier, int discount)
        {
            var go = InitUnitShopCore(core, sides, ID, _cost, tier);
            go.transform.parent = DisplayRoot;
            var unit = go.GetComponentInChildren<Unit>();
            hardwarePrice = UnitHardwarePrice(core, sides);
            _hardwarePrices[ID] = hardwarePrice;
            //TODO
            unit.SetShop(ID, UnitRetailPrice(ID, tier), discount, _cost, true);
            _items[ID] = go;
            return go;
        }

        protected CoreType GenerateSelfCoreAndTier(in int i, out int tier)
        {
            tier = TierProgress(currentLevelAsset.LevelProgress);
            switch (i)
            {
                case 0:
                    return FieldUnitTypeA;
                case 1:
                default:
                    return FieldUnitTypeB;
            }
        }

        protected SideType[] GenerateSide(in int i)
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
            var go = InitUnitShop(core, GenerateSide(j), out var hardwarePrice, ID, 0, tier, discount);
            go.transform.localPosition = new Vector3(j * Offset, YOffset, i * OffsetX);
        }

        private bool CoreUnitTypeBOnBoard => GameBoard.GetCountByType(CoreUnitTypeB) > 0;
        private bool CoreUnitTypeAOnBoard => GameBoard.GetCountByType(CoreUnitTypeA) > 0;

        private void CreatePremiumUnit(int i, int j, int discount)
        {
            var ID = IJtoID(i, j);

            CoreType core;
            if (CoreUnitTypeBOnBoard && CoreUnitTypeAOnBoard)
            {
                //RISK 这里的生成有问题。还是要确认一下。
                //这个函数是基类提供的；那个配置代码没有和UnitType解耦干净。
                core = GenerateRandomCore();
            }
            else if (CoreUnitTypeBOnBoard)
            {
                core = CoreUnitTypeA;
            }
            else if (CoreUnitTypeAOnBoard)
            {
                core = CoreUnitTypeB;
            }
            else
            {
                //hmmmmm这里先这样吧…………
                core = Random.value > 0.5 ? CoreUnitTypeA : CoreUnitTypeB;
            }

            //TEMP 这个Tier到时候还是统一管理一下。
            var tier = TierProgress(currentLevelAsset.LevelProgress) + (Random.value > 0.5f ? 1 : 2);
            SideType[] sides;
            if (tier > 4)
            {
                sides = new[] {SideType.Connection, SideType.Connection, SideType.Connection, SideType.Connection};
            }
            else
            {
                do
                {
                    //PB的Unit接口数至少是3。
                    sides = GenerateRandomSideArray(core);
                } while (sides.Count(side => side == SideType.Connection) < 3);
            }

            var go = InitUnitShop(core, sides, out var hardwarePrice, ID, 0, tier, discount);
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

        public override void ShopInit(GameAssets _currentLevelAsset)
        {
            currentLevelAsset = _currentLevelAsset;
            _items = new GameObject[MaxDisplayCount];
            _hardwarePrices = new float[MaxDisplayCount];
        }

        public override void ShopStart()
        {
            InitPrice();
            InitSideCoreWeight();
            CreateSelfUnit();
        }

        private GameObject InitUnitShop(Unit SelfUnit)
        {
            var go = Instantiate(UnitTemplate);
            go.name = "Unit_" + Hash128.Compute(Utils.LastRandom.ToString());
            var unit = go.GetComponentInChildren<Unit>();
            unit.InitPosWithAnimation(Vector2Int.zero);
            unit.InitUnit(SelfUnit.UnitCore, SelfUnit.UnitSides.Values.ToArray(), SelfUnit.Tier);
            return go;
        }

        public override bool BuyToRandom(int shopID)
        {
            var itemID = ItemIDFromShopID(shopID);
            if (!_items[itemID]) return false;

            if (!CurrentGameStateMgr.SpendShopCurrency(UnitRetailPrice(itemID, _itemUnit[itemID].Tier))) return false;

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

        public override bool RequestBuy(int shopID, out int postalPrice)
        {
            postalPrice = 0;
            var itemID = ItemIDFromShopID(shopID);
            if (!_items[itemID]) return false;

            var totalPrice = UnitRetailPrice(itemID, _itemUnit[itemID].Tier);
            if (CurrentGameStateMgr.GetCurrency() >= totalPrice)
            {
                _items[itemID].GetComponentInChildren<Unit>().SetPendingBuying = true;
                return true;
            }

            return false;
        }

        public override void ResetPendingBuy()
        {
            //throw new NotImplementedException();
        }

        public override bool BuyToPos(int idx, Vector2Int pos, bool crash)
        {
            return false;
            //throw new NotImplementedException();
        }
    }
}