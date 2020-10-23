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

    public class SkillMgr : MonoBehaviour
    {
        public Transform IconFramework;
        public SkillData SkillData;

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

        //就是整个技能框架还是要弄一套配置框架………………🤣
        private void ActiveSkill(GameAssets currentLevelAsset, SkillBase skill)
        {
            switch (skill.SklType)
            {
                case SkillType.TimeFromMoney:
                    currentLevelAsset.GameStateMgr.SpendShopCurrency(skill.Cost);
                    WorldCycler.ExpectedStepDecrement(skill.TimeGain);
                    break;
                case SkillType.FastForward:
                    //折扣的问题还是没弄。
                    WorldCycler.ExpectedStepIncrement(skill.FastForwardCount);
                    break;
                case SkillType.Swap:
                    break;
                case SkillType.RefreshHeatSink:
                    break;
                case SkillType.Discount:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void UpdateSkill(GameAssets currentLevelAsset,in ControllingPack ctrlPack)
        {
            var AutoDrive = WorldCycler.NeedAutoDriveStep;

            if (!AutoDrive.HasValue)
            {
                if (!_skillEnabled || !ctrlPack.HasFlag(ControllingCommand.Skill)) return;

                ActiveSkill(currentLevelAsset, SkillData.SkillDataList[ctrlPack.SkillID]);
            }
            else
            {
                
            }
        }
    }
}