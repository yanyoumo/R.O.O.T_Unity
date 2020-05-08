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

        public void SetNumber(int number,bool _PosOrNeg = true)
        {
            number = number % 10000;
            Digit0.PosOrNeg = _PosOrNeg;
            Digit1.PosOrNeg = _PosOrNeg;
            Digit2.PosOrNeg = _PosOrNeg;
            Digit3.PosOrNeg = _PosOrNeg;

            Digit3.SetDigit(-1);
            Digit2.SetDigit(-1);
            Digit1.SetDigit(-1);
            Digit0.SetDigit(-1);

            Digit3.SetDigit(Mathf.FloorToInt(number % 10));
            if (number > 9)
            {
                Digit2.SetDigit(Mathf.FloorToInt(number / 10) % 10);
                if (number > 99)
                {
                    Digit1.SetDigit(Mathf.FloorToInt(number / 100) % 10);
                    if (number > 999)
                    {
                        Digit0.SetDigit(Mathf.FloorToInt(number / 1000));
                    }
                }
            }
        }
    }
}