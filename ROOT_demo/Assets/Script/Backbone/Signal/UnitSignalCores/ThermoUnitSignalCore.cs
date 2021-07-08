using System.Collections.Generic;
using System.Linq;
using ROOT.Consts;
using ROOT.RootMath;
using Sirenix.Utilities;
using UnityEngine;

// ReSharper disable PossibleMultipleEnumeration

namespace ROOT.Signal
{
    //TODO Thermo单元的信息范围还是需要优化和调整(？)
    public class ThermoUnitSignalCore : UnitSignalCoreBase
    {
        public override SignalType SignalType => SignalType.Thermo;

        private static readonly int[] ExpellingPatternListTierMapping = {2, 2, 2, 2, 2};
        //8、8、8、8、8
        private static readonly float[] PerGridScoreByTier = {1, 2, 3, 4, 5};
        //8、16、24、32、40
        private static readonly float ThermoScoreMultiplier = 0.5f;
        //4、8、12、16、20


        //热力单元有若干不相关参量：
        //1、所需范围的关系——等级越高范围越小、但是系数要比较浅。
            //还是问题是被遮挡后的数据怎么配置？
        //2、分数和被占领的面积的关系——负相关关系，这样的话，等级越高分数per占领格点会是指数上升。
        //3、分数极限和等级的关系——等级越高总数越高，同样比较浅。
        
        //迪公说的对、在另外两个玩法中、Tier的提高是一个正反馈的内容；但是现在的设计含有负反馈内容。
        //根据现在的实现、改上面三个变量就可以很方便的改正；想改成纯正反馈也很简单：
        //所有等级所需的范围相同；但是每个空单元单位提成是等比例的「1、2、3、4、5」；（自然、总数也会是和Tier等比例的。
        private IEnumerable<Vector2Int> ExpellingPatternList
        {
            get
            {
                var circleTier = ExpellingPatternListTierMapping[Owner.Tier - 1];//Tier是Base1的；这里转成Base0的。
                var zone = Utils.GetPixelateCircle_Tier(circleTier);
                return zone.CenteredPatternList.Select(s => s + Owner.CurrentBoardPosition).ToList();
            }
        }
        
        private IEnumerable<Vector2Int> RawInfoList//这个之所以要单列出来是因为这个要根据Tier有不同的范围。
        {
            get
            {
                var zone = Utils.GetPixelateCircle_Tier(Owner.Tier + 2);
                return zone.CenteredPatternList.Select(s => s + Owner.CurrentBoardPosition);
            }
        }
        
        private IEnumerable<Vector2Int> GetFilledExpellingPos()
        {
            var validPattern = RawInfoList.Where(Board.CheckBoardPosValidStatic);
            var emptyPos = validPattern.Where(Owner.GameBoard.CheckBoardPosValidAndFilled);
            return emptyPos;
        }
        
        private IEnumerable<Vector2Int> GetEmptyExpellingPos()
        {
            var validPattern = ExpellingPatternList.Where(Board.CheckBoardPosValidStatic);
            var emptyPos = validPattern.Where(Owner.GameBoard.CheckBoardPosValidAndEmpty);
            return emptyPos;
        }
        
        private UnitNeighbDataAsset _neighbDataAsset => SignalMasterMgr.Instance.GetUnitAssetByUnitType(SignalType, HardwareType.Field).NeighbouringData[0];
        protected override void InitNeighbouringLinkageDisplay()
        {
            foreach (var mat in Owner.UnitNeighbouringRendererRoot.LinkageIcons.Select(m=>m.material))
            {
                mat.mainTexture = _neighbDataAsset.NeighbouringSprite;
                mat.color = _neighbDataAsset.ColorTint;
            }
        }
        protected override void NeighbouringLinkageDisplay()
        {
            if (cachedCursorPos != Owner.CurrentBoardPosition|| Owner.UnitHardware != HardwareType.Field || !IsUnitActive )
            {
                Owner.UnitNeighbouringRendererRoot.LinkageIcons.ForEach(l => l.gameObject.SetActive(false));
                return;
            }

            var _8DirArray = StaticNumericData.V2Int8DirLib.ToArray();

            for (var i = 0; i < _8DirArray.Length; i++)
            {
                var inquiryBoardPos = Owner.CurrentBoardPosition + _8DirArray[i];
                var displayIcon = GameBoard != null && ((!GameBoard.CheckBoardPosValid(inquiryBoardPos))||GameBoard.CheckBoardPosValidAndFilled(inquiryBoardPos));
                Owner.UnitNeighbouringRendererRoot.LinkageIcons[i].gameObject.SetActive(displayIcon && ShowingNeighbouringLinkage);
            }
        }
        
        public override float SingleUnitScore
        {
            get
            {
                if (IsActiveFieldUnitThisSignal(Owner))
                {
                    return GetEmptyExpellingPos().Count() * PerGridScoreByTier[Owner.Tier - 1] * ThermoScoreMultiplier;
                }
                return 0.0f;
            }
        }

        private const float SenderRadius = 0.25f;
        private const float BlockerRadius = 1f;

        private void getRadiusedBlockList(ref List<Vector2Int> resKillList, Vector2Int[] rawInfoList, Point[] cirPts, Line[] cirLines, Vector2 center)
        {
            //RISK 这个先不做了感觉Simple版不够用之后再优化。
        }

        private void getSimpleBlockList(ref List<Vector2Int> resKillList, Vector2Int[] rawInfoList, Point[] cirPts, Line[] cirLines, Vector2 center)
        {
            var lineCenter = cirLines[0].Intersect(cirLines[1]);

            var v0 = (Point.ToVector2(cirPts[1]) - lineCenter).normalized;
            var v1 = (Point.ToVector2(cirPts[3]) - lineCenter).normalized;
            var vd = (v0 + v1) / 2.0f;
            var v01d = Vector2.Dot(vd, v1);
                    
            var facingLine = new Line(cirPts[1], cirPts[3]);
            var facingIndex = facingLine.SideIndex(center);
                    
            foreach (var vector2Int in rawInfoList)
            {
                var vC = (vector2Int - lineCenter).normalized;
                if (Vector2.Dot(vd, vC) > v01d)
                {
                    //vector2Int is within blocking angle.
                    var blockerFacingIndex = facingLine.SideIndex(vector2Int);
                    if (blockerFacingIndex * facingIndex < 0)
                    {
                        //vector2Int is within blocking angle and on blocking side.
                        resKillList.Add(vector2Int);
                    }
                }
            }
        }
        
        public override List<Vector2Int> SingleInfoCollectorZone
        {
            get
            {
                if (Owner.UnitHardware == HardwareType.Core)
                {
                    return new List<Vector2Int>();
                }

                var res = RawInfoList.ToArray();
                var resKillList = new List<Vector2Int>();

                var center = Owner.CurrentBoardPosition;
                var c0 = new Circle(Owner.CurrentBoardPosition, SenderRadius);

                foreach (var filledPos in GetFilledExpellingPos())
                {
                    if (filledPos==center)
                    {
                        continue;
                    }
                    
                    var c1 = new Circle(filledPos, BlockerRadius);
                    var lines = RootMath.RootMath.GetOuterCoTangentOf2Circle(c0, c1, out var resp);
                    getSimpleBlockList(ref resKillList,res, resp, lines, center);//日了，这个需要是填充的。
                    resKillList.Add(filledPos);
                }
                resKillList = resKillList.Distinct().ToList();
                return res.Where(v => !resKillList.Contains(v)).ToList();
            }
        }
    }
}
