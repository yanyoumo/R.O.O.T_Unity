using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
    public enum RootFSMStatus
    {
        //����дȫ���ġ�Rootϵ���С�ȫ������ʹ�õ�Ǳ��״̬��
        PreInit,
        Idle,
        Upkeep,
        Cycle,
        BossInit,
        Boss,
        Animate,
        CleanUp,
    }

    public abstract class RootFSMBase : MonoBehaviour
    {
        [ReadOnly] public LevelLogic owner;
        [ReadOnly] public RootFSMStatus currentStatus = RootFSMStatus.PreInit;

        protected List<RootFSMTransition> Actions;
        protected abstract void InitActions();

        public void Transit()
        {
            var satisfiedTransition = Actions.Where(a => a.StartingStatus == currentStatus)
                .Where(msmTransition => msmTransition.AdditionalReq()).ToList();
            if (satisfiedTransition.Count > 0)
            {
                satisfiedTransition.Sort(); //����������ǽ���
                satisfiedTransition[0].Consequence();
            }
        }

        public void Execute()
        {
            Debug.Log(currentStatus);
        }

        void Awake()
        {
            InitActions();
        }
    }
}