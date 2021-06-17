using System;
using ROOT.SetupAsset;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace ROOT
{
    public enum SkillStatus
    {
        //这个玩意儿意外地可能会冲突。特么可能得用一个flags
        Normal,
        SystemLock,
        MoneyLock,
        CoolDownLock,
        NoCountLock,
    }
    
    public class SkillPalette : MonoBehaviour
    {
        [ReadOnly]public int SkillID;
        [ReadOnly]public SkillType SklType;
        public TextMeshPro SkillTag;
        public TextMeshPro SkillKeyText;
        public SpriteRenderer SkillIcon;
        public SpriteRenderer LockedIcon;

        public Material DefaultMat;
        public Material BWMat;

        public SkillCountLEDArray CounterLEDArray;
        
        private string cachedSkillTagText;

        private string SkillTagText_Content(InstancedSkillData skill)
        {
            if (skill.Cost <= 0)
            {
                return "FREE";
            }
            return skill.Cost.ToString("D");
        }
        public void InitPaletteBySkillData(InstancedSkillData skillData)
        {
            SklType = skillData.SklType;
            SkillTagText = SkillTagText_Content(skillData);
            SkillIconSprite = skillData.SkillIcon;
            
            if (skillData.RemainingCount!=-1)
            {
                MaxSkillCount = skillData.RemainingCount;
            }
            else
            {
                CounterLEDArray.gameObject.SetActive(false);
            }

            UpdatePaletteBySkillData(skillData);
        }

        public void UpdatePaletteBySkillData(InstancedSkillData skillData)
        {
            if (CounterLEDArray.gameObject.activeSelf)
            {
                CurrentSkillCount = skillData.RemainingCount;
            }

            if (!skillData.SkillEnabledSystem)
            {
                SkillStatus = SkillStatus.SystemLock;
            }
            else if (skillData.RemainingCount == 0)
            {
                SkillStatus = SkillStatus.NoCountLock;
            }
            else if (!skillData.SkillEnabledInternal)
            {
                SkillStatus = SkillStatus.MoneyLock;
            }
            else if (skillData.SkillCoolDown)
            {
                SkillStatus = SkillStatus.CoolDownLock;
            }
            else
            {
                SkillStatus = SkillStatus.Normal;
            }
        }

        public String SkillTagText
        {
            set => SkillTag.text = cachedSkillTagText = value;
        }

        public int SkillKeyIconID
        {
            set => SkillKeyText.text = "<sprite=" + value + ">";
        }
        
        public Sprite SkillIconSprite
        {
            set => SkillIcon.sprite = value;
        }

        public int MaxSkillCount
        {
            set => CounterLEDArray.InitSkillCountArray(value);
        }

        public int CurrentSkillCount
        {
            set => CounterLEDArray.Val = value;
        }
        
        public SkillStatus SkillStatus
        {
            set
            {
                //DefaultNormalStatus
                LockedIcon.enabled = false;
                SkillTag.color = ColorLibManager.Instance.ColorLib.ROOT_SKILL_NAME_MAIN;
                SkillIcon.color = Color.white;
                SkillIcon.material = DefaultMat;
                switch (value)
                {
                    case SkillStatus.Normal:
                        //DO Nothing.
                        break;
                    case SkillStatus.SystemLock:
                        SkillTag.text = "?????";
                        LockedIcon.enabled = true;
                        SkillIcon.color = new Color(0.5f, 0.5f, 0.5f, 0.65f);
                        SkillIcon.material = DefaultMat;
                        break;
                    case SkillStatus.MoneyLock:
                        SkillTag.color = ColorLibManager.Instance.ColorLib.MASTER_RED;
                        SkillIcon.material = BWMat;
                        SkillIcon.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                        break;
                    case SkillStatus.CoolDownLock:
                        SkillTag.text = "Activated";
                        SkillIcon.material = BWMat;
                        SkillIcon.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                        break;
                    case SkillStatus.NoCountLock:
                        SkillTag.text = "Run out";
                        SkillTag.color = ColorLibManager.Instance.ColorLib.MASTER_RED;
                        SkillIcon.material = BWMat;
                        SkillIcon.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
            }
        }
    }

}
