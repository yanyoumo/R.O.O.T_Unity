using System;
using com.ootii.Messages;
using DG.Tweening;
using ROOT.Message;
using UnityEngine;

namespace ROOT
{
    /*public static class WorldCycler//这个玩意儿应该弄成单例，因为这个是需要多态的。
    {
        public static bool GamePausedStatus { get; private set; } = false;

        public static void Reset()
        {
            TelemetryStage = false;
            TelemetryPause = false;
            InitCycler();
        }

        public static bool AnimationTimeLongSwitch => TelemetryStage && !TelemetryPause;

        public static int Step => ApparentStep;

        public static bool TelemetryStage = false;
        public static bool TelemetryPause = false;

        private static bool? RawNeedAutoDriveStep
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
        public static bool? NeedAutoDriveStep
        {
            get
            {
                if (TelemetryStage)
                {
                    if (TelemetryPause)
                    {
                        return null;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return RawNeedAutoDriveStep;
                }
            }
        }

        public static int RawStep { private set; get; }
        public static int ApparentOffset { private set; get; }
        public static int ExpectedStep { private set; get; }
        public static int ApparentStep
        {
            set => RawStep = value - ApparentOffset;
            get => RawStep + ApparentOffset;
        }


        public static void InitCycler()
        {
            Debug.Log("RawStep = 0;");
            RawStep = 0;
            ApparentOffset = 0;
            ExpectedStep = 0;
        }

        public static void StepUp()
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

        public static void StepDown()
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

        public static void ExpectedStepIncrement(int amount)
        {
            ExpectedStep += amount;
        }

        public static void ExpectedStepDecrement(int amount)
        {
            ExpectedStep -= amount;
        }

        public static void ResetApparentStep()
        {
            ApparentOffset = -RawStep;
            ExpectedStep = 0;
            MessageDispatcher.SendMessage(WorldEvent.ApparentStepResetedEvent);
        }

        private static void ToggleGamePause()
        {
            //FSM暂停 √
            //DOTween暂停 √
            //信号动画DOTween改修 
            //输入队列暂停
            GamePausedStatus = !GamePausedStatus;
            DOTween.TogglePauseAll();
            MessageDispatcher.SendMessage(new GamePauseInfo {GamePaused = GamePausedStatus});
        }

        private static void RespondToKeyGamePauseEvent(IMessage rMessage)
        {
            if (rMessage is ActionPack actPak)
            {
                if (actPak.IsAction(RewiredConsts.Action.Button.PauseMenu))
                {
                    ToggleGamePause();
                }
            }
        }

        private static void RespondToGamePauseEvent(IMessage rMessage)
        {
            ToggleGamePause();
        }

        static WorldCycler()
        {
            MessageDispatcher.AddListener(WorldEvent.ControllingEvent, RespondToKeyGamePauseEvent);
            MessageDispatcher.AddListener(WorldEvent.RequestGamePauseEvent, RespondToGamePauseEvent);
        }
    }*/
}