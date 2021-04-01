using System;
using com.ootii.Messages;
using ROOT.Message;
using ROOT.Message.ROOT.Message.Inquiry;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace ROOT
{
    public class AcquiringSignalPanel : RoundRelatedUIBase
    {
        private Func<int, int, float> _balancingSignalFunc = (a,b)=>0.0f;
        public TextMeshPro NormalSignal;
        public TextMeshPro NetworkSignal;
        public TextMeshPro SignalDelta;
        public TextMeshPro IncomeMultiplier;

        public Transform SeesawTickTotalX;
        public Transform SeesawTick;
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
        
        protected override void Awake()
        {
            base.Awake();
            MessageDispatcher.AddListener(WorldEvent.BoardSignalUpdatedEvent,BoardSignalUpdatedHandler);
            MessageDispatcher.SendMessage(new BalancingSignalSetupInquiry {
                BalancingSignalFuncCallBack= func=>_balancingSignalFunc=func
            });
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            MessageDispatcher.RemoveListener(WorldEvent.BoardSignalUpdatedEvent,BoardSignalUpdatedHandler);
        }


        private static string _padding(int a) => Common.Utils.PaddingNum2Digit(a);

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

            SetSeesawTick(del);
        }
    }
}