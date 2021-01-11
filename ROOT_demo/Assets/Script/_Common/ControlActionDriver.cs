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

        private void RespondToControlEvent(ActionPack actionPack)
        {
            var ctrlPack = new ControllingPack { CtrlCMD = ControllingCommand.Nop };
            /*ctrlPack = new ControllingPack { CtrlCMD = ControllingCommand.Nop };
            var anyDir = GetCommandDir(out var Direction);
            var anyDirAxis = GetCamMovementVec_KB(out var directionAxis);
            if (anyDir)
            {
                ctrlPack.CommandDir = Direction;
                ctrlPack.ReplaceFlag(ControllingCommand.Move); //Replace
                if (player.GetButton(StaticName.INPUT_BUTTON_NAME_MOVEUNIT))
                {
                    ctrlPack.ReplaceFlag(ControllingCommand.Drag); //Replace
                }
            }

            ctrlPack.CurrentPos = currentLevelAsset.Cursor.CurrentBoardPosition;
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