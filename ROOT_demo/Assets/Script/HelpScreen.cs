using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class HelpScreen : MonoBehaviour
    {
        public TutorialMainTextFrame TutorialMainTextFrame;

        private bool _atUpOrDown = false;
        private bool _sliding = false;
        private bool _slidingUpOrDown = true;

        private float _upPos = -1.64f;
        private float _downPos = -10.49f;

        private float _slideTimer = 0.0f;
        private float _slideDuration = 0.3f;

        public AnimationCurve Curve;

        private Vector2 _posXY;

        void Awake()
        {
            _posXY = new Vector2(transform.position.x, transform.position.y);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (!_sliding)
            {
                if (!_atUpOrDown)
                {
                    if (Input.GetButton(StaticName.INPUT_BUTTON_NAME_HINTCTRL))
                    {
                        _slidingUpOrDown = true;
                        _sliding = true;
                        _slideTimer = Time.time;
                    }
                }
                else
                {
                    if (!Input.GetButton(StaticName.INPUT_BUTTON_NAME_HINTCTRL))
                    {
                        _slidingUpOrDown = false;
                        _sliding = true;
                        _slideTimer = Time.time;
                    }
                }
            }
            else
            {
                float lerpingTime = (Time.time - _slideTimer) / _slideDuration;

                if (_slidingUpOrDown)
                {
                    if (lerpingTime < 1.0f)
                    {
                        transform.position = new Vector3(_posXY.x, _posXY.y, Mathf.Lerp(_downPos,_upPos,Curve.Evaluate(lerpingTime)));
                    }
                    else
                    {
                        transform.position = new Vector3(_posXY.x, _posXY.y, _upPos);
                        _atUpOrDown = true;
                        _sliding = false;
                    }
                }
                else
                {
                    if (lerpingTime < 1.0f)
                    {
                        transform.position = new Vector3(_posXY.x, _posXY.y, Mathf.Lerp(_upPos, _downPos,  Curve.Evaluate(lerpingTime)));
                    }
                    else
                    {
                        transform.position = new Vector3(_posXY.x, _posXY.y, _downPos);
                        _atUpOrDown = false;
                        _sliding = false;
                    }
                }
            }
        }
    }
}