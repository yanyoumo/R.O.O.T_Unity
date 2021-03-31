using System;
using com.ootii.Messages;
using DG.Tweening;
using Doozy.Engine.UI;
using ROOT.Message;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ROOT.UI
{
    public static class UIEvent
    {
        public delegate void InGameManualFootterUpdate(int TotalPageCount, int CurrentPageCount);

        public delegate void InGameOverlayToggle();
    }
    
    public enum HintEventType
    {
        SetGoalContent,
        SetGoalCheckListShow,
        SetTutorialTextContent,
        SetTutorialTextShow,
        GoalComplete,
        GoalFailed,
        SetHelpScreenShow,
        NextIsEnding,
        ToggleHandOnView,
    }
    
    public class HintEventInfo : RootMessageBase
    {
        public HintEventType HintEventType;
        public bool BoolData;
        public String StringData = "";
        public override string Type => WorldEvent.HintRelatedEvent;
    }
    
    public class HintMaster_UI : MonoBehaviour
    {
        public UIView TutorialCheckList;
        public UIView TutorialMainTextFrame;
        public UIView TutorialHandOff;
        
        public TextMeshProUGUI TutorialTextMainContent;
        public TutorialCheckList_Doozy TutorialCheckListCore;

        public TextMeshProUGUI TutorialNextContent;

        private void UIViewToggleWrapper(ref UIView view, bool toggle)
        {
            if (toggle)
            {
                view.Show();
            }
            else
            {
                view.Hide();
            }
        }
        
        private void HintEventHandler(IMessage rMessge)
        {
            if (rMessge is HintEventInfo info)
            {
                //基于事件的包有一个问题、就是显示教程内容的时间是一个————ShowTutorialTextFrame；又是开关显示的内容和实际内容。
                //之前这个有bug的问题就是之前相关EventInfo的内容每天对。
                switch (info.HintEventType)
                {
                    case HintEventType.SetGoalCheckListShow:
                        UIViewToggleWrapper(ref TutorialCheckList, info.BoolData);
                        break;
                    case HintEventType.SetTutorialTextShow:
                        UIViewToggleWrapper(ref TutorialMainTextFrame, info.BoolData);
                        break;
                    case HintEventType.SetTutorialTextContent:
                        if (info.StringData!="") TutorialTextMainContent.text = info.StringData;
                        break;
                    case HintEventType.SetGoalContent:
                        TutorialCheckListCore.SetupMainGoalContent(info.StringData);
                        return;
                    case HintEventType.GoalFailed:
                        TutorialCheckListCore.TutorialFailed = info.BoolData;
                        return;
                    case HintEventType.SetHelpScreenShow:
                        Debug.LogWarning("ShowHelpScreen Not yet implemented");
                        return;
                    case HintEventType.GoalComplete:
                        TutorialCheckListCore.MainGoalCompleted = info.BoolData;
                        break;
                    case HintEventType.NextIsEnding:
                        TutorialNextContent.text = "按[回车]以结束本关教程";
                        break;
                    case HintEventType.ToggleHandOnView:
                        UIViewToggleWrapper(ref TutorialHandOff, !info.BoolData);
                        break;
                    default:
                        throw new NotImplementedException();
                }
                return;
            }
            throw new ArgumentException("info type miss match");
        }

        private void Awake()
        {
            MessageDispatcher.AddListener(WorldEvent.HintRelatedEvent,HintEventHandler);
            TutorialCheckListCore.SetupSecondaryGoalContent("");
        }

        private void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.HintRelatedEvent,HintEventHandler);
        }
    }
}