using System.Collections;
using UnityEngine;

namespace ROOT
{
    //为了添加新的Unit，这里的灯光需要抽象化；就是不能是死的矩阵或者扫描信号了。
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
                SignalAArray.Val = val;
            }
            get => _normalSignalVal;
        }
        public int NetworkSignalVal
        {
            set
            {
                var val = value;
                _networkSignalVal = val;
                SignalBArray.Val = val;
            }
            get => _networkSignalVal;
        }

        public LEDArray SignalAArray;
        public LEDArray SignalBArray;

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