using System;
using System.Collections.Generic;
using System.Linq;
using ROOT.Common;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT
{
    using FSMActions= Dictionary<RootFSMStatus, Action>;

    public sealed class RootFSM
    {
        [ReadOnly] public FSMLevelLogic owner;
        [ReadOnly] public RootFSMStatus currentStatus = RootFSMStatus.PreInit;
        [ReadOnly] public bool waitForNextFrame = false;

        private RootFSMTranstionLib _transitions;
        private FSMActions _actions;
        private Dictionary<BreakingCommand, Action> _breakingActions;

        public void Breaking(BreakingCommand breakingCommand)
        {
            if (_breakingActions.ContainsKey(breakingCommand))
            {
                _breakingActions[breakingCommand]();
                return;
            }
            Debug.LogWarning("No action on assigned BreakingCommand!");
        }
        
        public void Transit()
        {
            var satisfiedTransition = _transitions.Where(a => a.StartingStatus == currentStatus).Where(msmTransition => msmTransition.AdditionalReq()).ToList();
            if (satisfiedTransition.Count > 0)
            {
                satisfiedTransition.Sort(); //这个是升序还是降序？现在是降序
                satisfiedTransition[0].Consequence();
            }
        }

        public void Execute()
        {
            if (_actions.ContainsKey(currentStatus))
            {
                _actions[currentStatus]();
                return;
            }
            Debug.LogWarning("No action on assigned status!");
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

        public void ReplaceBreaking(Dictionary<BreakingCommand, Action> breakingActions)
        {
            _breakingActions = breakingActions;
        }
        
        public void ReplaceTransition(RootFSMTranstionLib transitions)
        {
            _transitions = transitions;
            _transitions.ForEach(t => t.owner = this);
        }
    }
}