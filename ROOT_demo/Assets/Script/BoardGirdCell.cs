using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;

namespace ROOT
{
    public enum CellStatus
    {
        Normal,
        PreWarning,
        Warning,
        Sink,
        InfoCol,//先凑活一下。
    }

    public class BoardGirdCell : MonoBehaviour
    {
        private Color NormalColor=> ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_MAT_BOARDGRID_NORMAL);
        private Color WarningColor=> ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_MAT_BOARDGRID_WARNING);
        private Color HeatSinkColor=> ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_MAT_BOARDGRID_HEATSINK);
        private Color InfoColColor => ColorUtilityWrapper.ParseHtmlString("#00FFFF").Value;
        private Color PreWarningColor => ColorUtilityWrapper.ParseHtmlString("#CF9E00").Value;

        private Color NormalStrokeColor => ColorUtilityWrapper.ParseHtmlString("#141414").Value;
        private Color FloatingStrokeColor => ColorUtilityWrapper.ParseHtmlString("#99bcac").Value;
        private Color HighLightedStrokeColor => ColorUtilityWrapper.ParseHtmlString("#ee7959").Value;

        [HideInInspector]
        public Board owner;
        [HideInInspector]
        public Vector2Int OnboardPos;

        public List<SpriteRenderer> Edges;

        public MeshRenderer BoardGridMesh;
        public MeshRenderer BoardStrokeMesh;

        private CellStatus _cellStatus = CellStatus.Normal;

        public TextMeshPro FloatingText;
        
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
                    case CellStatus.PreWarning:
                        BoardGridMesh.material.color = PreWarningColor;
                        break;
                    case CellStatus.Warning:
                        BoardGridMesh.material.color = WarningColor;
                        break;
                    case CellStatus.Sink:
                        BoardGridMesh.material.color = HeatSinkColor;
                        break;
                    case CellStatus.InfoCol:
                        BoardGridMesh.material.color = InfoColColor;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            get => _cellStatus;
        }

        private RotationDirection[] rotLib = {
            RotationDirection.North,
            RotationDirection.East,
            RotationDirection.West,
            RotationDirection.South
        };

        private void UpdateEdgeSingleSide(RotationDirection side, List<Vector2Int> zone)
        {
            var res = false;
            var otherPos = OnboardPos + Utils.ConvertDirectionToBoardPosOffset(side);
            var inZone=zone.Contains(OnboardPos);
            var otherInZone = zone.Contains(otherPos) && owner.CheckBoardPosValid(otherPos);
            _edgeDic[side].enabled = inZone && !otherInZone;
        }

        public void UpdateEdge(List<Vector2Int> zone)
        {
            if (!zone.Contains(OnboardPos)) return;
            rotLib.ForEach(edge => UpdateEdgeSingleSide(edge, zone));
        }

        public void ClearEdge()
        {
            _edgeDic.Values.ForEach(renderer => renderer.enabled = false);
        }

        public void Blink()
        {
            BoardGridMesh.material.color = Color.green;
        }

        private Dictionary<RotationDirection, SpriteRenderer> _edgeDic;

        private const float ClickColorDelay = 0.1f;
        private bool ClickColoring = false;

        private IEnumerator ClickColorDelay_CO()
        {
            yield return new WaitForSeconds(ClickColorDelay);
            ClickColoring = false;
        }

        public void ChangeStrokeMode(LightUpBoardColor color)
        {
            if (ClickColoring) return;

            switch (color)
            {
                case LightUpBoardColor.Clicked:
                    Clicked();
                    ClickColoring = true;
                    StartCoroutine(ClickColorDelay_CO());
                    break;
                case LightUpBoardColor.Hovered:
                    Hovered();
                    break;
                case LightUpBoardColor.Unhovered:
                    Unhovered();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(color), color, null);
            }
        }

        private void Clicked()
        {
            BoardStrokeMesh.material.color = HighLightedStrokeColor;
        }

        private void Hovered()
        {
            BoardStrokeMesh.material.color = FloatingStrokeColor;
        }

        private void Unhovered()
        {
            BoardStrokeMesh.material.color = NormalStrokeColor;
        }

        void Awake()
        {
            BoardStrokeMesh.material.color = NormalStrokeColor;
            _cellStatus = CellStatus.Normal;
            
            _edgeDic = new Dictionary<RotationDirection, SpriteRenderer>
            {
                {RotationDirection.North, Edges[0]},
                {RotationDirection.East, Edges[1]},
                {RotationDirection.West, Edges[2]},
                {RotationDirection.South, Edges[3]}
            };
            ClearEdge();
        }
    }
}