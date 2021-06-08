using System;
using com.ootii.Messages;
using DG.Tweening;
using Doozy.Engine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ROOT.UI
{
    public class ScanPopUp_UI : MonoBehaviour
    {
        public UIView PopUpUI;
        public UIToggle ScanToggle;
        public TextMeshProUGUI PleaseReadText;
        public TextMeshProUGUI OkText;
        public Button ProceeButton;
        private const float totalTime = 5.0f;
        private const string DOTweenTimerID = "ScanPopUp_UI_TimerDOTween";

        private void SetCounterAndText(float x)
        {
            PleaseReadText.text = "请阅读-" + Mathf.RoundToInt(x);
        }
        
        private void AllOkButton()
        {
            PleaseReadText.enabled = false;
            OkText.enabled = true;
            ProceeButton.interactable = true;
        }

        public void ScanToggleUnselect()
        {
            PlayerPrefs.SetInt(Consts.StaticPlayerPrefName.UNLOCK_SCAN, 0);
            PlayerPrefs.Save();
            MessageDispatcher.SendMessage(WorldEvent.ScanUnitLockChangedEvent);
        }
        
        public void PopUIOKButtonPressed()
        {
            PlayerPrefs.SetInt(Consts.StaticPlayerPrefName.UNLOCK_SCAN, 1);
            PlayerPrefs.Save();
            MessageDispatcher.SendMessage(WorldEvent.ScanUnitLockChangedEvent);
            PopUpUI.Hide();
        }

        public void PopUINotButtonPressed()
        {
            ScanToggle.IsOn = false;
            PopUpUI.Hide();
        }
        
        private void OnEnable()
        {
            DOTween.To(() => totalTime, SetCounterAndText, 0.0f, totalTime)
                .SetId(DOTweenTimerID)
                .SetEase(Ease.Linear).onComplete = AllOkButton;
        }
        
        private void OnDisable()
        {
            PleaseReadText.enabled = true;
            OkText.enabled = false;
            ProceeButton.interactable = false;
            DOTween.Kill(DOTweenTimerID);
        }
    }
}