using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
    //这个东西比较蛋疼、键盘、手柄类基于按键的操作和鼠标这种基于时序的操作不同。
    //这里有点是两路输入拼在一起感觉、但是为了这个时间系统的解耦、目前就只能这么办。
    [Serializable]
    public struct ActionPack
    {
        public int ActionID => ActionEventData.actionId;
        public InputActionEventData ActionEventData;
        public RotationDirection ActionDirection;
        public Vector2 MouseScreenPosA;
        public Vector2 MouseScreenPosB;
        public int FuncID;//把之前Func0~9的ID都整合到一个compositeAction上面、然后把具体的ID写在这儿。
        public bool HoldForDrag; //set TRUE if space is pressed down; set FALSE if space is up

        public bool IsAction(int actionID)
        {
            return ActionPackIsAction(this, actionID);
        }

        public static bool ActionPackIsAction(ActionPack actPack, int actionID)
        {
            return actPack.ActionID == actionID;
        }
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

        public static WorldEvent.ControllingEventHandler ControllingEvent;

        private static bool holdForDrag = false;
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            player = ReInput.players.GetPlayer(playerId);
            player.AddInputEventDelegate(OnInputUpdateCurser, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "CursorUp");
            player.AddInputEventDelegate(OnInputUpdateCurser, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "CursorDown");
            player.AddInputEventDelegate(OnInputUpdateCurser, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "CursorLeft");
            player.AddInputEventDelegate(OnInputUpdateCurser, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "CursorRight");

            player.AddInputEventDelegate(OnInputUpdateFunc, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Func0");
            player.AddInputEventDelegate(OnInputUpdateFunc, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Func1");
            player.AddInputEventDelegate(OnInputUpdateFunc, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Func2");
            player.AddInputEventDelegate(OnInputUpdateFunc, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Func3");
            player.AddInputEventDelegate(OnInputUpdateFunc, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Func4");
            player.AddInputEventDelegate(OnInputUpdateFunc, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Func5");
            player.AddInputEventDelegate(OnInputUpdateFunc, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Func6");
            player.AddInputEventDelegate(OnInputUpdateFunc, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Func7");
            player.AddInputEventDelegate(OnInputUpdateFunc, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Func8");
            player.AddInputEventDelegate(OnInputUpdateFunc, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Func9");

            player.AddInputEventDelegate(OnInputUpdateSpaceDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Confirm0");
            player.AddInputEventDelegate(OnInputUpdateSpaceUp, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, "Confirm0");
        }

        private void OnInputUpdateCurser(InputActionEventData obj)
        {
            var actionPack = new ActionPack
            {
                //so on......
                ActionEventData = obj,
                HoldForDrag = holdForDrag,
            };
            switch (actionPack.ActionEventData.actionName)
            {
                case "CursorUp":
                    actionPack.ActionDirection = RotationDirection.North;
                    break;
                case "CursorDown":
                    actionPack.ActionDirection = RotationDirection.South;
                    break;
                case "CursorLeft":
                    actionPack.ActionDirection = RotationDirection.West;
                    break;
                case "CursorRight":
                    actionPack.ActionDirection = RotationDirection.East;
                    break;
            }

            ControllingEvent?.Invoke(actionPack);
        }

        private void OnInputUpdateFunc(InputActionEventData obj)
        {
            var actionPack = new ActionPack
            {
                //so on......
                ActionEventData = obj,
                HoldForDrag = holdForDrag,
            };
            actionPack.FuncID = actionPack.ActionEventData.actionName[4] - '0';
            ControllingEvent?.Invoke(actionPack);
        }

        private void OnInputUpdateSpaceDown(InputActionEventData obj)
        {
            holdForDrag = true;
            var actionPack = new ActionPack
            {
                //so on......
                ActionEventData = obj,
                HoldForDrag = holdForDrag,
            };
            ControllingEvent?.Invoke(actionPack);
            Debug.Log("Down");
        }
        private void OnInputUpdateSpaceUp(InputActionEventData obj)
        {
            holdForDrag = false;
            Debug.Log("Up");
        }
    }
}