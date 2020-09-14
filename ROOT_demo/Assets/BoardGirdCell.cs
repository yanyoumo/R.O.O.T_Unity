using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class BoardGirdCell : MonoBehaviour
    {
        private Color NormalColor=> ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_MAT_BOARDGRID_NORMAL);
        private Color HeatSinkColor=> ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_MAT_BOARDGRID_HEATSINK);

        public MeshRenderer BoardGridMesh;

        private bool _normalOrHeatSink = false;
        public bool NormalOrHeatSink
        {
            set
            {
                _normalOrHeatSink = value;
                BoardGridMesh.material.color = _normalOrHeatSink ? HeatSinkColor : NormalColor;
            }
            get => _normalOrHeatSink;
        }

        void Awake()
        {
            NormalOrHeatSink = false;
        }
    }
}