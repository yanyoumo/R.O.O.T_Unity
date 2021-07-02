using System;
using com.ootii.Messages;
using ROOT.Common;
using ROOT.Message.Inquiry;
using ROOT.SetupAsset;
using TMPro;
using UnityEngine;
// ReSharper disable PossibleInvalidOperationException

namespace ROOT.UI
{
    public sealed class TelemetrySignalPanel : RoundRelatedUIBase
    {
        protected override UITag UITag => UITag.SignalPanel_Telemetry;

        public TextMeshPro NormalSignal;
        public TextMeshPro NetworkSignal;
        public TextMeshPro MissionTarget;
        public TextMeshPro NormalTierText;
        public TextMeshPro NetworkTierText;

        public TextMeshPro MissionTarget_Big;
        public TextMeshPro SignalText;
        public TextMeshPro PausedTag;

        private readonly Func<int, string> _padding = Utils.PaddingNum2Digit;

        private BoardSignalUpdatedData _cachedData;//这种缓存机制还是得有。

        private void BoardSignalUpdatedHandler(IMessage rmMessage)
        {
            if (rmMessage is BoardSignalUpdatedInfo info)
            {
                //Debug.Log("BoardSignalUpdatedHandler");
                //Debug.Log("info.SignalData.IsTelemetryStage=" + info.SignalData.IsTelemetryStage);
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
        
        private void UpdateCachedData(BoardSignalUpdatedData inComingData, int index)
        {
            if (inComingData[index] != int.MaxValue) _cachedData[index] = inComingData[index];
        }
        
        private void UpdateCachedData(BoardSignalUpdatedData inComingData)
        {
            if (inComingData.TelemetryPaused.HasValue)
            {
                _cachedData.TelemetryPaused = inComingData.TelemetryPaused.Value;
            }
            
            if (inComingData.IsTelemetryStage.HasValue)
            {
                _cachedData.IsTelemetryStage = inComingData.IsTelemetryStage.Value;
            }

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
            SignalText.text = Utils.PaddingNum4Digit(_cachedData.InfoCounter) + "/" + Utils.PaddingNum4Digit(_cachedData.InfoTarget);

            UpdateIsTelemetry(_cachedData.IsTelemetryStage.Value);
            UpdatePauseTag(_cachedData.TelemetryPaused.Value, _cachedData.IsTelemetryStage.Value);
        }

        protected override void Awake()
        {
            base.Awake();
            MessageDispatcher.AddListener(WorldEvent.BoardSignalUpdatedEvent,BoardSignalUpdatedHandler);
            MessageDispatcher.SendMessage(new CurrentSignalTypeInquiryData
            {
                CurrentSignalCallBack = SetupSignalType,
            });
            _cachedData = new BoardSignalUpdatedData {IsTelemetryStage = false, TelemetryPaused = false};
        }

        protected override void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.BoardSignalUpdatedEvent,BoardSignalUpdatedHandler);
            base.OnDestroy();
        }
    }
}