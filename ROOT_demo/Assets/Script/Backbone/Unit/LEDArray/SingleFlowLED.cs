using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
    public class SingleFlowLED : LEDArray
    {
        /*public SpriteRenderer PosXArrow;
        public SpriteRenderer NegXArrow;*/
        
        public MeshRenderer PosXArrow_Tex;
        public MeshRenderer NegXArrow_Tex;

        private Vector3 MaxArrowScale = new Vector3(0.33f, 0.33f, 1.0f);
        private Vector3 MinArrowScale = new Vector3(0.1f, 0.1f, 1.0f);

        private int minVal = 1;
        private int maxVal = 10;
        
        private void Awake()
        {
            PosXArrow_Tex.sharedMaterial.color = LEDColor;
            NegXArrow_Tex.sharedMaterial.color = LEDColor;
        }

        private void Update()
        {
            PosXArrow_Tex.sharedMaterial.mainTextureOffset = new Vector2(-0.01f * Time.frameCount, 0.0f);
        }

        public override int Val
        {
            set
            {
                /*var absVal = Mathf.Abs(value);
                var scale = Mathf.Max(absVal / (float) maxVal, 1e-3f);
                var scaleVec = Vector3.Lerp(MinArrowScale, MaxArrowScale, scale);*/
                //var scaleVec = Vector3.Lerp(MinArrowScale, MaxArrowScale, 1.0f);
                /*PosXArrow_Tex.transform.localScale = scaleVec;
                NegXArrow_Tex.transform.localScale = scaleVec;*/
                if (value==0)
                {
                    //目前是要用==0来关掉的。
                    PosXArrow_Tex.enabled = false;
                    NegXArrow_Tex.enabled = false;
                }else if (value > 0)
                {
                    PosXArrow_Tex.enabled = true;
                    NegXArrow_Tex.enabled = false;
                }
                else
                {
                    PosXArrow_Tex.enabled = false;
                    NegXArrow_Tex.enabled = true;
                }
            }
        }
    }
}