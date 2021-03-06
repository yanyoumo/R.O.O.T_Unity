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
            {UITag.Currency_BareBone, new Tuple<Vector3, Vector3>(new Vector3(-4.3827f,0.0f,4.9814f), new Vector3(7.723504f,2.637291f,1.0f))},
            {UITag.Currency_Career, new Tuple<Vector3, Vector3>(new Vector3(-4.3827f,0.0f,4.9814f), new Vector3(7.723504f,2.637291f,1.0f))},
            {UITag.Currency_Acquiring, new Tuple<Vector3, Vector3>(new Vector3(-3.4393f,0.0f,5.0412f), new Vector3(12.5594f,3.118955f,1.0f))},
            {UITag.TimeLine, new Tuple<Vector3, Vector3>(new Vector3(7.18f,0.0f,4.18f), new Vector3(15.31289f,4.711506f,1.0f))},
            {UITag.SignalPanel_Acquiring, new Tuple<Vector3, Vector3>(new Vector3(-0.6117f,0.0f,-2.9898f), new Vector3(12.54302f,3.068625f,1.0f))},
            {UITag.SignalPanel_Telemetry, new Tuple<Vector3, Vector3>(new Vector3(-0.6117f,0.0f,-2.9898f), new Vector3(12.54302f,3.068625f,1.0f))},
            {UITag.MainBoard, new Tuple<Vector3, Vector3>(new Vector3(-0.6475f,0.0f,0.9712f), new Vector3(18.17862f,18.2992f,1.0f))},
            {UITag.Shop, new Tuple<Vector3, Vector3>(new Vector3(7.166f,0.0f,0.1978f), new Vector3(16.87271f,15.77461f,1.0f))},
            {UITag.Skill, new Tuple<Vector3, Vector3>(new Vector3(7.166f,0.0f,0.1978f), new Vector3(16.87271f,15.77461f,1.0f))},
        };
            
        public SpriteRenderer CurtainImage;
        public SpriteRenderer CarveOutStoke;
        public Transform CarveOut;

        private void HighLightingUIChangedHandler(IMessage rMessage)
        {
            if (rMessage is HighLightingUIChangedData data)
            {
                if (data.CustomBool)
                {
                    CarveOut.localPosition = new Vector3(data.pos.x, 0.0f, data.pos.y);
                    CarveOut.localScale = new Vector3(data.scale.x, data.scale.y, 1.0f);
                }
                else
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
                }

                CurtainImage.enabled = data.Toggle;
                CarveOutStoke.enabled = data.Toggle;
            }
        }

        private void Awake()
        {
            CurtainImage.enabled = false;
            CarveOutStoke.enabled = false;
            MessageDispatcher.AddListener(WorldEvent.HighLightingUIChangedEvent,HighLightingUIChangedHandler);
        }

        private void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.HighLightingUIChangedEvent,HighLightingUIChangedHandler);
        }
    }
}