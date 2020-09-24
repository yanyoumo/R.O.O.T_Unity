using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//using MBInstructList= System.Collections.Generic.List<ROOT.MeteoriteBomberInstruction>;
using Random = UnityEngine.Random;

/*
namespace ROOT
{
    public enum BomberStage
    {
        Idling,
        Strike,
    }

    public abstract class MeteoriteBomberInstruction
    {
        public abstract BomberStage GetBomberStage { get; }
    }

    public class MeteoriteBomberIdle : MeteoriteBomberInstruction
    {
        public override BomberStage GetBomberStage => BomberStage.Idling;
        public int IdlePeriod;
    }

    public class MeteoriteBomberStrike : MeteoriteBomberInstruction
    {
        public override BomberStage GetBomberStage => BomberStage.Strike;
        public int StrikeCount;
    }

    internal static class MeteoriteBomberInstructor
    {
        private const int MaxCount = 150;
        private const int MaxStrikeCount = 3;
        private const int MinLoopStep = 5;

        //这么搞还有一个好处，就是这个可以放到一开始的协程里面去弄。
        public static MBInstructList GenerateInstructionArray(int counterLoopMedian = 4,
            int counterLoopVariance = 1)
        {
            var res = new MBInstructList();

            bool IdleOrStrike = false;
            for (var i = 0; i < MaxCount; i++)
            {
                if (IdleOrStrike)
                {
                    var tmp = new MeteoriteBomberIdle {IdlePeriod = 3};
                    res.Add(tmp);
                }
                else
                {
                    var tmp = new MeteoriteBomberStrike {StrikeCount = 2};
                    res.Add(tmp);
                }

                IdleOrStrike = !IdleOrStrike;
            }

            return res;
        }
    }

    public class MeteoriteBomberNeo
    {
        private MBInstructList list;
        private int listIdx = 0;
        private int idleCounter = 0;

        public Board GameBoard { get; set; }
        private WarningDestoryerStatus _status;
        public WarningDestoryerStatus GetStatus => _status;

        public Color GetWaringColor
        {
            get
            {
                switch (_status)
                {
                    case WarningDestoryerStatus.Warning:
                        return ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_DESTORYER_WARNING);
                    case WarningDestoryerStatus.Striking:
                        return ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_DESTORYER_STRIKING);
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public Vector2Int[] NextStrikingPos(out int count)
        {
            throw new System.NotImplementedException();
        }

        public void Init(int counterLoopMedian = 4, int counterLoopVariance = 1)
        {
            list = MeteoriteBomberInstructor.GenerateInstructionArray(counterLoopMedian, counterLoopVariance);
        }

        private float[] RandomMulexRatio = new[]
        {
            0.2f,
            0.45f,
            0.35f
        };

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
            int randX = Mathf.FloorToInt(GameBoard.BoardLength * Random.value);
            int randY = Mathf.FloorToInt(GameBoard.BoardLength * Random.value);
            return new Vector2Int(randX, randY);
        }

        private Vector2Int? RandomHeatSinkUnitTarget()
        {
            var oHunit = GameBoard.OverlapHeatSinkUnit;
            if (oHunit != null)
            {
                return Utils.RandomItem(oHunit.Select(unit => unit.CurrentBoardPosition));
            }
            else
            {
                return null;
            }
        }

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
                if ((!vector2Int.HasValue) || (dic.ContainsKey(vector2Int.Value)))
                    continue;
                dic.Add(vector2Int.Value, RandomMulexRatio[i]);
                totalRatio += RandomMulexRatio[i];
            }

            ;
            if (dic.Count == 1)
                return dic.Keys.ToArray()[0];

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

        private CoreType? Strike(int count)
        {
            CoreType? res = null;
            for (int i = 0; i < count; i++)
            {
                GameBoard.TryDeleteCertainUnit(ComplexRandomTarget(), out res);
            }

            return res;
        }

        public void Step()
        {
            Step(out var tmp);
        }

        public void Step(out CoreType? destoryedCore)
        {
            destoryedCore = null;
            if (idleCounter != 0)
            {
                idleCounter--;
                if (idleCounter == 1)
                {
                    _status = WarningDestoryerStatus.Warning;
                }
                else if (idleCounter == 0)
                {
                    _status = WarningDestoryerStatus.Striking;
                }
                else if (idleCounter > 1)
                {
                    _status = WarningDestoryerStatus.Dormant;
                }
                else
                {
                    _status = WarningDestoryerStatus.Dormant;
                }
            }
            else
            {
                var instr = list[listIdx];
                switch (instr.GetBomberStage)
                {
                    case BomberStage.Idling:
                        idleCounter = ((MeteoriteBomberIdle) instr).IdlePeriod;
                        break;
                    case BomberStage.Strike:
                        destoryedCore = Strike(((MeteoriteBomberStrike) instr).StrikeCount);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                listIdx++;
            }
        }
    }
}*/