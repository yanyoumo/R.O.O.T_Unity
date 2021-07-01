using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT
{
    public class HelpScreen_SignalSub_UI : MonoBehaviour
    {
        public SingleSignalHint_UI[] SignalHintArray;

        public float UpperPosZ;
        public float LowerPosZ;

        //TODO 还要处理遥测部分的逻辑。
        public void SetupSignalHints(SignalType signalTypeUpper, SignalType signalTypeLower, bool TelemetryOrNot)
        {
            SignalHintArray.ForEach(t => t.gameObject.SetActive(false));

            var upperSignalHint = SignalHintArray[(int) signalTypeUpper];
            var lowerSignalHint = SignalHintArray[(int) signalTypeLower];

            var upperPos = upperSignalHint.transform.localPosition;
            var lowerPos = lowerSignalHint.transform.localPosition;

            upperSignalHint.gameObject.SetActive(true);
            lowerSignalHint.gameObject.SetActive(true);

            upperSignalHint.UseTelemetry = TelemetryOrNot;
            lowerSignalHint.UseTelemetry = TelemetryOrNot;
            
            upperSignalHint.transform.localPosition = new Vector3(upperPos.x, upperPos.y, UpperPosZ);
            lowerSignalHint.transform.localPosition = new Vector3(lowerPos.x, lowerPos.y, LowerPosZ);
        }
    }
}