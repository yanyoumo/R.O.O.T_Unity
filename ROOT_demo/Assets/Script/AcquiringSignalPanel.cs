using System;
using com.ootii.Messages;
using ROOT.Message;
using TMPro;

namespace ROOT
{
    public class BalancingSignalSetupInquiry : RootMessageBase
    {
        public Func<Func<int, int, float>,bool> BalancingSignalFuncCallBack;
        public override string Type => WorldEvent.BalancingSignalSetupInquiry;
    }
    
    public class AcquiringSignalPanel : RoundRelatedUIBase
    {
        private Func<int, int, float> _balancingSignalFunc = (a,b)=>0.0f;

        private bool BalancingSignalFuncCallBack(Func<int, int, float> balancingSignalFunc)
        {
            _balancingSignalFunc = balancingSignalFunc;
            return true;
        }

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
            MessageDispatcher.SendMessage(new BalancingSignalSetupInquiry{BalancingSignalFuncCallBack=this.BalancingSignalFuncCallBack});
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            MessageDispatcher.RemoveListener(WorldEvent.BoardSignalUpdatedEvent,BoardSignalUpdatedHandler);
        }

        public TextMeshPro NormalSignal;
        public TextMeshPro NetworkSignal;
        public TextMeshPro SignalDelta;
        public TextMeshPro IncomeMultiplier;

        private static string _padding(int a) => Utils.PaddingNum2Digit(a);

        private int cachedTypeAVal = -1;
        private int cachedTypeBVal = -1;
        private float cachedIncomeMultiplierVal = float.NaN;


        private void UpdateCachedData(BoardSignalUpdatedData inComingData)
        {
            if (inComingData.CrtTypeASignal != int.MaxValue) cachedTypeAVal = inComingData.CrtTypeASignal;
            if (inComingData.CrtTypeBSignal != int.MaxValue) cachedTypeBVal = inComingData.CrtTypeBSignal;
        }

        private void UpdateNumbersCore()
        {
            NormalSignal.text = _padding(cachedTypeAVal);
            NetworkSignal.text = _padding(cachedTypeBVal);

            var del = cachedTypeAVal - cachedTypeBVal;
            var resString = _padding(Math.Abs(del));

            if (del > 0)
            {
                resString = "+" + resString;
            }
            else if (del == 0)
            {
                resString = "=" + resString;
            }
            else
            {
                resString = "-" + resString;
            }

            SignalDelta.text = resString;

            var multiplier = _balancingSignalFunc(cachedTypeAVal, cachedTypeBVal);

            IncomeMultiplier.text = "x" + multiplier.ToString("F");
        }
    }
}