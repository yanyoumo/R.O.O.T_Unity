using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public abstract class CompositeLEDArray : MonoBehaviour
    {
        public LEDArray SignalFlowLEDA;
        public LEDArray SignalFlowLEDB;
        
        public Color Signal_A_Col
        {
            set => SignalFlowLEDA.LEDColor = value;
        }

        public Color Signal_B_Col
        {
            set => SignalFlowLEDB.LEDColor = value;
        }
        
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