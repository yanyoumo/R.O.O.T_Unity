using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Messages;
using ROOT.Message;
using ROOT.SetupAsset;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT
{
    public class SkillMgr : MonoBehaviour
    {
        /*public Material DefaultMat;
        public Material BWMat;*/
        
        public List<SkillPalette> SkillPalettes;
        private List<InstancedSkillData> InstancedSkillData { get; set; }
        public Transform IconFramework;
        public SkillData SkillData;
        
        private float _fastForwardRebate = -1.0f;
        private int _swapRadius = -1;
        
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
        
        public SkillType? CurrentSkillType { private set; get; } = null;
        private int _currentSkillID = -1;
        
        #region SkillTemporalFramework

        private void UpdateUICurrencyVal(GameAssets currentLevelAsset)
        {
            var message = new CurrencyUpdatedInfo()
            {
                CurrencyVal = Mathf.RoundToInt(currentLevelAsset.GameCurrencyMgr.Currency),
                TotalIncomesVal = -1,
            };
            MessageDispatcher.SendMessage(message);
        }

        private void ActiveSkill(GameAssets currentLevelAsset, int skillIndex)
        {
            var skill = InstancedSkillData[skillIndex];
            if (!skill.SkillEnabled) return;
            if (skill.CountLimit != -1 && skill.RemainingCount <= 0) return;

            var moneySpent = currentLevelAsset.GameCurrencyMgr.SpendSkillCurrency(skill.Cost);
            var skillActived = false;
            CurrentSkillType = null;
            _currentSkillID = -1;
            switch (skill.SklType)
            {
                case SkillType.TimeFromMoney:
                    if (moneySpent)
                    {
                        skillActived = true;
                        CurrentSkillType = SkillType.TimeFromMoney;
                        _currentSkillID = skillIndex;
                        WorldCycler.ExpectedStepDecrement(skill.TimeGain);
                        UpdateUICurrencyVal(currentLevelAsset); //因为这个时间点后就AutoDrive了，所以就没机会调UpdateBoard了，所以先在这里调一下。
                    }
                    break;
                case SkillType.FastForward:
                    skillActived = true;
                    _fastForwardRebate = 1.00f + 0.01f * skill.AdditionalIncome;
                    WorldCycler.ExpectedStepIncrement(skill.FastForwardCount);
                    CurrentSkillType = SkillType.FastForward;
                    _currentSkillID = skillIndex;
                    break;
                case SkillType.Swap:
                    if (moneySpent)
                    {
                        skillActived = true;
                        swapAlipay = skill.Cost;
                        CurrentSkillType = SkillType.Swap;
                        _currentSkillID = skillIndex;
                        _swapRadius = skill.radius;
                        unitAPosition = currentLevelAsset.Cursor.CurrentBoardPosition;
                        UpdateAIndicator(currentLevelAsset, unitAPosition);
                        UpdateUICurrencyVal(currentLevelAsset);
                    }
                    break;
                case SkillType.Discount:
                    if (moneySpent)
                    {
                        skillActived = true;
                        discount = skill.Discount;
                        skill.SkillCoolDown = true;
                        UpdateUICurrencyVal(currentLevelAsset);
                    }
                    break;
                case SkillType.RefreshHeatSink:
                    if (moneySpent)
                    {
                        currentLevelAsset.GameBoard.BoardGirdDriver.UpdatePatternID();
                        UpdateUICurrencyVal(currentLevelAsset);
                    }
                    break;
                case SkillType.ResetHeatSink:
                    if (moneySpent)
                    {
                        currentLevelAsset.GameBoard.BoardGirdDriver.ResetHeatSink();
                        UpdateUICurrencyVal(currentLevelAsset);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            
            if (skillActived)
            {
                if (skill.SklType != SkillType.Swap)
                {
                    //这里是可以记录一般技能的使用次数、但是Swap的逻辑要完全独立做。
                    skill.UsedCount++;
                }

                if (skill.CountLimit != -1)
                {
                    skill.RemainingCount--;
                    if (skill.RemainingCount <= 0)
                    {
                        skill.SkillEnabledInternal = false;
                    }
                }
            }
            UpdateSkillPalettes();
        }

        private void UpdateSkillActive(GameAssets currentLevelAsset)
        {
            //是在这儿，把Discount的enable数据清掉了。Discount的SkillCost还真是大于0.
            //蛋疼，那个实例化Skill里面再加一个coolDown
            InstancedSkillData.Where(skill => skill.Cost > 0).ForEach(skill => skill.SkillEnabledInternal = (skill.Cost <= currentLevelAsset.GameCurrencyMgr.Currency));
            UpdateSkillPalettes();
        }

        private bool _mouseWaitingUnitA = false;
        private bool _mouseWaitingUnitB = false;

        public void UpKeepSkill(GameAssets currentLevelAsset)
        {
            //这里为什么没有和Swap部分整合？因为这里的逻辑不会懂FSM运行状态、而Swap会。
            var autoDrive = WorldCycler.NeedAutoDriveStep;
            UpdateSkillActive(currentLevelAsset);
            if (!CurrentSkillType.HasValue) return;
            switch (CurrentSkillType.Value)
            {
                case SkillType.FastForward:
                    currentLevelAsset.CurrencyRebate = 1.00f;
                    if (!autoDrive.HasValue)
                    {
                        _fastForwardRebate = -1.00f;
                        CurrentSkillType = null;
                        UpdateUICurrencyVal(currentLevelAsset);
                    }

                    if (_fastForwardRebate > 0.0f)
                    {
                        currentLevelAsset.CurrencyRebate = _fastForwardRebate;
                    }

                    break;
                case SkillType.TimeFromMoney:
                    if (!autoDrive.HasValue)
                    {
                        CurrentSkillType = null;
                    }
                    break;
                case SkillType.RefreshHeatSink:
                case SkillType.Discount:
                case SkillType.Swap:
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

        private static void CleanIndicatorFrame(GameAssets currentLevelAsset)
        {
            if (currentLevelAsset.SkillIndGoB == null || currentLevelAsset.SkillIndGoB.Length <= 0) return;
            foreach (var go in currentLevelAsset.SkillIndGoB)
            {
                Destroy(go);
                currentLevelAsset.SkillIndGoB = null;
            }
        }

        private static void CleanIndicator(GameAssets currentLevelAsset)
        {
            if (currentLevelAsset.SkillIndGoA != null)
            {
                Destroy(currentLevelAsset.SkillIndGoA.gameObject);
                currentLevelAsset.SkillIndGoA = null;
            }

            CleanIndicatorFrame(currentLevelAsset);
        }

        private void UpdateAIndicator(GameAssets currentLevelAsset, Vector2Int Pos)
        {
            currentLevelAsset.SkillIndGoA = WorldExecutor.CreateIndicator(currentLevelAsset, Pos, ColorLibManager.Instance.ColorLib.ROOT_SKILL_SWAP_UNITA);
        }

        private void UpdateBIndicator(GameAssets currentLevelAsset,List<Vector2Int> incomings)
        {
            var count = incomings.Count;
            currentLevelAsset.SkillIndGoB = new GameObject[count];
            for (var i = 0; i < count; i++)
            {
                currentLevelAsset.SkillIndGoB[i] = WorldExecutor.CreateIndicator(currentLevelAsset, incomings[i], ColorLibManager.Instance.ColorLib.ROOT_SKILL_SWAP_UNITB);
            }
        }

        #endregion

        private Vector2Int unitAPosition = new Vector2Int(-1, -1);//这个的值赋进去了，只是要想着再画出来。
        private int swapAlipay = 0;
        private Vector2Int oldCurrentPos = new Vector2Int(-1, -1);

        private void SwapComplete(int skillID)
        {
            InstancedSkillData[skillID].UsedCount++;//记录Swap相关的次数只能在这里写。
        }
        
        public void SwapTick_FSM(GameAssets currentLevelAsset, ControllingPack ctrlPack)
        {
            Debug.Log("SwapTicking");
            Debug.Assert(_swapRadius != -1);

            //在绘制相关标记的时候、是在Cursor已经标记移动、但是是在动画执行完毕前绘制；所以正确的光标位置已经在Next部分了。
            var crtPos = currentLevelAsset.Cursor.NextBoardPosition;

            var res = Utils.PositionRandomization_NormalDistro(
                crtPos, _swapRadius, 0.65f, Board.BoardLength,
                out var selected);

            if (oldCurrentPos != crtPos)
            {
                //这个加个Anti-spam。
                CleanIndicatorFrame(currentLevelAsset);
                //这里根据res把所有的标记都画出来。
                UpdateBIndicator(currentLevelAsset, res);
                oldCurrentPos = crtPos;
            }

            //Confirm Or Cancel Gate
            if (!ctrlPack.HasFlag(ControllingCommand.SwapConfirm) && !ctrlPack.HasFlag(ControllingCommand.Cancel)) return;

            if (CurrentSkillType == SkillType.Swap)
            {
                var hasConfirm = ctrlPack.HasFlag(ControllingCommand.SwapConfirm);
                var hasCancel = ctrlPack.HasFlag(ControllingCommand.Cancel);

                if (hasConfirm && hasCancel) hasConfirm = false; //防止某些不是人的玩家真把确定和取消同时按下去了、把取消优先级提上去。

                Debug.Assert(hasConfirm ^ hasCancel);

                var swapSuccess = false;

                if (hasConfirm)
                {
                    //保证A单元不是静态的。
                    var aisAStationaryUnit = currentLevelAsset.GameBoard.CheckHasUnitAndStationary(unitAPosition);

                    //保证可能范围内不都是Stationary单元。
                    var bisAllStationary = false;
                    if (res.All(r => currentLevelAsset.GameBoard.CheckBoardPosValidAndFilled(r)))
                    {
                        bisAllStationary = res.Select(r => currentLevelAsset.GameBoard.FindUnitByPos(r)).All(u => u != null && u.Immovable);
                    }

                    if (!aisAStationaryUnit && !bisAllStationary)
                    {
                        bool notValidBPos;
                        Vector2Int unitBPosition;
                        do
                        {
                            //RISK 为了静态单元、这里数据重选的流程可以拆开的；现在重新调很费。
                            var resAlt = Utils.PositionRandomization_NormalDistro(crtPos, _swapRadius, 0.65f, Board.BoardLength, out var selectedAlt);
                            unitBPosition = resAlt[selectedAlt];
                            notValidBPos = currentLevelAsset.GameBoard.CheckHasUnitAndStationary(unitBPosition);
                        } while (notValidBPos);

                        swapSuccess = currentLevelAsset.GameBoard.SwapUnit(unitAPosition, unitBPosition);
                    }
                }

                if (hasCancel||(!swapSuccess))
                {
                    currentLevelAsset.GameCurrencyMgr.AddCurrency(swapAlipay);
                    swapAlipay = 0;
                    UpdateUICurrencyVal(currentLevelAsset);
                }
                else
                {
                    SwapComplete(_currentSkillID);
                    MessageDispatcher.SendMessage(WorldEvent.BoardShouldUpdateEvent);
                }

                if (!swapSuccess)
                {
                    Debug.LogWarning("swap failed!!");
                }
                
                CleanIndicator(currentLevelAsset);
                CurrentSkillType = null;
            }
        }

        #endregion

        public void SkillSystemSet(int skillID,bool setOrUnset)
        {
            try
            {
                InstancedSkillData[skillID].SkillEnabledSystem = setOrUnset;
            }
            catch (IndexOutOfRangeException)
            {
                Debug.LogError("skillID not present!!!");
                return;
            }
            UpdateSkillPalettes();
        }

        public int SkillUsedCountByID(int skillID)
        {
            try
            {
                return InstancedSkillData[skillID].UsedCount;
            }
            catch (IndexOutOfRangeException)
            {
                Debug.LogError("skillID not present!!!");
                return -1;
            }
        }
        
        private string MainColorHEX => "#" + ColorUtility.ToHtmlStringRGB(ColorLibManager.Instance.ColorLib.ROOT_SKILL_NAME_MAIN);
        private string SubColorHEX => "#" + ColorUtility.ToHtmlStringRGB(ColorLibManager.Instance.ColorLib.ROOT_SKILL_NAME_SUB);
        private string RemainColorHEX => "#" + ColorUtility.ToHtmlStringRGB(ColorLibManager.Instance.ColorLib.ROOT_SKILL_NAME_RMN);

        private string ColorTextPostFix => "</color>";

        private string ColorTextPrefix(string colorHex)
        {
            return "<color=" + colorHex + ">";
        }

        private string ColoredText(string content,string colorHex)
        {
            return ColorTextPrefix(colorHex) + content + ColorTextPostFix;
        }

        private void PopulateInstancedSkill()
        {
            InstancedSkillData = new List<InstancedSkillData>();
            foreach (var skillDataUnit in SkillData.SkillDataList)
            {
                InstancedSkillData.Add(new InstancedSkillData(skillDataUnit));
            }
        }

        private void InitSkillPalettes()
        {
            for (var i = 0; i < SkillPalettes.Count; i++)
            {
                SkillPalettes[i].SkillID = i;
                SkillPalettes[i].SkillKeyIconID = (i + 1) % 10;
                SkillPalettes[i].InitPaletteBySkillData(InstancedSkillData[i]);
            }
        }
        
        private void UpdateSkillPalettes()
        {
            for (var i = 0; i < SkillPalettes.Count; i++)
            {
                SkillPalettes[i].UpdatePaletteBySkillData(InstancedSkillData[i]);
            }
        }

        
        public void Awake()
        {
            PopulateInstancedSkill();
            InitSkillPalettes();
        }
    }
}