using System;
using System.Collections;
using com.ootii.Messages;
using Rewired;
using Rewired.Dev;
using ROOT.Consts;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using static RewiredConsts.Action;
using Action = RewiredConsts.Action;

namespace ROOT
{
    //这个东西比较蛋疼、键盘、手柄类基于按键的操作和鼠标这种基于时序的操作不同。
    //这里有点是两路输入拼在一起感觉、但是为了这个时间系统的解耦、目前就只能这么办。

    //这个类的实例化和相关初始化已经搞定。
    //这个类认为是对玩家意愿的解释——拿到RAW的硬件事件后、“试图”理解玩家的意图，然后转化为Action实例。
    //有一点是，这个类只做事实判断、不去考虑是否合法。只是重视地反馈玩家意图的Action。
    public class ControllingEventMgr : MonoBehaviour
    {
        [ReadOnly] public int playerId = 0;
        private Player player;

        private static bool holdForDrag = false;

        private static Vector2 MouseScreenPos;

        private float minHoldShift => Mathf.Pow(1, Mathf.Lerp(-3.0f, -1.0f, MouseDragSensitivity / 100f));

        private int MouseDragSensitivity = 50;

        #region ReflectionSection
        //TODO 这里反射的代码、还是要中Editor相关的宏框一下。

        [PropertySpace]
        [ValueDropdown("CursorActionAsDDList")] 
        public int[] CursorActions;
        
        [PropertySpace]
        [ValueDropdown("FunctionalActionAsDDList")] 
        public int[] FunctionalActions;

        [PropertySpace]
        [ValueDropdown("BasicButtonActionAsDDList")] 
        public int[] BasicActions;
        
        private void RegisterCursorAction()
        {
            foreach (var cursorAction in CursorActions)
            {
                player.AddInputEventDelegate(OnInputUpdateCurser, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, cursorAction);
            }
        }

        private void RegisterBasicKeyAction(int ActionID)
        {
            player.AddInputEventDelegate(OnInputUpdateBasicButton, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, ActionID);
        }

        private IEnumerable BasicButtonActionAsDDList => GetDDListFromCatagory(typeof(Button));
        private IEnumerable CursorActionAsDDList => GetDDListFromCatagory(typeof(Action.Cursor));
        private IEnumerable FunctionalActionAsDDList => GetDDListFromCatagory(typeof(Functional));

        private IEnumerable GetDDListFromCatagory(Type targetType)
        {
            var res = new ValueDropdownList<int>();
            foreach (var fieldInfo in targetType.GetFields())
            {
                var actionAttribute = Attribute.GetCustomAttribute(fieldInfo, typeof(ActionIdFieldInfoAttribute));
                if (actionAttribute is ActionIdFieldInfoAttribute)
                {
                    res.Add(fieldInfo.Name, (int) fieldInfo.GetValue(null));
                }
            }
            return res;
        }

        #endregion
        
        void Awake()
        {
            DontDestroyOnLoad(gameObject);

            if (PlayerPrefs.HasKey(StaticPlayerPrefName.MOUSE_DRAG_SENSITIVITY))
            {
                MouseDragSensitivity = PlayerPrefs.GetInt(StaticPlayerPrefName.MOUSE_DRAG_SENSITIVITY);
            }
            else
            {
                PlayerPrefs.SetInt(StaticPlayerPrefName.MOUSE_DRAG_SENSITIVITY, 50);
            }

            player = ReInput.players.GetPlayer(playerId);

            RegisterCursorAction();
            FunctionalActions.ForEach(RegisterBasicKeyAction);
            BasicActions.ForEach(RegisterBasicKeyAction);

            player.AddInputEventDelegate(OnInputUpdateSpaceDown, UpdateLoopType.Update,
                InputActionEventType.ButtonJustPressed,
                Button.HoldForDrag);
            player.AddInputEventDelegate(OnInputUpdateSpaceUp, UpdateLoopType.Update,
                InputActionEventType.ButtonJustReleased,
                Button.HoldForDrag);

            player.AddInputEventDelegate(OnInputHintUp, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed,
                Button.HintControl);
            player.AddInputEventDelegate(OnInputHintDown, UpdateLoopType.Update,
                InputActionEventType.ButtonJustReleased, Button
                    .HintControl);

            player.AddInputEventDelegate(OnInputUpdateMouseSingleClickLeftDown, UpdateLoopType.Update,
                InputActionEventType.ButtonJustSinglePressed, Passthough.MouseLeft);
            player.AddInputEventDelegate(OnInputUpdateMouseSingleClickLeftUp, UpdateLoopType.Update,
                InputActionEventType.ButtonJustReleased, Passthough.MouseLeft);

            player.AddInputEventDelegate(OnInputUpdateMouseSingleClickRight, UpdateLoopType.Update,
                InputActionEventType.ButtonJustSinglePressed, Passthough.MouseRight);
            player.AddInputEventDelegate(OnInputUpdateMouseSingleClickMiddle, UpdateLoopType.Update,
                InputActionEventType.ButtonJustSinglePressed, Passthough.MouseMiddle);
            player.AddInputEventDelegate(OnInputUpdateMouseWheel, UpdateLoopType.Update,
                InputActionEventType.AxisActive,
                Passthough.MouseWheel);
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
                case Action.Cursor.CursorUp:
                    actionPack.ActionDirection = RotationDirection.North;
                    break;
                case Action.Cursor.CursorDown:
                    actionPack.ActionDirection = RotationDirection.South;
                    break;
                case Action.Cursor.CursorLeft:
                    actionPack.ActionDirection = RotationDirection.West;
                    break;
                case Action.Cursor.CursorRight:
                    actionPack.ActionDirection = RotationDirection.East;
                    break;
            }

            actionPack.Sender = this;
            MessageDispatcher.SendMessage(actionPack);
        }

        private void OnInputHintUp(InputActionEventData obj)
        {
            var actionPack = new ActionPack
            {
                ActionID = Button.HintControl,
                eventType = obj.eventType,
                HoldForHint = true,
                Sender = this,
            };
            MessageDispatcher.SendMessage(actionPack);
        }

        private void OnInputHintDown(InputActionEventData obj)
        {
            var actionPack = new ActionPack
            {
                ActionID = Button.HintControl,
                eventType = obj.eventType,
                HoldForHint = false,
                Sender = this,
            };
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
            //Debug.Log("OnInputUpdateBasicButton");
            MessageDispatcher.SendMessage(actionPack);
        }

        private void OnInputUpdateMouseSingleClickLeftDown(InputActionEventData obj)
        {
            MouseScreenPos = player.controllers.Mouse.screenPosition;
            RootDebug.Log("Mouse Single Click Down", NameID.JiangDigong_Log);
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
            RootDebug.Log("Mouse Single Click Up", NameID.JiangDigong_Log);
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

            MouseScreenPos = new Vector2(Single.NaN, Single.NaN);
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
            RootDebug.Log("Mouse Wheel Delta " + player.GetAxisDelta(obj.actionId), NameID.JiangDigong_Log);
        }
    }
}