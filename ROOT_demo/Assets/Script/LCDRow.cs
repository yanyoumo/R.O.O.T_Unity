using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class LCDRow : MonoBehaviour
    {
        private bool Animating = false;
        private int lastNum = 0;
        //public int StringInt;
        public LCD Digit0;
        public LCD Digit1;
        public LCD Digit2;
        public LCD Digit3;

        private LCD[] lcdS;

        private readonly float AnimationDuration = 0.3f;
        private float AnimationTimerOrigin=0.0f;
        private float animationTimer => Time.time - AnimationTimerOrigin;
        private float AnimationLerper
        {
            get
            {
                float res = animationTimer / AnimationDuration;
                return Mathf.Min(res, 1.0f);
            }
        }
        // Start is called before the first frame update
        void Awake()
        {
            lcdS = new[] {Digit0, Digit1, Digit2, Digit3};
        }

        IEnumerator SetAniNumberCo(int number)
        {
            AnimationTimerOrigin = Time.time;
            while (AnimationLerper<1.0f)
            {
                yield return 0;
                var val = Mathf.FloorToInt(Mathf.Lerp((float) lastNum, (float) number, AnimationLerper));
                SetNumber(val);
            }
            Animating = false;
            lastNum = number;
            SetNumber(number);
        }

        public void SetAniNumber(int nextNumber)
        {
            //Debug.Log("Here");
            if (nextNumber!=lastNum&&!Animating)
            {
                Animating = true;
                StartCoroutine(SetAniNumberCo(nextNumber));
            }
        }

        public void SetNumber(int number)
        {
            lastNum = number;
            bool posOrNeg = (number >= 0);
            number = Math.Abs(number);
            if (number == 0)
            {
                lcdS[0].SetDigit(-1);
                lcdS[1].SetDigit(-1);
                lcdS[2].SetDigit(-1);
                lcdS[3].SetDigit(0);
                lcdS[3].PosOrNeg = false;
            }
            else
            {
                number %= 10000;
                for (var i = 0; i < lcdS.Length; i++)
                {
                    lcdS[3 - i].PosOrNeg = posOrNeg;
                    bool upper = (number >= Mathf.Pow(10, i));
                    int val = Mathf.FloorToInt((number / Mathf.Pow(10, i))) % 10;
                    lcdS[3 - i].SetDigit(upper ? val : -1);
                }
            }
        }
    }
}