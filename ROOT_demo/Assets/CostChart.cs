using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ROOT
{
    public class CostChart : MonoBehaviour
    {
        public TextMeshPro Income;
        public TextMeshPro PresentCost;
        public TextMeshPro PendingCost;
        public TextMeshPro Benefit;

        public int IncomeVal
        {
            set => Income.text = Utils.PaddingNum3Digit(value);
        }

        public int CostVal
        {
            set => PresentCost.text = Utils.PaddingNum3Digit(value);
        }

        public int BenefitVal
        {
            set
            {
                if (value >= 0)
                {
                    Benefit.text = Utils.PaddingNum3Digit(value);
                    Benefit.color = Color.green;
                }
                else
                {
                    Benefit.text = "-" + Utils.PaddingNum2Digit(Math.Abs(value));
                    Benefit.color = Color.red;
                }
            }
        }
    }
}