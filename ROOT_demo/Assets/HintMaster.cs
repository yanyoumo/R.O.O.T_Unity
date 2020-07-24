using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public partial class HintMaster : MonoBehaviour
    {
        private bool TutorialMainTextFrameSuppressed = false;

        public bool RequestedShowTutorialContent
        {
            set => ShowTutorialContent = value;
        }

        public bool ShowTutorialContent
        {
            set => TutorialMainTextFrame.ShouldShow = value;
        }

        public string TutorialContent
        {
            set => TutorialMainTextFrame.ContentText.text = value;
        }

        public bool RequestedShowHelpScreen
        {
            set
            {
                HelpScreen.ShouldShow = value;
                if (value)
                {
                    if (TutorialMainTextFrame._showed)
                    {
                        TutorialMainTextFrame.ShouldShow = false;
                        TutorialMainTextFrameSuppressed = true;
                    }
                }
                else
                {
                    if (TutorialMainTextFrameSuppressed)
                    {
                        TutorialMainTextFrame.ShouldShow = true;
                        TutorialMainTextFrameSuppressed = false;
                    }
                }
            }
        }

        void Update()
        {
            RequestedShowHelpScreen = Input.GetButton(StaticName.INPUT_BUTTON_NAME_HINTCTRL);
        }
    }
}