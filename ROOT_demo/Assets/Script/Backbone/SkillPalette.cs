using System;
using System.Collections;
using System.Collections.Generic;
using ROOT;
using TMPro;
using UnityEngine;

public class SkillPalette : MonoBehaviour
{
    public int SkillID;
    public SkillType SklType;
    public TextMeshPro SkillTag;
    public SpriteRenderer SkillIcon;

    public String SkillTagText
    {
        set => SkillTag.text = value;
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
