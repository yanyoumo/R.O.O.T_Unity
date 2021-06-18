using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT.UI
{
    public class Tooltip_UI : MonoBehaviour
    {
        public void ActiveTooltip()
        {
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