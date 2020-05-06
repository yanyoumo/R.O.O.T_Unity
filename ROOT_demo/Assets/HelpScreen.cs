using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class HelpScreen : MonoBehaviour
    {
        private bool AtUpOrDown = true;
        private bool sliding = false;
        private bool slidingUpOrDown = true;

        private float UpPos = -1.64f;
        private float DownPos = -10.22f;
        private float Sec = 0.1f;
        private float speedPerMultiplier=1.75f;
        private float YPos= 1.53f;

        private float slideTimer = 0.0f;

        public AnimationCurve Curve;

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
                float lerpingTime = Time.time - slideTimer;

                if (slidingUpOrDown)
                {
                    if (transform.position.z < UpPos)
                    {
                        transform.position += new Vector3(0.0f, 0.0f, speedPerMultiplier*Curve.Evaluate(lerpingTime));
                    }
                    else
                    {
                        transform.position = new Vector3(0.0f, YPos, UpPos);
                        AtUpOrDown = true;
                        sliding = false;
                    }
                }
                else
                {
                    if (transform.position.z > DownPos)
                    {
                        transform.position -= new Vector3(0.0f, 0.0f, speedPerMultiplier * Curve.Evaluate(lerpingTime));
                    }
                    else
                    {
                        transform.position = new Vector3(0.0f, YPos, DownPos);
                        AtUpOrDown = false;
                        sliding = false;
                    }
                }
            }
        }
    }
}