using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace ROOT
{
    public class CostChart : MonoBehaviour
    {
        public TextMeshPro Currency;
        public TextMeshPro Incomes;
        private int _incomesVal;
        private int _currencyVal;

        [ReadOnly]
        public int IncomesVal
        {
            set
            {
                _incomesVal = value;
                UpdateIncomeVal();
            }
            get => _incomesVal;
        }

        [ReadOnly]
        public int CurrencyVal
        {
            set
            {
                _currencyVal = value;
                Currency.text = Utils.PaddingNum(_currencyVal, 4);
            }
            get => _currencyVal;
        }

        private bool active = true;

        private void UpdateIncomeVal()
        {
            if (IncomesVal > 0)
            {
                Incomes.text = Utils.PaddingNum(IncomesVal,3);
                Incomes.color = Color.green;
            }
            else if (IncomesVal == 0)
            {
                Incomes.text = "000";
                Incomes.color = Color.red;
            }
            else
            {
                Incomes.text = "-" + Utils.PaddingNum(Math.Abs(IncomesVal),2);
                Incomes.color = Color.red;
            }
        }

        public bool Active
        {
            set
            {
                active = value;
                if (active)
                {
                    UpdateIncomeVal();
                }
                else
                {
                    Incomes.text = "---";
                    Incomes.color = Color.black;
                }
            }
            get => active;
        }
    }
}