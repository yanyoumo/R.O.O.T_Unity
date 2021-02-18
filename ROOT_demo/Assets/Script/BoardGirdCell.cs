using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using com.ootii.Messages;
using ROOT.Message;
using static ROOT.WorldEvent;

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

    public partial class BoardGirdCell : MonoBehaviour
    {
        private Color NormalColor=> ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_MAT_BOARDGRID_NORMAL);
        private Color WarningColor=> ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_MAT_BOARDGRID_WARNING);
        private Color HeatSinkColor=> ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_MAT_BOARDGRID_HEATSINK);
        private Color InfoColColor => ColorUtilityWrapper.ParseHtmlString("#00FFFF").Value;
        private Color PreWarningColor => ColorUtilityWrapper.ParseHtmlString("#CF9E00").Value;

        private Color NormalStrokeColor => ColorUtilityWrapper.ParseHtmlString("#141414").Value;
        private Color FloatingStrokeColor => ColorUtilityWrapper.ParseHtmlString("#99bcac").Value;
        private Color HighLightedStrokeColor => ColorUtilityWrapper.ParseHtmlString("#ee7959").Value;

        [ReadOnly]
        public Board owner;
        [ReadOnly]
        public Vector2Int OnboardPos;

        public List<SpriteRenderer> Edges;

        public MeshRenderer BoardGridMesh;
        public MeshRenderer BoardStrokeMesh;

        private CellStatus _cellStatus = CellStatus.Normal;

        public TextMeshPro FloatingText;
        public TextMeshPro CashingText;

        public Color NegativeCashColoring;
        public Color PositiveCashColoring;
        public Color NeutralCashColoring;
        
        
        
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
            Utils.ROTATION_LIST.ForEach(edge => UpdateEdgeSingleSide(edge, zone));
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

        private int GetCashIO()
        {
            if (!_boardCouldIOCurrency)
            {
                return 0;
            }
            else
            {
                if (_unitCouldGenerateIncome)
                {
                    if (owner.CheckBoardPosValidAndEmpty(OnboardPos))
                    {
                        return 0;
                    }

                    var unit=owner.FindUnitUnderBoardPos(OnboardPos)?.GetComponentInChildren<Unit>();
                    if (unit != null)
                    {
                        //这么写的话、CalSingleUnitScore到是可以不考虑HeatSink了。
                        return Mathf.RoundToInt(unit.SignalCore.SingleUnitScore) - HeatSinkCost;
                    }
                    else
                    {
                        throw new ArgumentException();
                    }
                }
                else
                {
                    if (owner.CheckBoardPosValidAndEmpty(OnboardPos))
                    {
                        return 0;
                    }
                    
                    return -HeatSinkCost;
                }
            }
        }


        private void SetText(int number)
        {
            if (!_boardCouldIOCurrency)
            {
                CashingText.color = NeutralCashColoring;
                CashingText.text = "---";
            }
            else
            {
                if (_unitCouldGenerateIncome)
                {
                    if (number == 0)
                    {
                        if (owner.CheckBoardPosValidAndEmpty(OnboardPos))
                        {
                            CashingText.color = NeutralCashColoring;
                            CashingText.text = "+00";
                            return;
                        }

                        if (_cellStatus == CellStatus.Warning)
                        {
                            CashingText.color = NegativeCashColoring;
                            CashingText.text = "+00";
                        }
                        else
                        {
                            CashingText.color = PositiveCashColoring;
                            CashingText.text = "+00";
                        }
                    }
                    else if (number > 0)
                    {
                        CashingText.color = PositiveCashColoring;
                        CashingText.text = "+" + Utils.PaddingNum2Digit(number);
                    }
                    else
                    {
                        CashingText.color = NegativeCashColoring;
                        CashingText.text = "-" + Utils.PaddingNum2Digit(Math.Abs(number));
                    }
                }
                else
                {
                    if (number == 0)
                    {
                        CashingText.color = NeutralCashColoring;
                        CashingText.text = "+00";
                    }
                    else
                    {
                        CashingText.color = NegativeCashColoring;
                        CashingText.text = "-" + Utils.PaddingNum2Digit(Math.Abs(number));
                    }
                }
            }
        }

        private void Update()
        {
            if (CashingText.enabled)
            {
                SetText(GetCashIO());
            }
        }

        private void TextToggle()
        {
            //RISK 如果真是Toggle的话、那么还针对随着单元移动而修改位置。到是姑且可以写在Update里面。
            //现在认为_stageType状态是能够正常更新了。
            //就相当浪费、但是目前也没有很好的办法。
            CashingText.enabled = !CashingText.enabled;
            SetText(GetCashIO());
        }

        private bool _boardCouldIOCurrency;
        private bool _unitCouldGenerateIncome;

        private void CurrencyIOStatusChangedEventHandler(IMessage rmessage)
        {
            if (rmessage is TimingEventInfo info)
            {
                _boardCouldIOCurrency = info.BoardCouldIOCurrencyData;
                _unitCouldGenerateIncome = info.UnitCouldGenerateIncomeData;
            }
        }
        
        protected void Awake()
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

            ControlActionDriver.InGameOverlayToggleEvent += TextToggle;
            
            ClearEdge();

            CashingText.color = NeutralCashColoring;
            CashingText.enabled = false;
            
            MessageDispatcher.AddListener(Timing_Event.CurrencyIOStatusChangedEvent,CurrencyIOStatusChangedEventHandler);
        }

        protected void OnDestroy()
        {
            MessageDispatcher.RemoveListener(Timing_Event.CurrencyIOStatusChangedEvent,CurrencyIOStatusChangedEventHandler);
        }
    }
}