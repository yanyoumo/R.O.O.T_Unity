using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public sealed class ScoreSet
    {
        public float Currency;
        public int GameTime;

        public ScoreSet()
        {
            Currency = 0;
            GameTime = 0;
        }

        public ScoreSet(float initCurrency,int initTime)
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

        public abstract void InitGameMode(ScoreSet initScoreSet, PerMoveData perMoveData);
        public abstract bool PerMove(ScoreSet initScoreSet, PerMoveData perMoveData);
        public abstract bool EndGameCheck(ScoreSet initScoreSet, PerMoveData perMoveData);
    }

    public class StandardGameStateMgr : GameStateMgr
    {
        public override void InitGameMode(ScoreSet initScoreSet, PerMoveData perMoveData)
        {
            StartingMoney = initScoreSet.Currency;
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
}
