using UnityEngine;

namespace ROOT
{
    public partial class CompoundAnalogLEDArraySub : LEDArray
    {
        internal int subVal;
        internal int MaxVal = 50;

        internal void SetNVal(float nVal)
        {
            float normalizedVal = Mathf.Abs(nVal);
            float posX;
            if (OtherwayAround)
            {
                posX = Starting + Mathf.Abs(Starting) * normalizedVal;
            }
            else
            {
                posX = -Starting - Mathf.Abs(Starting) * normalizedVal;
            }

            OnMeshRoot.transform.localScale = new Vector3(normalizedVal, 1.0f, 1.0f);
            OnMeshRoot.transform.localPosition = new Vector3(posX, 0.0f, 0.0f);
        }
    }

    public class CompoundAnalogLEDArray : MonoBehaviour
    {
        private const int MaxTotal = 25;
        public CompoundAnalogLEDArraySub NormalLED;
        public CompoundAnalogLEDArraySub NetworkLED;

        private float NormalizedNormalVal = 0.0f;
        private float NormalizedNetworkVal = 0.0f;

        void UpdateNormalizedVal()
        {
            var nrVal = NormalLED.subVal;
            var nwVal = NetworkLED.subVal;
            if (nrVal+nwVal<=MaxTotal)
            {
                NormalizedNormalVal = nrVal / (float) MaxTotal;
                NormalizedNetworkVal = nwVal / (float) MaxTotal;
            }
            else
            {
                NormalizedNormalVal = nrVal / (float) (nrVal+ nwVal);
                NormalizedNetworkVal = nwVal / (float) (nrVal + nwVal);
            }
            NormalizedNormalVal = Mathf.Pow(NormalizedNormalVal, 1 / 1.5f);
            NormalizedNetworkVal = Mathf.Pow(NormalizedNetworkVal, 1 / 1.5f);
        }

        void Start()
        {
            NormalLED.MaxVal = MaxTotal;
            NetworkLED.MaxVal = MaxTotal;
            NetworkLED.OtherwayAround = true;
        }

        // Update is called once per frame
        void Update()
        {
            UpdateNormalizedVal();
            NormalLED.SetNVal(NormalizedNormalVal);
            NetworkLED.SetNVal(NormalizedNetworkVal);
        }
    }
}