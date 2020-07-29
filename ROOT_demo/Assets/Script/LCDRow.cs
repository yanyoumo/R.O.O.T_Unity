using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class LCDRow : MonoBehaviour
    {
        //public int StringInt;
        public LCD Digit0;
        public LCD Digit1;
        public LCD Digit2;
        public LCD Digit3;

        private LCD[] lcdS;
        // Start is called before the first frame update
        void Awake()
        {
            lcdS = new[] {Digit0, Digit1, Digit2, Digit3};
        }

        public void SetNumber(int number, bool posOrNeg = true)
        {
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

        void Update()
        {
            //TODO 可以弄一个减数的动画；但是和结果没啥关系。
            //SetNumber(Time.frameCount);
        }
    }
}