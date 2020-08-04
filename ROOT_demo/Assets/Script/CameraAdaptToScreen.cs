using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEditor;
using UnityEngine;

namespace ROOT
{
    public class CameraAdaptToScreen : MonoBehaviour
    {
        public CinemachineFreeLook Crane;

        void Awake()
        {
            switch (StartGameMgr.DetectedScreenRatio)
            {
                case SupportedScreenRatio.XGA:
                    Crane.m_Orbits[1].m_Radius = 30.0f;
                    break;
                case SupportedScreenRatio.HD:
                    break;
                case SupportedScreenRatio.AppleHD:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Destroy(this);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}