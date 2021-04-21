using System;
using System.Collections.Generic;
using com.ootii.Messages;
using UnityEngine;
using ROOT.Common;
using ROOT.Message;

namespace ROOT.UI
{
    public class HighLightingFakeUI : MonoBehaviour
    {
        private Dictionary<UITag, Tuple<Vector3, Vector3>> UITransLib = new Dictionary<UITag, Tuple<Vector3, Vector3>>
        {
            {UITag.Currency, new Tuple<Vector3, Vector3>(new Vector3(-4.3827f,0.0f,4.9814f), new Vector3(7.723504f,2.637291f,1.0f))},
            {UITag.Currency_Acquiring, new Tuple<Vector3, Vector3>(new Vector3(-3.4393f,0.0f,5.0412f), new Vector3(12.5594f,3.118955f,1.0f))},
            {UITag.TimeLine, new Tuple<Vector3, Vector3>(new Vector3(7.18f,0.0f,4.18f), new Vector3(15.31289f,4.711506f,1.0f))},
            {UITag.SignalPanel_Acquiring, new Tuple<Vector3, Vector3>(new Vector3(-0.6117f,0.0f,-2.9898f), new Vector3(12.54302f,3.068625f,1.0f))},
            {UITag.SignalPanel_Telemetry, new Tuple<Vector3, Vector3>(new Vector3(-0.6117f,0.0f,-2.9898f), new Vector3(12.54302f,3.068625f,1.0f))},
        };
            
        public SpriteRenderer CurtainImage;
        public Transform CarveOut;

        private void HighLightingUIChangedHandler(IMessage rMessage)
        {
            if (rMessage is HighLightingUIChangedData data)
            {
                try
                {
                    CarveOut.localPosition = UITransLib[data.uiTag].Item1;
                    CarveOut.localScale = UITransLib[data.uiTag].Item2;
                }
                catch (KeyNotFoundException)
                {
                    Debug.LogError("Key " + data.uiTag + " is not present in 2D pos Lib, please add.");
                }
                CurtainImage.enabled = data.Toggle;
            }
        }
        
        private void Awake()
        {
            CurtainImage.enabled = false;
            MessageDispatcher.AddListener(WorldEvent.HighLightingUIChangedEvent,HighLightingUIChangedHandler);
        }

        private void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.HighLightingUIChangedEvent,HighLightingUIChangedHandler);
        }
    }
}