using System.Linq;
using com.ootii.Messages;
using DG.Tweening;
using I2.Loc;
using ROOT.Message;
using TMPro;
using UnityEngine;

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
        [HideInInspector]public bool ShouldShow = false;
        public bool Animating { get; private set; } = false;
        public AnimationCurve Curve;

        private Vector2 _posXY;
        private float TimeLerper => (Time.time - _slideTimer) / _slideDuration;
        private bool _animatingUpOrDown = true;

        private float _upPos = -1.64f;
        private float _downPos = -10.49f;

        private float _slideTimer = 0.0f;
        private float _slideDuration = 0.3f;

        private bool _atUpOrDown = false;
        void ToggleHintUIUpEventHandler(IMessage rMessage)
        {
            ShouldShow = true;
        }
        
        void ToggleHintUIDownEventHandler(IMessage rMessage)
        {
            ShouldShow = false;
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
                for (var i = 0; i < targetingPages.Length; i++)
                {
                    targetingPages[i].gameObject.SetActive(data.Toggle && (i == data.PageNum));
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
            PressHForHintText.transform.DOShakePosition(0.3f).OnComplete(HelpScreenAlertComplete);
        }
        
        void Awake()
        {
            _posXY = new Vector2(transform.position.x, transform.position.y);
            HorHintText.Term = ScriptTerms.HForHint_KM;

            _gameplayPages = GameplayPageRoot.GetComponentsInChildren<Transform>().Where(t => t.parent == GameplayPageRoot).ToArray();
            _tutorialPages = TutorialPageRoot.GetComponentsInChildren<Transform>().Where(t => t.parent == TutorialPageRoot).ToArray();

            OldPressHForHintTextPos = PressHForHintText.transform.localPosition;
            
            MessageDispatcher.AddListener(WorldEvent.ToggleHintUIUpEvent, ToggleHintUIUpEventHandler);
            MessageDispatcher.AddListener(WorldEvent.ToggleHintUIDownEvent, ToggleHintUIDownEventHandler);
            MessageDispatcher.AddListener(WorldEvent.HintScreenChangedEvent, HintPageChangedEventHandler);
            MessageDispatcher.AddListener(WorldEvent.HelpScreenShouldAlertEvent, HelpScreenShouldAlertHandler);
            Debug.Log("_gameplayPageCount=" + _gameplayPageCount);
            Debug.Log("_tutorialPageCount=" + _tutorialPageCount);
        }

        private void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.HelpScreenShouldAlertEvent,HelpScreenShouldAlertHandler);
            MessageDispatcher.RemoveListener(WorldEvent.HintScreenChangedEvent,HintPageChangedEventHandler);
            MessageDispatcher.RemoveListener(WorldEvent.ToggleHintUIDownEvent,ToggleHintUIDownEventHandler);
            MessageDispatcher.RemoveListener(WorldEvent.ToggleHintUIUpEvent,ToggleHintUIUpEventHandler);
        }

        // Update is called once per frame
        void Update()
        {
            if (!Animating)
            {
                if (!_atUpOrDown)
                {
                    if (ShouldShow)
                    {
                        _animatingUpOrDown = true;
                        Animating = true;
                        _slideTimer = Time.time;
                        HorHintText.Term = ScriptTerms.ReleaseToReturn_KM;
                    }
                }
                else
                {
                    if (!ShouldShow)
                    {
                        _animatingUpOrDown = false;
                        Animating = true;
                        _slideTimer = Time.time;
                        HorHintText.Term = ScriptTerms.HForHint_KM;
                    }
                }
            }
            else
            {
                if (TimeLerper < 1.0f)
                {
                    float A = _animatingUpOrDown ? _downPos : _upPos;
                    float B = _animatingUpOrDown ? _upPos : _downPos;
                    transform.position = new Vector3(_posXY.x, _posXY.y, Mathf.Lerp(A, B, Curve.Evaluate(TimeLerper)));
                }
                else
                {
                    transform.position = new Vector3(_posXY.x, _posXY.y, _animatingUpOrDown ? _upPos : _downPos);
                    _atUpOrDown = _animatingUpOrDown;
                    Animating = false;
                }
            }
        }
    }
}