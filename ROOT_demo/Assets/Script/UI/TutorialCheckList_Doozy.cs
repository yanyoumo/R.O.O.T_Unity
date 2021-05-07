using System;
using com.ootii.Messages;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ROOT.UI
{
    public class TutorialCheckList_Doozy : MonoBehaviour
    {
        public Sprite CrossSprite;
        public Sprite TickSprite;

        public GameObject MainEntryPanel;
        public GameObject SecondaryEntryPanel;
        
        public Image MainEntryTickSprite;
        public Image SecondaryEntryTickSprite;

        public TextMeshProUGUI MainEntryContent;
        public TextMeshProUGUI SecondaryEntryContent;
        
        public GameObject PressReturnToComplete;
        public GameObject PressESCToReturn;
        
        private bool _hasSecondaryEntry = false;

        private bool _tutorialFailed;
        public bool TutorialFailed
        {
            set
            {
                _tutorialFailed = value;
                if (_tutorialFailed) PressESCToReturn.gameObject.SetActive(true);
            }
        }

        private void CheckCompleted() => PressReturnToComplete.gameObject.SetActive(_mainEntryCompleted && (!_hasSecondaryEntry || _secondaryEntryCompleted));

        private bool _mainEntryCompleted = false;
        private bool _secondaryEntryCompleted = false;

        private void UpdateTickSprite(bool TickOrCross, ref Image renderer)
        {
            renderer.color = TickOrCross ? Color.green : Color.red;
            renderer.sprite = TickOrCross ? TickSprite : CrossSprite;
        }
        
        public bool MainGoalCompleted
        {
            set
            {
                _mainEntryCompleted = value;
                UpdateTickSprite(_mainEntryCompleted, ref MainEntryTickSprite);
                CheckCompleted();
            }
        }

        public bool SecondaryGoalCompleted
        {
            set
            {
                _secondaryEntryCompleted = value;
                UpdateTickSprite(_secondaryEntryCompleted, ref SecondaryEntryTickSprite);
                CheckCompleted();
            }
        }

        private Vector3 OldMissionPos;

        private void TutorialMissionShouldAlertCompleted()
        {
            MainEntryPanel.transform.localPosition = OldMissionPos;
        }
        
        private void TutorialMissionShouldAlertHandler(IMessage rMessage)
        {
            MainEntryPanel.transform.DOShakePosition(3f, (Vector3.up + Vector3.left) * 7.5f).OnComplete(TutorialMissionShouldAlertCompleted);
        }
        
        void Awake()
        {
            PressESCToReturn.gameObject.SetActive(false);
            CheckCompleted();
            OldMissionPos = MainEntryPanel.transform.localPosition;
            MessageDispatcher.AddListener(WorldEvent.TutorialMissionShouldAlertEvent,TutorialMissionShouldAlertHandler);
        }

        private void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.TutorialMissionShouldAlertEvent,TutorialMissionShouldAlertHandler);
        }

        public void SetupMainGoalContent(string mainEntryContent)
        {
            MainEntryContent.text = mainEntryContent;
        }

        public void SetupSecondaryGoalContent(string secondaryEntryContent)
        {
            _hasSecondaryEntry = (secondaryEntryContent != "");
            SecondaryEntryPanel.gameObject.SetActive(_hasSecondaryEntry);
            SecondaryEntryContent.text = secondaryEntryContent;
        }
    }
}