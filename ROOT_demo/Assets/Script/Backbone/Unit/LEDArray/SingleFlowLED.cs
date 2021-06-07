using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
    public class SingleFlowLED : LEDArray
    {
        public MeshRenderer PosXArrow_Tex;
        public MeshRenderer NegXArrow_Tex;

        private Vector3 MaxArrowScale = new Vector3(0.33f, 0.33f, 1.0f);
        private Vector3 MinArrowScale = new Vector3(0.1f, 0.1f, 1.0f);

        private int minVal = 1;
        private int maxVal = 10;
        
        private void Awake()
        {
            PosXArrow_Tex.material = new Material(PosXArrow_Tex.material);
            NegXArrow_Tex.material = new Material(NegXArrow_Tex.material);
            PosXArrow_Tex.sharedMaterial.color = LEDColor;
            NegXArrow_Tex.sharedMaterial.color = LEDColor;
        }

        private void Update()//TODO 到时候更精巧的一些就是这里的流程用DOTween改修一下。
        {
            if (WorldCycler.GamePausedStatus) return;
            PosXArrow_Tex.sharedMaterial.mainTextureOffset = new Vector2(-0.01f * Time.frameCount, 0.0f);
            NegXArrow_Tex.sharedMaterial.mainTextureOffset = new Vector2(-0.01f * Time.frameCount, 0.0f);
        }

        public override int Val
        {
            set
            {
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