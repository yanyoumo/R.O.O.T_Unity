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

        public static int Step => ActualStep;

        public static bool TelemetryStage = false;
        public static bool TelemetryPause = false;

        private static bool? RawNeedAutoDriveStep
        {
            get
            {
                if (ActualStep == ExpectedStep) return null;
                return ExpectedStep > ActualStep;
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

        public static int ActualStep { private set; get; }
        public static int ExpectedStep { private set; get; }

        public static void InitCycler()
        {
            ActualStep = 0;
            ExpectedStep = 0;
        }

        public static void StepUp()
        {
            if (ExpectedStep < ActualStep)
            {
                throw new Exception("Should not further Increase Step when ExpectedStep is Lower");
            }
            else if (ExpectedStep > ActualStep)
            {
                ActualStep++;
            }
            else if (ExpectedStep == ActualStep)
            {
                ActualStep++;
                ExpectedStep++;
            }
        }

        public static void StepDown()
        {
            if (ExpectedStep > ActualStep)
            {
                throw new Exception("Should not further Decrease Step when ExpectedStep is Higher");
            }
            else if (ExpectedStep < ActualStep)
            {
                ActualStep--;
            }
            else if (ExpectedStep == ActualStep)
            {
                ActualStep--;
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
    }
}