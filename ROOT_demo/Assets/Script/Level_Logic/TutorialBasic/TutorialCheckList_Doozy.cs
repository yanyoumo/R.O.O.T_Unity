using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ROOT
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
                if (_tutorialFailed)
                {
                    PressESCToReturn.gameObject.SetActive(true);
                }
            }
        }

        private void CheckCompleted()
        {
            if (_hasSecondaryEntry)
            {
                PressReturnToComplete.gameObject.SetActive(_mainEntryCompleted && _secondaryEntryEntryCompleted);
            }
            else
            {
                PressReturnToComplete.gameObject.SetActive(_mainEntryCompleted);
            }
        }

        private bool _mainEntryCompleted = false;
        private bool _secondaryEntryEntryCompleted = false;
        
        public bool MainGoalCompleted
        {
            set
            {
                _mainEntryCompleted = value; 
                if (_mainEntryCompleted)
                {
                    MainEntryTickSprite.color=Color.green;
                    MainEntryTickSprite.sprite = TickSprite;
                }
                else
                {
                    MainEntryTickSprite.color=Color.red;
                    MainEntryTickSprite.sprite = CrossSprite;
                }
                CheckCompleted();
            }
        }

        public bool SecondaryGoalCompleted
        {
            set
            {
                _secondaryEntryEntryCompleted = value;
                if (_secondaryEntryEntryCompleted)
                {
                    MainEntryTickSprite.color=Color.green;
                    MainEntryTickSprite.sprite = TickSprite;
                }
                else
                {
                    MainEntryTickSprite.color=Color.red;
                    MainEntryTickSprite.sprite = CrossSprite;
                }
                CheckCompleted();
            }
        }

        private readonly float DistanceFromCamera = 20.0f;

        void UpdatePosition()
        {
            CameraAdaptToScreen.CameraUpdated -= UpdatePosition;
            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, DistanceFromCamera));
            transform.localPosition = pos;
        }

        void Awake()
        {
            PressESCToReturn.gameObject.SetActive(false);
            CheckCompleted();
        }

        public void SetupMainGoalContent(string mainEntryContent)
        {
            MainEntryContent.text = mainEntryContent;
        }

        public void SetupSecondaryGoalContent(string secondaryEntryContent)
        {
            if (secondaryEntryContent != "")
            {
                _hasSecondaryEntry = true;
                SecondaryEntryContent.text = secondaryEntryContent;
            }
            else
            {
                _hasSecondaryEntry = false;
                SecondaryEntryPanel.gameObject.SetActive(false);
            }
        }
    }
}