using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

namespace ROOT
{
    public enum CellStatus
    {
        Normal,
        Warning,
        Sink,
    }

    public class BoardGirdCell : MonoBehaviour
    {
        private Color NormalColor=> ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_MAT_BOARDGRID_NORMAL);
        private Color WarningColor=> ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_MAT_BOARDGRID_WARNING);
        private Color HeatSinkColor=> ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_MAT_BOARDGRID_HEATSINK);

        public MeshRenderer BoardGridMesh;

        private CellStatus _cellStatus = CellStatus.Normal;

        public CellStatus CellStatus
        {
            set
            {
                _cellStatus = value;
                switch (_cellStatus)
                {
                    case CellStatus.Normal:
                        BoardGridMesh.material.color = NormalColor;
                        break;
                    case CellStatus.Warning:
                        BoardGridMesh.material.color = WarningColor;
                        break;
                    case CellStatus.Sink:
                        BoardGridMesh.material.color = HeatSinkColor;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            get => _cellStatus;
        }

        /*private bool _normalOrHeatSink = false;
        public bool NormalOrHeatSink
        {
            set
            {
                _normalOrHeatSink = value;
                BoardGridMesh.material.color = _normalOrHeatSink ? HeatSinkColor : NormalColor;
            }
            get => _normalOrHeatSink;
        }*/
        void Awake()
        {
            _cellStatus = CellStatus.Normal;
            //NormalOrHeatSink = false;
        }
    }
}