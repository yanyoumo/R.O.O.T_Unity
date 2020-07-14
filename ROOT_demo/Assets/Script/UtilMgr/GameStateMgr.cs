using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public sealed class ScoreSet
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
            if (delta>=0)
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
                Debug.Assert(Currency>=0);
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

    public struct PerMoveData
    {
        public float DeltaCurrency;
        public int DeltaTime;
        public PerMoveData(float deltaCurrency, int deltaTime)
        {
            DeltaCurrency = deltaCurrency;
            DeltaTime = deltaTime;
        }
    }

    public interface IGameLoopStepCheck
    {
        void InitGameMode(ScoreSet initScoreSet, PerMoveData perMoveData);
        bool PerMove(ScoreSet initScoreSet, PerMoveData perMoveData);
        bool EndGameCheck(ScoreSet initScoreSet, PerMoveData perMoveData);
    }

    public abstract class GameStateMgr: IGameLoopStepCheck
    {
        public float StartingMoney { protected set; get; }
        public int StartingTime { protected set; get; }
        public ScoreSet GameScoreSet { protected set; get; }

        public virtual bool SpendCurrency(float price)
        {
            return GameScoreSet.SpendCurrency(price);
        }

        public virtual void AddCurrency(float income)
        {
            GameScoreSet.AddCurrency(income);
        }

        public float GetCurrency()
        {
            return GameScoreSet.Currency;
        }

        public int GetGameTime()
        {
            return GameScoreSet.GameTime;
        }

        public float GetCurrencyRatio()
        {
            return GameScoreSet.Currency / StartingMoney;
        }

        public float GetTimeRatio()
        {
            return GameScoreSet.GameTime / (float) StartingTime;
        }

        public abstract void InitGameMode(ScoreSet initScoreSet, PerMoveData perMoveData);
        public abstract bool PerMove(ScoreSet initScoreSet, PerMoveData perMoveData);
        public abstract bool EndGameCheck(ScoreSet initScoreSet, PerMoveData perMoveData);

        public static GameStateMgr GenerateGameStateMgrByType(Type type)
        {
            if (type==typeof(StandardGameStateMgr))
            {
                return new StandardGameStateMgr();
            }
            else if (type == typeof(InfiniteGameStateMgr))
            {
                return new InfiniteGameStateMgr();
            }

            throw new NotImplementedException();
        }
    }

    public class StandardGameStateMgr : GameStateMgr
    {
        public override void InitGameMode(ScoreSet initScoreSet, PerMoveData perMoveData)
        {
            StartingMoney = initScoreSet.Currency;
            StartingTime = initScoreSet.GameTime;
            GameScoreSet = initScoreSet;
        }

        public override bool PerMove(ScoreSet initScoreSet, PerMoveData perMoveData)
        {
            GameScoreSet.TimePass();
            return GameScoreSet.ChangeCurrency(perMoveData.DeltaCurrency);
        }

        public override bool EndGameCheck(ScoreSet initScoreSet, PerMoveData perMoveData)
        {
            return (GetCurrency() < 0) || (GetGameTime() <= 0);
        }
    }

    public class InfiniteGameStateMgr : GameStateMgr
    {
        //TODO 这个东西的界面表现可以再优化一下。
        public override void InitGameMode(ScoreSet initScoreSet, PerMoveData perMoveData)
        {
            StartingMoney = initScoreSet.Currency;
            StartingTime = initScoreSet.GameTime;
            GameScoreSet = initScoreSet;
        }

        public override bool PerMove(ScoreSet initScoreSet, PerMoveData perMoveData)
        {
            return true;
        }

        public override bool EndGameCheck(ScoreSet initScoreSet, PerMoveData perMoveData)
        {
            return false;
        }
    }
}
