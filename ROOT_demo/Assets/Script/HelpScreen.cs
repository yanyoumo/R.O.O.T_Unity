using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class HelpScreen : MonoBehaviour
    {
        private bool AtUpOrDown = false;
        private bool sliding = false;
        private bool slidingUpOrDown = true;

        private float UpPos = -1.64f;
        private float DownPos = -10.49f;
        //private float Sec = 0.1f;
        //private float speedPerMultiplier=1.75f;
        //private float YPos= 1.53f;

        private float slideTimer = 0.0f;
        private float slideDuration = 0.3f;

        public AnimationCurve Curve;

        private Vector2 posXY;

        void Awake()
        {
            posXY = new Vector2(transform.position.x, transform.position.y);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (!sliding)
            {
                if (!AtUpOrDown)
                {
                    if (Input.GetButton(StaticName.INPUT_BUTTON_NAME_HINTCTRL))
                    {
                        slidingUpOrDown = true;
                        sliding = true;
                        slideTimer = Time.time;
                    }
                }
                else
                {
                    if (!Input.GetButton(StaticName.INPUT_BUTTON_NAME_HINTCTRL))
                    {
                        slidingUpOrDown = false;
                        sliding = true;
                        slideTimer = Time.time;
                    }
                }
            }
            else
            {
                //这个东西涉及到一个积分的东西。//曲线直接控制位置不就得了…………
                float lerpingTime = (Time.time - slideTimer) / slideDuration;

                if (slidingUpOrDown)
                {
                    if (lerpingTime < 1.0f)
                    {
                        transform.position = new Vector3(posXY.x, posXY.y, Mathf.Lerp(DownPos,UpPos,Curve.Evaluate(lerpingTime)));
                    }
                    else
                    {
                        transform.position = new Vector3(posXY.x, posXY.y, UpPos);
                        AtUpOrDown = true;
                        sliding = false;
                    }
                }
                else
                {
                    if (lerpingTime < 1.0f)
                    {
                        transform.position = new Vector3(posXY.x, posXY.y, Mathf.Lerp(UpPos, DownPos,  Curve.Evaluate(lerpingTime)));
                    }
                    else
                    {
                        transform.position = new Vector3(posXY.x, posXY.y, DownPos);
                        AtUpOrDown = false;
                        sliding = false;
                    }
                }
            }
        }
    }
}