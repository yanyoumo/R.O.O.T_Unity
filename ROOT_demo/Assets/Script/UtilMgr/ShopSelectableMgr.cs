using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ROOT
{
    public class ShopSelectableMgr : ShopBase
    {

        public Transform DisplayRoot;
        private readonly float Offset = 1.589f;
        private readonly float OffsetX = -1.926f;
        private readonly int HorizontalCount = 4;
        private readonly int VerticalCount = 3;
        private readonly float YOffset=0.05f;

        private int UnitRetailPrice(int idx)
        {

            var (item1, priceMutilpier, item3) = TierMultiplier(_itemUnit[idx].Tier);

            //现在使用时间节奏调整价格。
            var val = Mathf.FloorToInt(_hardwarePrices[idx] * priceMutilpier);
            //在基价已经比较便宜的时候，这个算完后可能为0.
            return Math.Max(val, 1);
        }

        //TEMP 这个还是要统一管理起来。
        private int UnitHardwarePrice(CoreType core,SideType[] sides)
        {
            _priceByCore.TryGetValue(core, out var corePrice);
            var hardwarePrice = corePrice + sides.Sum(TryGetPrice);
            return Mathf.RoundToInt(hardwarePrice);
        }

        private GameObject InitUnitShop(CoreType core, SideType[] sides, out int hardwarePrice, int ID, int _cost, int tier)
        {
            var go = InitUnitShopCore(core, sides, ID, _cost, tier);
            go.transform.parent = DisplayRoot;
            var unit = go.GetComponentInChildren<Unit>();
            hardwarePrice = UnitHardwarePrice(core, sides);
            unit.SetShop(ID, hardwarePrice, _cost, true);
            return go;
        }

        /*protected CoreType GeneratePremiumCoreAndTier(in int i, out int tier)
        {

        }*/

        protected CoreType GenerateSelfCoreAndTier(in int i,out int tier)
        {
            tier = TierProgress(currentLevelAsset.LevelProgress);
            switch (i)
            {
                case 0:
                    return CoreType.HardDrive;
                case 1:
                default:
                    return CoreType.NetworkCable;
            }
        }

        protected SideType[] GenerateSide(in int i)
        {
            switch (i)
            {
                case 0:
                    return new[] {SideType.Connection, SideType.NoConnection, SideType.NoConnection, SideType.NoConnection };
                case 1:
                    return new[] { SideType.Connection, SideType.Connection, SideType.NoConnection, SideType.NoConnection };
                case 2:
                    return new[] { SideType.Connection, SideType.NoConnection, SideType.Connection, SideType.NoConnection };
                case 3:
                default:
                    return new[] {SideType.Connection, SideType.Connection, SideType.Connection, SideType.NoConnection };
            }
        }

        private void CreateSelfUnit(int ID, int i, int j)
        {
            var core = GenerateSelfCoreAndTier(in i, out var tier);
            var go = InitUnitShop(core, GenerateSide(j), out var hardwarePrice, ID, 0, tier);
            go.transform.localPosition = new Vector3(j * Offset, YOffset, i * OffsetX);
        }

        private void CreatePremiumUnit(int ID, int i, int j)
        {
            var core = GenerateRandomCore();
            //TEMP 这个Tier到时候还是统一管理一下。
            var tier = TierProgress(currentLevelAsset.LevelProgress) + (Random.value > 0.5f ? 1 : 2);
            //想办法让PB的Unit接口至少是2。
            var go = InitUnitShop(core, GenerateRandomSideArray(core), out var hardwarePrice, ID, 0, tier);
            go.transform.localPosition = new Vector3(j * Offset, YOffset, i * OffsetX);
        }

        private void CreateSelfUnit()
        {
            for (var i = 0; i < VerticalCount; i++)
            {
                for (var j = 0; j < HorizontalCount; j++)
                {
                    var ID = j + VerticalCount * i;
                    if (i==VerticalCount-1)
                    {
                        if (j < HorizontalCount - 2) continue;
                        CreatePremiumUnit(ID, i, j);
                        totalCount++;
                    }
                    else
                    {
                        CreateSelfUnit(ID, i, j);
                        totalCount++;
                    }
                }
            }
        }

        public override void ShopInit(GameAssets _currentLevelAsset)
        {
            currentLevelAsset = _currentLevelAsset;
        }
        
        public override void ShopStart()
        {
            InitPrice();
            InitSideCoreWeight();
            CreateSelfUnit();
        }

        public override bool BuyToRandom(int idx)
        {
            throw new NotImplementedException();
        }

        public override bool RequestBuy(int idx, out int postalPrice)
        {
            throw new NotImplementedException();
        }

        public override bool ShopOpening { get; set; }


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