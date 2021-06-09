﻿using System.Linq;
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
        
        private int _gameplayPageCount => _gameplayPages.Length;
        private int _tutorialPageCount => _tutorialPages.Length;
        
        public Localize HorHintText;
        private bool shouldShow = false;//这个其实就是简单的队列了。
        private bool animating = false;
        private bool atUpOrDown = false;
        private const float _upPos = -1.64f;
        private const float _downPos = -10.49f;
        private const float _slideDuration = 0.3f;

        private void TogglePage_Additive(Transform[] targetingPages, HintPageChangedData data)
        {
            targetingPages[data.PageNum].gameObject.SetActive(data.Toggle);
        }

        private void TogglePage_Replace(Transform[] targetingPages, HintPageChangedData data)
        {
            for (var i = 0; i < targetingPages.Length; i++)
            {
                targetingPages[i].gameObject.SetActive(data.Toggle && (i == data.PageNum));
            }
        }

        void HintPageChangedEventHandler(IMessage rMessage)
        {
            if (rMessage is HintPageChangedData data)
            {
                var targetingPages = data.TutorialOrGameplay ? _tutorialPages : _gameplayPages;
                Debug.Log("targetingPages.Length=" + targetingPages.Length);
                if (data.Toggle && (data.PageNum < 0 || data.PageNum >= targetingPages.Length))
                {
                    Debug.LogWarning("Requesting page:"+data.PageNum+" for "+ (data.TutorialOrGameplay ? "Tutorial" : "Gameplay")+" is not valid, request ignored!");
                    return;
                }
                if (data.AdditiveOrReplace)
                {
                    TogglePage_Additive(targetingPages, data);
                }
                else
                {
                    TogglePage_Replace(targetingPages, data);
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