using com.ootii.Messages;
using ROOT.Clock;
using ROOT.Message;
using ROOT.SetupAsset;
using UnityEngine;

namespace ROOT
{
    public class SingleFlowLED : LEDArray
    {
        public MeshRenderer PosXArrow_Tex;
        public MeshRenderer NegXArrow_Tex;

        private Vector3 MaxArrowScale = new Vector3(0.33f, 0.33f, 1.0f);
        private Vector3 MinArrowScale = new Vector3(0.1f, 0.1f, 1.0f);

        private int minVal = 1;
        private int maxVal = 10;

        private Color _cachedColor;
        private Color blinkingColor => ColorLibManager.Instance.ColorLib.ROOT_MASTER_INFO;
        
        /*public void SetupLEDColor(Color col)
        {
            LEDColor = col;
            PosXArrow_Tex.sharedMaterial.color = LEDColor;
            NegXArrow_Tex.sharedMaterial.color = LEDColor;
        }*/

        private void BoardSignalUpdatedHandler(IMessage rMessage)
        {
            if (rMessage is BoardSignalUpdatedInfo info)
            {
                if (info.SignalData.IsTelemetryStage.HasValue && info.SignalData.IsTelemetryStage.Value)
                {
                    if (info.SignalData.TelemetryPaused.HasValue && info.SignalData.TelemetryPaused.Value)
                    {
                        PosXArrow_Tex.sharedMaterial.color = _cachedColor;
                        NegXArrow_Tex.sharedMaterial.color = _cachedColor;
                    }

                    if (!info.SignalData.TelemetryPaused.HasValue || !info.SignalData.TelemetryPaused.Value)
                    {
                        PosXArrow_Tex.sharedMaterial.color = blinkingColor;
                        NegXArrow_Tex.sharedMaterial.color = blinkingColor;
                    }
                }
            }
        }

        private void Awake()
        {
            PosXArrow_Tex.material = new Material(PosXArrow_Tex.material);
            NegXArrow_Tex.material = new Material(NegXArrow_Tex.material);
            PosXArrow_Tex.sharedMaterial.color = LEDColor;
            NegXArrow_Tex.sharedMaterial.color = LEDColor;
            _cachedColor = LEDColor;
            MessageDispatcher.AddListener(WorldEvent.BoardSignalUpdatedEvent,BoardSignalUpdatedHandler);
        }

        private void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.BoardSignalUpdatedEvent,BoardSignalUpdatedHandler);
        }

        private void Update()//TODO 到时候更精巧的一些就是这里的流程用DOTween改修一下。
        {
            if (MasterClock.Instance.GamePausedStatus) return;
            PosXArrow_Tex.sharedMaterial.mainTextureOffset = new Vector2(-0.01f * Time.frameCount, 0.0f);
            NegXArrow_Tex.sharedMaterial.mainTextureOffset = new Vector2(-0.01f * Time.frameCount, 0.0f);
        }

        public override int Val
        {
            set
            {
                if (value==0)
                {
                    //目前是要用==0来关掉的。
                    PosXArrow_Tex.enabled = false;
                    NegXArrow_Tex.enabled = false;
                }else if (value > 0)
                {
                    PosXArrow_Tex.enabled = true;
                    NegXArrow_Tex.enabled = false;
                }
                else
                {
                    PosXArrow_Tex.enabled = false;
                    NegXArrow_Tex.enabled = true;
                }
            }
        }
    }
}