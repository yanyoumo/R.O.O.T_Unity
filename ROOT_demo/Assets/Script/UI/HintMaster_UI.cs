using System;
using com.ootii.Messages;
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

        /*public bool ShouldShowCheckList
        {
            set => TutorialCheckList.Show();
        }

        private bool _tutorialMainTextFrameSuppressed = false;

        public bool ShowTutorialContent
        {
            set => TutorialMainTextFrame.Show();
        }*/

        private void HintEventHandler(IMessage rMessge)
        {
            if (rMessge is HintEventInfo info)
            {
                switch (info.HintEventType)
                {
                    case HintEventType.ShowGoalCheckList:
                        if (info.BoolData)
                        {
                            TutorialCheckList.Show();
                        }
                        else
                        {
                            TutorialCheckList.Hide();
                        }
                        break;
                    case HintEventType.ShowTutorialTextFrame:
                        if (info.BoolData)
                        {
                            TutorialMainTextFrame.Show();
                        }
                        else
                        {
                            TutorialMainTextFrame.Hide();
                        }
                        break;
                    case HintEventType.ShowHelpScreen:
                        break;
                    default:
                        return;
                }
            }

            throw new ArgumentException("info type miss match");
        }

        private void Awake()
        {
            MessageDispatcher.AddListener(WorldEvent.HintRelatedEvent,HintEventHandler);
        }

        private void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.HintRelatedEvent,HintEventHandler);
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