using System;
using System.Collections.Generic;
using static RewiredConsts.Action.Button;
using static RewiredConsts.Action.Composite;
using static RewiredConsts.Action.Passthough;

namespace ROOT
{
    public class ControlActionDriver
    {
        private FSMLevelLogic _owner;
        private RootFSM _mainFsm;
        private Queue<ControllingPack> _ctrlPackQueue;
        public bool CtrlQueueNonEmpty => _ctrlPackQueue.Count != 0;
        public bool PendingRequestedBreak => _requestedBreakType.HasValue;
        public BreakingCommand RequestedBreakType
        {
            get
            {
                if (PendingRequestedBreak)
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    return _requestedBreakType.Value;
                }
                throw new ArgumentException();
            }
        }
        private BreakingCommand? _requestedBreakType;
        public ControllingPack CtrlQueueHeader
        {
            get
            {
                if (CtrlQueueNonEmpty) return _ctrlPackQueue.Dequeue();
                var ctrlPack = new ControllingPack {CtrlCMD = ControllingCommand.Nop};
                return ctrlPack;
            }
        }

        public ControlActionDriver(FSMLevelLogic _owner, RootFSM _fsm)
        {
            _ctrlPackQueue = new Queue<ControllingPack>();
            this._owner = _owner;
            _mainFsm = _fsm;
            ControllingEventMgr.ControllingEvent += RespondToControlEvent;
        }

        void filterDir(ActionPack actionPack, out RotationDirection? Direction)
        {
            switch (actionPack.ActionID)
            {
                case CursorUp:
                    Direction = RotationDirection.North;
                    return;
                case CursorDown:
                    Direction = RotationDirection.South;
                    return;
                case CursorLeft:
                    Direction = RotationDirection.West;
                    return;
                case CursorRight:
                    Direction = RotationDirection.East;
                    return;
            }

            Direction = null;
        }

        private static bool SkillID(ref ControllingPack ctrlPack, in ActionPack actionPack)
        {
            if (actionPack.ActionID != FuncComp) return false;
            ctrlPack.SetFlag(ControllingCommand.Skill);
            ctrlPack.SkillID = actionPack.FuncID - 1;//base 0和base 1的转换。
            if (ctrlPack.SkillID<0)
            {
                ctrlPack.SkillID += 10;
            }
            return true;
        }

        private static bool ShopBuyID(ref ControllingPack ctrlPack, in ActionPack actionPack)
        {
            if (actionPack.ActionID != FuncComp) return false;
            ctrlPack.SetFlag(ControllingCommand.Buy);
            ctrlPack.SkillID = actionPack.FuncID - 1;//base 0和base 1的转换。
            if (ctrlPack.SkillID < 0)
            {
                ctrlPack.SkillID += 10;
            }
            return true;
        }

        public void BreakDealt()
        {
            _requestedBreakType = null;
        }

        private void RespondToControlEvent(ActionPack actionPack)
        {
            var ctrlPack = new ControllingPack {CtrlCMD = ControllingCommand.Nop};
            filterDir(actionPack, out var dir);
            if (dir.HasValue)
            {
                ctrlPack.CommandDir = dir.Value;
                ctrlPack.ReplaceFlag(actionPack.HoldForDrag ? ControllingCommand.Drag : ControllingCommand.Move);
                ctrlPack.CurrentPos = _owner.LevelAsset.Cursor.CurrentBoardPosition;
                ctrlPack.NextPos = _owner.LevelAsset.Cursor.GetCoord(ctrlPack.CommandDir);
            }
            else if (actionPack.IsAction(RotateUnit))
            {
                //�ƶ����϶������ȼ�����ת�ߡ�
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

            //RISK 这里的逻辑具体怎么调整？
            if (WorldCycler.BossStage)
            {
                if (actionPack.IsAction(BossPause))
                {
                    if (WorldCycler.BossStagePause)
                    {
                        ctrlPack.SetFlag(ControllingCommand.BossUnPause);
                    }
                    else
                    {
                        _requestedBreakType = BreakingCommand.BossPause;
                    }
                }
            }


            //TODO 下面两套的流程应该能有更好的管理方法。
            ShopBuyID(ref ctrlPack, in actionPack);
            SkillID(ref ctrlPack, in actionPack);
            //Debug.Log("Enqueue:" + ctrlPack.CtrlCMD);
            //����Ҫ��������һ��ctrlPack�Ƿ������������ݡ�
            _ctrlPackQueue.Enqueue(ctrlPack);
        }

        ~ControlActionDriver()
        {
            // ReSharper disable once DelegateSubtraction
            ControllingEventMgr.ControllingEvent -= RespondToControlEvent;
        }
    }
}