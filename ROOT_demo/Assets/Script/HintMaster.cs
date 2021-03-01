using System;
using System.Collections;
using System.Collections.Generic;
using ROOT.Message;
using UnityEngine;

namespace ROOT
{

    public enum HintEventType
    {
        ShowGoalCheckList,
        ShowTutorialTextFrame,
        ShowHelpScreen,
    }
    
    public class HintEventInfo : RootMessageBase
    {
        public HintEventType HintEventType;
        public bool BoolData;
        public String StringData;
        public override string Type => WorldEvent.HintRelatedEvent;
    }

    public partial class HintMaster : MonoBehaviour
    {
        /*public int ShopHintPostalPrice
        {
            set => shopPostalHint.PostalPrice = value;
        }

        public bool ShouldShowShopHint
        {
            set => shopPostalHint.gameObject.SetActive(value);
        }*/

        public bool ShouldShowCheckList
        {
            set => TutorialCheckList.gameObject.SetActive(value);
        }

        private bool _tutorialMainTextFrameSuppressed = false;

        public bool HideTutorialFrame
        {
            set => TutorialMainTextFrame.gameObject.SetActive(value);
        }

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
        }

        public void UpdateHintMaster(ControllingPack ctrlPack)
        {
            RequestedShowHelpScreen = ctrlPack.HasFlag(ControllingCommand.PlayHint);
        }
    }
}