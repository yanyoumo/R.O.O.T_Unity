using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public abstract class CompositeLEDArray : MonoBehaviour
    {
        public LEDArray SignalFlowLEDA;
        public LEDArray SignalFlowLEDB;
        
        public int Signal_A_Val
        {
            set => SignalFlowLEDA.Val = value;
        }
        public int Signal_B_Val
        {
            set => SignalFlowLEDB.Val = value;
        }

        public abstract void Blink(float duration, bool inORout = true);
    }
}