﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
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
        Board GameBoard { set; }
        //void SetBoard(ref Board gameBoard);
        WarningDestoryerStatus GetStatus { get; }
        Color GetWaringColor { get; }
        Vector2Int[] NextStrikingPos(out int count);
        void Init(int counterLoopMedian = 4, int counterLoopVariance = 1);
        void Step();
        void Step(out CoreType? destoryedCore);
        void ForceReset();
    }

    /// <summary>
    /// 这套系统其实问题蛮大的，就是记忆性太强了，这些逻辑不好整理。
    /// </summary>
    public class MeteoriteBomber: IWarningDestoryer
    {
        //现在是每NextStrikeUpCounter的StrikeLevel提高，每次StrikeLevel多攻击一发，中间值和偏移值降低。
        public int HasStrikedTimes { private set; get; }//这个是攻击了多少次
        public int HasStrikedCount { private set; get; }//这个是攻击了多少发（可能一次好几发）

        public const int MaxStrikeCount = 3;
        public const int NextStrikeUpCounter = 2;

        public Board GameBoard { get; set; }
        
        public int NextStrikingCount { internal set; get; }
        private WarningDestoryerStatus Status;

        public int Counter { private set; get; }
        public int StartingMedian { private set; get; }
        public int StartingVariance { private set; get; }

        private int StrikeLevel => Mathf.RoundToInt(HasStrikedTimes / (float) NextStrikeUpCounter);
        private int LoopMedian => Math.Max(StartingMedian - StrikeLevel, MinLoopStep);
        private int LoopVariance => Math.Max(StartingVariance - StrikeLevel, 0);

        public const int MinLoopStep=5;

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
                //只有
                bool noSource = true;
                Vector2Int pos;
                do
                {
                    pos = ComplexRandomTarget();
                    if (GameBoard.UnitsGameObjects.TryGetValue(pos, out var value))
                    {
                        noSource = (value.GetComponentInChildren<Unit>().UnitCoreGenre != CoreGenre.Source);
                    }
                    else
                    {
                        noSource = true;
                    }
                } while (!noSource);

                NextIncomes[i] = pos;
            }
        }
        
        public void Init(int startingMedian = 4, int startingVariance = 1)
        {
            NextStrikingCount = 1;
            StartingMedian = startingMedian;
            StartingVariance = startingVariance;

            HasStrikedTimes = 0;
            HasStrikedCount = 0;

            Counter = GenerateNextLoop();
            Status = WarningDestoryerStatus.Dormant;
            GenerateNewIncomes();
            Debug.Assert(GameBoard);
        }
        
        private int GenerateNextLoop()
        {
            var variance = 0;
            if (LoopVariance != 0)
            {
                variance = Mathf.RoundToInt(Random.Range(-LoopVariance, LoopVariance));
            }
            return Mathf.Max(LoopMedian + variance, MinLoopStep);
        }

        #region 瞄准部分，这次不用改

        private float[] RandomMulexRatio = new[]
        {
            0.2f,
            0.45f,
            0.35f
        };

        private Vector2Int ComplexRandomTarget()
        {
            var rawTargets = new[]
            {
                PureRandomTarget(),
                RandomUnitTarget(),
                RandomHeatSinkUnitTarget(),
            };
            var dic = new Dictionary<Vector2Int, float>();
            var totalRatio = 0.0f;
            for (var i = 0; i < rawTargets.Length; i++)
            {
                var vector2Int = rawTargets[i];
                if ((!vector2Int.HasValue)||(dic.ContainsKey(vector2Int.Value)))
                    continue;
                dic.Add(vector2Int.Value, RandomMulexRatio[i]);
                totalRatio += RandomMulexRatio[i];
            };
            if (dic.Count==1) return dic.Keys.ToArray()[0];

            ShopMgr.NormalizeDicVal(ref dic);

            Vector2Int res;
            const int countMax = 100;
            var count = 0;
            try
            {
                do
                {
                    res = Utils.GenerateWeightedRandom(dic);
                    count++;
                    if (count >= countMax)
                        throw new ArithmeticException();
                } while (res == new Vector2Int(-1, -1) || res == new Vector2Int(-2, -2));
            }
            catch (ArithmeticException)
            {
                Debug.LogWarning("随机生成流程未在规定时间内生成合理结果。");
                res = PureRandomTarget();
            }

            return res;
        }

        /// <summary>
        /// 瞄准压住了HeatSink的单元打
        /// </summary>
        /// <returns>如果棋盘上没有压住HeatSink的单位，则返回null</returns>
        private Vector2Int? RandomHeatSinkUnitTarget()
        {
            var oHunit = GameBoard.OverlapHeatSinkUnit;
            if (oHunit != null && oHunit.Length != 0)
            {
                return Utils.RandomItem(oHunit.Select(unit => unit.CurrentBoardPosition));
            }
            else
            {
                return null;
            }
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

        #endregion

        private void UpdateNextStrikingCount()
        {
            NextStrikingCount = Mathf.FloorToInt(HasStrikedTimes / (float) NextStrikeUpCounter)+1;
            NextStrikingCount = Mathf.Min(NextStrikingCount, MaxStrikeCount);
        }

        public void ForceReset()
        {
            Status = WarningDestoryerStatus.Dormant;
            Counter = GenerateNextLoop();
            GenerateNewIncomes();
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
                foreach (var nextIncome in NextIncomes)
                {
                    //因为商店会销售静态单元，所以又可以摧毁了。
                    GameBoard.TryDeleteCertainUnit(nextIncome, out destoryedCore);
                    //_gameBoard.TryDeleteCertainNoStationUnit(nextIncome, out destoryedCore);
                }
                //得摧毁之后才更新数据。
                GenerateNewIncomes();
            }
            else
            {
                Counter--;
            }

            switch (Counter)
            {
                case 1:
                    Status = WarningDestoryerStatus.Warning;
                    break;
                case 0:
                    Status = WarningDestoryerStatus.Striking;
                    break;
                default:
                    Status = WarningDestoryerStatus.Dormant;
                    break;
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
    }
}