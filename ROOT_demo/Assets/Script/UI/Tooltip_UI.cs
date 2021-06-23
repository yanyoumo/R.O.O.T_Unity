using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ROOT.UI
{
    public class Tooltip_UI : MonoBehaviour
    {
        public TextMeshProUGUI TooltipTMP;
        
        public void ActiveTooltip(string textInfo)
        {
            TooltipTMP.text = textInfo;
            gameObject.SetActive(true);
        }
        
        public void DeactivateTooltip()
        {
            gameObject.SetActive(false);
        }
        
        private void Update()
        {
            transform.position = Input.mousePosition;
        }
    }
}