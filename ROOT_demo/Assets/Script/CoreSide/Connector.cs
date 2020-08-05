using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class Connector : MonoBehaviour
    {
        public MeshFilter StubMesh;

        public bool UseScrVersion
        {
            set
            {
                if (value)
                {
                    StubMesh.mesh = ScrStub;
                }
                else
                {
                    StubMesh.mesh = DestStub;
                }
            }
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
                int val = Mathf.Clamp(value, 0, 9);
                _normalSignalVal = val;
                NormalSignalArray.Val = val;
            }
            get => _normalSignalVal;
        }
        public int NetworkSignalVal
        {
            set
            {
                int val = Mathf.Clamp(value, 0, 9);
                _networkSignalVal = val;
                NetworkSignalArray.Val = val;
            }
            get => _networkSignalVal;
        }

        public LEDArray NormalSignalArray;
        public LEDArray NetworkSignalArray;
    }
}