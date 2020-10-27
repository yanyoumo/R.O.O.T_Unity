using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public enum SkillType
    {
        TimeFromMoney,          //α：花钱买时间。
        FastForward,            //β：快速演进时间。（收费可能有返利）
        Swap,                   //γ：单元交换位置。（操作是个问题）
        RefreshHeatSink,        //δ：强制刷新HeatsinkPattern/清理HeatSink添加的Pattern
        Discount,               //ε：下次商店会有折扣。
    }

    /// <summary>
    /// 是这个的问题，TriggerSkill和UpdateSkill可能要分开的。
    /// 换句话说，这个玩意儿还要有个内部状态。这个内部状态很难搞。
    /// </summary>
    public class SkillMgr : MonoBehaviour
    {
        public Transform IconFramework;
        public SkillData SkillData;

        public SkillType? CurrentSkillType { private set; get; } = null;

        private bool _skillEnabled;
        public bool SkillEnabled
        {
            set
            {
                _skillEnabled = value;
                IconFramework.gameObject.SetActive(_skillEnabled);
            }
            get => _skillEnabled;
        }

        private float _fastForwardRebate = -1.0f;
        private int _swapRadius = -1;

        //就是整个技能框架还是要弄一套配置框架………………🤣
        private void ActiveSkill(GameAssets currentLevelAsset, SkillBase skill)
        {
            bool moneySpent = false;
            switch (skill.SklType)
            {
                case SkillType.TimeFromMoney:
                    //持续技能
                    //RISK 之前说到的这个技能可能无法很好的解反馈的问题还在。（Gameplay的问题，不是程序的问题。）
                    moneySpent = currentLevelAsset.GameStateMgr.SpendSkillCurrency(skill.Cost);
                    if (moneySpent)
                    {
                        CurrentSkillType = SkillType.TimeFromMoney;
                        WorldCycler.ExpectedStepDecrement(skill.TimeGain);
                        //因为这个时间点后就AutoDrive了，所以就没机会调UpdateBoard了，所以先在这里调一下。
                        WorldLogic.UpdateBoardData(currentLevelAsset);
                    }
                    break;
                case SkillType.FastForward:
                    //持续技能
                    _fastForwardRebate = 1.00f + 0.01f * skill.AdditionalIncome;
                    WorldCycler.ExpectedStepIncrement(skill.FastForwardCount);
                    CurrentSkillType = SkillType.FastForward;
                    break;
                case SkillType.Swap:
                    //瞬时技能
                    moneySpent = currentLevelAsset.GameStateMgr.SpendSkillCurrency(skill.Cost);
                    if (moneySpent)
                    {
                        CurrentSkillType = SkillType.Swap;
                        _swapRadius = skill.radius;
                        unitAPosition = currentLevelAsset.Cursor.CurrentBoardPosition;
                        UpdateAIndicator(currentLevelAsset, unitAPosition);
                    }
                    break;
                case SkillType.RefreshHeatSink:
                    //瞬时技能
                    break;
                case SkillType.Discount:
                    //延迟技能
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //currentSkillType = skill.SklType; 
        }

        internal static void CleanIndicatorFrame(GameAssets currentLevelAsset)
        {
            if (currentLevelAsset.SkillIndGoB != null)
            {
                if (currentLevelAsset.SkillIndGoB.Length > 0)
                {
                    foreach (var go in currentLevelAsset.SkillIndGoB)
                    {
                        currentLevelAsset.Owner.WorldLogicRequestDestroy(go);
                        currentLevelAsset.SkillIndGoB = null;
                    }
                }
            }
        }


        internal static void CleanIndicator(GameAssets currentLevelAsset)
        {
            if (currentLevelAsset.SkillIndGoA != null)
            {
                currentLevelAsset.Owner.WorldLogicRequestDestroy(currentLevelAsset.SkillIndGoA.gameObject);
                currentLevelAsset.SkillIndGoA = null;
            }

            CleanIndicatorFrame(currentLevelAsset);
        }

        private GameObject CreateIndicator(GameAssets currentLevelAsset, Vector2Int pos, Color col)
        {
            GameObject indicator = currentLevelAsset.Owner.WorldLogicRequestInstantiate(currentLevelAsset.CursorTemplate);
            Cursor indicatorCursor = indicator.GetComponent<Cursor>();
            indicatorCursor.SetIndMesh();
            indicatorCursor.InitPosWithAnimation(pos);
            indicatorCursor.UpdateTransform(currentLevelAsset.GameBoard.GetFloatTransform(indicatorCursor.CurrentBoardPosition));
            indicatorCursor.CursorColor = col;
            return indicator;
        }

        private void UpdateAIndicator(GameAssets currentLevelAsset, Vector2Int Pos)
        {
            var col = ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_SKILL_SWAP_UNITA);
            currentLevelAsset.SkillIndGoA = CreateIndicator(currentLevelAsset, Pos, col);
        }

        private void UpdateBIndicator(GameAssets currentLevelAsset,List<Vector2Int> incomings)
        {
            var count = incomings.Count;
            currentLevelAsset.SkillIndGoB = new GameObject[count];
            for (var i = 0; i < count; i++)
            {
                var col = ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_SKILL_SWAP_UNITB);
                currentLevelAsset.SkillIndGoB[i] = CreateIndicator(currentLevelAsset, incomings[i], col);
            }
        }

        private Vector2Int unitAPosition = new Vector2Int(-1, -1);//这个的值赋进去了，只是要想着再画出来。
        private Vector2Int oldCurrentPos = new Vector2Int(-1, -1);

        public void SwapTick(GameAssets currentLevelAsset, ControllingPack ctrlPack)
        {
            Debug.Assert(_swapRadius != -1);
            var res = Utils.PositionRandomization_Dummy(
                ctrlPack.CurrentPos, _swapRadius, 0.3f, Board.BoardLength,
                out var selected);

            if (oldCurrentPos != ctrlPack.CurrentPos)
            {
                //这个加个Anti-spam。
                CleanIndicatorFrame(currentLevelAsset);
                //这里根据res把所有的标记都画出来。
                UpdateBIndicator(currentLevelAsset, res);
                oldCurrentPos = ctrlPack.CurrentPos;
            }
            
            //Confirm Or Cancel Gate
            if (!ctrlPack.HasFlag(ControllingCommand.Confirm) && !ctrlPack.HasFlag(ControllingCommand.Cancel)) return;

            if (ctrlPack.HasFlag(ControllingCommand.Confirm))
            {
                var unitBPosition = res[selected];
                if (unitAPosition != unitBPosition)
                {
                    var res1 = currentLevelAsset.GameBoard.SwapUnit(unitAPosition, unitBPosition);
                    Debug.Assert(res1);
                }
            }

            if (CurrentSkillType == SkillType.Swap)
            {
                CleanIndicator(currentLevelAsset);
                CurrentSkillType = null;
            }
        }

        public void UpKeepSkill(GameAssets currentLevelAsset)
        {
            var AutoDrive = WorldCycler.NeedAutoDriveStep;

            if (CurrentSkillType.HasValue)
            {
                var skltyp = CurrentSkillType.Value;
                switch (skltyp)
                {
                    case SkillType.Swap:
                        break;
                    case SkillType.RefreshHeatSink:
                        break;
                    case SkillType.FastForward:
                        currentLevelAsset.CurrencyRebate = 1.00f;

                        if (!AutoDrive.HasValue)
                        {
                            _fastForwardRebate = -1.00f;
                            CurrentSkillType = null;
                            //RISK 本质上是在乱搞flow，这个还是得想辙。而且这个函数也不能这么搞。
                            //flow结构这个时候不要那么八股，还是先用上，需求多了，这个可能要改成基于监听的。
                            WorldLogic.UpdateBoardData(currentLevelAsset);
                        }

                        if (_fastForwardRebate > 0.0f)
                        {
                            currentLevelAsset.CurrencyRebate = _fastForwardRebate;
                        }

                        break;
                    case SkillType.Discount:
                        break;
                    case SkillType.TimeFromMoney:
                        if (!AutoDrive.HasValue)
                        {
                            CurrentSkillType = null;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void TriggerSkill(GameAssets currentLevelAsset,in ControllingPack ctrlPack)
        {
            if (!_skillEnabled || !ctrlPack.HasFlag(ControllingCommand.Skill) || CurrentSkillType.HasValue) return;//这里return掉了。
            ActiveSkill(currentLevelAsset, SkillData.SkillDataList[ctrlPack.SkillID]);
        }
    }
}