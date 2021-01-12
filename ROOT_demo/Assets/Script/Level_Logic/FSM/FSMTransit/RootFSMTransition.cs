using System;

namespace ROOT
{
    //����FSM��ʵ�֣��ǻ���Transit�����̡�ÿ����ͬ��FSMֻ��Ҫ��һ��ʼע�᲻ͬ��Transit����ô����Ϊ��ͬ��FSM��
    //��ͬ��FSM����ͬ״ִ̬�е��߼����뿪�߼�����ͬ�ġ�
    //ÿ��״̬�����н����߼���ת���߼���������Ž���ת���߼����жϡ�
    public sealed class RootFSMTransition : IComparable<RootFSMTransition>
    {
        public RootFSM owner;
        public readonly int priority; //���ֵԽ�����ȼ�Խ�ߡ�
        public readonly RootFSMStatus StartingStatus; //Transit�ĸ����ȼ�Ҫ�����FSM�������״̬�򲻿��ǡ�
        public readonly RootFSMStatus TargetingStatus; //Transit�ĸ����ȼ�Ҫ�����FSM�������״̬�򲻿��ǡ�
        public readonly Func<bool> AdditionalReq;
        public readonly Action Consequence;
        public readonly bool WaitForFrameAfterTransition;

        public int CompareTo(RootFSMTransition other)
        {
            return other.priority - priority;
        }

        private static bool AutoTrans()
        {
            return true;
        }

        private void DefaultConsequence()
        {
            owner.currentStatus = TargetingStatus;
            owner.waitForNextFrame = WaitForFrameAfterTransition;
        }

        public RootFSMTransition(RootFSMStatus _loopingStatus)
        {
            StartingStatus = _loopingStatus;
            TargetingStatus = _loopingStatus;
            priority = 0;
            AdditionalReq = AutoTrans;
            Consequence = DefaultConsequence;
            WaitForFrameAfterTransition = true;
        }

        public RootFSMTransition(RootFSMStatus _startingStatus, RootFSMStatus _targetingStatus, int _priority):
            this(_startingStatus, _targetingStatus, _priority, AutoTrans) {}

        public RootFSMTransition(RootFSMStatus _startingStatus, RootFSMStatus _targetingStatus, int _priority, Func<bool> req)
        {
            StartingStatus = _startingStatus;
            TargetingStatus = _targetingStatus;
            priority = _priority;
            AdditionalReq = req;
            Consequence = DefaultConsequence;
            WaitForFrameAfterTransition = false;
        }

        public RootFSMTransition(RootFSMStatus _startingStatus, RootFSMStatus _targetingStatus, int _priority, bool waitForNext, Func<bool> req)
        {
            StartingStatus = _startingStatus;
            TargetingStatus = _targetingStatus;
            priority = _priority;
            AdditionalReq = req;
            Consequence = DefaultConsequence;
            WaitForFrameAfterTransition = waitForNext;
        }

        public RootFSMTransition(RootFSMStatus _startingStatus, RootFSMStatus _targetingStatus, int _priority, Func<bool> req, Action cons)
        {
            StartingStatus = _startingStatus;
            TargetingStatus = _targetingStatus;
            priority = _priority;
            AdditionalReq = req;
            Consequence = cons;
            WaitForFrameAfterTransition = false;
        }

        public RootFSMTransition(RootFSMStatus _startingStatus, RootFSMStatus _targetingStatus, int _priority,bool waitForNext, Func<bool> req, Action cons)
        {
            StartingStatus = _startingStatus;
            TargetingStatus = _targetingStatus;
            priority = _priority;
            AdditionalReq = req;
            Consequence = cons;
            WaitForFrameAfterTransition = waitForNext;
        }
    }
}