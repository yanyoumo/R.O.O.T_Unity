using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ROOT
{
    public class SimplerCubeLED : SingleLED
    {
        //public TextMeshPro LumpedText;
        public Transform LEDRoot;
        public MeshRenderer LED;

        public float LEDSize
        {
            set => LEDRoot.localScale = new Vector3(value, 1.0f, 1.0f);
        }

        public override void TurnOn()
        {
            LED.material.color = TurnOnColor;
        }

        public override void TurnOff()
        {
            LED.material.color = _turnOffColor;
        }

        /*public void InitLED(int val = -1)
        {
            if (val == -1)
            {
                LumpedText.gameObject.SetActive(false);
            }
            else
            {
                LumpedText.text = val + "";
            }
        }*/
    }
}