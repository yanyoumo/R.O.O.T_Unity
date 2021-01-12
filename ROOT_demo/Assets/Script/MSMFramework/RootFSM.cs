using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
    using FSMActions= Dictionary<RootFSMStatus, Action>;

    public enum RootFSMStatus
    {
        //����дȫ���ġ�Rootϵ���С�ȫ������ʹ�õ�Ǳ��״̬��
        PreInit,
        UpKeep,
        R_Cycle,
        F_Cycle,//��Ϊ����������߼�����
        Career_Cycle,//ֻ�����С�ְҵ��ģʽ��Ҫ���߼�
        R_IO,//ReactToIO
        Skill,
        BossInit,
        Boss,
        Animate,
        CleanUp,
    }

    public sealed class RootFSM
    {
        [ReadOnly] public LevelLogic owner;
        [ReadOnly] public RootFSMStatus currentStatus = RootFSMStatus.PreInit;
        [ReadOnly] public bool waitForNextFrame = false;

        private List<RootFSMTransition> _transitions;
        private FSMActions _actions;
        

        public void Transit()
        {
            var satisfiedTransition = _transitions.Where(a => a.StartingStatus == currentStatus)
                .Where(msmTransition => msmTransition.AdditionalReq()).ToList();
            if (satisfiedTransition.Count > 0)
            {
                satisfiedTransition.Sort(); //����������ǽ��������ǽ���
                satisfiedTransition[0].Consequence();
            }
        }

        public void Execute()
        {
            if (_actions.ContainsKey(currentStatus))
            {
                _actions[currentStatus]();
            }
            else
            {
                Debug.LogWarning("No action on assigned status!");
            }
        }

        public void AppendAction(RootFSMStatus FSMStatus,Action action)
        {
            if (_actions.ContainsKey(FSMStatus))
            {
                throw new ArgumentException("Status Exists");
            }
            _actions[FSMStatus] = action;
        }

        public void ReplaceActions(FSMActions actions)
        {
            _actions = actions;
        }

        public void ReplaceTransition(List<RootFSMTransition> transitions)
        {
            _transitions = transitions;
            foreach (var msmTransition in _transitions)
            {
                msmTransition.owner = this;
            }
        }
    }
}