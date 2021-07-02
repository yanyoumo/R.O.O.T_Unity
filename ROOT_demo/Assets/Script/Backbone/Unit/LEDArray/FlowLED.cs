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
            if (!gameObject.activeSelf)
            {
                return;
            }
            
            SignalFlowLEDA.Val = inORout ? 1 : -1;
            SignalFlowLEDB.Val = inORout ? 1 : -1;
            
            if (SignalFlowLEDA is SingleFlowLED sfLEDA)
            {
                sfLEDA.SetupLEDColor(ColorLibManager.Instance.ColorLib.ROOT_MASTER_INFO);
            }
            
            if (SignalFlowLEDB is SingleFlowLED sfLEDB)
            {
                sfLEDB.SetupLEDColor(ColorLibManager.Instance.ColorLib.ROOT_MASTER_INFO);
            }
            
            //TODO 会报一个这样的问题。（？）
            //Coroutine couldn't be started because the the game object 'DirectionalFlowLED' is inactive!
            StartCoroutine(Timer(duration));
        }
    }
}