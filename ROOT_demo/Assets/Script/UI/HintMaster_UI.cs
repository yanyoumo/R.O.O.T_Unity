using System;
using com.ootii.Messages;
using Doozy.Engine.UI;
using ROOT.Message;
using TMPro;
using UnityEngine;

namespace ROOT.UI
{
    public static class UIEvent
    {
        public delegate void InGameManualFootterUpdate(int TotalPageCount, int CurrentPageCount);

        public delegate void InGameOverlayToggle();
    }
    
    public enum HintEventType
    {
        //这里估计还是要有大改。
        ShowGoalCheckList,
        MainGoalComplete,
        SecondaryGoalComplete,
        SetMainGoalContent,
        SetSecondaryGoalContent,
        ShowTutorialTextFrame,
        TutorialFailed,
        ShowHelpScreen,
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

        public TextMeshProUGUI TutorialTextMainContent;
        public TutorialCheckList_Doozy TutorialCheckListCore;
        
        private void HintEventHandler(IMessage rMessge)
        {
            if (rMessge is HintEventInfo info)
            {
                //基于事件的包有一个问题、就是显示教程内容的时间是一个————ShowTutorialTextFrame；又是开关显示的内容和实际内容。
                //之前这个有bug的问题就是之前相关EventInfo的内容每天对。
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
                            if (info.StringData!="") TutorialTextMainContent.text = info.StringData;
                            break;
                        }
                        TutorialMainTextFrame.Hide();
                        break;
                    case HintEventType.SetMainGoalContent:
                        TutorialCheckListCore.SetupMainGoalContent(info.StringData);
                        return;
                    case HintEventType.SetSecondaryGoalContent:
                        TutorialCheckListCore.SetupSecondaryGoalContent(info.StringData);
                        return;
                    case HintEventType.MainGoalComplete:
                        TutorialCheckListCore.MainGoalCompleted = info.BoolData;
                        return;
                    case HintEventType.SecondaryGoalComplete:
                        TutorialCheckListCore.SecondaryGoalCompleted = info.BoolData;
                        return;
                    case HintEventType.TutorialFailed:
                        TutorialCheckListCore.TutorialFailed = info.BoolData;
                        return;
                    case HintEventType.ShowHelpScreen:
                        Debug.LogWarning("ShowHelpScreen Not yet implemented");
                        return;
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
        }

        private void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.HintRelatedEvent,HintEventHandler);
        }
    }
}