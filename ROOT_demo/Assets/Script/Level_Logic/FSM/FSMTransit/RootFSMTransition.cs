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
        }

        public RootFSMTransition(RootFSMStatus _loopingStatus) :
            this(_loopingStatus, _loopingStatus, 0, AutoTrans)
        { }

        public RootFSMTransition(RootFSMStatus _startingStatus, RootFSMStatus _targetingStatus, int _priority) :
            this(_startingStatus, _targetingStatus, _priority, AutoTrans) {}

        public RootFSMTransition(RootFSMStatus _startingStatus, RootFSMStatus _targetingStatus, int _priority, Func<bool> req)
        {
            //�����Ͻ�����ôдҲ���ԡ�
            StartingStatus = _startingStatus;
            TargetingStatus = _targetingStatus;
            priority = _priority;
            AdditionalReq = req;
            Consequence = DefaultConsequence;
        }

        public RootFSMTransition(RootFSMStatus _startingStatus, RootFSMStatus _targetingStatus, int _priority, Func<bool> req, Action cons)
        {
            //�����Ͻ�����ôдҲ���ԡ�
            StartingStatus = _startingStatus;
            TargetingStatus = _targetingStatus;
            priority = _priority;
            AdditionalReq = req;
            Consequence = cons;
        }
    }
}