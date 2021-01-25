using TMPro;
using UnityEngine;

namespace ROOT
{
    public class SignalPanel : MonoBehaviour
    {
        public void Awake()
        {
            IsTelemetryStage = false;
            TelemetryPaused = false;
        }

        [HideInInspector]
        private bool _isTelemetryStage;

        public bool IsTelemetryStage
        {
            set
            {
                _isTelemetryStage = value;
                if (_isTelemetryStage)
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
            get => _isTelemetryStage;
        }

        public TextMeshPro NormalSignal;
        public TextMeshPro NetworkSignal;
        public TextMeshPro MissionTarget;
        public TextMeshPro NormalTierText;
        public TextMeshPro NetworkTierText;

        public TextMeshPro MissionTarget_Big;
        public TextMeshPro SignalText;
        public TextMeshPro PausedTag;

        public bool TelemetryPaused
        {
            set => PausedTag.enabled = value&& _isTelemetryStage;
        }

        private int _crtNormalSignal;
        public int CrtNormalSignal
        {
            set
            {
                _crtNormalSignal = value;
                UpdateNumbers();
            }
            get => _crtNormalSignal;
        }

        private int _tgtNormalSignal;
        public int TgtNormalSignal
        {
            set
            {
                _tgtNormalSignal = value;
                UpdateNumbers();
            }
            get => _tgtNormalSignal;
        }

        private int _crtNetworkSignal;
        public int CrtNetworkSignal
        {
            set
            {
                _crtNetworkSignal = value;
                UpdateNumbers();
            }
            get => _crtNetworkSignal;
        }

        private int _tgtNetworkSignal;
        public int TgtNetworkSignal
        {
            set
            {
                _tgtNetworkSignal = value;
                UpdateNumbers();
            }
            get => _tgtNetworkSignal;
        }
        
        private int _crtMission;
        public int CrtMission
        {
            set
            {
                _crtMission = value;
                UpdateNumbers();
            }
            get => _crtMission;
        }

        private int _tgtMission;
        public int TgTtMission
        {
            set
            {
                _tgtMission = value;
                UpdateNumbers();
            }
            get => _tgtMission;
        }

        private int _normalTier;
        public int NormalTier
        {
            get => _normalTier;
            set
            {
                _normalTier = value;
                UpdateNumbers();
            }
        }

        private int _networkTier;
        public int NetworkTier
        {
            get => _networkTier;
            set
            {
                _networkTier = value;
                UpdateNumbers();
            }
        }

        private int _signalCounter;
        public int SignalCounter
        {
            get => _signalCounter;
            set
            {
                _signalCounter = value;
                UpdateNumbers();
            }
        }

        private int _signalTarget;
        public int SignalTarget
        {
            get => _signalTarget;
            set
            {
                _signalTarget = value;
                UpdateNumbers();
            }
        }

        private string Padding(int v)
        {
            return Utils.PaddingNum2Digit(v);
        }

        private void UpdateNumbers()
        {
            NormalSignal.text = Padding(_crtNormalSignal) + "/" + Padding(_tgtNormalSignal);
            NetworkSignal.text = Padding(_crtNetworkSignal) + "/" + Padding(_tgtNetworkSignal);
            MissionTarget.text = "[" + Padding(_crtMission) + "]";
            MissionTarget_Big.text = "[" + Padding(_crtMission) + "]";
            NormalTierText.text = "["+ Padding(_normalTier) + "]";
            NetworkTierText.text = "[" + Padding(_networkTier) + "]";
            SignalText.text = Utils.PaddingNum4Digit(_signalCounter) + "/" + Utils.PaddingNum4Digit(_signalTarget);
        }
    }
}