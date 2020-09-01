using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ROOT
{
    public class CubeLED : MonoBehaviour
    {
        private int representingVal = 0;

        public int RepresentingVal
        {
            set
            {
                representingVal = value;
                RepText.text = representingVal + "";
            }
            get => representingVal;
        }
        public TextMeshPro LumpedText;
        public TextMeshPro RepText;
        public MeshRenderer LED;
        public Transform LEDRoot;
        public Transform CostArrowRoot;

        public float LEDSize
        {
            set => LEDRoot.localScale = new Vector3(value, 1.0f, 1.0f);
        }

        public bool On
        {
            set => LED.material.color = value ? Color.green : Color.grey * 0.3f;
        }

        public bool SetArrow
        {
            set => CostArrowRoot.gameObject.SetActive(value);
        }

        public void InitLED(int val = -1)
        {
            CostArrowRoot.gameObject.SetActive(false);
            RepText.gameObject.SetActive(false);
            if (val == -1)
            {
                LumpedText.gameObject.SetActive(false);
            }
            else
            {
                LumpedText.text = val + "";
            }
        }
    }
}