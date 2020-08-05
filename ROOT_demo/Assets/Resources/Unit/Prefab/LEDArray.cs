using System.Collections;
using System.Collections.Generic;
using ROOT;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
    public class LEDArray : MonoBehaviour
    {
        [ShowInInspector]
        public SignalLED[] _LEDArray;

        public int Val
        {
            set
            {
                for (var i1 = 0; i1 < _LEDArray.Length; i1++)
                {
                    if (i1 >= value)
                    {
                        _LEDArray[i1].TurnOff();
                    }
                    else
                    {
                        _LEDArray[i1].TurnOn();
                    }
                }
            }
        }

        void Awake()
        {
            Val = 0;
        }
    }
}