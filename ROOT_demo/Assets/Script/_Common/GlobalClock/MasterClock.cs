using System;
using System.Collections;
using System.Collections.Generic;
using com.ootii.Messages;
using DG.Tweening;
using ROOT.Message;
using UnityEngine;

namespace ROOT.Clock
{
    public class MasterClock : MonoBehaviour
    {
        private static MasterClock instance=null;
        public static MasterClock Instance
        {
            get
            {
                if (instance == null)
                {
                    throw new ApplicationException("MasterClock not exist!!");
                }
                return instance;
            }
        }

        private ClockPauseModule _pauseModule => GetComponent<ClockPauseModule>();

        public void Reset()
        {
            if (_pauseModule != null)
            {
                _pauseModule.ResetClockPauseModule();
            }
            InitCycler();
        }
        
        public int Step => ApparentStep;

        private bool? RawNeedAutoDriveStep
        {
            get
            {
                if (ApparentStep == ExpectedStep) return null;
                return ExpectedStep > ApparentStep;
            }
        }

        /// <summary>
        /// NULL: 不需要自动演进。
        /// True: 需要自动往前演进。
        /// False:需要自动逆向演进。
        /// </summary>
        public bool? NeedAutoDriveStep
        {
            get
            {
                if (_pauseModule != null)
                {
                    if (_pauseModule.TelemetryStage)
                    {
                        if (_pauseModule.TelemetryPause)
                        {
                            return null;
                        }
                        return true;
                    }
                }
                return RawNeedAutoDriveStep;
            }
        }

        public int RawStep { private set; get; }
        public int ApparentOffset { private set; get; }
        public int ExpectedStep { private set; get; }
        public int ApparentStep
        {
            set => RawStep = value - ApparentOffset;
            get => RawStep + ApparentOffset;
        }


        public void InitCycler()
        {
            Debug.Log("RawStep = 0;");
            RawStep = 0;
            ApparentOffset = 0;
            ExpectedStep = 0;
        }

        public void StepUp()
        {
            if (ExpectedStep < ApparentStep)
            {
                throw new Exception("Should not further Increase Step when ExpectedStep is Lower");
            }
            else if (ExpectedStep > ApparentStep)
            {
                ApparentStep++;
            }
            else if (ExpectedStep == ApparentStep)
            {
                ApparentStep++;
                ExpectedStep++;
            }
        }

        public void StepDown()
        {
            if (ExpectedStep > ApparentStep)
            {
                throw new Exception("Should not further Decrease Step when ExpectedStep is Higher");
            }
            else if (ExpectedStep < ApparentStep)
            {
                ApparentStep--;
            }
            else if (ExpectedStep == ApparentStep)
            {
                ApparentStep--;
                ExpectedStep--;
            }
        }

        public void ExpectedStepIncrement(int amount)
        {
            ExpectedStep += amount;
        }

        public void ExpectedStepDecrement(int amount)
        {
            ExpectedStep -= amount;
        }

        public void ResetApparentStep()
        {
            ApparentOffset = -RawStep;
            ExpectedStep = 0;
            MessageDispatcher.SendMessage(WorldEvent.ApparentStepResetedEvent);
        }

        private void Awake()
        {
            try
            {
                if (Instance != null)
                {
                    Debug.LogWarning("MasterClock exists, Init abort");
                    Destroy(gameObject);
                    return;
                }
            }
            catch (ApplicationException) { }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    [RequireComponent(typeof(MasterClock))]
    public class ClockPauseModule : MonoBehaviour
    {
        public bool GamePausedStatus { get; private set; } = false;
        public bool TelemetryStage = false;
        public bool TelemetryPause = false;
        public bool AnimationTimeLongSwitch => TelemetryStage && !TelemetryPause;

        public void ResetClockPauseModule()
        {
            TelemetryStage = false;
            TelemetryPause = false;
        }
        
        private void ToggleGamePause()
        {
            GamePausedStatus = !GamePausedStatus;
            DOTween.TogglePauseAll();
            MessageDispatcher.SendMessage(new GamePauseInfo {GamePaused = GamePausedStatus});
        }
        
        private void RespondToKeyGamePauseEvent(IMessage rMessage)
        {
            if (rMessage is ActionPack actPak)
            {
                if (actPak.IsAction(RewiredConsts.Action.Button.PauseMenu))
                {
                    ToggleGamePause();
                }
            }
        }

        private void RespondToGamePauseEvent(IMessage rMessage)
        {
            ToggleGamePause();
        }
        
        private void Awake()
        {
            MessageDispatcher.AddListener(WorldEvent.ControllingEvent, RespondToKeyGamePauseEvent);
            MessageDispatcher.AddListener(WorldEvent.RequestGamePauseEvent, RespondToGamePauseEvent);
        }
    }
}