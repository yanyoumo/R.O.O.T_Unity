using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT
{
    public class HelpScreen_SignalSub_UI : MonoBehaviour
    {
        public Transform[] SignalHintArray;

        public float UpperPosZ;
        public float LowerPosZ;

        //TODO 还要处理遥测部分的逻辑。
        public void SetupSignalHints(SignalType signalTypeUpper, SignalType signalTypeLower, bool TelemetryOrNot)
        {
            SignalHintArray.ForEach(t => t.gameObject.SetActive(false));

            var upperTrans = SignalHintArray[(int) signalTypeUpper];
            var lowerTrans = SignalHintArray[(int) signalTypeLower];

            var upperPos = upperTrans.localPosition;
            var lowerPos = lowerTrans.localPosition;

            upperTrans.gameObject.SetActive(true);
            lowerTrans.gameObject.SetActive(true);

            upperTrans.localPosition = new Vector3(upperPos.x, upperPos.y, UpperPosZ);
            lowerTrans.localPosition = new Vector3(lowerPos.x, lowerPos.y, LowerPosZ);
        }
    }
}