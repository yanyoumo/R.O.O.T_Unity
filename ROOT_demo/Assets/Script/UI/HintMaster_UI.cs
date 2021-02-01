using System;
using Doozy.Engine.UI;
using UnityEngine;

namespace ROOT.UI
{
    public static class UIEvent
    {
        public delegate void InGameManualFootterUpdate(int TotalPageCount, int CurrentPageCount);
    }
    
    public class HintMaster_UI : MonoBehaviour
    {
        public UIView TestView;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                TestView.Toggle();
            }
        }
    }
}