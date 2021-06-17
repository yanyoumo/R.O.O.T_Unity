using System;
using ROOT.SetupAsset;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace ROOT
{
    [Flags]
    public enum SkillStatus
    {
        Normal = 0,
        MoneyLock = 1 << 1,
        NoCountLock = 1 << 2,
        CoolDownLock = 1 << 3,
        SystemLock = 1 << 4,
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
        
        private String SkillTagText
        {
            set => SkillTag.text = cachedSkillTagText = value;
        }

        private Sprite SkillIconSprite
        {
            set => SkillIcon.sprite = value;
        }

        private int MaxSkillCount
        {
            set => CounterLEDArray.InitSkillCountArray(value);
        }

        private int CurrentSkillCount
        {
            set => CounterLEDArray.Val = value;
        }
        
        private SkillStatus SkillStatus
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
                        SkillIcon.color = new Color(0.5f, 0.5f, 0.5f, 0.65f);
                        SkillIcon.material = DefaultMat;
                        LockedIcon.enabled = true;
                        break;
                    case SkillStatus.MoneyLock:
                        SkillTag.color = ColorLibManager.Instance.ColorLib.MASTER_RED;
                        SkillIcon.material = BWMat;
                        SkillIcon.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                        break;
                    case SkillStatus.CoolDownLock:
                        SkillTag.text = "Activated";
                        SkillTag.color = ColorLibManager.Instance.ColorLib.MASTER_RED;
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
        
        private int SkillKeyIconID
        {
            set => SkillKeyText.text = "<sprite=" + value + ">";
        }
        
        private string SkillTagText_Content(InstancedSkillData skill)
        {
            if (skill.Cost <= 0)
            {
                return "FREE";
            }
            return skill.Cost.ToString("D");
        }

        
        public void InitPaletteBySkillData(InstancedSkillData skillData,int _skillKeyIconID)
        {
            SklType = skillData.SklType;
            SkillTagText = SkillTagText_Content(skillData);
            SkillIconSprite = skillData.SkillIcon;
            SkillKeyIconID = _skillKeyIconID;
            
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
            SkillTagText = SkillTagText_Content(skillData);

            if (CounterLEDArray.gameObject.activeSelf)
            {
                CurrentSkillCount = skillData.RemainingCount;
            }

            var combinedStatus = SkillStatus.Normal;
            
            if (!skillData.SkillEnabledSystem)
            {
                combinedStatus |= SkillStatus.SystemLock;
            }
            
            if (skillData.RemainingCount == 0)
            {
                combinedStatus |= SkillStatus.NoCountLock;
            }
            
            if (!skillData.SkillEnabledInternal)
            {
                combinedStatus |= SkillStatus.MoneyLock;
            }
            
            if (skillData.SkillCoolDown)
            {
                combinedStatus |= SkillStatus.CoolDownLock;
            }

            SkillStatus = combinedStatus.MaxPriorityFlag();
        }
    }

}
