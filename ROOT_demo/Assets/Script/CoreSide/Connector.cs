using System.Collections;
using UnityEngine;

namespace ROOT
{
    public class Connector : MonoBehaviour
    {
        public MeshFilter StubMesh;

        public bool UseScrVersion
        {
            set => StubMesh.mesh = value ? ScrStub : DestStub;
        }

        private bool _hided;
        public bool Hided
        {
            get => _hided;
            set
            {
                _hided = value;
                Display.SetActive(!_hided && _connected);
            }
        }

        private bool _connected;
        public bool Connected
        {
            get => _connected;
            set
            {
                _connected = value;
                Display.SetActive(!_hided && _connected);
            }
        }

        public GameObject Display;
        public Mesh ScrStub;
        public Mesh DestStub;
        private int _normalSignalVal;
        private int _networkSignalVal;

        public int NormalSignalVal
        {
            set
            {
                var val = value;
                _normalSignalVal = val;
                NormalSignalArray.Val = val;
            }
            get => _normalSignalVal;
        }
        public int NetworkSignalVal
        {
            set
            {
                var val = value;
                _networkSignalVal = val;
                NetworkSignalArray.Val = val;
            }
            get => _networkSignalVal;
        }

        public LEDArray NormalSignalArray;
        public LEDArray NetworkSignalArray;

        public GameObject BlinkCube;
        public GameObject NormalED;

        public void Awake()
        {
            Hided = true;
            BlinkCube.gameObject.SetActive(false);
            NormalED.gameObject.SetActive(true);
        }

        IEnumerator Blink_Coroutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            BlinkCube.gameObject.SetActive(false);
            NormalED.gameObject.SetActive(true);
        }

        public void Blink(float duration)
        {
            BlinkCube.gameObject.SetActive(true);
            NormalED.gameObject.SetActive(false);
            StartCoroutine("Blink_Coroutine", duration);
        }
    }
}