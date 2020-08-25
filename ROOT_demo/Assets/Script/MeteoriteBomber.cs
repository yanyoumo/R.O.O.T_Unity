using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
        WarningDestoryerStatus GetStatus { get; }
        Color GetWaringColor { get; }
        Vector2Int[] NextStrikingPos(out int count);
        void Init(int counterLoopMedian = 4, int counterLoopVariance = 1);
        void RequestUpStrikeCount(int requestAmount);//外界可以申请对这一系统的攻击力度加码。
        void Step();
        void Step(out CoreType? destoryedCore);
    }

    public class MeteoriteBomber: IWarningDestoryer
    {
        private const float ComplexModeRandomRatio = 0.2f;
        public Board GameBoard;
        public int NextStrikingCount { internal set; get; }

        //TODO 具体的提高Strike数据的逻辑还没定，现在就很简单的每4次加一次。
        public const int NextStrikeUpCounter = 4;
        public int HasStrikedTimes { private set; get; }//这个是攻击了多少次
        public int HasStrikedCount { private set; get; }//这个是攻击了多少发（可能一次好几发）


        private WarningDestoryerStatus Status;

        public int Counter { private set; get; }
        public int CounterLoopMedian { private set; get; }
        public int CounterLoopVariance { private set; get; }
        public const int MinLoopStep=3;

        public Vector2Int[] NextIncomes { private set; get; }

        internal void ForceSetDestoryer(Vector2Int nextIncome)
        {
            NextStrikingCount = 1;
            NextIncomes = new[] { nextIncome };
        }

        private void GenerateNewIncomes()
        {
            NextIncomes = new Vector2Int[NextStrikingCount];
            for (int i = 0; i < NextStrikingCount; i++)
            {
                //NextIncomes[i] = PureRandomTarget();
                NextIncomes[i] = ComplexRandomTarget();
            }
        }

        public void RequestUpStrikeCount(int requestAmount)
        {
            //这个函数肯定是异步调的，这里要保证这个东西不乱加。
        }

        public void Init(int counterLoopMedian = 4, int counterLoopVariance = 1)
        {
            NextStrikingCount = 1;
            CounterLoopMedian = counterLoopMedian;
            CounterLoopVariance = counterLoopVariance;

            HasStrikedTimes = 0;
            HasStrikedCount = 0;

            Counter = GenerateNextLoop();
            Status = WarningDestoryerStatus.Dormant;
            NextIncomes = new Vector2Int[NextStrikingCount];
            GenerateNewIncomes();
            Debug.Assert(GameBoard);
        }

        private int GenerateNextLoop()
        {
            int variance=Mathf.FloorToInt(Random.Range(-CounterLoopVariance, CounterLoopVariance));
            return Mathf.Max(CounterLoopMedian + variance, MinLoopStep);
        }

        private Vector2Int ComplexRandomTarget()
        {
            return Random.value <= ComplexModeRandomRatio ? PureRandomTarget() : (RandomUnitTarget() ?? PureRandomTarget());
        }

        /// <summary>
        /// 选出随机单元作为单元
        /// </summary>
        /// <returns>如果棋盘上没有单位，则返回null</returns>
        private Vector2Int? RandomUnitTarget()
        {
            if (GameBoard.RandomUnit == null)
            {
                return null;
            }
            else
            {
                return GameBoard.RandomUnit.CurrentBoardPosition;
            }
        }

        private Vector2Int PureRandomTarget()
        {
            int randX=Mathf.FloorToInt(GameBoard.BoardLength * Random.value); 
            int randY=Mathf.FloorToInt(GameBoard.BoardLength * Random.value);
            return new Vector2Int(randX, randY);
        }

        private void UpdateNextStrikingCount()
        {
            NextStrikingCount = Mathf.FloorToInt(HasStrikedTimes / (float) NextStrikeUpCounter)+1;
        }

        public virtual void Step()
        {
            Step(out CoreType? destoryedCore);
        }

        public virtual void Step(out CoreType? destoryedCore)
        {
            destoryedCore = null;
            //这里计算的节奏意外地关键。
            if (Counter == 0)
            {
                HasStrikedTimes++;
                HasStrikedCount += NextStrikingCount;
                UpdateNextStrikingCount();
                Counter = GenerateNextLoop();
                //Debug.Log("Aiming=" + NextIncome.ToString());
                foreach (var nextIncome in NextIncomes)
                {
                    //GameBoard.TryDeleteCertainUnit(nextIncome, out destoryedCore);
                    GameBoard.TryDeleteCertainNoStationUnit(nextIncome, out destoryedCore);
                }
                //得摧毁之后才更新数据。
                GenerateNewIncomes();
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
                //TODO "Striked waiting for next" State.   
                Status = WarningDestoryerStatus.Dormant;
            }
            else
            {
                Status = WarningDestoryerStatus.Dormant;
            }
        }

        public virtual WarningDestoryerStatus GetStatus => Status;

        public Color GetWaringColor
        {
            get
            {
                switch (Status)
                {
                    case WarningDestoryerStatus.Warning:
                        return Color.yellow;
                    case WarningDestoryerStatus.Striking:
                        ColorUtility.TryParseHtmlString("#FF3300", out Color col);
                        return col;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public virtual Vector2Int[] NextStrikingPos(out int count)
        {
            count = NextStrikingCount;
            return NextIncomes;
        }

        public void SetBoard(ref Board gameBoard)
        {
            GameBoard = gameBoard;
        }
    }
}