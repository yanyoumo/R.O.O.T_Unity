using System;
using com.ootii.Messages;
using JetBrains.Annotations;
using Rewired;
using ROOT.Message;
using Sirenix.OdinInspector;
using UnityEngine;
using static RewiredConsts.Action;

namespace ROOT
{
    //这个东西比较蛋疼、键盘、手柄类基于按键的操作和鼠标这种基于时序的操作不同。
    //这里有点是两路输入拼在一起感觉、但是为了这个时间系统的解耦、目前就只能这么办。
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

    //这个类的实例化和相关初始化已经搞定。
    //这个类认为是对玩家意愿的解释——拿到RAW的硬件事件后、“试图”理解玩家的意图，然后转化为Action实例。
    //有一点是，这个类只做事实判断、不去考虑是否合法。只是重视地反馈玩家意图的Action。
    public class ControllingEventMgr : MonoBehaviour
    {
        [NotNull] private static ControllingEventMgr _instance;
        public static ControllingEventMgr Instance => _instance;

        [ReadOnly] public int playerId = 0;
        private Player player;

        //public static WorldEvent.ControllingEventHandler ControllingEvent;

        private static bool holdForDrag = false;

        private static Vector2 MouseScreenPos;

        private float minHoldShift => Mathf.Pow(1, Mathf.Lerp(-3.0f, -1.0f, MouseDragSensitivity / 100f));

        private int MouseDragSensitivity = 50;
        
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            
            if (PlayerPrefs.HasKey("MouseDragSensitivity"))
            {
                MouseDragSensitivity = PlayerPrefs.GetInt("MouseDragSensitivity");
            }
            else
            {
                PlayerPrefs.SetInt("MouseDragSensitivity", 50);
            }
            
            
            player = ReInput.players.GetPlayer(playerId);

            player.AddInputEventDelegate(OnInputUpdateCurser, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Button.CursorUp);
            player.AddInputEventDelegate(OnInputUpdateCurser, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Button.CursorDown);
            player.AddInputEventDelegate(OnInputUpdateCurser, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Button.CursorLeft);
            player.AddInputEventDelegate(OnInputUpdateCurser, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Button.CursorRight);

            player.AddInputEventDelegate(OnInputUpdateFunc, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Button.Func0);
            player.AddInputEventDelegate(OnInputUpdateFunc, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Button.Func1);
            player.AddInputEventDelegate(OnInputUpdateFunc, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Button.Func2);
            player.AddInputEventDelegate(OnInputUpdateFunc, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Button.Func3);
            player.AddInputEventDelegate(OnInputUpdateFunc, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Button.Func4);
            player.AddInputEventDelegate(OnInputUpdateFunc, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Button.Func5);
            player.AddInputEventDelegate(OnInputUpdateFunc, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Button.Func6);
            player.AddInputEventDelegate(OnInputUpdateFunc, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Button.Func7);
            player.AddInputEventDelegate(OnInputUpdateFunc, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Button.Func8);
            player.AddInputEventDelegate(OnInputUpdateFunc, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Button.Func9);

            player.AddInputEventDelegate(OnInputUpdateSpaceDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Button.Confirm0);
            player.AddInputEventDelegate(OnInputUpdateSpaceUp, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, Button.Confirm0);

            player.AddInputEventDelegate(OnInputUpdateBasicButton, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Button.TelemetryPause);
            player.AddInputEventDelegate(OnInputUpdateBasicButton, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Composite.RotateUnit);
            player.AddInputEventDelegate(OnInputUpdateBasicButton, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Button.HintHDD);
            player.AddInputEventDelegate(OnInputUpdateBasicButton, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Button.HintNetwork);
            player.AddInputEventDelegate(OnInputUpdateBasicButton, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Button.HintControl);
            player.AddInputEventDelegate(OnInputUpdateBasicButton, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Button.CycleNext);
            player.AddInputEventDelegate(OnInputUpdateBasicButton, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Button.InGameOverLayToggle);
            
            player.AddInputEventDelegate(OnInputUpdateBasicButton, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Composite.ShopTierUp);
            player.AddInputEventDelegate(OnInputUpdateBasicButton, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Composite.ShopTierDown);

            player.AddInputEventDelegate(OnInputUpdateMouseSingleClickLeftDown, UpdateLoopType.Update, InputActionEventType.ButtonJustSinglePressed, Passthough.MouseLeft);
            player.AddInputEventDelegate(OnInputUpdateMouseSingleClickLeftUp, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, Passthough.MouseLeft);
            //player.AddInputEventDelegate(OnInputUpdateMouseDoubleClick, UpdateLoopType.Update, InputActionEventType.ButtonJustDoublePressed, Passthough.MouseLeft);
            //player.AddInputEventDelegate(OnInputUpdateMouseHold, UpdateLoopType.Update, InputActionEventType.ButtonJustPressedForTime, Passthough.MouseLeft, new object[] { 1.5f });
            player.AddInputEventDelegate(OnInputUpdateMouseSingleClickRight, UpdateLoopType.Update, InputActionEventType.ButtonJustSinglePressed, Passthough.MouseRight);
            player.AddInputEventDelegate(OnInputUpdateMouseSingleClickMiddle, UpdateLoopType.Update, InputActionEventType.ButtonJustSinglePressed, Passthough.MouseMiddle);
            player.AddInputEventDelegate(OnInputUpdateMouseWheel, UpdateLoopType.Update, InputActionEventType.AxisActive, Passthough.MouseWheel);
        }

        private void OnInputUpdateCurser(InputActionEventData obj)
        {
            var actionPack = new ActionPack
            {
                ActionID = obj.actionId,
                eventType = obj.eventType,
                HoldForDrag = holdForDrag,
                Sender = this,
            };
            switch (actionPack.ActionID)
            {
                case Button.CursorUp:
                    actionPack.ActionDirection = RotationDirection.North;
                    break;
                case Button.CursorDown:
                    actionPack.ActionDirection = RotationDirection.South;
                    break;
                case Button.CursorLeft:
                    actionPack.ActionDirection = RotationDirection.West;
                    break;
                case Button.CursorRight:
                    actionPack.ActionDirection = RotationDirection.East;
                    break;
            }
            actionPack.Sender = this;
            MessageDispatcher.SendMessage(actionPack);
        }

        private void OnInputUpdateFunc(InputActionEventData obj)
        {
            var actionPack = new ActionPack
            {
                ActionID = Composite.FuncComp,
                eventType = obj.eventType,
                HoldForDrag = holdForDrag,
                FuncID = int.Parse(obj.actionName.Substring(4)),
                Sender = this,
            };
            MessageDispatcher.SendMessage(actionPack);
        }

        private void OnInputUpdateSpaceDown(InputActionEventData obj)
        {
            holdForDrag = true;
            var actionPack = new ActionPack
            {
                ActionID = obj.actionId,
                eventType = obj.eventType,
                HoldForDrag = holdForDrag,
                Sender = this,
            };
            MessageDispatcher.SendMessage(actionPack);
        }

        private void OnInputUpdateSpaceUp(InputActionEventData obj)
        {
            holdForDrag = false;
        }

        private void OnInputUpdateBasicButton(InputActionEventData obj)
        {
            var actionPack = new ActionPack
            {
                ActionID = obj.actionId,
                eventType = obj.eventType,
                HoldForDrag = holdForDrag,
                Sender = this,
            };
            MessageDispatcher.SendMessage(actionPack);
        }

        private void OnInputUpdateMouseSingleClickLeftDown(InputActionEventData obj)
        {
            MouseScreenPos = player.controllers.Mouse.screenPosition; 
            RootDebug.Log("Mouse Single Click Down",NameID.JiangDigong_Log);
        }

        private void OnInputUpdateMouseSingleClickLeftUp(InputActionEventData obj)
        {
            var actionPack = new ActionPack
            {
                MouseScreenPosA = MouseScreenPos,
                MouseScreenPosB = player.controllers.Mouse.screenPosition,
                Sender = this,
            };
            bool hasAction = false;
            RootDebug.Log("Mouse Single Click Up",NameID.JiangDigong_Log);
            if (!MouseScreenPos.Equals(new Vector2(Single.NaN, Single.NaN)))
            {
                //Debug.Log(Screen.width + "==" + Screen.height);
                if (Utils.GetCustomizedDistance(actionPack.MouseScreenPosA, actionPack.MouseScreenPosB) < minHoldShift)
                {
                    actionPack.ActionID = Passthough.MouseLeft;
                    actionPack.eventType = InputActionEventType.ButtonSinglePressed;
                    RootDebug.Log("Mouse Single Click", NameID.JiangDigong_Log);
                }
                else
                { 
                    actionPack.ActionID = Composite.Drag;
                    actionPack.eventType = InputActionEventType.AxisActive;
                    RootDebug.Log("Mouse Drag", NameID.JiangDigong_Log);
                }
                hasAction = true;
            }
            MouseScreenPos=new Vector2(Single.NaN, Single.NaN);
            if (hasAction)
                MessageDispatcher.SendMessage(actionPack);
        }

        /*
        private void OnInputUpdateMouseDoubleClick(InputActionEventData obj)
        {
            var actionPack = new ActionPack
            {
                ActionID = obj.actionId,
                eventType = obj.eventType,
                Sender = this,
            };
            MessageDispatcher.SendMessage(actionPack);
            RootDebug.Log("Mouse Double Click",NameID.JiangDigong_Log);
        }
        */

        /*
        private void OnInputUpdateMouseHold(InputActionEventData obj)
        {
            var actionPack = new ActionPack
            {
                ActionID = obj.actionId,
                eventType = obj.eventType,
                Sender = this,
            };
            MessageDispatcher.SendMessage(actionPack);
            RootDebug.Log("Mouse Hold",NameID.JiangDigong_Log);
        }
        */
        private void OnInputUpdateMouseSingleClickRight(InputActionEventData obj)
        {
            var actionPack = new ActionPack
            {
                ActionID = obj.actionId,
                eventType = obj.eventType,
                Sender = this,
            };
            MessageDispatcher.SendMessage(actionPack);
            RootDebug.Log("Mouse Single Click Right", NameID.JiangDigong_Log);
        }
        private void OnInputUpdateMouseSingleClickMiddle(InputActionEventData obj)
        {
            var actionPack = new ActionPack
            {
                ActionID = obj.actionId,
                eventType = obj.eventType,
                Sender = this,
            };
            MessageDispatcher.SendMessage(actionPack);
            RootDebug.Log("Mouse Single Click Middle", NameID.JiangDigong_Log);
        }
        private void OnInputUpdateMouseWheel(InputActionEventData obj)
        {
            var actionPack = new ActionPack
            {
                ActionID = obj.actionId,
                eventType = obj.eventType,
                Sender = this,
                MouseWheelDelta = player.GetAxisDelta(obj.actionId),
            };
            MessageDispatcher.SendMessage(actionPack);
            RootDebug.Log("Mouse Wheel Delta "+ player.GetAxisDelta(obj.actionId), NameID.JiangDigong_Log);
        }
    }
}