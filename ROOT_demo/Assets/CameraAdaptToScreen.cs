using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEditor;
using UnityEngine;

namespace ROOT
{
    public enum SupportedDevice
    {
        StandAlone,
        iPXR,
        iPadPro4,
    }

    public class CameraAdaptToScreen : MonoBehaviour
    {
        public CinemachineFreeLook Crane;

        void Awake()
        {
            switch (StartGameMgr.DetectedDevice)
            {
                case SupportedDevice.StandAlone:
                    //DO NOTHING 16:9
                    break;
                case SupportedDevice.iPXR:
                    //IPHONE XR
                    break;
                case SupportedDevice.iPadPro4:
                    Crane.m_Orbits[1].m_Radius = 30.0f;
                    break;
                default:
#if UNITY_EDITOR
                    throw new ArgumentOutOfRangeException();
#else
                    break;                  
                    //Fall back to PC.
#endif
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