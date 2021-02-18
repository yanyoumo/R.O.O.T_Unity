using System;
using com.ootii.Messages;
using ROOT.Message;
using TMPro;
using UnityEngine;

namespace ROOT
{
    public class BoardSignalUpdatedData
    {
        public bool IsTelemetryStage = false;
        public bool TelemetryPaused = false;

        public int CrtTypeASignal = Int32.MaxValue;
        public int TgtTypeASignal = Int32.MaxValue;
        public int CrtTypeBSignal = Int32.MaxValue;
        public int TgtTypeBSignal = Int32.MaxValue;
        public int CrtMission = Int32.MaxValue;
        public int TgtMission = Int32.MaxValue;
        public int TypeATier = Int32.MaxValue;
        public int TypeBTier = Int32.MaxValue;
        public int InfoCounter = Int32.MaxValue;
        public int InfoTarget = Int32.MaxValue;
    }

    public class BoardSignalUpdatedInfo : RootMessageBase
    {
        public BoardSignalUpdatedData SignalData;
        public override string Type => WorldEvent.Visual_Event.BoardSignalUpdatedEvent;
    }
    
    public class SignalPanel : RoundRelatedUIBase
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
            MessageDispatcher.AddListener(WorldEvent.Visual_Event.BoardSignalUpdatedEvent,BoardSignalUpdatedHandler);
            cachedData = new BoardSignalUpdatedData();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            MessageDispatcher.RemoveListener(WorldEvent.Visual_Event.BoardSignalUpdatedEvent,BoardSignalUpdatedHandler);
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

        private string Padding(int v)
        {
            return Utils.PaddingNum2Digit(v);
        }

        private BoardSignalUpdatedData cachedData;//这种缓存机制还是得有。

        private void UpdateCachedData(BoardSignalUpdatedData inComingData)
        {
            cachedData.TelemetryPaused = inComingData.TelemetryPaused;
            cachedData.IsTelemetryStage = inComingData.IsTelemetryStage;

            if (inComingData.CrtTypeASignal != Int32.MaxValue) cachedData.CrtTypeASignal = inComingData.CrtTypeASignal;
            if (inComingData.TgtTypeASignal != Int32.MaxValue) cachedData.TgtTypeASignal = inComingData.TgtTypeASignal;
            if (inComingData.CrtTypeBSignal != Int32.MaxValue) cachedData.CrtTypeBSignal = inComingData.CrtTypeBSignal;
            if (inComingData.TgtTypeBSignal != Int32.MaxValue) cachedData.TgtTypeBSignal = inComingData.TgtTypeBSignal;
            if (inComingData.CrtMission != Int32.MaxValue) cachedData.CrtMission = inComingData.CrtMission;
            if (inComingData.TgtMission != Int32.MaxValue) cachedData.TgtMission = inComingData.TgtMission;
            if (inComingData.TypeATier != Int32.MaxValue) cachedData.TypeATier = inComingData.TypeATier;
            if (inComingData.TypeBTier != Int32.MaxValue) cachedData.TypeBTier = inComingData.TypeBTier;
            if (inComingData.InfoCounter != Int32.MaxValue) cachedData.InfoCounter = inComingData.InfoCounter;
            if (inComingData.InfoTarget != Int32.MaxValue) cachedData.InfoTarget = inComingData.InfoTarget;
        }

        private void UpdateNumbersCore()
        {
            NormalSignal.text = Padding(cachedData.CrtTypeASignal) + "/" + Padding(cachedData.TgtTypeASignal);
            NetworkSignal.text = Padding(cachedData.CrtTypeBSignal) + "/" + Padding(cachedData.TgtTypeBSignal);
            MissionTarget.text = "[" + Padding(cachedData.CrtMission) + "]";
            MissionTarget_Big.text = "[" + Padding(cachedData.CrtMission) + "]";
            NormalTierText.text = "[" + Padding(cachedData.TypeATier) + "]";
            NetworkTierText.text = "[" + Padding(cachedData.TypeBTier) + "]";
            SignalText.text = Utils.PaddingNum4Digit(cachedData.InfoCounter) + "/" + Utils.PaddingNum4Digit(cachedData.InfoTarget);

            UpdateIsTelemetry(cachedData.IsTelemetryStage);
            UpdatePauseTag(cachedData.TelemetryPaused,cachedData.IsTelemetryStage);
        }
    }
}