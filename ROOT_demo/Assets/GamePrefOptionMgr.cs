using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ROOT.UI
{
    public class GamePrefOptionMgr : MonoBehaviour
    {
        public Slider MouseDragSensitivitySlider;
        private int MouseDragSensitivity;
        private void Awake()
        {
            if (PlayerPrefs.HasKey("MouseDragSensitivity"))
            {
                MouseDragSensitivity = PlayerPrefs.GetInt("MouseDragSensitivity");
            }
            else
            {
                PlayerPrefs.SetInt("MouseDragSensitivity", 50);
            }

            MouseDragSensitivitySlider.value = MouseDragSensitivity;
        }

        public void MouseDragSensitivitySliderValChanged(Single val)
        {
            PlayerPrefs.SetInt("MouseDragSensitivity", (int) val);
        }
    }
}

