using System.Collections.Generic;
using System.Linq;
using ROOT.Consts;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT.Signal
{
    public class MatrixUnitSignalCore : UnitSignalCoreBase
    {
        [ShowInInspector] public override SignalType SignalType => SignalType.Matrix;
        
        private const int perMatrixFieldUnitPrice = 1;
        private float[] scoreMultiplier = {0.075f, 0.25f, 0.5f, 0.75f, 1.0f};

        public override List<Vector2Int> SingleInfoCollectorZone
        {
            get
            {
                if (IsUnitActive && Owner.UnitHardware == HardwareType.Field)
                {
                    foreach (var matrixIsland in MatrixSignalAsset.MatrixIslandPack)
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

            var _4DirArray = StaticNumericData.V2Int4DirLib.ToArray();

            for (var i = 0; i < _4DirArray.Length; i++)
            {
                var inquiryBoardPos = Owner.CurrentBoardPosition + _4DirArray[i];
                var displayIcon = false;
                if (GameBoard != null && GameBoard.CheckBoardPosValidAndFilled(inquiryBoardPos))
                {
                    var otherUnit = GameBoard.FindUnitByPos(inquiryBoardPos);
                    Debug.Assert(otherUnit != null);
                    displayIcon = IsActiveFieldUnitThisSignal(otherUnit);
                }
                Owner.UnitNeighbouringRendererRoot.LinkageIcons[i].gameObject.SetActive(displayIcon && ShowingNeighbouringLinkage);
            }
        }

        private bool IsActiveFieldUnitThisSignal(Unit u) => u.SignalCore.IsUnitActive && u.UnitSignal == SignalType && u.UnitHardware == HardwareType.Field;
        private IEnumerable<Vector2Int> SearchingPatternList => Utils.GetPixelateCircle_Tier(1).CenteredPatternList.Select(s => s + Owner.CurrentBoardPosition).ToList();
        private int NeighbouringMatrixUnitCount => SearchingPatternList.Select(p => GameBoard.FindUnitByPos(p)).Count(u => u != null && u != Owner && IsActiveFieldUnitThisSignal(u));
        public override float SingleUnitScore => IsActiveFieldUnitThisSignal(Owner) ? perMatrixFieldUnitPrice * scoreMultiplier[NeighbouringMatrixUnitCount] * Owner.Tier : 0.0f;
    }
}