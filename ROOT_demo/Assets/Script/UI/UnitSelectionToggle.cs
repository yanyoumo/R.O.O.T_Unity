using System;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ROOT.UI
{
    public class UnitSelectionToggle : MonoBehaviour
    {
        public Toggle CoreToggle;
        public TextMeshProUGUI ToggleText;
        public String LabelTextTerm
        {
            set => ToggleText.text = LocalizationManager.GetTranslation(value) + "单元";
        }
    }
}