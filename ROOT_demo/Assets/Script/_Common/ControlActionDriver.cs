using System;
using System.Collections.Generic;
using System.Linq;
using static RewiredConsts.Action.Button;
using static RewiredConsts.Action.Composite;
using static RewiredConsts.Action.Passthough;

namespace ROOT
{
    using RespToCtrlEvent= Func<ActionPack,ControllingPack,ControllingPack>;
    public abstract class ControlActionDriver
    {
        protected readonly FSMLevelLogic _owner;
        protected RootFSM _mainFsm;
        protected Queue<ControllingPack> _ctrlPackQueue;
        protected Queue<BreakingCommand> _breakingCMDQueue;

        protected void EnqueueBreakingCommand(BreakingCommand brkingCmd)
        {
            _breakingCMDQueue.Enqueue(brkingCmd);
        }
        public bool CtrlQueueNonEmpty => _ctrlPackQueue.Count != 0;
        public bool PendingRequestedBreak => _breakingCMDQueue.Count != 0;
        
        public BreakingCommand RequestedBreakType
        {
            get
            {
                if (PendingRequestedBreak)
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    return _breakingCMDQueue.Dequeue();
                }
                throw new ArgumentException();
            }
        }

        public ControllingPack CtrlQueueHeader
        {
            get
            {
                if (CtrlQueueNonEmpty) return _ctrlPackQueue.Dequeue();
                var ctrlPack = new ControllingPack {CtrlCMD = ControllingCommand.Nop};
                return ctrlPack;
            }
        }

        protected ControlActionDriver(FSMLevelLogic owner, RootFSM fsm)
        {
            _ctrlPackQueue = new Queue<ControllingPack>();
            _breakingCMDQueue = new Queue<BreakingCommand>();
            this._owner = owner;
            _mainFsm = fsm;
            ControllingEventMgr.ControllingEvent += RespondToControlEvent;
        }

        protected void FilterDir(ActionPack actionPack, out RotationDirection? direction)
        {
            switch (actionPack.ActionID)
            {
                case CursorUp:
                    direction = RotationDirection.North;
                    return;
                case CursorDown:
                    direction = RotationDirection.South;
                    return;
                case CursorLeft:
                    direction = RotationDirection.West;
                    return;
                case CursorRight:
                    direction = RotationDirection.East;
                    return;
            }

            direction = null;
        }

        protected static bool SkillID(ref ControllingPack ctrlPack, in ActionPack actionPack)
        {
            if (actionPack.ActionID != FuncComp) return false;
            ctrlPack.SetFlag(ControllingCommand.Skill);
            ctrlPack.SkillID = actionPack.FuncID - 1; //base 0和base 1的转换。
            if (ctrlPack.SkillID < 0)
            {
                ctrlPack.SkillID += 10;
            }
            return true;
        }

        protected static bool ShopBuyID(ref ControllingPack ctrlPack, in ActionPack actionPack)
        {
            if (actionPack.ActionID != FuncComp) return false;
            ctrlPack.SetFlag(ControllingCommand.Buy);
            ctrlPack.SkillID = actionPack.FuncID - 1; //base 0和base 1的转换。
            if (ctrlPack.SkillID < 0)
            {
                ctrlPack.SkillID += 10;
            }
            return true;
        }
        
        //protected virtual 
        
        //这个一定实要进行配置的、但是这个本质上是要构建一棵树、但是可以构建一棵树的方法还是FSM。
        protected abstract void RespondToControlEvent(ActionPack actionPack);

        ~ControlActionDriver()
        {
            // ReSharper disable once DelegateSubtraction
            ControllingEventMgr.ControllingEvent -= RespondToControlEvent;
        }
    }

    public class CareerControlActionDriver : ControlActionDriver
    {
        public CareerControlActionDriver(FSMLevelLogic owner, RootFSM fsm) : base(owner, fsm) { }
        
        //上面也说了；这个Driver本质上是要建立一棵树、没有特别好将这棵树进行量化和配置化的流程；
        //目前的流程是每个不同的Driver都从头建立一棵树。（重载RespondToControlEvent函数）
        protected override void RespondToControlEvent(ActionPack actionPack)
        {
            var ctrlPack = new ControllingPack {CtrlCMD = ControllingCommand.Nop};
            FilterDir(actionPack, out var dir);
            if (dir.HasValue)
            {
                ctrlPack.CommandDir = dir.Value;
                ctrlPack.ReplaceFlag(actionPack.HoldForDrag ? ControllingCommand.Drag : ControllingCommand.Move);
                ctrlPack.CurrentPos = _owner.LevelAsset.Cursor.CurrentBoardPosition;
                ctrlPack.NextPos = _owner.LevelAsset.Cursor.GetCoord(ctrlPack.CommandDir);
            }
            else if (actionPack.IsAction(RotateUnit))
            {
                ctrlPack.CurrentPos = _owner.LevelAsset.Cursor.CurrentBoardPosition;
                ctrlPack.SetFlag(ControllingCommand.Rotate);
            }

            if (actionPack.IsAction(CycleNext))
            {
                ctrlPack.SetFlag(ControllingCommand.CycleNext);
            }

            if (actionPack.IsAction(Enter))
            {
                ctrlPack.SetFlag(ControllingCommand.Confirm);
            }

            if (actionPack.IsAction(LeftAlt))
            {
                ctrlPack.SetFlag(ControllingCommand.Cancel);
            }
            //TODO 下面两套的流程应该能有更好的管理方法。
            ShopBuyID(ref ctrlPack, in actionPack);
            SkillID(ref ctrlPack, in actionPack);

            if ((WorldCycler.BossStage && actionPack.IsAction(BossPause)))
            {
                if (WorldCycler.BossStagePause)
                {
                    ctrlPack.SetFlag(ControllingCommand.BossResume);
                }
                else
                {
                    EnqueueBreakingCommand(BreakingCommand.BossPause);
                }
            }

            if (!WorldCycler.BossStage || WorldCycler.BossStagePause)
            {
                //Boss阶段非暂停的时候、输入不进入队列。
                _ctrlPackQueue.Enqueue(ctrlPack);
            }
        }

    }
}