using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

namespace ROOT
{
    public partial class HintMaster : MonoBehaviour
    {
        public HelpScreen HelpScreen;
    }

    public class HelpScreen : MonoBehaviour
    {
        public Localize HorHintText;

        private bool _atUpOrDown = false;
        internal bool ShouldShow = false;
        internal bool Animating { get; private set; } = false;
        private bool _animatingUpOrDown = true;

        private float _upPos = -1.64f;
        private float _downPos = -10.49f;

        private float _slideTimer = 0.0f;
        private float _slideDuration = 0.3f;
        private float TimeLerper => (Time.time - _slideTimer) / _slideDuration;

        public AnimationCurve Curve;

        private Vector2 _posXY;

        void Awake()
        {
            _posXY = new Vector2(transform.position.x, transform.position.y);
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
                        HorHintText.Term = ScriptTerms.ReleaseToReturn;
                    }
                }
                else
                {
                    if (!ShouldShow)
                    {
                        _animatingUpOrDown = false;
                        Animating = true;
                        _slideTimer = Time.time;
                        HorHintText.Term = ScriptTerms.HForHint;
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