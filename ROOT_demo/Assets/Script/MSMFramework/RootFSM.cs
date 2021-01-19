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
        PreInit,//FSM逻辑在初始化完成之前“阻塞性”逻辑、原则上里面不写实际逻辑。
        MajorUpKeep,//查询玩家的输入事件、并且进行基础的清理、更新逻辑。
        MinorUpKeep,//即使在Animate流程也会执行的逻辑部分、主要是查询是否有打断输入。
        R_Cycle,//倒行逻辑的部分。
        F_Cycle,//整形逻辑的核心逻辑、主要是执行具体的主干更新、数据更新等等。
        Career_Cycle,//现有“职业”模式需要的逻辑、包含但不限于对时间轴数据的更新、等等。
        R_IO,//ReactToIO、对从Driver获得的CtrlPack转换成实际执行的逻辑。
        Skill,//
        BossInit,//
        BossMajorUpKeep,//
        BossMinorUpKeep,//
        BossPause,//
        Animate,//将动画向前执行一帧、但是Root的动画流程时绑定时间而不是绑定帧数的。
        CleanUp,//将所有FSM的类数据重置、并且是FSM流程等待一帧的充分条件。
    }

    public sealed class RootFSM
    {
        [ReadOnly] public LevelLogic owner;
        [ReadOnly] public RootFSMStatus currentStatus = RootFSMStatus.PreInit;
        [ReadOnly] public bool waitForNextFrame = false;

        private HashSet<RootFSMTransition> _transitions;
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
        
        public void ReplaceTransition(HashSet<RootFSMTransition> transitions)
        {
            _transitions = transitions;
            foreach (var msmTransition in _transitions)
            {
                msmTransition.owner = this;
            }
        }
    }
}