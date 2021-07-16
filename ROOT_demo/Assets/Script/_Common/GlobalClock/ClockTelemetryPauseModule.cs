using UnityEngine;

namespace ROOT.Clock
{
    [RequireComponent(typeof(MasterClock))]
    public class ClockTelemetryPauseModule : ClockModuleBase
    {
        public bool TelemetryStage = false;
        public bool TelemetryPause = false;
        public bool AnimationTimeLongSwitch => TelemetryStage && !TelemetryPause;
    
        public void ResetClockPauseModule()
        {
            TelemetryStage = false;
            TelemetryPause = false;
        }

        private MasterClock owner => GetComponent<MasterClock>();
    }
}