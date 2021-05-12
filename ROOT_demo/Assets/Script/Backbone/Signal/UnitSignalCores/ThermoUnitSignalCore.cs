using System.Collections.Generic;
using System.Linq;
using com.ootii.Messages;
using UnityEngine;
// ReSharper disable PossibleMultipleEnumeration

namespace ROOT.Signal
{
    public class ThermoUnitSignalCore : UnitSignalCoreBase
    {
        public override SignalType SignalType => SignalType.Thermo;

        private static readonly int[] ExpellingPatternListTierMapping = {2, 2, 2, 2, 2};
        //8、8、8、8、8
        private static readonly float[] PerGridScoreByTier = {1, 2, 3, 4, 5};
        //8、16、24、32、40
        private static readonly float ThermoScoreMultiplier = 1/2.0f;
        //4、8、12、16、20


        //热力单元有若干不相关参量：
        //1、所需范围的关系——等级越高范围越小、但是系数要比较浅。
            //还是问题是被遮挡后的数据怎么配置？
        //2、分数和被占领的面积的关系——负相关关系，这样的话，等级越高分数per占领格点会是指数上升。
        //3、分数极限和等级的关系——等级越高总数越高，同样比较浅。
        
        //迪公说的对、在另外两个玩法中、Tier的提高是一个正反馈的内容；但是现在的设计含有负反馈内容。
        //根据现在的实现、改上面三个变量就可以很方便的改正；想改成纯正反馈也很简单：
            //所有等级所需的范围相同；但是每个空单元单位提成是等比例的「1、2、3、4、5」；（自然、总数也会是和Tier等比例的。
        public List<Vector2Int> ExpellingPatternList
        {
            get
            {
                var circleTier = ExpellingPatternListTierMapping[Owner.Tier - 1];//Tier是Base1的；这里转成Base0的。
                var zone = Utils.GetPixelateCircle_Tier(circleTier);
                return zone.CenteredPatternList.Select(s => s + Owner.CurrentBoardPosition).ToList();
            }
        }

        public override List<Vector2Int> SingleInfoCollectorZone => ExpellingPatternList;
        
        private IEnumerable<Vector2Int> GetEmptyExpellingPos()
        {
            var validPattern = ExpellingPatternList.Where(Board.CheckBoardPosValidStatic);
            var emptyPos = validPattern.Where(Owner.GameBoard.CheckBoardPosValidAndEmpty);
            return emptyPos;
        }
        
        public override float SingleUnitScore
        {
            get
            {
                if (IsUnitActive && Owner.UnitHardware == HardwareType.Field)
                {
                    return Mathf.Round(GetEmptyExpellingPos().Count() * PerGridScoreByTier[Owner.Tier - 1] * ThermoScoreMultiplier);
                }
                return 0.0f;
            }
        }
        
        private bool ShowingThremoRange = false;

        private void ThremoRangeToggle(IMessage rMessage)
        {
            ShowingThremoRange = !ShowingThremoRange;
            UpdateRangeDisplay();
        }

        private Color totalemptyColor = new Color(0.0f, 1.0f, 0.0f);
        private Color totalBlockedColor =new Color(1.0f, 0.0f, 0.0f);
        private float ColorLerpingIdx => (8 - GetEmptyExpellingPos().Count()) / 8.0f;
        
        private void UpdateRangeDisplay()
        {
            if (Owner != null && Owner.UnitHardware == HardwareType.Field && Owner.ShopID == -1)
            {
                //这个颜色机制远不够更好，但总比没有强（一点儿）。
                var rawCol = Color.Lerp(totalemptyColor,totalBlockedColor,  ColorLerpingIdx);
                rawCol.a = ColorLerpingIdx;
                Owner.ThermoRangeIndicatorRenderer.color = rawCol;
                Owner.ThermoRangeIndicatorRoot.gameObject.SetActive(ShowingThremoRange && IsUnitActive);
                foreach (var rotationDirection in Common.Utils.ROTATION_LIST)
                {
                    var otherpos = Owner.CurrentBoardPosition + Common.Utils.ConvertDirectionToBoardPosOffset(rotationDirection);
                    Owner.FindThermoRangeIndicatorByDirection(rotationDirection).enabled = !Owner.GameBoard.CheckBoardPosValid(otherpos);
                }
            }
        }

        private void BoardSignalUpdatedHandler(IMessage rMessage)
        {
            UpdateRangeDisplay();
        }

        protected override void Awake()
        {
            base.Awake();
            MessageDispatcher.AddListener(WorldEvent.InGameOverlayToggleEvent, ThremoRangeToggle);
            MessageDispatcher.AddListener(WorldEvent.BoardSignalUpdatedEvent, BoardSignalUpdatedHandler);
        }
        
        private void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.BoardSignalUpdatedEvent, BoardSignalUpdatedHandler);
            MessageDispatcher.RemoveListener(WorldEvent.InGameOverlayToggleEvent, ThremoRangeToggle);
        }
    }
}
