using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class SingleSignalHint_UI : MonoBehaviour
    {
        public Transform NonTelemetryText;
        public Transform TelemetryText;

        public bool UseTelemetry
        {
            set
            {
                NonTelemetryText.gameObject.SetActive(!value);
                TelemetryText.gameObject.SetActive(value);
            }
        }
    }
}