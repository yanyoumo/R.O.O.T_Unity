using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    internal sealed class ScoreSet
    {
        public float Currency;
        public int GameTime;

        public ScoreSet(float initCurrency = 1000.0f, int initTime = 60)
        {
            Currency = initCurrency;
            GameTime = initTime;
        }

        public bool ChangeCurrency(float delta)
        {
            if (delta >= 0)
            {
                Currency += delta;
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
            if (price > Currency)
            {
                return false;
            }
            else
            {
                Currency -= price;
                Debug.Assert(Currency >= 0);
                return true;
            }
        }

        public bool ForceSpendCurrency(float price)
        {
            Currency -= price;
            return (Currency >= 0);
        }

        public void AddCurrency(float income)
        {
            Currency += income;
        }

        public void TimePass()
        {
            GameTime--;
        }
    }

    public sealed class GameStateMgr
    {
        public float StartingMoney { private set; get; }
        public int StartingTime { private set; get; }
        private ScoreSet _gameScoreSet;
        private GameModeAsset _startingGameMode;

        public bool SpendSkillCurrency(float price)
        {
            return _gameScoreSet.SpendCurrency(price);
        }

        public bool SpendShopCurrency(float price)
        {
            if (_startingGameMode.ShopCost)
            {
                return _gameScoreSet.SpendCurrency(price);
            }
            else
            {
                return true;
            }
        }

        public void AddCurrency(float income)
        {
            _gameScoreSet.AddCurrency(income);
        }

        public float GetCurrency()
        {
            return _gameScoreSet.Currency;
        }

        public int GetGameTime()
        {
            return _gameScoreSet.GameTime;
        }

        public float GetCurrencyRatio()
        {
            return _gameScoreSet.Currency / StartingMoney;
        }

        public float GetTimeRatio()
        {
            return _gameScoreSet.GameTime / (float) StartingTime;
        }

        public void InitGameMode(GameModeAsset startingGameMode)
        {
            StartingMoney = startingGameMode.InitialCurrency;
            StartingTime = startingGameMode.InitialTime;
            _startingGameMode = startingGameMode;
            _gameScoreSet = new ScoreSet(StartingMoney, StartingTime);
        }

        public bool PerMove(float deltaCurrency)
        {
            _gameScoreSet.TimePass();
            if (_startingGameMode.UnitCost)
            {
                return _gameScoreSet.ChangeCurrency(deltaCurrency);
            }
            else
            {
                return true;
            }
        }

        public bool EndGameCheck()
        {
            return (GetCurrency() < 0) || (GetGameTime() <= 0);
        }
    }
}
