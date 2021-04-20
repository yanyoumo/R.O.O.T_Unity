using System;

namespace ROOT
{
    public static class WorldCycler
    {
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
            set => RawStep = value;
            get => RawStep + ApparentOffset;
        }


        public static void InitCycler()
        {
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
        }
    }
}