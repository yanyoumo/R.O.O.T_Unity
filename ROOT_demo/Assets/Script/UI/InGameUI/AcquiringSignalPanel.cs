using System;
using com.ootii.Messages;
using ROOT.Common;
using ROOT.Message;
using ROOT.Message.Inquiry;
using ROOT.SetupAsset;
using TMPro;
using UnityEngine;

namespace ROOT.UI
{
    public class AcquiringSignalPanel : RoundRelatedUIBase
    {
        protected override UITag UITag => UITag.SignalPanel_Acquiring;

        private Func<int, int, float> _balancingSignalFunc = (a,b)=>0.0f;
        public TextMeshPro NormalSignal;
        public TextMeshPro NetworkSignal;
        public TextMeshPro SignalDelta;
        public TextMeshPro IncomeMultiplier;

        public Transform SeesawTickTotalX;
        public Transform SeesawTick;
        
        private int cachedTypeAVal = -1;
        private int cachedTypeBVal = -1;
        private float cachedIncomeMultiplierVal = float.NaN;
        private static string _padding(int a) => Utils.PaddingNum2Digit(a);

        private float seeSawTickUnit => SeesawTickTotalX.localPosition.x / 4.0f;
        private void SetSeesawTick(int del)
        {
            var pos = SeesawTick.localPosition;
            pos.x = del * seeSawTickUnit;
            SeesawTick.localPosition = pos;
        }

        private void BoardSignalUpdatedHandler(IMessage rmMessage)
        {
            if (rmMessage is BoardSignalUpdatedInfo info)
            {
                UpdateCachedData(info.SignalData);
                UpdateNumbersCore();
            }
        }

        private void SetupSignalType(SignalType signalTypeA, SignalType signalTypeB)
        {
            NormalSignal.color = ColorLibManager.Instance.GetColorBySignalType(signalTypeA);
            NetworkSignal.color = ColorLibManager.Instance.GetColorBySignalType(signalTypeB);
        }

        private void UpdateCachedData(BoardSignalUpdatedData inComingData)
        {
            if (inComingData.CurrentActivatedTierSumA != int.MaxValue) cachedTypeAVal = inComingData.CurrentActivatedTierSumA;
            if (inComingData.CurrentActivatedTierSumB != int.MaxValue) cachedTypeBVal = inComingData.CurrentActivatedTierSumB;
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
            
            var multiplier = _balancingSignalFunc(cachedTypeAVal, cachedTypeBVal);

            var frac_mul = multiplier - 1.0f;
            var abs_frac_mul = Mathf.Abs(frac_mul);
            var sign = frac_mul >= 0 ? "+" : "-";

            IncomeMultiplier.text = sign + abs_frac_mul.ToString("#0%");

            SetSeesawTick(del);
        }
        
        protected override void Awake()
        {
            base.Awake();
            MessageDispatcher.AddListener(WorldEvent.BoardSignalUpdatedEvent,BoardSignalUpdatedHandler);
            MessageDispatcher.SendMessage(new BalancingSignalSetupInquiryData
            {
                BalancingSignalFuncCallBack = func => _balancingSignalFunc = func
            });
            MessageDispatcher.SendMessage(new CurrentSignalTypeInquiryData
            {
                CurrentSignalCallBack = SetupSignalType,
            });
        }

        protected override void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.BoardSignalUpdatedEvent,BoardSignalUpdatedHandler);
            base.OnDestroy();
        }
    }
}