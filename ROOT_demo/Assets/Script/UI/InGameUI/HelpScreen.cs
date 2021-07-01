using System;
using System.Linq;
using com.ootii.Messages;
using DG.Tweening;
using I2.Loc;
using ROOT.Message;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using static I2.Loc.ScriptTerms;

namespace ROOT
{
    public class HelpScreen : MonoBehaviour
    {
        public TextMeshPro PressHForHintText;
        public Transform GameplayPageRoot;
        public Transform TutorialPageRoot;
        public HelpScreen_SignalSub_UI SignalSub;
        
        private int _gameplayPageCount => _gameplayPages.Length;
        private int _tutorialPageCount => _tutorialPages.Length;
        
        public Localize HorHintText;
        private bool shouldShow = false;//这个其实就是简单的队列了。
        private bool animating = false;
        private bool atUpOrDown = false;
        private const float _upPos = -1.64f;
        private const float _downPos = -10.49f;
        private const float _slideDuration = 0.3f;

        private void toggleSinglePage(Transform[] targetingPages,bool toggle,int pageNum)
        {
            if (pageNum < 0 || pageNum >= targetingPages.Length)
            {
                Debug.LogWarning("Requesting page:" + pageNum + " for is not valid, request ignored!");
                return;
            }
            targetingPages[pageNum].gameObject.SetActive(toggle);
        }

        void HintPageChangedEventHandler(IMessage rMessage)
        {
            //RISK 现在是好使的，但是toggleOff部分的逻辑不完整，但是现在符合需求了。先不动了。
            if (rMessage is HintPageChangedData data)
            {

                if (data.changeSignalHint)
                {
                    SignalSub.SetupSignalHints(data.UpperSignal, data.LowerSignal, data.TelemetryOrNot);
                }
                
                var targetingPages = data.TutorialOrGameplay ? _tutorialPages : _gameplayPages;

                if (data.PageNum >= 0)
                {
                    if (data.AdditiveOrReplace)
                    {
                        toggleSinglePage(targetingPages, data.Toggle, data.PageNum);
                    }
                    else
                    {
                        for (var i = 0; i < targetingPages.Length; i++)
                        {
                            toggleSinglePage(targetingPages, data.Toggle && (i == data.PageNum), i);
                        }
                    }
                }
                else
                {
                    if (data.PageNums == null || !data.PageNums.Any())
                    {
                        Debug.LogWarning("Not correct page data in here");
                        return;
                    }
                    if (data.AdditiveOrReplace)
                    {
                        foreach (var dataPageNum in data.PageNums)
                        {
                            toggleSinglePage(targetingPages, data.Toggle, dataPageNum);
                        }
                    }
                    else
                    {
                        for (var i = 0; i < targetingPages.Length; i++)
                        {
                            toggleSinglePage(targetingPages, data.Toggle && data.PageNums.Contains(i), i);
                        }
                    }
                }
            }
        }

        //这两个不能变为属性、因为如果实时拿的话、Inactive的Page会拿不到、就只能在一开始全齐的时候拿一下然后存起来。
        private Transform[] _gameplayPages;
        private Transform[] _tutorialPages;

        private Vector3 OldPressHForHintTextPos;

        private void HelpScreenAlertComplete()
        {
            PressHForHintText.transform.localPosition = OldPressHForHintTextPos;
        }

        private void HelpScreenShouldAlertHandler(IMessage rMessage)
        {
            PressHForHintText.transform.DOShakePosition(3f, (Vector3.up + Vector3.left) * 1.5f).OnComplete(HelpScreenAlertComplete);
        }

        void ToggleHintUIUpEventHandler(IMessage rMessage)
        {
            shouldShow = true;
        }
        
        void ToggleHintUIDownEventHandler(IMessage rMessage)
        {
            shouldShow = false;
        }
        
        private void ToggleUpOrDownByDOTween(bool UpOrDown)
        {
            MessageDispatcher.SendMessage(UpOrDown ? WorldEvent.UIShouldMakeWay : WorldEvent.UICouldResume);
            transform.DOMoveZ(UpOrDown ? _upPos : _downPos, _slideDuration).onComplete = () =>
            {
                atUpOrDown = UpOrDown;
                animating = false;
            };
            HorHintText.Term = UpOrDown ? ReleaseToReturn_KM : HForHint_KM;
            animating = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (shouldShow != atUpOrDown && !animating)
            {
                ToggleUpOrDownByDOTween(shouldShow);
            }
        }
        
        void Awake()
        {
            HorHintText.Term = HForHint_KM;

            _gameplayPages = GameplayPageRoot.GetComponentsInChildren<Transform>().Where(t => t.parent == GameplayPageRoot).ToArray();
            _tutorialPages = TutorialPageRoot.GetComponentsInChildren<Transform>().Where(t => t.parent == TutorialPageRoot).ToArray();

            OldPressHForHintTextPos = PressHForHintText.transform.localPosition;
            
            MessageDispatcher.AddListener(WorldEvent.ToggleHintUIUpEvent, ToggleHintUIUpEventHandler);
            MessageDispatcher.AddListener(WorldEvent.ToggleHintUIDownEvent, ToggleHintUIDownEventHandler);
            MessageDispatcher.AddListener(WorldEvent.HintScreenChangedEvent, HintPageChangedEventHandler);
            MessageDispatcher.AddListener(WorldEvent.HelpScreenShouldAlertEvent, HelpScreenShouldAlertHandler);
        }
        private void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.HelpScreenShouldAlertEvent,HelpScreenShouldAlertHandler);
            MessageDispatcher.RemoveListener(WorldEvent.HintScreenChangedEvent,HintPageChangedEventHandler);
            MessageDispatcher.RemoveListener(WorldEvent.ToggleHintUIDownEvent,ToggleHintUIDownEventHandler);
            MessageDispatcher.RemoveListener(WorldEvent.ToggleHintUIUpEvent,ToggleHintUIUpEventHandler);
        }
    }
}