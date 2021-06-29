using System.Collections.Generic;
using System.Linq;
using ROOT.Consts;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT.Signal
{
    public class ClusterUnitSignalCore : UnitSignalCoreBase
    {
        [ShowInInspector] public override SignalType SignalType => SignalType.Cluster;
        
        private const int perMatrixFieldUnitPrice = 1;
        private float[] scoreMultiplier = {0.2f, 0.3f, 0.4f, 0.5f, 0.75f, 1f, 1.5f, 2f, 2.5f};

        public override List<Vector2Int> SingleInfoCollectorZone
        {
            get
            {
                if (IsUnitActive && Owner.UnitHardware == HardwareType.Field)
                {
                    foreach (var matrixIsland in ClusterSignalAsset.ClusterIslandPack)
                    {
                        if (matrixIsland.Contains(Owner.CurrentBoardPosition))
                        {
                            return matrixIsland.GetMatrixIslandInfoZone().ToList();
                        }
                    }
                }
                return new List<Vector2Int>();
            }
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
                var displayIcon = false;
                if (GameBoard != null && GameBoard.CheckBoardPosValidAndFilled(inquiryBoardPos))
                {
                    var otherUnit = GameBoard.FindUnitByPos(inquiryBoardPos);
                    Debug.Assert(otherUnit != null);
                    displayIcon = IsActiveUnitThisSignal(otherUnit);
                }
                Owner.UnitNeighbouringRendererRoot.LinkageIcons[i].gameObject.SetActive(displayIcon && ShowingNeighbouringLinkage);
            }
        }
        
        private IEnumerable<Vector2Int> SearchingPatternList => Utils.GetPixelateCircle_Tier(2).CenteredPatternList.Select(s => s + Owner.CurrentBoardPosition).ToList();
        private int NeighbouringClusterUnitCount => SearchingPatternList.Select(p => GameBoard.FindUnitByPos(p)).Count(u => u != null && u != Owner && IsActiveUnitThisSignal(u));
        public override float SingleUnitScore => IsActiveFieldUnitThisSignal(Owner) ? perMatrixFieldUnitPrice * scoreMultiplier[NeighbouringClusterUnitCount] * Owner.Tier : 0.0f;
    }
}