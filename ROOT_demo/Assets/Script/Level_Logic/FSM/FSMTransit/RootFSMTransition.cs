using System;

namespace ROOT
{
    //这套FSM的实现，是基于Transit的流程、每个不同的FSM只需要在一开始注册不同的Transit，那么将成为不同的FSM。
    //不同的FSM在相同状态执行的逻辑和离开逻辑是相同的。
    //每个状态将会有结束逻辑和转移逻辑、结束后才进行转移逻辑的判断。
    public sealed class RootFSMTransition : IComparable<RootFSMTransition>
    {
        public RootFSM owner;
        public readonly int priority; //这个值越高优先级越高。
        public readonly RootFSMStatus StartingStatus; //Transit的高优先级要求，如果FSM不是这个状态则不考虑。
        public readonly RootFSMStatus TargetingStatus; //Transit的高优先级要求，如果FSM不是这个状态则不考虑。
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
            //理论上讲，这么写也可以。
            StartingStatus = _startingStatus;
            TargetingStatus = _targetingStatus;
            priority = _priority;
            AdditionalReq = req;
            Consequence = DefaultConsequence;
        }

        public RootFSMTransition(RootFSMStatus _startingStatus, RootFSMStatus _targetingStatus, int _priority, Func<bool> req, Action cons)
        {
            //理论上讲，这么写也可以。
            StartingStatus = _startingStatus;
            TargetingStatus = _targetingStatus;
            priority = _priority;
            AdditionalReq = req;
            Consequence = cons;
        }
    }
}