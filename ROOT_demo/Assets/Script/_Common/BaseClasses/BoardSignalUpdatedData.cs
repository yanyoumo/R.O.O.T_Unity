using System;

namespace ROOT
{
    public class BoardSignalUpdatedData
    {
        //这里面只放原生数据、派生数据不要用这个传；按需传函数过去。
        public bool? IsTelemetryStage = null;
        public bool? TelemetryPaused = null;

        public int CurrentActivatedTierSumA = int.MaxValue;
        public int TargetActivatedTierSumA = int.MaxValue;
        public int CurrentActivatedTierSumB = int.MaxValue;
        public int TargetActivatedTierSumB = int.MaxValue;
        public int CrtMission = int.MaxValue;
        public int TgtMission = int.MaxValue;
        public int CurrentTotalTierSumA = int.MaxValue;//包含统计未激活的。
        public int CurrentTotalTierSumB = int.MaxValue;//包含统计未激活的。
        public int InfoCounter = int.MaxValue;
        public int InfoTarget = int.MaxValue;

        public int this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return CurrentActivatedTierSumA;
                    case 1:
                        return TargetActivatedTierSumA;
                    case 2:
                        return CurrentActivatedTierSumB;
                    case 3:
                        return TargetActivatedTierSumB;
                    case 4:
                        return CrtMission;
                    case 5:
                        return TgtMission;
                    case 6:
                        return CurrentTotalTierSumA;
                    case 7:
                        return CurrentTotalTierSumB;
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
                        CurrentActivatedTierSumA = value;
                        return;
                    case 1:
                        TargetActivatedTierSumA = value;
                        return;
                    case 2:
                        CurrentActivatedTierSumB = value;
                        return;
                    case 3:
                        TargetActivatedTierSumB = value;
                        return;
                    case 4:
                        CrtMission = value;
                        return;
                    case 5:
                        TgtMission = value;
                        return;
                    case 6:
                        CurrentTotalTierSumA = value;
                        return;
                    case 7:
                        CurrentTotalTierSumB = value;
                        return;
                    case 8:
                        InfoCounter = value;
                        return;
                    case 9:
                        InfoTarget = value;
                        return;
                }

                throw new IndexOutOfRangeException();
            }
        }
    }
}