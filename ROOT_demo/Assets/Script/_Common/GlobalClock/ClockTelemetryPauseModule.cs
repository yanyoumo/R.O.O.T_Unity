using System;
using ROOT.Consts;
using ROOT.RTAttribute;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT.Clock
{
    [RequireComponent(typeof(MasterClock))]
    public class ClockTelemetryPauseModule : ClockModuleBase
    {
        [ReadOnly]public bool TelemetryStage = false;
        [ReadOnly]public bool TelemetryPause = false;
        public bool AnimationTimeLongSwitch => TelemetryStage && !TelemetryPause;
        
        [Replaceable] public Func<float> AnimationDuration = () => MasterClock.Instance.TelemetryPauseModule.AnimationTimeLongSwitch
            ? StaticNumericData.AutoAnimationDuration
            : StaticNumericData.DefaultAnimationDuration;

        public void ResetClockPauseModule()
        {
            TelemetryStage = false;
            TelemetryPause = false;
        }

        private MasterClock owner => GetComponent<MasterClock>();
    }
}