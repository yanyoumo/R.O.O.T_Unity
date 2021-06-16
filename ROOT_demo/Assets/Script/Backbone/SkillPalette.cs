using System;
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
                //TODO 具体的外观变化还要弄。
                SkillTag.text = value == SkillStatus.Normal ? SkillTag.text : "?????";
                LockedIcon.enabled = value != SkillStatus.Normal;
                SkillIcon.color = value == SkillStatus.Normal ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.65f);
            }
        }
    }

}
