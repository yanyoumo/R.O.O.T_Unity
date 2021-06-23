using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT.Signal
{
    public class FirewallUnitSignalCore : UnitSignalCoreBase
    {
        //TODO 这个数据可以考虑直接写进去。
        private float[] scoreMultiplier = {0.3f, 0.65f, 1.0f, 0.9f, 0.8f, 0.7f, 0.5f, 0.3f, 0.1f};
        
        [ShowInInspector] public override SignalType SignalType => SignalType.Firewall;

        //只要这么写，查询任何一个在圈内防火墙单元返回的都是全部的面积。上层轮询的时候反正是distinct的，所以理论上无所谓。
        public override List<Vector2Int> SingleInfoCollectorZone => FirewallSignalAsset.CurrentFirewallCircle.Contains(Owner.CurrentBoardPosition) ? FirewallSignalAsset.CurrentFirewallCircle : new List<Vector2Int>();

        private List<Vector2Int> SearchingPatternList => Utils.GetPixelateCircle_Tier(2).CenteredPatternList.Select(s => s + Owner.CurrentBoardPosition).ToList();

        private int NeighbouringFirewallUnitCount => SearchingPatternList.Select(p => GameBoard.FindUnitByPos(p)).Count(u => u != null && u != Owner && u.UnitSignal == SignalType.Firewall);
        
        private const int perFirewallFieldUnitPrice = 1;//这个系数一定要往上调。

        public override float SingleUnitScore => (IsUnitActive && Owner.UnitHardware == HardwareType.Field) ? perFirewallFieldUnitPrice * scoreMultiplier[NeighbouringFirewallUnitCount] * Owner.Tier : 0.0f;
        
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

            for (var i = 0; i < neighbouringOffsetList.Length; i++)
            {
                var inquiryBoardPos = Owner.CurrentBoardPosition + neighbouringOffsetList[i];
                var displayIcon = false;
                if (GameBoard != null && GameBoard.CheckBoardPosValidAndFilled(inquiryBoardPos))
                {
                    var otherUnit = GameBoard.FindUnitByPos(inquiryBoardPos);
                    Debug.Assert(otherUnit != null);
                    displayIcon = otherUnit.UnitSignal == SignalType.Firewall;
                }

                switch (NeighbouringFirewallUnitCount)
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