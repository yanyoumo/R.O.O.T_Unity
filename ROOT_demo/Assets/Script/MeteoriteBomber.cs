using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public enum WarningDestoryerStatus
    {
        Dormant,
        Warning,
        Striking
    }

    public interface IWarningDestoryer
    {
        void SetBoard(ref Board gameBoard);
        WarningDestoryerStatus GetStatus();
        Vector2Int[] NextStrikingPos(out int count);
        void Init(int counterLoopMedian = 4, int counterLoopVariance = 1);
        void Step();
    }

    public class MeteoriteBomber: IWarningDestoryer
    {
        public Board GameBoard;

        private WarningDestoryerStatus Status;

        public int Counter { private set; get; }
        public int CounterLoopMedian { private set; get; }
        public int CounterLoopVariance { private set; get; }
        public static readonly int MinLoopStep=3;

        public Vector2Int NextIncome { private set; get; }

        public void Init(int counterLoopMedian = 4, int counterLoopVariance = 1)
        {
            CounterLoopMedian = counterLoopMedian;
            CounterLoopVariance = counterLoopVariance;

            Counter = GenerateNextLoop();
            Status = WarningDestoryerStatus.Dormant;
            NextIncome = PureRandomTarget();
            Debug.Assert(GameBoard);
        }

        private int GenerateNextLoop()
        {
            //TODO 到时候得弄一个随机数的库。这个东西分布在设计上很重要。
            int variance=Mathf.FloorToInt(Random.Range(-CounterLoopVariance, CounterLoopVariance));
            return Mathf.Max(CounterLoopMedian + variance, MinLoopStep);
        }

        private Vector2Int PureRandomTarget()
        {
            int randX=Mathf.FloorToInt(GameBoard.BoardLength * Random.value); 
            int randY=Mathf.FloorToInt(GameBoard.BoardLength * Random.value);
            return new Vector2Int(randX, randY);
        }

        public virtual void Step()
        {
            //这里计算的节奏意外地关键。
            if (Counter == 0)
            {
                Counter = GenerateNextLoop();
                //Debug.Log("Aiming=" + NextIncome.ToString());
                GameBoard.TryDeleteCertainUnit(NextIncome);
                //得摧毁之后才更新数据。
                NextIncome = PureRandomTarget();
            }
            else
            {
                Counter--;
            }

            if (Counter==1)
            {
                Status = WarningDestoryerStatus.Warning;
            }
            else if (Counter == 0)
            {
                Status = WarningDestoryerStatus.Striking;
            }
            else if (Counter > 1)
            {
                //TODO: Striked  waiting for next.   
                Status = WarningDestoryerStatus.Dormant;
            }
            else
            {
                Status = WarningDestoryerStatus.Dormant;
            }
        }

        public virtual WarningDestoryerStatus GetStatus()
        {
            return Status;
        }

        public virtual Vector2Int[] NextStrikingPos(out int count)
        {
            count = 1;
            return new[] {NextIncome};
        }

        public void SetBoard(ref Board gameBoard)
        {
            GameBoard = gameBoard;
        }
    }
}