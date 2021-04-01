using TMPro;
using UnityEngine;

namespace ROOT
{
    public class CubeLED : SingleLED
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

        public override void TurnOn()
        {
            LED.material.color = TurnOnColor;
        }

        public override void TurnOff()
        {
            LED.material.color = _turnOffColor;
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