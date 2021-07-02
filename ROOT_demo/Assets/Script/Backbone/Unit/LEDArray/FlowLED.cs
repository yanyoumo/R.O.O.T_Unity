using System.Collections;
using System.Collections.Generic;
using Doozy;
using ROOT.SetupAsset;
using UnityEngine;

namespace ROOT
{
    public class FlowLED : CompositeLEDArray
    {
        private IEnumerator Timer(float duration)
        {
            yield return new WaitForSeconds(duration);
            SignalFlowLEDA.Val = 0;
            SignalFlowLEDB.Val = 0;
        }
        
        public override void Blink(float duration, bool inORout)
        {
            SignalFlowLEDA.Val = inORout ? 1 : -1;
            SignalFlowLEDB.Val = inORout ? 1 : -1;
            StartCoroutine(Timer(duration));
        }
    }
}