using System;
using TMPro;
using UnityEngine;

namespace ROOT
{
    public class SkillPalette : MonoBehaviour
    {
        public int SkillID;
        public SkillType SklType;
        public TextMeshPro SkillTag;
        public TextMeshPro SkillKeyText;
        public SpriteRenderer SkillIcon;

        public String SkillTagText
        {
            set => SkillTag.text = value;
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
            set => SkillIcon.color = value ? Color.white : Color.grey;
        }
    }

}
