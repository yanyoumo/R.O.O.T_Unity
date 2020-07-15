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
        private float ShowTime = 0.1f;
        private float ShowTimer = 0.1f;
        private bool shouldShow;

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

                if (shouldShow == showed)
                {
                    shouldShow = value;
                }
            }
            get => shouldShow;
        }

        private bool showed;
        private float PosXNotShow= -13.14f;
        private float PosXShow = -4.1f;
        
        void Update()
        {
            if (shouldShow != showed)
            {
                float PosX = 0.0f;
                Vector3 pos = transform.position;
                float timeLerp = Time.timeSinceLevelLoad - ShowTimer;
                timeLerp /= ShowTime;
                if (timeLerp > 1.0f)
                {
                    showed = shouldShow;
                    timeLerp = 1.0f;
                }

                PosX = shouldShow
                    ? Mathf.Lerp(PosXNotShow, PosXShow, timeLerp)
                    : Mathf.Lerp(PosXShow, PosXNotShow, timeLerp);
                transform.position = new Vector3(PosX, pos.y, pos.z);
            }
            else
            {
                ShowTimer = Time.timeSinceLevelLoad;
            }
        }
    }
}