using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public partial class HintMaster : MonoBehaviour
    {
        public TutorialCheckList TutorialCheckList;
    }

    public class TutorialCheckList : MonoBehaviour
    {
        //这个触摸判断不应该在这里判断。
        //public bool PlayerRequestedEnd { get; private set; } = false;
        public GameObject EntryTemplate;

        public Transform MainEntryRoot;
        public Transform SecondaryEntryRoot;
        public TutorialGoalEntry MainEntry;
        public TutorialGoalEntry SecondaryEntry;

        public Transform PressReturnToComplete;
        public Transform PressESCToReturn;

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
                PressReturnToComplete.gameObject.SetActive(MainEntry.Completed&& SecondaryEntry.Completed);
            }
            else
            {
                PressReturnToComplete.gameObject.SetActive(MainEntry.Completed);
            }
        }

        public bool MainGoalCompleted
        {
            set
            {
                MainEntry.Completed = value; 
                CheckCompleted();
            }
        }

        public bool SecondaryGoalCompleted
        {
            set
            {
                SecondaryEntry.Completed = value;
                CheckCompleted();
            }
        }

        void Awake()
        {
            MainEntry = Instantiate(EntryTemplate, MainEntryRoot).GetComponent<TutorialGoalEntry>();
            SecondaryEntry = Instantiate(EntryTemplate, SecondaryEntryRoot).GetComponent<TutorialGoalEntry>();
            MainEntry.Completed = false;
            SecondaryEntry.Completed = false;
            PressESCToReturn.gameObject.SetActive(false);
        }

        public void SetupEntryContent(string mainEntryContent,string secondaryEntryContent = "")
        {
            MainEntry.Content = mainEntryContent;
            if (secondaryEntryContent.Length>0)
            {
                _hasSecondaryEntry = true;
                SecondaryEntry.Content = secondaryEntryContent;
            }
            else
            {
                SecondaryEntry.gameObject.SetActive(false);
            }
        }
    }
}