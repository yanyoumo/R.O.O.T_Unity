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
        //这里写全部的、Root系列中、全部可以使用的潜在状态。
        PreInit,
        UpKeep,
        R_Cycle,
        F_Cycle,//认为是最基本的逻辑核心
        Career_Cycle,//只有现有“职业”模式需要的逻辑
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
                satisfiedTransition.Sort(); //这个是升序还是降序？现在是降序
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