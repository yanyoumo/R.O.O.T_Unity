using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEditor;
using UnityEngine;

namespace ROOT
{
    [Obsolete]//TODO 这个玩意儿也要处理掉
    public class CameraAdaptToScreen : MonoBehaviour
    {
        public delegate void CameraDelegate();
        public static CameraDelegate CameraUpdated;

        public CinemachineFreeLook Crane;

        IEnumerator DelayedDelegate()
        {
            //这个现在不要搞这个、用广播对时事件去弄。
            yield return 0;
            yield return 0;
            yield return 0;
            CameraUpdated();
            Destroy(this);
        }

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

            StartCoroutine(DelayedDelegate());
        }
    }
}