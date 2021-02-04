using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT.Signal
{
    public class ThermoUnitSignalCore : UnitSignalCoreBase
    {
        public override SignalType SignalType => SignalType.Thermo;

        private bool isValidPos(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < Board.BoardLength && pos.y >= 0 && pos.y < Board.BoardLength;
        }
        public override List<Vector2Int> SingleInfoCollectorZone
        {
            get
            {
                //TODO
                var res = new List<Vector2Int>();
                return res;
            }
        }

        private float getScoreFromPercentage(float x)
        {
            float k = 1, b = 1;
            return k * x + b;
        }

        public override bool IsUnitActive => HasCertainSignal(SignalType);

        public override float SingleUnitScore
        {
            get
            {
                var sum = 0;
                var counting = 0;
                foreach (var pattern in Utils.GetPixelateCircle_Tier(Owner.Tier).PatternList)
                {
                    var currentPos = Owner.CurrentBoardPosition + pattern;
                    if (isValidPos(currentPos))
                    {
                        ++sum;
                        if (Owner.GameBoard.CheckBoardPosValidAndEmpty(currentPos))
                            ++counting;
                    }
                }

                Debug.Log("Sum:"+sum+" counting:"+counting);
                return getScoreFromPercentage(1f*counting/sum);
            }
        }
    }
}
