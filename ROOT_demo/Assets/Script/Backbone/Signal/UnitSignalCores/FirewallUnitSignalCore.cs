using System;
using System.Collections.Generic;
using System.Linq;
using ROOT.Consts;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT.Signal
{
    public class FirewallUnitSignalCore : UnitSignalCoreBase
    {
        //这个数据对于已有的1~5的Tier粒度不够。可能要比较极限的数据配置。可能改四方向。 不用了，系统支持float数据了
        private float[] scoreMultiplier = {0.3f, 0.65f, 1.0f, 0.9f, 0.8f, 0.7f, 0.5f, 0.3f, 0.1f};
        
        [ShowInInspector] public override SignalType SignalType => SignalType.Firewall;

        private List<Vector2Int> FirewallCircle => FirewallSignalAsset.CurrentFirewallCircle;
        private List<Vector2Int> FirewallInner => FirewallSignalAsset.CurrentFirewallInner;
        
        
        //只要这么写，查询任何一个在圈内防火墙单元返回的都是全部的面积。上层轮询的时候反正是distinct的，所以理论上无所谓。
        public override List<Vector2Int> SingleInfoCollectorZone
        {
            get
            {
                var withInFireWallCircle = FirewallCircle.Contains(Owner.CurrentBoardPosition);
                if (!withInFireWallCircle) return new List<Vector2Int>();

                if (Owner.Tier < 2) return FirewallInner;

                var localZone = Utils.GetPixelateCircle_Tier(Owner.Tier - 2);
                var zone = localZone.PatternList.Select(v => v + Owner.CurrentBoardPosition- new Vector2Int(localZone.CircleRadius, localZone.CircleRadius));
                return FirewallInner.Union(zone).Distinct().ToList();
            }
        }

        private List<Vector2Int> SearchingPatternList => Utils.GetPixelateCircle_Tier(2).CenteredPatternList.Select(s => s + Owner.CurrentBoardPosition).ToList();
        private int NeighbFirewallUnitCount => SearchingPatternList.Select(p => GameBoard.FindUnitByPos(p)).Count(u => u != null && u != Owner && u.SignalCore.IsUnitActive && u.UnitSignal == SignalType.Firewall);

        public override float SingleUnitScore => IsActiveFieldUnitThisSignal(Owner) ? scoreMultiplier[NeighbFirewallUnitCount] * Owner.Tier : 0.0f;
        
        private UnitNeighbDataAsset _defaultNeighbDataAsset => SignalMasterMgr.Instance.GetUnitAssetByUnitType(SignalType, HardwareType.Field).NeighbouringData[0];
        private UnitNeighbDataAsset _lowNeighbDataAsset => SignalMasterMgr.Instance.GetUnitAssetByUnitType(SignalType, HardwareType.Field).NeighbouringData[1];
        private UnitNeighbDataAsset _highNeighbDataAsset => SignalMasterMgr.Instance.GetUnitAssetByUnitType(SignalType, HardwareType.Field).NeighbouringData[2];
        protected override void InitNeighbouringLinkageDisplay()
        {
            foreach (var mat in Owner.UnitNeighbouringRendererRoot.LinkageIcons.Select(m=>m.material))
            {
                mat.mainTexture = _defaultNeighbDataAsset.NeighbouringSprite;
                mat.color = _defaultNeighbDataAsset.ColorTint;
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
                var displayIcon = false;
                if (GameBoard != null && GameBoard.CheckBoardPosValidAndFilled(inquiryBoardPos))
                {
                    var otherUnit = GameBoard.FindUnitByPos(inquiryBoardPos);
                    Debug.Assert(otherUnit != null);
                    displayIcon = otherUnit.SignalCore.IsUnitActive && (otherUnit.UnitSignal == SignalType.Firewall);
                }

                switch (NeighbFirewallUnitCount)
                {
                    case 0:
                    case 1:
                        Owner.UnitNeighbouringRendererRoot.LinkageIcons[i].material.mainTexture = _lowNeighbDataAsset.NeighbouringSprite;
                        Owner.UnitNeighbouringRendererRoot.LinkageIcons[i].material.color = _lowNeighbDataAsset.ColorTint;
                        break;
                    case 2:
                        Owner.UnitNeighbouringRendererRoot.LinkageIcons[i].material.mainTexture = _defaultNeighbDataAsset.NeighbouringSprite;
                        Owner.UnitNeighbouringRendererRoot.LinkageIcons[i].material.color = _defaultNeighbDataAsset.ColorTint;
                        break;
                    default:
                        Owner.UnitNeighbouringRendererRoot.LinkageIcons[i].material.mainTexture = _highNeighbDataAsset.NeighbouringSprite;
                        Owner.UnitNeighbouringRendererRoot.LinkageIcons[i].material.color = _highNeighbDataAsset.ColorTint;
                        break;
                }
                Owner.UnitNeighbouringRendererRoot.LinkageIcons[i].gameObject.SetActive(displayIcon && ShowingNeighbouringLinkage);
            }
        }
    }
}