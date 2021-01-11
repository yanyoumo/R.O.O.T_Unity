using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class ControlActionDriver
    {
        private FSMLevelLogic _owner;
        private RootFSM _mainFsm;
        private Queue<ControllingPack> _ctrlPackQueue;
        public bool CtrlQueueNonEmpty => _ctrlPackQueue.Count != 0;
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
            switch (actionPack.ActionEventData.actionId)
            {
                case RewiredConsts.Action.Button.CursorUp:
                    Direction = RotationDirection.North;
                    return;
                case RewiredConsts.Action.Button.CursorDown:
                    Direction = RotationDirection.South;
                    return;
                case RewiredConsts.Action.Button.CursorLeft:
                    Direction = RotationDirection.West;
                    return;
                case RewiredConsts.Action.Button.CursorRight:
                    Direction = RotationDirection.East;
                    return;
            }

            Direction = null;
        }

        private static bool SkillID(ref ControllingPack ctrlPack, in ActionPack actionPack)
        {
            if (actionPack.ActionID != RewiredConsts.Action.Composite.FuncComp) return false;
            ctrlPack.SetFlag(ControllingCommand.Skill);
            ctrlPack.ShopID = actionPack.FuncID;
            return true;
        }

        private static bool ShopBuyID(ref ControllingPack ctrlPack, in ActionPack actionPack)
        {
            if (actionPack.ActionID != RewiredConsts.Action.Composite.FuncComp) return false;
            ctrlPack.SetFlag(ControllingCommand.Buy);
            ctrlPack.ShopID = actionPack.FuncID;
            return true;
        }

        private void RespondToControlEvent(ActionPack actionPack)
        {
            var ctrlPack = new ControllingPack {CtrlCMD = ControllingCommand.Nop};
            if (actionPack.ActionID == RewiredConsts.Action.Composite.MoveUnit)
            {
                ctrlPack.CommandDir = actionPack.ActionDirection;
                ctrlPack.ReplaceFlag(ControllingCommand.Drag);
            }
            else
            {
                filterDir(actionPack, out var dir);
                if (dir.HasValue)
                {
                    ctrlPack.CommandDir = dir.Value;
                    ctrlPack.ReplaceFlag(ControllingCommand.Move);
                }
            }

            ctrlPack.CurrentPos = _owner.LevelAsset.Cursor.CurrentBoardPosition;
            ctrlPack.NextPos = _owner.LevelAsset.Cursor.GetCoord(ctrlPack.CommandDir);

            if (actionPack.ActionID == RewiredConsts.Action.Composite.RotateUnit &&
                ctrlPack.CtrlCMD == ControllingCommand.Nop)
            {
                //移动和拖动的优先级比旋转高。
                ctrlPack.CurrentPos = _owner.LevelAsset.Cursor.CurrentBoardPosition;
                ctrlPack.SetFlag(ControllingCommand.Rotate);
            }

            if (actionPack.ActionID == RewiredConsts.Action.Button.CycleNext)
            {
                ctrlPack.SetFlag(ControllingCommand.CycleNext);
            }

            if (actionPack.ActionID == RewiredConsts.Action.Passthough.Enter)
            {
                ctrlPack.SetFlag(ControllingCommand.Confirm);
            }

            if (actionPack.ActionID == RewiredConsts.Action.Passthough.LeftAlt)
            {
                ctrlPack.SetFlag(ControllingCommand.Cancel);
            }

            ShopBuyID(ref ctrlPack, in actionPack);
            SkillID(ref ctrlPack, in actionPack);
            //Debug.Log("Enqueue:" + ctrlPack.CtrlCMD);
            //还需要在这里标记一个ctrlPack是否是阻塞的数据。
            _ctrlPackQueue.Enqueue(ctrlPack);
        }

        ~ControlActionDriver()
        {
            // ReSharper disable once DelegateSubtraction
            ControllingEventMgr.ControllingEvent -= RespondToControlEvent;
        }
    }
}