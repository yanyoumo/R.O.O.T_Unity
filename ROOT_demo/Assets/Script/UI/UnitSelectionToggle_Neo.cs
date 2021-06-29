using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ROOT.UI
{
    public class UnitSelectionToggle_Neo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Image SignalIcon;
        public Toggle CoreToggle;
        public TextMeshProUGUI ToggleText;
        [ReadOnly]public String SignalInfo;
        [ReadOnly]public Tooltip_UI TooltipUI;
        
        public String LabelTextTerm
        {
            set => ToggleText.text = LocalizationManager.GetTranslation(value) + "单元";
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            //Debug.Log("OnPointerEnter");
            TooltipUI.ActiveTooltip(SignalInfo);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipUI.DeactivateTooltip();
            //Debug.Log("OnPointerExit");
        }
    }
}