using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using com.ootii.Messages;
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
            CameraUpdated?.Invoke();
            Destroy(this);
        }

        void Awake()
        {
            //TODO 这个需求现在不重要了、但是还是要整理一下。
            /*switch (StartGameMgr.DetectedScreenRatio)
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
            }*/

            MessageDispatcher.SendMessage(WorldEvent.MainCameraReadyEvent);
        }
    }
}