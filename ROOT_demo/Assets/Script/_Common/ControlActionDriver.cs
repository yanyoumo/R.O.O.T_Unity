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
        public ControllingPack CtrlQueueHeader => _ctrlPackQueue.Dequeue();
        
        public ControlActionDriver(FSMLevelLogic _owner,RootFSM _fsm)
        {
            _ctrlPackQueue = new Queue<ControllingPack>();
            this._owner = _owner;
            _mainFsm = _fsm;
            ControllingEventMgr.ControllingEvent += RespondToControlEvent;
        }

        void filterDir(ActionPack actionPack,out RotationDirection? Direction)
        {
            switch (actionPack.ActionEventData.actionId)
            {
                case RewiredConsts.Action.CursorUp:
                    Direction = RotationDirection.North;
                    break;
                case RewiredConsts.Action.CursorDown:
                    Direction = RotationDirection.South;
                    break;
                case RewiredConsts.Action.CursorLeft:
                    Direction = RotationDirection.West;
                    break;
                case RewiredConsts.Action.CursorRight:
                    Direction = RotationDirection.East;
                    break;
            }
            Direction = null;
        }

        private void RespondToControlEvent(ActionPack actionPack)
        {
            var ctrlPack = new ControllingPack { CtrlCMD = ControllingCommand.Nop };
            filterDir(actionPack,out var direction);
            if (direction.HasValue)
            {
                ctrlPack.CommandDir = direction.Value;
                ctrlPack.ReplaceFlag(ControllingCommand.Move); //Replace
                /*if (player.GetButton(StaticName.INPUT_BUTTON_NAME_MOVEUNIT))
                {
                    ctrlPack.ReplaceFlag(ControllingCommand.Drag); //Replace
                }*/
            }

            /*ctrlPack.CurrentPos = currentLevelAsset.Cursor.CurrentBoardPosition;
            ctrlPack.NextPos = currentLevelAsset.Cursor.GetCoord(ctrlPack.CommandDir);

            if (player.GetButtonDown(StaticName.INPUT_BUTTON_NAME_REMOVEUNIT))
            {
                //ctrlPack.CurrentPos = currentLevelAsset.Cursor.CurrentBoardPosition;
                //ctrlPack.SetFlag(ControllingCommand.RemoveUnit);
            }

            if (player.GetButtonDown(StaticName.INPUT_BUTTON_NAME_ROTATEUNIT) &&
                ctrlPack.CtrlCMD == ControllingCommand.Nop)
            {
                //移动和拖动的优先级比旋转高。
                ctrlPack.CurrentPos = currentLevelAsset.Cursor.CurrentBoardPosition;
                ctrlPack.SetFlag(ControllingCommand.Rotate);
            }

            if (player.GetButton(StaticName.INPUT_BUTTON_NAME_HINTHDD) ||
                player.GetButton(StaticName.INPUT_BUTTON_NAME_HINTNET))
            {
                ctrlPack.SetFlag(ControllingCommand.SignalHint);
            }

            if (player.GetButton(StaticName.INPUT_BUTTON_NAME_HINTCTRL))
            {
                ctrlPack.SetFlag(ControllingCommand.PlayHint);
            }

            if (player.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CYCLENEXT))
            {
                ctrlPack.SetFlag(ControllingCommand.CycleNext);
            }

            if (player.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CONFIRM))
            {
                ctrlPack.SetFlag(ControllingCommand.Confirm);
            }

            if (player.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CANCELED))
            {
                ctrlPack.SetFlag(ControllingCommand.Cancel);
            }

            if (currentLevelAsset.BuyingCursor)
            {
                if (player.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SHOPRANDOM))
                {
                    ctrlPack.SetFlag(ControllingCommand.BuyRandom);
                }
            }

            var anyBuy = ShopBuyID(ref ctrlPack);
            var anySkill = SkillID(ref ctrlPack);*/
            _ctrlPackQueue.Enqueue(ctrlPack);
        }

         ~ControlActionDriver()
         {
             // ReSharper disable once DelegateSubtraction
             ControllingEventMgr.ControllingEvent -= RespondToControlEvent;
         }
    }
}