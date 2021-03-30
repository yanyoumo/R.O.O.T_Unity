using System;
using System.Collections;
using System.Collections.Generic;
using Doozy.Engine.UI;
using ROOT.Consts;
using UnityEngine;
using UnityEngine.UI;

namespace ROOT.UI
{
    public class GamePrefOptionMgr : MonoBehaviour
    {
        public UIView OptionView;
        public Slider MouseDragSensitivitySlider;
        private int MouseDragSensitivity;
        private void Awake()
        {
            MouseDragSensitivity = PlayerPrefs.GetInt(StaticPlayerPrefName.MOUSE_DRAG_SENSITIVITY);
            MouseDragSensitivitySlider.value = MouseDragSensitivity;
        }

        public void MouseDragSensitivitySliderValChanged(Single val)
        {
            PlayerPrefs.SetInt(StaticPlayerPrefName.MOUSE_DRAG_SENSITIVITY, (int) val);
        }

        public void OptionMgrClosed()
        {
            PlayerPrefs.Save();
            OptionView.Hide();
        }
    }
}

