using UnityEngine;

namespace ROOT
{
    internal sealed class Currency
    {
        private float _currency;

        public Currency(float initCurrency)
        {
            _currency = initCurrency;
        }

        public bool ChangeCurrency(float delta)
        {
            if (delta >= 0)
            {
                _currency += delta;
                return true;
            }
            else
            {
                return ForceSpendCurrency(Mathf.Abs(delta));
            }
        }

        //price应该是个正数
        public bool SpendCurrency(float price)
        {
            if (price > _currency)
            {
                return false;
            }
            else
            {
                _currency -= price;
                Debug.Assert(_currency >= 0);
                return true;
            }
        }

        private bool ForceSpendCurrency(float price)
        {
            _currency -= price;
            return (_currency >= 0);
        }

        public static implicit operator float(Currency c) => c._currency;

        public static Currency operator +(Currency b, double c)
        {
            return new Currency(b._currency + (float)c);
        }
        
        public override bool Equals(object obj)
        {
            if (obj is decimal dec)
            {
                return dec == (decimal) _currency;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _currency.GetHashCode();
        }
    }

    public sealed class GameCurrencyMgr
    {
        public float StartingMoney { private set; get; }
        private Currency _currency;
        private bool _shopCost;
        private bool _unitCost;
        
        public void InitGameMode((int, bool, bool) GameStartingData)
        {
            StartingMoney = GameStartingData.Item1;
            _shopCost = GameStartingData.Item2;
            _unitCost = GameStartingData.Item3;
            _currency = new Currency(StartingMoney);
        }

        public float Currency => _currency;
        public void AddCurrency(float income) => _currency += income;
        public bool SpendSkillCurrency(float price) => _currency.SpendCurrency(price);
        public bool SpendShopCurrency(float price) => !_shopCost || _currency.SpendCurrency(price);
        public bool PerMove(float deltaCurrency) => !_unitCost || _currency.ChangeCurrency(deltaCurrency);

        public bool EndGameCheck()
        {
            return _currency < 0; //|| (GetGameTime() <= 0);
        }
    }
}
