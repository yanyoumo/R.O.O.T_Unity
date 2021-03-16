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

    public enum EdgeStatus
    {
        //这个东西有个隐含的需要优先级（队列）的设计。怎么搞？
        //队列还是分层？可能要分层。有了分层还要有顺序的概念。
        //目前这个顺序干脆就设计成这个enum从下往上的逻辑、或者得弄一个数列。
        InfoZone,
        ThermoZone,
        Off,
    }
    
    public class BoardGridThermoZoneInquiry : RootMessageBase
    {
        public Func<List<Vector2Int>,bool> BoardGridThermoZoneInquiryCallBack;
        public override string Type => WorldEvent.BoardGridThermoZoneInquiry;
    }
    
    public partial class BoardGirdCell : MonoBehaviour
    {
        EdgeStatus checkEdgeStatusByPriority()
        {
            EdgeStatus[] PriorityList = {EdgeStatus.ThermoZone, EdgeStatus.InfoZone};
            foreach (var edgeStatus in PriorityList)
            {
                if (LayeringEdgeStatus.ContainsKey(edgeStatus))
                {
                    if (LayeringEdgeStatus[edgeStatus])
                    {
                        return edgeStatus;
                    }
                }
            }
            return EdgeStatus.Off;
        }
        
        public Dictionary<EdgeStatus, bool> LayeringEdgeStatus;
        private Color NormalColor=> ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_MAT_BOARDGRID_NORMAL);
        private Color WarningColor=> ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_MAT_BOARDGRID_WARNING);
        private Color HeatSinkColor=> ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_MAT_BOARDGRID_HEATSINK);
        private Color InfoColColor => ColorUtilityWrapper.ParseHtmlStringNotNull("#00FFFF");
        private Color PreWarningColor => ColorUtilityWrapper.ParseHtmlStringNotNull("#CF9E00");
        private Color NormalStrokeColor => ColorUtilityWrapper.ParseHtmlStringNotNull("#141414");
        private Color FloatingStrokeColor => ColorUtilityWrapper.ParseHtmlStringNotNull("#99bcac");
        private Color HighLightedStrokeColor => ColorUtilityWrapper.ParseHtmlStringNotNull("#ee7959");

        [ReadOnly]
        public Board owner;
        [ReadOnly]
        public Vector2Int OnboardPos;

        public List<SpriteRenderer> Edges;

        public MeshRenderer BoardGridMesh;
        public MeshRenderer BoardStrokeMesh;

        private CellStatus _cellStatus = CellStatus.Normal;
        //private EdgeStatus _edgeStatus = EdgeStatus.Off;

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

        private Color GetColorFromEdgeStatus(EdgeStatus edgeStatus)
        {
            switch (edgeStatus)
            {
                case EdgeStatus.InfoZone:
                    return ColorUtilityWrapper.ParseHtmlStringNotNull("#00B3B3");
                case EdgeStatus.ThermoZone:
                    return ColorUtilityWrapper.ParseHtmlStringNotNull("#C83B00");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void UpdateEdgeSingleSide(RotationDirection side, List<Vector2Int> zone,EdgeStatus edgeStatus)
        {
            //set是true的话是设置新的Zone、如果是false走fallback流程。
            var otherPos = OnboardPos + Utils.ConvertDirectionToBoardPosOffset(side);
            var inZone = zone.Contains(OnboardPos);
            var otherInZone = zone.Contains(otherPos) && owner.CheckBoardPosValid(otherPos);
            _edgeDic[side].enabled = inZone && !otherInZone;
            _edgeDic[side].color = GetColorFromEdgeStatus(edgeStatus);
        }

        private void UpdateEdge(List<Vector2Int> zone, bool set)
        {
            var edgeStatus = checkEdgeStatusByPriority();
            if (edgeStatus == EdgeStatus.Off)
            {
                _edgeDic.Values.ForEach(renderer => renderer.enabled = false);
                return;
            }
            if (!set) zone = owner.BoardGirdDriver.ExtractCachedZone(edgeStatus);
            Utils.ROTATION_LIST.ForEach(edge => UpdateEdgeSingleSide(edge, zone, edgeStatus));
        }

        public void SetEdge(List<Vector2Int> zone, EdgeStatus edgeStatus)
        {
            if (!zone.Contains(OnboardPos))
            {
                ClearEdge(edgeStatus);
            }

            LayeringEdgeStatus[edgeStatus] = true;
            UpdateEdge(zone,true);
        }

        public void ClearEdge(EdgeStatus edgeStatus)
        {
            LayeringEdgeStatus[edgeStatus] = false;
            UpdateEdge(new List<Vector2Int>(), false);
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

        private bool showingThremoBoarder = false;

        private bool BoardGridThermoZoneInquiry(List<Vector2Int> ThermoZone)
        {
            SetEdge(ThermoZone, EdgeStatus.ThermoZone);
            return true;
        }
        
        private void HintToggle(IMessage rMessage)
        {
            //RISK 如果真是Toggle的话、那么还针对随着单元移动而修改位置。到是姑且可以写在Update里面。
            //现在认为_stageType状态是能够正常更新了。
            //就相当浪费、但是目前也没有很好的办法。
            CashingText.enabled = !CashingText.enabled;
            SetText(GetCashIO());
            showingThremoBoarder = !showingThremoBoarder;
            if (showingThremoBoarder)
            {
                var data = new BoardGridThermoZoneInquiry { BoardGridThermoZoneInquiryCallBack = BoardGridThermoZoneInquiry};
                MessageDispatcher.SendMessage(data);
            }
            else
            {
                ClearEdge(EdgeStatus.ThermoZone);
            }
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

            LayeringEdgeStatus = new Dictionary<EdgeStatus, bool>
            {
                {EdgeStatus.InfoZone,false},
                {EdgeStatus.ThermoZone,false},
            };
            UpdateEdge(new List<Vector2Int>(), false);
            
            CashingText.color = NeutralCashColoring;
            CashingText.enabled = false;
            
            MessageDispatcher.AddListener(InGameOverlayToggleEvent, HintToggle);
            MessageDispatcher.AddListener(CurrencyIOStatusChangedEvent,CurrencyIOStatusChangedEventHandler);
        }

        protected void OnDestroy()
        {
            MessageDispatcher.RemoveListener(CurrencyIOStatusChangedEvent,CurrencyIOStatusChangedEventHandler);
        }
    }
}