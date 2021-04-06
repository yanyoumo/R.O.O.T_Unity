using UnityEngine;

namespace ROOT
{
    public abstract class SingleLED : MonoBehaviour
    {
        public Color TurnOnColor;
        protected readonly Color _turnOffColor = Color.gray * 0.2f;
        public abstract void TurnOn();
        public abstract void TurnOff();
    }

    public class SignalLED : SingleLED
    {
        public override void TurnOn()
        {
            GetComponentInChildren<MeshRenderer>().material.color = TurnOnColor;
        }

        public override void TurnOff()
        {
            GetComponentInChildren<MeshRenderer>().material.color = _turnOffColor;
        }
    }
}
