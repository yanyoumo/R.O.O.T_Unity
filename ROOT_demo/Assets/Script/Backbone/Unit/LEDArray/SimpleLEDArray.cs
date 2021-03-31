using System.Collections;
using System.Collections.Generic;
using ROOT;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT
{
    public abstract class LEDArray : MonoBehaviour
    {
        public Color LEDColor;
        [ShowInInspector]
        public SingleLED[] _LEDArray;
        public abstract int Val { set; }
    }

    public class SimpleLEDArray : LEDArray
    {
        public override int Val
        {
            //Val=[1,5]
            set
            {
                for (var i1 = 0; i1 < _LEDArray.Length; i1++)
                {
                    if (i1 < value)
                    {
                        _LEDArray[i1].TurnOn();
                    }
                    else
                    {
                        _LEDArray[i1].TurnOff();
                    }
                }
            }
        }

        void Awake()
        {
            Val = 3;
            _LEDArray.ForEach(LED => LED.TurnOnColor = LEDColor);
        }
    }
}