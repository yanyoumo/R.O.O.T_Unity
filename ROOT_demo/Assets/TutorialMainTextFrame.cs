using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;

namespace ROOT
{
    //TODO 这个东西不知道是不是要和另外那个联合起来，要不然这个管理很不利。
    public class TutorialMainTextFrame : MonoBehaviour
    {
        public TextMeshPro ContentText;
        public TextMeshPro NextExtended;
        public TextMeshPro NextCollapsed;
        public AnimationCurve Curve;
        private readonly float ShowTime = 0.1f;
        private float _showTimer = 0.1f;

        public string Content
        {
            set => ContentText.text = value;
            get => ContentText.text;
        }
        public bool Showed=false;
        public bool ShouldShow
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

        void Awake()
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(0.0f, 0.0f, DistanceFromCamera));
            Debug.Log(pos);
            float ZeroX = pos.x;
            Vector3 currentPos = transform.position;
            float camZ = 25;
            if (StartGameMgr.DetectedDevice==SupportedDevice.iPadPro4)
            {
                camZ = 30;
            }
            currentPos.y = camZ - DistanceFromCamera;
            transform.position = currentPos;
            PosXNotShow = ZeroX - 4.21f;
            PosXShow = ZeroX + 4.71f;
        }

        IEnumerator Animate(bool shouldShow)
        {
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
                    Showed = shouldShow;
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