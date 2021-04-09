namespace ROOT
{
    /*public sealed partial class ShopMgr : ShopBase
    {
        private int StationaryRateListLastIndex = 0;
        //因为静态单位对游戏体验还是太重要了，不能随便放给随机。还是要控制起来。
        //使用一堆Tuple来控制，<x,y>指代为x个里面有y个静态的，并且是依次的，
        //<0,0>标记为不再产生静态单位。
        //这个应该也是要放在Asset里面，因为一系列Token的长度不同，下面的设置应该也完全不同。
        //原来用的Tuple<int,int>，但是Tuple好像在Odin里面没法正常序列化
        //所以为了在ActionAsset里面用，就只能先用V2Int了。
        //静态单位的概念意外地可以试试复活了。
        private readonly Vector2Int[] DefaultStationaryRateList =
        {
            new Vector2Int(12,9),//75%
            new Vector2Int(10,7),//70%
            new Vector2Int(8,5),//62.5%
            new Vector2Int(6,3),//50%
            new Vector2Int(4,1),//25%
            new Vector2Int(0,0)//0%
            //25
        };

        //静态单元很影响游戏性，不再能交给随机性，使用上面的列表去 真·伪随机。
        [Obsolete]
        private float StationaryRate
        {
            get
            {
                //这个东西需要仔仔细细的调整数据。线形也要仔细调整。
                const float initialRate = 0.8f;
                const float curveRate = 0.75f;
                var val = (1 - Mathf.Pow(currentLevelAsset.LevelProgress, curveRate)) * initialRate;
                return Mathf.Clamp01(val);
            }
        }

        private float StationaryDiscount(SideType[] sides)
        {
            //静态单元的价格就是端口的数量。
            return sides.Count(side => side == SideType.Connection);
        }

        private readonly float[] _priceShopDiscount = {0.5f, 0.67f, 0.8f, 1.0f};

        public float PriceMultiplier(float gameProgress)
        {
            const float maxMultiplier = 7.0f;//这个目前来看要十分高，可能到高达两位数。
            return maxMultiplier * gameProgress + 1.0f;
        }

        private float PostalMultiplier(float gameProgress)
        {
            const float baseMul = 1.6f;
            const float maxMul = 2.2f;
            return Mathf.Lerp(baseMul, maxMul, gameProgress);
        }

        public int CalculatePostalPrice(int unitPrice,float gameProgress, out int postalPrice)
        {
            //邮费也应该越来越贵。
            var totalPrice = Mathf.FloorToInt(unitPrice * PostalMultiplier(gameProgress));
            postalPrice = totalPrice - unitPrice;
            return totalPrice;
        }
    }*/
}