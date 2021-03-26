using System;
using System.Collections;
using System.Collections.Generic;
using ROOT.Message;
using UnityEngine;

namespace ROOT
{
    /*public partial class HintMaster : MonoBehaviour
    {
    }*/

    /*public partial class HintMaster : MonoBehaviour
    {
        public int ShopHintPostalPrice
        {
            set => shopPostalHint.PostalPrice = value;
        }

        public bool ShouldShowShopHint
        {
            set => shopPostalHint.gameObject.SetActive(value);
        }

        private bool nil = false;
        private string nils = "false";
        
        public bool ShouldShowCheckList
        {
            //set => TutorialCheckList.gameObject.SetActive(value);
            set => nil=value;
        }

        private bool _tutorialMainTextFrameSuppressed = false;

        public bool HideTutorialFrame
        {
            //set => TutorialMainTextFrame.gameObject.SetActive(value);
            set => nil=value;
        }

        public bool RequestedShowTutorialContent
        {
            //set => ShowTutorialContent = value;
            set => nil=value;
        }

        public bool ShowTutorialContent
        {
            //set => TutorialMainTextFrame.ShouldShow = value;
            set => nil=value;
        }

        public string TutorialContent
        {
            //set => TutorialMainTextFrame.ContentText.text = value;
            set => nils=value;
        }

        public bool RequestedShowHelpScreen
        {
            set => nil=value;
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
    }*/
}