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

        public int CrtTypeASignal = int.MaxValue;
        public int TgtTypeASignal = int.MaxValue;
        public int CrtTypeBSignal = int.MaxValue;
        public int TgtTypeBSignal = int.MaxValue;
        public int CrtMission = int.MaxValue;
        public int TgtMission = int.MaxValue;
        public int TypeATier = int.MaxValue;
        public int TypeBTier = int.MaxValue;
        public int InfoCounter = int.MaxValue;
        public int InfoTarget = int.MaxValue;

        public int this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return CrtTypeASignal;
                    case 1:
                        return TgtTypeASignal;
                    case 2:
                        return CrtTypeBSignal;
                    case 3:
                        return TgtTypeBSignal;
                    case 4:
                        return CrtMission;
                    case 5:
                        return TgtMission;
                    case 6:
                        return TypeATier;
                    case 7:
                        return TypeBTier;
                    case 8:
                        return InfoCounter;
                    case 9:
                        return InfoTarget;
                }
                throw new IndexOutOfRangeException();
            }
            set
            {
                switch (i)
                {
                    case 0:
                        CrtTypeASignal=value;
                        return;
                    case 1:
                        TgtTypeASignal=value;
                        return;
                    case 2:
                        CrtTypeBSignal=value;
                        return;
                    case 3:
                        TgtTypeBSignal=value;
                        return;
                    case 4:
                        CrtMission=value;
                        return;
                    case 5:
                        TgtMission=value;
                        return;
                    case 6:
                        TypeATier=value;
                        return;
                    case 7:
                        TypeBTier=value;
                        return;
                    case 8:
                        InfoCounter=value;
                        return;
                    case 9:
                        InfoTarget=value;
                        return;
                }
                throw new IndexOutOfRangeException();
            }
        }
    }

    public class BoardSignalUpdatedInfo : RootMessageBase
    {
        public BoardSignalUpdatedData SignalData;
        public override string Type => WorldEvent.BoardSignalUpdatedEvent;
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