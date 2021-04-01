using UnityEngine;

namespace ROOT
{
    public class AnalogLEDArray : LEDArray
    {
        private const int MaxVal = 50;
        private const float Starting = -3.85f;
        public MeshRenderer OnMesh;
        public Transform OnMeshRoot;

        void Awake()
        {
            OnMesh.material.color = LEDColor;
        }

        public override int Val
        {
            set
            {
                float normalizedVal = value / (float) MaxVal;
                normalizedVal = Mathf.Clamp01(normalizedVal);
                normalizedVal = Mathf.Pow(normalizedVal, 1/1.5f);
                float posX = Starting + Mathf.Abs(Starting) * normalizedVal;
                OnMeshRoot.transform.localScale = new Vector3(normalizedVal, 1.0f, 1.0f);
                OnMeshRoot.transform.localPosition = new Vector3(posX, 0.0f, 0.0f);
            }
        }

        void Update()
        {
            //Val = (Time.frameCount / 30);
        }
    }
}