using System;
using Rewired;
using ROOT.Message;
using UnityEngine;

namespace ROOT
{
    [Serializable]
    public class ActionPack : RootMessageBase
    {
        public int ActionID;
        public InputActionEventType eventType;
        public RotationDirection ActionDirection;
        public Vector2 MouseScreenPosA;
        public Vector2 MouseScreenPosB;
        public int FuncID;
        public bool HoldForDrag;
        public float MouseWheelDelta;

        public bool IsAction(int actionID)
        {
            return ActionPackIsAction(this, actionID);
        }

        public static bool ActionPackIsAction(ActionPack actPack, int actionID)
        {
            return actPack.ActionID == actionID;
        }
        
        public override string Type => WorldEvent.ControllingEvent;
    }
}