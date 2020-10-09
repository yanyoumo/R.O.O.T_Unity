using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
//using Padding= ROOT.Utils.PaddingNum2Digit();

namespace ROOT
{
    public class SignalPanel : MonoBehaviour
    {
        public TextMeshPro NormalSignal;
        public TextMeshPro NetworkSignal;
        public TextMeshPro MissionTarget;

        private int crtNormalSignal;
        public int CRTNormalSignal
        {
            set
            {
                crtNormalSignal = value;
                UpdateNumbers();
            }
            get => crtNormalSignal;
        }

        private int tgtNormalSignal;
        public int TGTNormalSignal
        {
            set
            {
                tgtNormalSignal = value;
                UpdateNumbers();
            }
            get => tgtNormalSignal;
        }

        private int crtNetworkSignal;
        public int CRTNetworkSignal
        {
            set
            {
                crtNetworkSignal = value;
                UpdateNumbers();
            }
            get => crtNetworkSignal;
        }

        private int tgtNetworkSignal;
        public int TGTNetworkSignal
        {
            set
            {
                tgtNetworkSignal = value;
                UpdateNumbers();
            }
            get => tgtNetworkSignal;
        }
        
        private int crtMission;
        public int CRTMission
        {
            set
            {
                crtMission = value;
                UpdateNumbers();
            }
            get => crtMission;
        }

        private int tgtMission;
        public int TGTtMission
        {
            set
            {
                tgtMission = value;
                UpdateNumbers();
            }
            get => tgtMission;
        }

        private string Padding(int v)
        {
            return Utils.PaddingNum2Digit(v);
        }

        private void UpdateNumbers()
        {
            NormalSignal.text = Padding(crtNormalSignal) + "/" + Padding(tgtNormalSignal);
            NetworkSignal.text = Padding(crtNetworkSignal) + "/" + Padding(tgtNetworkSignal);
            MissionTarget.text = Padding(crtMission) + "/" + Padding(tgtMission);
        }
    }
}