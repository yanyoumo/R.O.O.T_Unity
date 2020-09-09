using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public partial class CompoundAnalogLEDArraySub : LEDArray
    {
        private const float Starting = -3.85f;
        public MeshRenderer OnMesh;
        public Transform OnMeshRoot;
        public bool OtherwayAround = false;

        void Awake()
        {
            OnMesh.material.color = LEDColor;
        }

        public override int Val
        {
            set => subVal = value;
        }
    }
}