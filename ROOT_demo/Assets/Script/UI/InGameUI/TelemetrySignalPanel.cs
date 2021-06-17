using System;
using com.ootii.Messages;
using ROOT.Common;
using ROOT.Message;
using ROOT.Message.Inquiry;
using ROOT.SetupAsset;
using TMPro;
using UnityEngine;

namespace ROOT.UI
{
    public sealed class TelemetrySignalPanel : RoundRelatedUIBase
    {
        protected override UITag UITag => UITag.SignalPanel_Telemetry;

        private void BoardSignalUpdatedHandler(IMessage rmMessage)
        {
            if (rmMessage is BoardSignalUpdatedInfo info)
            {
                UpdateCachedData(info.SignalData);
                UpdateNumbersCore();
            }
        }
        
        private void SetupSignalType(SignalType signalTypeA, SignalType signalTypeB)
        {
            NormalSignal.color = ColorLibManager.Instance.GetColorBySignalType(signalTypeA);
            NormalTierText.color = ColorLibManager.Instance.GetColorBySignalType(signalTypeA);
            NetworkSignal.color = ColorLibManager.Instance.GetColorBySignalType(signalTypeB);
            NetworkTierText.color = ColorLibManager.Instance.GetColorBySignalType(signalTypeB);
        }

        protected override void Awake()
        {
            base.Awake();
            MessageDispatcher.AddListener(WorldEvent.BoardSignalUpdatedEvent,BoardSignalUpdatedHandler);
            MessageDispatcher.SendMessage(new CurrentSignalTypeInquiryData
            {
                CurrentSignalCallBack = SetupSignalType,
            });
            _cachedData = new BoardSignalUpdatedData();
        }

        protected override void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.BoardSignalUpdatedEvent,BoardSignalUpdatedHandler);
            base.OnDestroy();
        }

        private void UpdatePauseTag(bool _telemetryPaused,bool _istelemetryStage)
        {
            PausedTag.enabled = _telemetryPaused && _istelemetryStage;
        }

        private void UpdateIsTelemetry(bool _istelemetryStage)
        {
            if (_istelemetryStage)
            {
                NormalSignal.enabled = false;
                NetworkSignal.enabled = false;
                MissionTarget.enabled = false;
                NormalTierText.enabled = false;
                NetworkTierText.enabled = false;
                MissionTarget_Big.enabled = true;
                SignalText.enabled = true;
            }
            else
            {
                NormalSignal.enabled = true;
                NetworkSignal.enabled = true;
                MissionTarget.enabled = true;
                NormalTierText.enabled = true;
                NetworkTierText.enabled = true;
                MissionTarget_Big.enabled = false;
                SignalText.enabled = false;
            }
        }

        public TextMeshPro NormalSignal;
        public TextMeshPro NetworkSignal;
        public TextMeshPro MissionTarget;
        public TextMeshPro NormalTierText;
        public TextMeshPro NetworkTierText;

        public TextMeshPro MissionTarget_Big;
        public TextMeshPro SignalText;
        public TextMeshPro PausedTag;

        private readonly Func<int, string> _padding = Common.Utils.PaddingNum2Digit;

        private BoardSignalUpdatedData _cachedData;//这种缓存机制还是得有。

        private void UpdateCachedData(BoardSignalUpdatedData inComingData, int index)
        {
            if (inComingData[index] != int.MaxValue) _cachedData[index] = inComingData[index];
        }
        
        private void UpdateCachedData(BoardSignalUpdatedData inComingData)
        {
            _cachedData.TelemetryPaused = inComingData.TelemetryPaused;
            _cachedData.IsTelemetryStage = inComingData.IsTelemetryStage;
            
            for (var i = 0; i < 10; i++)
            {
                UpdateCachedData(inComingData, i);
            }
        }

        private void UpdateNumbersCore()
        {
            NormalSignal.text = _padding(_cachedData.CrtTypeASignal) + "/" + _padding(_cachedData.TgtTypeASignal);
            NetworkSignal.text = _padding(_cachedData.CrtTypeBSignal) + "/" + _padding(_cachedData.TgtTypeBSignal);
            MissionTarget.text = "[" + _padding(_cachedData.CrtMission) + "]";
            MissionTarget_Big.text = "[" + _padding(_cachedData.CrtMission) + "]";
            NormalTierText.text = "[" + _padding(_cachedData.TypeATier) + "]";
            NetworkTierText.text = "[" + _padding(_cachedData.TypeBTier) + "]";
            SignalText.text = Common.Utils.PaddingNum4Digit(_cachedData.InfoCounter) + "/" + Common.Utils.PaddingNum4Digit(_cachedData.InfoTarget);

            UpdateIsTelemetry(_cachedData.IsTelemetryStage);
            UpdatePauseTag(_cachedData.TelemetryPaused,_cachedData.IsTelemetryStage);
        }
    }
}