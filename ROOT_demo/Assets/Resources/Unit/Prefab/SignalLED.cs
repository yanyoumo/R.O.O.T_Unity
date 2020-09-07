using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class SignalLED : MonoBehaviour
    {
        public Color TurnOnColor;
        private readonly Color _turnOffColor = Color.gray * 0.2f;

        public void TurnOn()
        {
            GetComponentInChildren<MeshRenderer>().material.color = TurnOnColor;
        }

        public void TurnOff()
        {
            GetComponentInChildren<MeshRenderer>().material.color = _turnOffColor;
        }
    }
}
