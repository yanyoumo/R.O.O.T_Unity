using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace ROOT
{
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

        public bool SkillEnabled
        {
            set
            {
                SkillTag.text = value ? SkillTag.text : "?????";
                LockedIcon.enabled = !value;
                SkillIcon.color = value ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.65f);
            }
        }
    }

}
