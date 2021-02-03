using System;
using Doozy.Engine.UI;
using UnityEngine;

namespace ROOT.UI
{
    public static class UIEvent
    {
        public delegate void InGameManualFootterUpdate(int TotalPageCount, int CurrentPageCount);

        public delegate void InGameOverlayToggle();
    }
    
    public class HintMaster_UI : MonoBehaviour
    {
        public UIView TutorialCheckList;
        public UIView TutorialMainTextFrame;

        public bool ShouldShowCheckList
        {
            set => TutorialCheckList.Show();
        }

        private bool _tutorialMainTextFrameSuppressed = false;

        public bool HideTutorialFrame
        {
            set => TutorialMainTextFrame.Hide();
        }

        public bool RequestedShowTutorialContent
        {
            set => ShowTutorialContent = value;
        }

        public bool ShowTutorialContent
        {
            set => TutorialMainTextFrame.Show();
        }

        /*public string TutorialContent
        {
            set => TutorialMainTextFrame.ContentText.text = value;
        }*/

        /*public bool RequestedShowHelpScreen
        {
            set
            {
                HelpScreen.ShouldShow = value;
                if (value)
                {
                    if (TutorialMainTextFrame._showed)
                    {
                        TutorialMainTextFrame.ShouldShow = false;
                        _tutorialMainTextFrameSuppressed = true;
                    }
                }
                else
                {
                    if (_tutorialMainTextFrameSuppressed)
                    {
                        TutorialMainTextFrame.ShouldShow = true;
                        _tutorialMainTextFrameSuppressed = false;
                    }
                }
            }
        }*/
    }
}