using com.ootii.Messages;
using DG.Tweening;
using Doozy.Engine.UI;
using ROOT.Consts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ROOT.UI
{
    public class ScanPopUp_UI : MonoBehaviour
    {
        public UIView PopUpUI;
        public Toggle ScanToggle;
        public TextMeshProUGUI PleaseReadText;
        public TextMeshProUGUI OkText;
        public Button ProceeButton;
        private const float totalTime = 5.0f;
        private const string DOTweenTimerID = "ScanPopUp_UI_TimerDOTween";

        private bool PlayerScanUnlocked => (PlayerPrefs.GetInt(StaticPlayerPrefName.SCAN_UNLOCKED, 0) == 1);

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

        public void ToggleChangedHandler(bool OnOrOff)
        {
            if (ScanToggle.isOn)
            {
                if (!PlayerScanUnlocked)
                {
                    PopUpUI.Show();
                }
            }
            else
            {
                if (PlayerScanUnlocked)
                {
                    ScanToggleUnselect();
                }
            }
        }

        private void ScanToggleUnselect()
        {
            PlayerPrefs.SetInt(StaticPlayerPrefName.SCAN_UNLOCKED, 0);
            PlayerPrefs.Save();
            MessageDispatcher.SendMessage(WorldEvent.ScanUnitLockChangedEvent);
        }
        
        public void PopUIOKButtonPressed()
        {
            PlayerPrefs.SetInt(StaticPlayerPrefName.SCAN_UNLOCKED, 1);
            PlayerPrefs.Save();
            MessageDispatcher.SendMessage(WorldEvent.ScanUnitLockChangedEvent);
            PopUpUI.Hide();
        }

        public void PopUINotButtonPressed()
        {
            ScanToggle.isOn = false;
            PopUpUI.Hide();
        }

        private void Awake()
        {
            ScanToggle.isOn = PlayerScanUnlocked;
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