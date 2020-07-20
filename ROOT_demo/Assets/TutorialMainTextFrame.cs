using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ROOT
{
    public class TutorialMainTextFrame : MonoBehaviour
    {
        public TextMeshPro ContentText;
        public TextMeshPro Next_Extended;
        public TextMeshPro Next_Collapsed;
        public AnimationCurve Curve;
        private readonly float ShowTime = 0.1f;
        private float _showTimer = 0.1f;

        public string Content
        {
            set => ContentText.text = value;
            get => ContentText.text;
        }
        public bool ShouldShow
        {
            set
            {
                Next_Extended.enabled = value;
                Next_Collapsed.enabled = !value;

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

        private readonly float PosXNotShow= -13.14f;
        private readonly float PosXShow = -4.1f;
        private bool _animating = false;

        void Show()
        {
            if (!_animating)
            {
                _showTimer = Time.timeSinceLevelLoad;
                StartCoroutine(Animate(true));
            }
        }

        void Hide()
        {
            if (!_animating)
            {
                _showTimer = Time.timeSinceLevelLoad;
                StartCoroutine(Animate(false));
            }
        }

        IEnumerator Animate(bool shouldShow)
        {
            _animating = true;
            float PosX = 0.0f;
            Vector3 pos = transform.position;
            while (true)
            {
                if (TimeLerper >= 1.0f)
                {
                    _animating = false;

                    PosX = shouldShow ? PosXShow : PosXNotShow;
                    transform.position = new Vector3(PosX, pos.y, pos.z);

                    yield break;
                }

                PosX = shouldShow
                    ? Mathf.Lerp(PosXNotShow, PosXShow, TimeLerper)
                    : Mathf.Lerp(PosXShow, PosXNotShow, TimeLerper);
                transform.position = new Vector3(PosX, pos.y, pos.z);
                yield return 0;
            }
        }
    }
}