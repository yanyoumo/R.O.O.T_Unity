using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

namespace ROOT
{
    public partial class HintMaster : MonoBehaviour
    {
        public TutorialCheckList TutorialCheckList;
    }

    //TODO 还没有处理触摸屏适配。
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

        public Localize passedLLE;
        public Localize failedLLE;

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

        private readonly float DistanceFromCamera = 20.0f;

        void Awake()
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, DistanceFromCamera));
            transform.localPosition = pos;
            MainEntry = Instantiate(EntryTemplate, MainEntryRoot).GetComponent<TutorialGoalEntry>();
            SecondaryEntry = Instantiate(EntryTemplate, SecondaryEntryRoot).GetComponent<TutorialGoalEntry>();
            MainEntry.Completed = false;
            SecondaryEntry.Completed = false;
            PressESCToReturn.gameObject.SetActive(false);
            if (StartGameMgr.UseTouchScreen)
            {
                passedLLE.Term = ScriptTerms.TouchToComplete;
                failedLLE.Term = ScriptTerms.TouchToReturn;
            }
            else
            {
                passedLLE.Term = ScriptTerms.KMEnterToComplete;
                failedLLE.Term = ScriptTerms.KMEnterToReturn;
            }
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