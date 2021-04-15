using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
    public class SingleFlowLED : LEDArray
    {
        public SpriteRenderer PosXArrow;
        public SpriteRenderer NegXArrow;

        private Vector3 MaxArrowScale = new Vector3(0.33f, 0.33f, 1.0f);
        private Vector3 MinArrowScale = new Vector3(0.1f, 0.1f, 1.0f);

        private int minVal = 1;
        private int maxVal = 10;
        
        private void Awake()
        {
            PosXArrow.color = LEDColor;
            NegXArrow.color = LEDColor;
        }

        public override int Val
        {
            set
            {
                var absVal = Mathf.Abs(value);
                var scale = Mathf.Max(absVal / (float) maxVal, 1e-3f);
                var scaleVec = Vector3.Lerp(MinArrowScale, MaxArrowScale, scale);
                PosXArrow.transform.localScale = scaleVec;
                NegXArrow.transform.localScale = scaleVec;
                if (value==0)
                {
                    //目前是要用==0来关掉的。
                    PosXArrow.enabled = false;
                    NegXArrow.enabled = false;
                }else if (value > 0)
                {
                    PosXArrow.enabled = true;
                    NegXArrow.enabled = false;
                }
                else
                {
                    PosXArrow.enabled = false;
                    NegXArrow.enabled = true;
                }
            }
        }
    }
}