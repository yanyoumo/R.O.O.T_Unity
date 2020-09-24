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
        public TextMeshPro Text;
        [ReadOnly] public int Val;

        private bool active = true;

        public bool Active
        {
            set
            {
                active = value;
                if (active)
                {
                    if (Val > 0)
                    {
                        Text.text = Utils.PaddingNum3Digit(Val);
                        Text.color = Color.green;
                    }
                    else if (Val == 0)
                    {
                        Text.text = "000";
                        Text.color = Color.red;
                    }
                    else
                    {
                        Text.text = "-" + Utils.PaddingNum2Digit(Math.Abs(Val));
                        Text.color = Color.red;
                    }
                }
                else
                {
                    Text.text = "---";
                    Text.color = Color.black;
                }
            }
            get => active;
        }
    }
}