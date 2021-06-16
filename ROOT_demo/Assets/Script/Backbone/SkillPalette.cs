using System;
using ROOT.SetupAsset;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace ROOT
{
    public enum SkillStatus
    {
        Normal,
        SystemLock,
        MoneyLock,
        CoolDownLock,
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
        
        private string cachedSkillTagText;
        
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
                        throw new NotImplementedException();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
            }
        }
    }

}
