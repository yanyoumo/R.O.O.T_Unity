using System;
using System.Collections.Generic;
using System.Linq;
using ROOT.Common;

namespace ROOT
{
    public sealed class RootFSMTranstionLib : HashSet<RootFSMTransition>
    {
        public int GetMaxPriorityByStatus(RootFSMStatus status)
        {
            return this.Where(t => t.StartingStatus == status).Select(t => t.priority).Max();          
        }
    }

    //这套FSM的实现，是基于Transit的流程、每个不同的FSM只需要在一开始注册不同的Transit，那么将成为不同的FSM。
    //不同的FSM在相同状态执行的逻辑和离开逻辑是相同的。
    //每个状态将会有结束逻辑和转移逻辑、结束后才进行转移逻辑的判断。
    public sealed class RootFSMTransition : IComparable<RootFSMTransition>, IEquatable<RootFSMTransition>
    {
        public RootFSM owner;
        public readonly int priority; //这个值越高优先级越高。
        public readonly RootFSMStatus StartingStatus; //Transit的高优先级要求，如果FSM不是这个状态则不考虑。
        public readonly RootFSMStatus TargetingStatus;
        public readonly Func<bool> AdditionalReq;
        public readonly Action Consequence;
        public readonly bool WaitForFrameAfterTransition;
        
        private static bool AutoTrans()
        {
            return true;
        }

        private void DefaultConsequence()
        {
            owner.currentStatus = TargetingStatus;
            owner.waitForNextFrame = WaitForFrameAfterTransition;
        }
 
        #region Constructor

        public RootFSMTransition(RootFSMStatus _loopingStatus)
        {
            StartingStatus = _loopingStatus;
            TargetingStatus = _loopingStatus;
            priority = 0;
            AdditionalReq = AutoTrans;
            Consequence = DefaultConsequence;
            WaitForFrameAfterTransition = true;
        }

        public RootFSMTransition(RootFSMStatus _startingStatus, RootFSMStatus _targetingStatus) : this(_startingStatus, _targetingStatus, 0, AutoTrans)
        {
        }

        public RootFSMTransition(RootFSMStatus _startingStatus, RootFSMStatus _targetingStatus, int _priority) : this(_startingStatus, _targetingStatus, _priority, AutoTrans)
        {
        }

        public RootFSMTransition(RootFSMStatus _startingStatus, RootFSMStatus _targetingStatus, int _priority, Func<bool> req)
        {
            StartingStatus = _startingStatus;
            TargetingStatus = _targetingStatus;
            priority = _priority;
            AdditionalReq = req;
            Consequence = DefaultConsequence;
            WaitForFrameAfterTransition = false;
        }

        public RootFSMTransition(RootFSMStatus _startingStatus, RootFSMStatus _targetingStatus, int _priority, bool waitForNext)
        {
            StartingStatus = _startingStatus;
            TargetingStatus = _targetingStatus;
            priority = _priority;
            AdditionalReq = AutoTrans;
            Consequence = DefaultConsequence;
            WaitForFrameAfterTransition = waitForNext;
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

        public RootFSMTransition(RootFSMStatus _startingStatus, RootFSMStatus _targetingStatus, int _priority, bool waitForNext, Func<bool> req, Action cons)
        {
            StartingStatus = _startingStatus;
            TargetingStatus = _targetingStatus;
            priority = _priority;
            AdditionalReq = req;
            Consequence = cons;
            WaitForFrameAfterTransition = waitForNext;
        }

        #endregion

        #region Interface_Func

        public int CompareTo(RootFSMTransition other)
        {
            return other.priority - priority;
        }
        public override bool Equals(object obj)
        {
            if (obj is RootFSMTransition transition)
            {
                return this == transition;
            }

            return false;
        }
        public override int GetHashCode()
        {
            const int count = (int)RootFSMStatus.COUNT;
            var sInt = (int) StartingStatus;
            var tInt = (int) TargetingStatus;
            return count * count * priority + count * sInt + tInt;
        }
        public bool Equals(RootFSMTransition other)
        {
            return other != null && GetHashCode() == other.GetHashCode();
        }

        #endregion
    }
}