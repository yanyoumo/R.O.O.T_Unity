using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
    public class TimeLineMarker : MonoBehaviour
    {
        private bool _normalOrGreyOut = true;

        public bool SetNormal
        {
            get => _normalOrGreyOut;
            set
            {
                _normalOrGreyOut = value;
                RodRenderer.material = _normalOrGreyOut ? NormalMat : GreyOutMat;
                SmallRodRenderer.material = _normalOrGreyOut ? NormalMat : GreyOutMat;
            }
        }

        public Material NormalMat;
        public Material GreyOutMat;
        public MeshRenderer RodRenderer;
        public MeshRenderer SmallRodRenderer;
        
        [HideInInspector]public bool PendingKill = false;
        public bool UseMajorMark
        {
            set
            {
                MajorMark.gameObject.SetActive(value);
                MinorMark.gameObject.SetActive(!value);
            }
        }
        public Transform TimeLineMarkerRoot;

        public MeshRenderer MajorMark;
        public MeshRenderer MinorMark;

        private void Awake()
        {
            //SetNormal = true;
        }

        void Update()
        {
            if (PendingKill)
            {
                Destroy(gameObject);
            }
        }

        void OnDestroy()
        {
            Destroy(gameObject);
        }
    }
}