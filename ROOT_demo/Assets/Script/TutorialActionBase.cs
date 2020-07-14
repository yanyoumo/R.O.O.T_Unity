using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public enum TutorialActionType
    {
        //各种动作直接打开对应的模块，模块的开关就不独立做了。
        //TODO,怎么结束啊？
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
            Vector2Int pos = new Vector2Int(),
            CoreType core=CoreType.PCB,
            SideType[] sides=null)
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
        public readonly Type LevelLogicType;

        protected TutorialActionBase(string title,Type levelLogicType, TutorialActionData[] actions)
        {
            LevelLogicType = levelLogicType;
            Actions = actions;
            Title = title;
        }

        public TutorialQuadDataPack GetTutorialQuadDataPack => new TutorialQuadDataPack(Title,"Play");
    }

    //这些不要弄ID什么的，最好写名字，要不然插队会很费劲。
    public class TutorialActionBasicControl : TutorialActionBase
    {
        public TutorialActionBasicControl() : base(
            "Basic Control",
            typeof(TutorialLevelBasicControlMgr),
            new[]
            {
                new TutorialActionData(0, TutorialActionType.Text, "你好，欢迎来到R.O.O.T.教程。这是一款基于棋盘的模拟经营游戏。"),
                new TutorialActionData(1, TutorialActionType.Text, "首先，这个是游戏中最重要的元素，称其为单元"),
                new TutorialActionData(1, TutorialActionType.CreateUnit, "", new Vector2Int(2, 3), CoreType.Processor,
                    new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(2, TutorialActionType.Text, "然后，这个是你的光标。"),
                new TutorialActionData(2, TutorialActionType.CreateCursor, "", new Vector2Int(2, 1)), //放光标
                new TutorialActionData(3, TutorialActionType.Text, "使用【方向键】移动，按住空格拖动。左Shift旋转。"),
                new TutorialActionData(4, TutorialActionType.Text, "我再多放几个单位，可以熟悉一下基本操作。"),
                new TutorialActionData(3, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.HardDrive,
                    new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(4, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.HardDrive,
                    new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(4, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.HardDrive,
                    new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(5, TutorialActionType.End),
            })
        {
        }
    }

    public class TutorialActionSignalBasic : TutorialActionBase
    {
        public TutorialActionSignalBasic() : base(
            "Signal Basic",
            typeof(TutorialLevelBasicControlMgr),
            new[]
            {
                new TutorialActionData(0, TutorialActionType.Text, "你好，欢迎来到R.O.O.T.教程。这是一款基于棋盘的模拟经营游戏。"),
                new TutorialActionData(1, TutorialActionType.End),
            })
        {
        }
    }

    public class TutorialActionGoal : TutorialActionBase
    {
        public TutorialActionGoal() : base(
            "Goal",
            typeof(TutorialLevelBasicControlMgr),
            new[]
            {
                new TutorialActionData(0, TutorialActionType.Text, "你好，欢迎来到R.O.O.T.教程。这是一款基于棋盘的模拟经营游戏。"),
                new TutorialActionData(1, TutorialActionType.End),
            })
        {
        }
    }


    public partial class TutorialMasterMgr : MonoBehaviour
    {
        private TutorialActionBase[] tutorialActions;

        private void InitTutorialActions()
        {
            tutorialActions = new TutorialActionBase[]
            {
                new TutorialActionBasicControl(),
                new TutorialActionSignalBasic(),
                new TutorialActionGoal(),
            };
        }
    }

    public partial class LevelMasterManager : MonoBehaviour
    {
        public void LoadLevelThenPlay(Type levelLogicType)
        {
            LoadLevelThenPlay(levelLogicType, new ScoreSet(), new PerMoveData());
        }

        public void LoadLevelThenPlay(Type levelLogicType, ScoreSet nextScoreSet, PerMoveData nextPerMoveData)
        {
            //这里是一个动态到静态的转换。
            if (levelLogicType == typeof(DefaultLevelMgr))
            {
                LoadLevelThenPlay<DefaultLevelMgr>(nextScoreSet, nextPerMoveData);
                return;
            }
            if (levelLogicType == typeof(ShortEndingLevelMgr))
            {
                LoadLevelThenPlay<ShortEndingLevelMgr>(nextScoreSet, nextPerMoveData);
                return;
            }
            if (levelLogicType == typeof(TutorialLevelBasicControlMgr))
            {
                LoadLevelThenPlay<TutorialLevelBasicControlMgr>(nextScoreSet, nextPerMoveData);
                return;
            }
            throw new NotImplementedException();
        }
    }
}