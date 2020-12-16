using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;

namespace ROOT
{
    public enum SkillType
    {
        TimeFromMoney,          //α：花钱买时间。
        FastForward,            //β：快速演进时间。（收费可能有返利）
        Swap,                   //γ：单元交换位置。（操作是个问题）
        RefreshHeatSink,        //δ-0：强制刷新HeatsinkPattern
        ResetHeatSink,        //δ-1：清理HeatSink添加的Pattern
        Discount,               //ε：下次商店会有折扣。
    }

    public class SkillMgr : MonoBehaviour
    {
        public List<SkillPalette> SkillPalettes;
        public List<InstancedSkillData> InstancedSkillData;
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

        private int discount = 0;

        public int CheckDiscount()
        {
            var tempDiscount = discount;
            discount = 0;
            InstancedSkillData.Where(skill => skill.SklType == SkillType.Discount).ForEach(skill => skill.SkillCoolDown = false);
            UpdateSkillPalettes();
            return tempDiscount;
        }

        private float _fastForwardRebate = -1.0f;
        private int _swapRadius = -1;
        
        #region SkillTemporalFramework

        //就是整个技能框架还是要弄一套配置框架………………🤣
        private void ActiveSkill(GameAssets currentLevelAsset, int skillIndex)
        {
            var skill = InstancedSkillData[skillIndex];
            if (!skill.SkillEnabled) return;
            if (skill.CountLimit != -1 && skill.RemainingCount <= 0) return;

            bool moneySpent = false, skillActived = false;
            switch (skill.SklType)
            {
                case SkillType.TimeFromMoney:
                    //持续技能
                    moneySpent = currentLevelAsset.GameStateMgr.SpendSkillCurrency(skill.Cost);
                    if (moneySpent)
                    {
                        skillActived = true;
                        CurrentSkillType = SkillType.TimeFromMoney;
                        WorldCycler.ExpectedStepDecrement(skill.TimeGain);
                        WorldLogic.UpdateUICurrencyVal(currentLevelAsset);//因为这个时间点后就AutoDrive了，所以就没机会调UpdateBoard了，所以先在这里调一下。
                    }
                    break;
                case SkillType.FastForward:
                    //持续技能
                    skillActived = true;
                    _fastForwardRebate = 1.00f + 0.01f * skill.AdditionalIncome;
                    WorldCycler.ExpectedStepIncrement(skill.FastForwardCount);
                    CurrentSkillType = SkillType.FastForward;
                    break;
                case SkillType.Swap:
                    //瞬时技能
                    //RISK 日了，这里使用键盘和鼠标流程得变，但是还是有问题。
                    moneySpent = currentLevelAsset.GameStateMgr.SpendSkillCurrency(skill.Cost);
                    if (moneySpent)
                    {
                        skillActived = true; //这里的计数还可以取消。
                        swapAlipay = skill.Cost;
                        CurrentSkillType = SkillType.Swap;
                        _swapRadius = skill.radius;
                        if (StartGameMgr.UseKeyboard)
                        {
                            unitAPosition = currentLevelAsset.Cursor.CurrentBoardPosition;
                            UpdateAIndicator(currentLevelAsset, unitAPosition);
                            WorldLogic.UpdateUICurrencyVal(currentLevelAsset);
                        }
                        else if (StartGameMgr.UseMouse)
                        {
                            _mouseWaitingUnitA = true;
                            //throw new NotImplementedException();
                        }
                    }

                    break;
                case SkillType.Discount:
                    //延迟技能
                    moneySpent = currentLevelAsset.GameStateMgr.SpendSkillCurrency(skill.Cost);
                    if (moneySpent)
                    {
                        skillActived = true;
                        discount = skill.Discount;
                        skill.SkillCoolDown = true;
                        WorldLogic.UpdateUICurrencyVal(currentLevelAsset);
                    }
                    break;
                case SkillType.RefreshHeatSink:
                    moneySpent = currentLevelAsset.GameStateMgr.SpendSkillCurrency(skill.Cost);
                    if (moneySpent)
                    {
                        currentLevelAsset.GameBoard.UpdatePatternID();
                        WorldLogic.UpdateUICurrencyVal(currentLevelAsset);
                    }
                    break;
                case SkillType.ResetHeatSink:
                    moneySpent = currentLevelAsset.GameStateMgr.SpendSkillCurrency(skill.Cost);
                    if (moneySpent)
                    {
                        currentLevelAsset.GameBoard.ResetHeatSink();
                        WorldLogic.UpdateUICurrencyVal(currentLevelAsset);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (skillActived&&skill.CountLimit!=-1)
            {
                skill.RemainingCount--;
                if (skill.RemainingCount<=0)
                {
                    skill.SkillEnabled = false;
                }
            }
            UpdateSkillPalettes();
        }

        private void UpdateSkillActive(GameAssets currentLevelAsset)
        {
            //是在这儿，把Discount的enable数据清掉了。Discount的SkillCost还真是大于0.
            //蛋疼，那个实例化Skill里面再加一个coolDown
            InstancedSkillData.Where(skill=>skill.Cost>0).ForEach(skill => skill.SkillEnabled = (skill.Cost <= currentLevelAsset.GameStateMgr.GetCurrency()));
            UpdateSkillPalettes();
        }

        private bool _mouseWaitingUnitA = false;
        private bool _mouseWaitingUnitB = false;

        public void UpKeepSkill(GameAssets currentLevelAsset)
        {
            var autoDrive = WorldCycler.NeedAutoDriveStep;
            UpdateSkillActive(currentLevelAsset);
            if (!CurrentSkillType.HasValue) return;
            switch (CurrentSkillType.Value)
            {
                case SkillType.Swap:
                    break;
                case SkillType.RefreshHeatSink:
                    break;
                case SkillType.FastForward:
                    currentLevelAsset.CurrencyRebate = 1.00f;

                    if (!autoDrive.HasValue)
                    {
                        _fastForwardRebate = -1.00f;
                        CurrentSkillType = null;
                        //RISK 本质上是在乱搞flow，这个还是得想辙。而且这个函数也不能这么搞。
                        //flow结构这个时候不要那么八股，还是先用上，需求多了，这个可能要改成基于监听的。
                        WorldLogic.UpdateUICurrencyVal(currentLevelAsset);
                    }

                    if (_fastForwardRebate > 0.0f)
                    {
                        currentLevelAsset.CurrencyRebate = _fastForwardRebate;
                    }

                    break;
                case SkillType.Discount:
                    break;
                case SkillType.TimeFromMoney:
                    if (!autoDrive.HasValue)
                    {
                        CurrentSkillType = null;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public void TriggerSkill(GameAssets currentLevelAsset,in ControllingPack ctrlPack)
        {
            if (!_skillEnabled || !ctrlPack.HasFlag(ControllingCommand.Skill) || CurrentSkillType.HasValue) return;//这里return掉了。
            ActiveSkill(currentLevelAsset, ctrlPack.SkillID);
        }

        #endregion

        #region SwapSection

        #region IndicatorRelated

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

        #endregion

        private Vector2Int unitAPosition = new Vector2Int(-1, -1);//这个的值赋进去了，只是要想着再画出来。
        private int swapAlipay = 0;
        private Vector2Int oldCurrentPos = new Vector2Int(-1, -1);

        IEnumerator DelayedCheckMouseUnitB()
        {
            yield return new WaitForSeconds(0.01f);
            _mouseWaitingUnitB = true;
        }

        public void SwapTick(GameAssets currentLevelAsset, ControllingPack ctrlPack)
        {
            //RISK 这里键盘⌨和鼠标🖱只能是两种逻辑，但是就是中间切了输入怎么办？
            //⌨=>🖱理论上哈可以，但是反过来是干脆缺一个阶段……
            //有两大解决方案：
            //1、给键盘强制多加一个阶段以和鼠标匹配。（可能还得这么搞，但是现在先不
            //   目前是在swap过程中不识别切换
            //2、干脆不允许局中切换……
            Debug.Assert(_swapRadius != -1);

            if (StartGameMgr.UseKeyboard)
            {
                //RISK 这里切换的时候会出问题……
                var res = Utils.PositionRandomization_NormalDistro(
                    ctrlPack.CurrentPos, _swapRadius, 0.65f, Board.BoardLength,
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
                if (!ctrlPack.HasFlag(ControllingCommand.Confirm) &&
                    !ctrlPack.HasFlag(ControllingCommand.Cancel)) return;

                if (CurrentSkillType == SkillType.Swap)
                {
                    if (ctrlPack.HasFlag(ControllingCommand.Confirm))
                    {
                        var unitBPosition = res[selected];
                        if (unitAPosition != unitBPosition)
                        {
                            var res1 = currentLevelAsset.GameBoard.SwapUnit(unitAPosition, unitBPosition);
                            if (!res1)
                            {
                                Debug.LogWarning("swap nothing to nothing!!");
                            }
                        }
                    }
                    else if (ctrlPack.HasFlag(ControllingCommand.Cancel))
                    {
                        currentLevelAsset.GameStateMgr.AddCurrency(swapAlipay);
                        swapAlipay = 0;
                        WorldLogic.UpdateUICurrencyVal(currentLevelAsset);
                    }

                    CleanIndicator(currentLevelAsset);
                    CurrentSkillType = null;
                }
            }
            else if (StartGameMgr.UseMouse)
            {
                //RISK 这里切换的时候会出问题……同上
                if (_mouseWaitingUnitA)
                {
                    if (ctrlPack.HasFlag(ControllingCommand.ClickOnGrid))
                    {
                        unitAPosition = ctrlPack.CurrentPos;
                        UpdateAIndicator(currentLevelAsset, unitAPosition);
                        WorldLogic.UpdateUICurrencyVal(currentLevelAsset);
                        _mouseWaitingUnitA = false;
                        StartCoroutine(DelayedCheckMouseUnitB()); //这里可能需要一个AntiSpam，可以加个协程延迟。
                    }
                    else if (ctrlPack.HasFlag(ControllingCommand.Cancel))
                    {
                        currentLevelAsset.GameStateMgr.AddCurrency(swapAlipay);
                        swapAlipay = 0;
                        WorldLogic.UpdateUICurrencyVal(currentLevelAsset);
                        _mouseWaitingUnitA = false;
                        _mouseWaitingUnitB = false;
                    }
                }
                else if (_mouseWaitingUnitB)
                {
                    List<Vector2Int> res = new List<Vector2Int>();
                    int selected = -1;
                    if (ctrlPack.HasFlag(ControllingCommand.FloatingOnGrid))
                    {
                        res = Utils.PositionRandomization_NormalDistro(
                            ctrlPack.CurrentPos, _swapRadius, 0.65f, Board.BoardLength,
                            out selected);

                        if (oldCurrentPos != ctrlPack.CurrentPos)
                        {
                            //这个加个Anti-spam。
                            CleanIndicatorFrame(currentLevelAsset);
                            //这里根据res把所有的标记都画出来。
                            UpdateBIndicator(currentLevelAsset, res);
                            oldCurrentPos = ctrlPack.CurrentPos;
                        }
                    }

                    if (ctrlPack.HasFlag(ControllingCommand.ClickOnGrid))
                    {
                        var unitBPosition = res[selected];
                        if (unitAPosition != unitBPosition)
                        {
                            var res1 = currentLevelAsset.GameBoard.SwapUnit(unitAPosition, unitBPosition);
                            if (!res1)
                            {
                                Debug.LogWarning("swap nothing to nothing!!");
                            }
                        }

                        _mouseWaitingUnitA = false;
                        _mouseWaitingUnitB = false;
                        CleanIndicator(currentLevelAsset);
                        CurrentSkillType = null;
                    }
                    else if (ctrlPack.HasFlag(ControllingCommand.Cancel))
                    {
                        currentLevelAsset.GameStateMgr.AddCurrency(swapAlipay);
                        swapAlipay = 0;
                        WorldLogic.UpdateUICurrencyVal(currentLevelAsset);

                        _mouseWaitingUnitA = false;
                        _mouseWaitingUnitB = false;
                        CleanIndicator(currentLevelAsset);
                        CurrentSkillType = null;
                    }
                }
            }
        }

        #endregion

        private string SkillTagText(InstancedSkillData skill)
        {
            switch (skill.SklType)
            {
                case SkillType.TimeFromMoney when skill.CountLimit != -1:
                    return "<color=#003663>RMN=" + skill.RemainingCount + "</color> <color=#00b35c>" + skill.TimeGain + "<<</color>";
                case SkillType.TimeFromMoney:
                    return "<color=#00b35c>" + skill.TimeGain + "<<</color>";
                case SkillType.FastForward:
                    return "<color=#8a0b00>>>" + skill.FastForwardCount + "</color> <color=#00b35c>+" + skill.AdditionalIncome + "%</color>";
                case SkillType.Swap:
                    return "<color=#8a0b00>-" + skill.Cost + "</color> <color=#00b35c>R=" + skill.radius + "</color>";
                case SkillType.RefreshHeatSink:
                    return "<color=#8a0b00>-" + skill.Cost + "</color><color=#00b35c>Refresh</color>";
                case SkillType.Discount:
                    return "<color=#8a0b00>-" + skill.Cost + "</color> <color=#00b35c>-" + skill.Discount + "%</color>";
                case SkillType.ResetHeatSink:
                    return "<color=#8a0b00>-" + skill.Cost + "</color><color=#00b35c>Reset</color>";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PopulateInstancedSkill()
        {
            InstancedSkillData = new List<InstancedSkillData>();
            for (var i = 0; i < SkillPalettes.Count; i++)
            {
                InstancedSkillData.Add(new InstancedSkillData(SkillData.SkillDataList[i]));
            }
        }

        private void UpdateSkillPalettes()
        {
            for (var i = 0; i < SkillPalettes.Count; i++)
            {
                SkillPalettes[i].SkillID = i;
                SkillPalettes[i].SklType = InstancedSkillData[i].SklType;
                SkillPalettes[i].SkillTagText = SkillTagText(InstancedSkillData[i]);
                SkillPalettes[i].SkillIconSprite = InstancedSkillData[i].SkillIcon;
                SkillPalettes[i].SkillEnabled = InstancedSkillData[i].SkillEnabled && !InstancedSkillData[i].SkillCoolDown;
            }
        }

        public void Awake()
        {
            PopulateInstancedSkill();
            UpdateSkillPalettes();
        }
    }
}