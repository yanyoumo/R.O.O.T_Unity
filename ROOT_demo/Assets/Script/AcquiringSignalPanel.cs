using System;
using com.ootii.Messages;
using TMPro;

namespace ROOT
{
    public class AcquiringSignalPanel : RoundRelatedUIBase
    {
        private void BoardSignalUpdatedHandler(IMessage rmMessage)
        {
            if (rmMessage is BoardSignalUpdatedInfo info)
            {
                UpdateCachedData(info.SignalData);
                UpdateNumbersCore();
            }
        }
        
        protected override void Awake()
        {
            base.Awake();
            MessageDispatcher.AddListener(WorldEvent.BoardSignalUpdatedEvent,BoardSignalUpdatedHandler);
            _cachedData = new BoardSignalUpdatedData();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            MessageDispatcher.RemoveListener(WorldEvent.BoardSignalUpdatedEvent,BoardSignalUpdatedHandler);
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

        private readonly Func<int, string> _padding = Utils.PaddingNum2Digit;

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
            SignalText.text = Utils.PaddingNum4Digit(_cachedData.InfoCounter) + "/" + Utils.PaddingNum4Digit(_cachedData.InfoTarget);

            UpdateIsTelemetry(_cachedData.IsTelemetryStage);
            UpdatePauseTag(_cachedData.TelemetryPaused,_cachedData.IsTelemetryStage);
        }
    }
}