using System.Collections;
using I2.Loc;
using TMPro;
using UnityEngine;
// ReSharper disable DelegateSubtraction

namespace ROOT
{
    /*public partial class HintMaster : MonoBehaviour
    {
        public TutorialMainTextFrame TutorialMainTextFrame;
    }*/
    
    public class TutorialMainTextFrame : MonoBehaviour
    {
        public TextMeshPro ContentText;
        public TextMeshPro NextExtended;
        public TextMeshPro NextCollapsed;
        public Localize NextCollapsedLLE;
        public Localize NextExtendedLLE;
        public AnimationCurve Curve;
        private readonly float ShowTime = 0.1f;
        private float _showTimer = 0.1f;
        private bool ChangedCam = false;

        public string Content
        {
            set => ContentText.text = value;
            get => ContentText.text;
        }
        internal bool _showed=false;
        internal bool ShouldShow
        {
            set
            {
                NextExtended.enabled = value;
                NextCollapsed.enabled = !value;
                ContentText.enabled = value;

                if (value)
                {
                    Show();
                }
                else
                {
                    Hide();
                }
            }
        }

        private float TimeLerper => (Time.timeSinceLevelLoad - _showTimer)/ ShowTime;

        private float PosXNotShow= -13.14f;
        private float PosXShow = -4.1f;
        private readonly float DistanceFromCamera = 20.0f;
        public bool Animating { get; private set; } = false;

        void Show()
        {
            if (!Animating)
            {
                _showTimer = Time.timeSinceLevelLoad;
                StartCoroutine(Animate(true));
            }
        }
        void Hide()
        {
            if (!Animating)
            {
                _showTimer = Time.timeSinceLevelLoad;
                StartCoroutine(Animate(false));
            }
        }

        void UpdatePosition()
        {
            CameraAdaptToScreen.CameraUpdated -= UpdatePosition;
            transform.position = Camera.main.ScreenToWorldPoint(new Vector3(0.0f, Screen.height, 20));
            float ZeroX = transform.position.x;
            PosXNotShow = ZeroX - 4.21f - 4.71f;
            PosXShow = ZeroX;
            ChangedCam = true;
        }

        void Awake()
        {
            CameraAdaptToScreen.CameraUpdated += UpdatePosition;
            if (StartGameMgr.UseTouchScreen)
            {
                NextCollapsedLLE.Term = ScriptTerms.TouchHintCollapsed;
                NextExtendedLLE.Term = ScriptTerms.TouchHintExtend;
            }
            else
            {
                NextCollapsedLLE.Term = ScriptTerms.KMHintCollapsed;
                NextExtendedLLE.Term = ScriptTerms.KMHintExtend;
            }
        }
        IEnumerator Animate(bool shouldShow)
        {
            if (!ChangedCam)
            {            
                //RISK 这个东西调整位置的时候和Crane的调整有一个竞争冒险效应，在调第一次这个Animate的时候，还没有初始化，就只能在这儿强制调一下。
                UpdatePosition();
            }
            Animating = true;
            Vector3 pos = transform.position;
            while (true)
            {
                var posX = 0.0f;
                if (TimeLerper >= 1.0f)
                {
                    Animating = false;

                    posX = shouldShow ? PosXShow : PosXNotShow;
                    transform.position = new Vector3(posX, pos.y, pos.z);
                    _showed = shouldShow;
                    yield break;
                }

                posX = shouldShow
                    ? Mathf.Lerp(PosXNotShow, PosXShow, TimeLerper)
                    : Mathf.Lerp(PosXShow, PosXNotShow, TimeLerper);
                transform.position = new Vector3(posX, pos.y, pos.z);
                yield return 0;
            }
        }
    }
}