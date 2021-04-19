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
        public CompositeLEDArray LEDArray_Comp;

        public int Signal_A_Val
        {
            set => LEDArray_Comp.Signal_A_Val = value;
        }
        public int Signal_B_Val
        {
            set => LEDArray_Comp.Signal_B_Val = value;
        }
        public void Blink(float duration, bool inORout = true) => LEDArray_Comp.Blink(duration, inORout);
        
        public void Awake()
        {
            Hided = true;
        }
    }
}