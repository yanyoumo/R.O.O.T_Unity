using System;

namespace ROOT
{
    public class BoardSignalUpdatedData
    {
        //这里面只放原生数据、派生数据不要用这个传；按需传函数过去。
        public bool? IsTelemetryStage = null;
        public bool? TelemetryPaused = null;

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
                        CrtTypeASignal = value;
                        return;
                    case 1:
                        TgtTypeASignal = value;
                        return;
                    case 2:
                        CrtTypeBSignal = value;
                        return;
                    case 3:
                        TgtTypeBSignal = value;
                        return;
                    case 4:
                        CrtMission = value;
                        return;
                    case 5:
                        TgtMission = value;
                        return;
                    case 6:
                        TypeATier = value;
                        return;
                    case 7:
                        TypeBTier = value;
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