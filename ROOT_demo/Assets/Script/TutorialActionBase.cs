using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public enum TutorialActionType
    {
        //TODO 开各种状态怎么办啊？
        Text,
        CreateUnit,
        CreateCursor,
        PlayerTry,
        Function,
        End,
    }

    public struct TutorialActionData
    {
        public int ActionIdx;
        public TutorialActionType ActionType;
        public string Text;
        public CoreType Core;
        public SideType[] Sides;
        public Vector2Int Pos;

        public TutorialActionData(int actionIdx=0,
            TutorialActionType actionType=TutorialActionType.Text,
            string text="",
            CoreType core=CoreType.PCB,
            SideType[] sides=null,
            Vector2Int pos=new Vector2Int())
        {
            ActionIdx = actionIdx;
            ActionType = actionType;
            Text = text;
            Core = core;
            Sides = sides;
            Pos = pos;
        }
    }

    public abstract class TutorialActionBase
    {
        public readonly TutorialActionData[] Actions;
        public readonly string Title;

        public abstract void CustomFunction();

        protected TutorialActionBase(string title,TutorialActionData[] actions)
        {
            Actions = actions;
            Title = title;
        }
    }

    /*"你好，欢迎来到R.O.O.T.教程。这是一款基于棋盘的模拟经营游戏。",
    "首先，这个是游戏中最重要的元素，称其为" + TmpBracketAndBold("单元") + "。",
    "然后，这个是你的光标。",
    "我再多放几个单位，可以熟悉一下基本操作。",*/

    public class TutorialAction0 : TutorialActionBase
    {
        public TutorialAction0() : base(
            "Basic Controlling and Concept",
            new[]
            {
                new TutorialActionData(0,TutorialActionType.Text, "你好，欢迎来到R.O.O.T.教程。这是一款基于棋盘的模拟经营游戏。",CoreType.PCB, new SideType[] { }, Vector2Int.zero), 
                new TutorialActionData(1,TutorialActionType.Text, "首先，这个是游戏中最重要的元素，称其为单元",CoreType.PCB, new SideType[] { }, Vector2Int.zero), 
                new TutorialActionData(1,TutorialActionType.CreateUnit, "",CoreType.PCB, new SideType[] { SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}, Vector2Int.zero), 
                new TutorialActionData(2,TutorialActionType.Text, "然后，这个是你的光标。",CoreType.PCB, new SideType[] { }, Vector2Int.zero), 
                new TutorialActionData(2,TutorialActionType.Function, "",CoreType.PCB, new SideType[] { }, Vector2Int.zero), //放光标
                new TutorialActionData(3,TutorialActionType.Text, "我再多放几个单位，可以熟悉一下基本操作。",CoreType.PCB, new SideType[] { }, Vector2Int.zero), 
                new TutorialActionData(3,TutorialActionType.CreateUnit, "",CoreType.HardDrive, new SideType[] { SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}, Vector2Int.zero), 
            })
        {
        }

        public override void CustomFunction()
        {
            throw new NotImplementedException();
        }
    }

    public class TutorialAction1 : TutorialActionBase
    {
        public TutorialAction1() : base(
            "TutorialAction1",
            new[]
            {
                new TutorialActionData(0,TutorialActionType.Text, "你好，欢迎来到R.O.O.T.教程。这是一款基于棋盘的模拟经营游戏。",CoreType.PCB, new SideType[] { }, Vector2Int.zero), 
            })
        {
        }

        public override void CustomFunction()
        {
            throw new NotImplementedException();
        }
    }

    public class TutorialAction2 : TutorialActionBase
    {
        public TutorialAction2() : base(
            "TutorialAction2",
            new[]
            {
                new TutorialActionData(0,TutorialActionType.Text, "你好，欢迎来到R.O.O.T.教程。这是一款基于棋盘的模拟经营游戏。",CoreType.PCB, new SideType[] { }, Vector2Int.zero),
            })
        {
        }

        public override void CustomFunction()
        {
            throw new NotImplementedException();
        }
    }
}