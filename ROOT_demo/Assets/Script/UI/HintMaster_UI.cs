using System;
using System.Collections.Generic;
using System.Linq;
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
        public UIView TutorialCheckList_Alter;
        public UIView TutorialMainTextFrame;
        public UIView TutorialMainTextFrame_Alter;
        public UIView TutorialHandOff;

        private Dictionary<UIView, bool> tmpHideStatusBuffer;
        
        public TextMeshProUGUI TutorialTextMainContent;
        public TextMeshProUGUI TutorialTextMainContent_Alter;
        public TutorialCheckList_Doozy TutorialCheckListCore;
        public TutorialCheckList_Doozy TutorialCheckListCore_Alt;

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

        private bool _usingAlternateMainText;
        private void setUsingAlternateMainText(bool toggle)
        {
            _usingAlternateMainText = toggle;
            if (TutorialMainTextFrame.IsVisible || TutorialMainTextFrame_Alter.IsVisible)
            {
                TutorialMainTextFrame.Toggle();
                TutorialMainTextFrame_Alter.Toggle();
            }
        }

        private bool _usingAlternateGoalPanel;
        private void setAlternateGoalPanel(bool toggle)
        {
            Debug.Log("setAlternateGoalPanel");
            _usingAlternateGoalPanel = toggle;
            if (TutorialCheckList.IsVisible || TutorialCheckList_Alter.IsVisible)
            {
                TutorialCheckList.Toggle();
                TutorialCheckList_Alter.Toggle();
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
                        SetTutorialCheckList(info);
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
                        TutorialCheckListCore_Alt.SetupMainGoalContent(info.StringData);
                        return;
                    case HintEventType.GoalFailed:
                        TutorialCheckListCore.TutorialFailed = info.BoolData;
                        TutorialCheckListCore_Alt.TutorialFailed = info.BoolData;
                        return;
                    case HintEventType.SetHelpScreenShow:
                        Debug.LogWarning("ShowHelpScreen Not yet implemented");
                        return;
                    case HintEventType.GoalComplete:
                        TutorialCheckListCore.MainGoalCompleted = info.BoolData;
                        TutorialCheckListCore_Alt.MainGoalCompleted = info.BoolData;
                        break;
                    case HintEventType.NextIsEnding:
                        TutorialNextContent.text = "按<sprite=\"FunctionKeyPacked_Lite\" index=2>以结束本关教程";
                        TutorialNextContent_Alter.text = "按<sprite=\"FunctionKeyPacked_Lite\" index=2>以结束本关教程";
                        break;
                    case HintEventType.ToggleHandOnView:
                        UIViewToggleWrapper(ref TutorialHandOff, !info.BoolData);
                        break;
                    case HintEventType.ToggleAlternateTextPos:
                        setUsingAlternateMainText(!_usingAlternateMainText);
                        break;
                    case HintEventType.ToggleAlternateCheckGoal:
                        setAlternateGoalPanel(!_usingAlternateGoalPanel);
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

        private void SetTutorialCheckList(HintEventInfo info)
        {
            if (!_usingAlternateGoalPanel || !info.BoolData)
            {
                UIViewToggleWrapper(ref TutorialCheckList, info.BoolData);
            }

            if (_usingAlternateGoalPanel || !info.BoolData)
            {
                UIViewToggleWrapper(ref TutorialCheckList_Alter, info.BoolData);
            }
        }
        
        private void SetTutorialText(HintEventInfo info)
        {
            if (!_usingAlternateMainText || !info.BoolData)
            {
                UIViewToggleWrapper(ref TutorialMainTextFrame, info.BoolData);
            }

            if (_usingAlternateMainText || !info.BoolData)
            {
                UIViewToggleWrapper(ref TutorialMainTextFrame_Alter, info.BoolData);
            }
        }

        private void ToggleHintUIUpEventHandler(IMessage rMessge)
        {
            foreach (var key in tmpHideStatusBuffer.Keys.ToArray())
            {
                tmpHideStatusBuffer[key] = key.IsVisible;
            }

            foreach (var uiView in tmpHideStatusBuffer.Keys)
            {
                uiView.Hide();
            }
        }

        private void ToggleHintUIDownEventHandler(IMessage rMessge)
        {
            foreach (var keyValuePair in tmpHideStatusBuffer)
            {
                if (keyValuePair.Value)
                {
                    keyValuePair.Key.Show();
                }
                else
                {
                    keyValuePair.Key.Hide();
                }
            }
        }
        
        private void Awake()
        {
            TutorialCheckListCore.SetupSecondaryGoalContent("");
            TutorialCheckListCore_Alt.SetupSecondaryGoalContent("");
            handsOffOldPos = TutorialHandOff.transform.localPosition;
            nextTextOldPos = TutorialNextContent.transform.localPosition;
            altNextTextOldPos = TutorialNextContent_Alter.transform.localPosition;
            tmpHideStatusBuffer = new Dictionary<UIView, bool>
            {
                {TutorialCheckList, false},
                {TutorialCheckList_Alter, false},
                {TutorialMainTextFrame, false},
                {TutorialMainTextFrame_Alter, false},
                {TutorialHandOff, false},
            };
            MessageDispatcher.AddListener(WorldEvent.HintRelatedEvent, HintEventHandler);
            MessageDispatcher.AddListener(WorldEvent.ToggleHintUIUpEvent, ToggleHintUIUpEventHandler);
            MessageDispatcher.AddListener(WorldEvent.ToggleHintUIDownEvent, ToggleHintUIDownEventHandler);
        }

        private void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.ToggleHintUIDownEvent,ToggleHintUIDownEventHandler);
            MessageDispatcher.RemoveListener(WorldEvent.ToggleHintUIUpEvent,ToggleHintUIUpEventHandler);
            MessageDispatcher.RemoveListener(WorldEvent.HintRelatedEvent, HintEventHandler);
        }
    }
}