using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using com.ootii.Messages;
using ROOT.Common;
using ROOT.Message;
using ROOT.Message.Inquiry;
using ROOT.SetupAsset;
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
        private readonly EdgeStatus[] _priorityList = {EdgeStatus.SingleInfoZone, EdgeStatus.InfoZone};

        EdgeStatus CurrentMaxPriorityEdgeStatus
        {
            get
            {
                //Debug.Log("LayeringEdgeStatus=" + LayeringEdgeStatus);
                foreach (var edgeStatus in _priorityList)
                {
                    if (LayeringEdgeStatus.HaveFlag(edgeStatus))
                    {
                        return edgeStatus;
                    }
                }
                return EdgeStatus.Off;
            }
        }

        [ReadOnly] public EdgeStatus LayeringEdgeStatus { private set; get; } = EdgeStatus.Off;
        private Color NormalColor => ColorLibManager.Instance.ColorLib.ROOT_MAT_BOARDGRID_NORMAL;
        private Color WarningColor=> ColorLibManager.Instance.ColorLib.ROOT_MAT_BOARDGRID_WARNING;
        private Color HeatSinkColor=> ColorLibManager.Instance.ColorLib.ROOT_MAT_BOARDGRID_HEATSINK;
        private Color InfoColColor => ColorLibManager.Instance.ColorLib.ROOT_MAT_BOARDGRID_INFO;
        private Color PreWarningColor => ColorLibManager.Instance.ColorLib.ROOT_MAT_BOARDGRID_PREWARNING;
        private Color NormalStrokeColor => ColorLibManager.Instance.ColorLib.ROOT_MAT_BOARDGRIDSTROKE_NORMAL;
        private Color FloatingStrokeColor => ColorLibManager.Instance.ColorLib.ROOT_MAT_BOARDGRIDSTROKE_FLOATING;
        private Color HighLightedStrokeColor => ColorLibManager.Instance.ColorLib.ROOT_MAT_BOARDGRIDSTROKE_HIGHLIGHTED;

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

        public Transform CashingTextRoot;
        
        public Color NegativeCashColoring;
        public Color PositiveCashColoring;
        public Color NeutralCashColoring;
        
        public CellStatus CellStatus
        {
            set
            {
                _cellStatus = value;
                UpdateCellColorByStatus();
            }
            get => _cellStatus;
        }

        private void UpdateCellColorByStatus()
        {
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

        private Color GetColorFromEdgeStatus(EdgeStatus edgeStatus)
        {
            switch (edgeStatus)
            {
                case EdgeStatus.InfoZone:
                    return ColorLibManager.Instance.ColorLib.ROOT_MAT_BOARDGRID_ZONE_INFO;
                case EdgeStatus.SingleInfoZone://TODO 这个考虑和单元本身联系起来。
                    return ColorLibManager.Instance.ColorLib.ROOT_MAT_BOARDGRID_ZONE_SINGLE;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateEdgeSingleSide(RotationDirection side, List<Vector2Int> zone,EdgeStatus edgeStatus)
        {
            //set是true的话是设置新的Zone、如果是false走fallback流程。
            var otherPos = OnboardPos + Common.Utils.ConvertDirectionToBoardPosOffset(side);
            var inZone = zone.Contains(OnboardPos);
            var otherInZone = zone.Contains(otherPos) && owner.CheckBoardPosValid(otherPos);
            _edgeDic[side].enabled = inZone && !otherInZone;
            _edgeDic[side].color = GetColorFromEdgeStatus(edgeStatus);
        }

        private void SetEdge_Core(EdgeStatus targetingStatus, List<Vector2Int> zone)
        {
            if (targetingStatus == EdgeStatus.Off) return;
            LayeringEdgeStatus = LayeringEdgeStatus.SetFlag(targetingStatus);
            if (CurrentMaxPriorityEdgeStatus != targetingStatus) return;
            Common.Utils.ROTATION_LIST.ForEach(edge => UpdateEdgeSingleSide(edge, zone, CurrentMaxPriorityEdgeStatus));
        }

        private void UnsetEdge_Core(EdgeStatus targetingStatus)
        {
            if (targetingStatus == EdgeStatus.Off) return;
            LayeringEdgeStatus = LayeringEdgeStatus.UnsetFlag(targetingStatus);
            if (CurrentMaxPriorityEdgeStatus == EdgeStatus.Off)
            {
                _edgeDic.Values.ForEach(renderer => renderer.enabled = false);
            }
            else
            {
                var zone = owner.BoardGirdDriver.ExtractCachedZone(CurrentMaxPriorityEdgeStatus);
                Common.Utils.ROTATION_LIST.ForEach(edge => UpdateEdgeSingleSide(edge, zone, CurrentMaxPriorityEdgeStatus));
            }
        }

        private void SetEdgeOff()
        {
            LayeringEdgeStatus = EdgeStatus.Off;
            _edgeDic.Values.ForEach(renderer => renderer.enabled = false);
        }

        public void SetEdge(List<Vector2Int> zone, EdgeStatus edgeStatus)
        {
            if (!zone.Contains(OnboardPos))
            {
                ClearEdge(edgeStatus);
                return;
            }

            SetEdge_Core(edgeStatus, zone);
        }

        public void ClearEdge(EdgeStatus edgeStatus)
        {
            UnsetEdge_Core(edgeStatus);
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

        private float GetCashIO()
        {
            if (!_boardCouldIOCurrency) return 0;

            if (_unitCouldGenerateIncome)
            {
                if (owner.CheckBoardPosValidAndEmpty(OnboardPos)) return 0;

                var unit=owner.FindUnitByPos(OnboardPos);
                if (unit == null) throw new ArgumentException();
                return unit.SignalCore.SingleUnitScore - HeatSinkCost;
            }

            if (owner.CheckBoardPosValidAndEmpty(OnboardPos)) return 0;
            
            return -HeatSinkCost;
        }

        private void SetText(float number)
        {
            var numberAsString = "";
            
            if (Math.Abs(number)>9.9f)
            {
                numberAsString = (number >= 0 ? "+" : "-") + Common.Utils.PaddingNum2Digit(Math.Abs(Mathf.RoundToInt(number)));
            }
            else if (number!=0.0f)
            {
                numberAsString = (number >= 0 ? "+" : "-") + Math.Abs(number).ToString("F1");
            }
            else
            {
                numberAsString = "+00";
            }

            if (!_boardCouldIOCurrency)
            {
                CashingText.color = NeutralCashColoring;
                CashingText.text = "---";
                return;
            }

            CashingText.text = numberAsString;

            if (!_unitCouldGenerateIncome)
            {
                CashingText.color = number == 0 ? NeutralCashColoring : NegativeCashColoring;
                return;
            }

            if (number != 0)
            {
                CashingText.color = number > 0 ? PositiveCashColoring : NegativeCashColoring;
                return;
            }

            var nonEmptyColoring = _cellStatus == CellStatus.Warning ? NegativeCashColoring : PositiveCashColoring;
            CashingText.color = owner.CheckBoardPosValidAndEmpty(OnboardPos) ? NeutralCashColoring : nonEmptyColoring;
        }

        /*private void SetText_Int(int number)
        {
            var numberAsString = (number >= 0 ? "+" : "-") + Common.Utils.PaddingNum2Digit(Math.Abs(number));
            
            if (!_boardCouldIOCurrency)
            {
                CashingText.color = NeutralCashColoring;
                CashingText.text = "---";
                return;
            }

            CashingText.text = numberAsString;

            if (!_unitCouldGenerateIncome)
            {
                CashingText.color = number == 0 ? NeutralCashColoring : NegativeCashColoring;
                return;
            }

            if (number != 0)
            {
                CashingText.color = number > 0 ? PositiveCashColoring : NegativeCashColoring;
                return;
            }

            var nonEmptyColoring = _cellStatus == CellStatus.Warning ? NegativeCashColoring : PositiveCashColoring;
            CashingText.color = owner.CheckBoardPosValidAndEmpty(OnboardPos) ? NeutralCashColoring : nonEmptyColoring;
        }*/

        private void Update()
        {
            if (CashingText.enabled)
            {
                SetText(GetCashIO());
            }
        }

        private bool displayOverlayhint=false;

        private bool hardwareToggle
        {
            get
            {
                if (!owner.CheckBoardPosValidAndFilled(OnboardPos)) return false;
                var unit = owner.FindUnitByPos(OnboardPos);
                return unit != null && unit.UnitHardware == HardwareType.Field;
            }
        }

        private void HintToggle(IMessage rMessage)
        {
            //RISK 如果真是Toggle的话、那么还针对随着单元移动而修改位置。到是姑且可以写在Update里面。
            //现在认为_stageType状态是能够正常更新了。
            //就相当浪费、但是目前也没有很好的办法。
            displayOverlayhint = !displayOverlayhint;
            CashingTextRoot.gameObject.SetActive(displayOverlayhint && hardwareToggle);
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
        
        private Vector2Int _cachedCursorPos = -Vector2Int.one;
        
        private void BoardSignalUpdatedHandler(IMessage rMessage)
        {
            if (rMessage is CursorMovedEventData data)
            {
                _cachedCursorPos = data.CurrentPosition;
            }
            CashingTextRoot.gameObject.SetActive(displayOverlayhint && hardwareToggle);
        }
        
        private void BoardGridHighLightSetHandler(IMessage rmessage)
        {
            if (rmessage is BoardGridHighLightSetData info)
            {
                //Debug.Log("(rmessage is BoardGridHighLightSetData info)");
                if (!info.Set)
                {
                    if (info.AllClear || info.Poses.Contains(OnboardPos))
                    {
                        UpdateCellColorByStatus();
                    }
                    return;
                }

                if (info.Poses.Contains(OnboardPos))
                {
                    switch (info.HLType)
                    {
                        case GridHighLightType.TypeA:
                            BoardGridMesh.material.color = ColorLibManager.Instance.ColorLib.ROOT_MAT_BOARDGRID_CUSTOM_HIGHLIGHTING_A;
                            break;
                        case GridHighLightType.TypeB:
                            BoardGridMesh.material.color = ColorLibManager.Instance.ColorLib.ROOT_MAT_BOARDGRID_CUSTOM_HIGHLIGHTING_B;
                            break;
                        default:
                            BoardGridMesh.material.color = ColorLibManager.Instance.ColorLib.ROOT_MAT_BOARDGRID_CUSTOM_HIGHLIGHTING_C;
                            break;
                    }
                }
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

            SetEdgeOff();
            
            CashingText.color = NeutralCashColoring;
            CashingTextRoot.gameObject.SetActive(false);
            
            MessageDispatcher.AddListener(InGameOverlayToggleEvent, HintToggle);
            MessageDispatcher.AddListener(CurrencyIOStatusChangedEvent,CurrencyIOStatusChangedEventHandler);
            MessageDispatcher.AddListener(BoardSignalUpdatedEvent, BoardSignalUpdatedHandler);
            MessageDispatcher.AddListener(BoardGridHighLightSetEvent, BoardGridHighLightSetHandler);
        }

        protected void OnDestroy()
        {
            MessageDispatcher.RemoveListener(BoardGridHighLightSetEvent, BoardGridHighLightSetHandler);
            MessageDispatcher.RemoveListener(BoardSignalUpdatedEvent, BoardSignalUpdatedHandler);
            MessageDispatcher.RemoveListener(CurrencyIOStatusChangedEvent,CurrencyIOStatusChangedEventHandler);
            MessageDispatcher.RemoveListener(InGameOverlayToggleEvent, HintToggle);
        }
    }
}