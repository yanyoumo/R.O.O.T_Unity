using System;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ROOT.UI
{
    public class UnitSelectionToggle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Toggle CoreToggle;
        public TextMeshProUGUI ToggleText;
        public Tooltip_UI TooltipUI;
        
        public String LabelTextTerm
        {
            set => ToggleText.text = LocalizationManager.GetTranslation(value) + "单元";
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("eventData.hovered.Count=" + eventData.hovered.Count);
            foreach (var o in eventData.hovered)
            {
                Debug.Log(o.name);
            }
            Debug.Log("OnPointerEnter");
            if (eventData.hovered.Contains(gameObject))
            {
                TooltipUI.ActiveTooltip();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("OnPointerExit");
            TooltipUI.DeactivateTooltip();
        }
    }
}