using System;
using com.ootii.Messages;
using DG.Tweening;
using Doozy.Engine.UI;
using ROOT.Message;
using TMPro;
using UnityEngine;

namespace ROOT.UI
{
    [Obsolete]
    public static class UIEvent
    {
        public delegate void InGameManualFootterUpdate(int TotalPageCount, int CurrentPageCount);

        public delegate void InGameOverlayToggle();
    }

    public class HintMaster_UI : MonoBehaviour
    {
        public UIView TutorialCheckList;
        public UIView TutorialMainTextFrame;
        public UIView TutorialMainTextFrame_Alter;
        public UIView TutorialHandOff;
        
        public TextMeshProUGUI TutorialTextMainContent;
        public TextMeshProUGUI TutorialTextMainContent_Alter;
        public TutorialCheckList_Doozy TutorialCheckListCore;

        public TextMeshProUGUI TutorialNextContent;
        public TextMeshProUGUI TutorialNextContent_Alter;

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

        private bool _usingAlternateFrame;
        
        private bool _setUsingAlternateFrame
        {
            set
            {
                _usingAlternateFrame = value;
                //Debug.Log(" _usingAlternateFrame = value;");
                if (TutorialMainTextFrame.IsVisible || TutorialMainTextFrame_Alter.IsVisible)//这个判断不靠谱？？
                {
                    //Debug.Log("TutorialMainTextFrame.IsShowing || TutorialMainTextFrame_Alter.IsShowing");
                    TutorialMainTextFrame.Toggle();
                    TutorialMainTextFrame_Alter.Toggle();
                }
            }
        }
        
        Vector3 handsOffOldPos;
        Vector3 nextTextOldPos;
        Vector3 altNextTextOldPos;
        private void HandOnShakeComplete() => TutorialHandOff.transform.localPosition = handsOffOldPos;
        private void nextTextShakeComplete() => TutorialNextContent.transform.localPosition = nextTextOldPos;
        private void altNextTextShakeComplete() => TutorialNextContent_Alter.transform.localPosition = altNextTextOldPos;

        private void BlinkControllerBlockedAlert()
        {
            //从技术上讲、DOShakePosition是会回到原点、但是这里像我们这种情况：短时间+高幅度、似乎就会有不回原点的问题。先这么处理一下。
            TutorialHandOff.transform.DOShakePosition(0.3f, 12.5f, 15).OnComplete(HandOnShakeComplete);
            TutorialNextContent.transform.DOShakePosition(0.3f, Vector3.left * 17f, 15).OnComplete(nextTextShakeComplete);
            TutorialNextContent_Alter.transform.DOShakePosition(0.3f, Vector3.right * 17f, 15).OnComplete(altNextTextShakeComplete);
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
                        SetTutorialText(info);
                        break;
                    case HintEventType.SetTutorialTextContent:
                        if (info.StringData != "")
                        {
                            TutorialTextMainContent.text = info.StringData;
                            TutorialTextMainContent_Alter.text = info.StringData;
                        }
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
                        TutorialNextContent_Alter.text = "按[回车]以结束本关教程";
                        break;
                    case HintEventType.ToggleHandOnView:
                        UIViewToggleWrapper(ref TutorialHandOff, !info.BoolData);
                        break;
                    case HintEventType.ToggleAlternateTextPos:
                        _setUsingAlternateFrame = !_usingAlternateFrame;
                        break;
                    case HintEventType.ControllerBlockedAlert:
                        BlinkControllerBlockedAlert();
                        break;
                    default:
                        throw new NotImplementedException();
                }
                return;
            }
            throw new ArgumentException("info type miss match");
        }

        private void SetTutorialText(HintEventInfo info)
        {
            if (!_usingAlternateFrame || !info.BoolData)
            {
                UIViewToggleWrapper(ref TutorialMainTextFrame, info.BoolData);
            }

            if (_usingAlternateFrame || !info.BoolData)
            {
                UIViewToggleWrapper(ref TutorialMainTextFrame_Alter, info.BoolData);
            }
        }

        private void Awake()
        {
            MessageDispatcher.AddListener(WorldEvent.HintRelatedEvent, HintEventHandler);
            TutorialCheckListCore.SetupSecondaryGoalContent("");
            handsOffOldPos = TutorialHandOff.transform.localPosition;
            nextTextOldPos = TutorialNextContent.transform.localPosition;
            altNextTextOldPos = TutorialNextContent_Alter.transform.localPosition;
        }

        /*private void Update()
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                BlinkControllerBlockedAlert();
            }
        }*/

        private void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.HintRelatedEvent, HintEventHandler);
        }
    }
}