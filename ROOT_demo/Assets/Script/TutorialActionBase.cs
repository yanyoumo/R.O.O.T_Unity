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
        ShowText,
        HideText,
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
    public abstract partial class TutorialActionBase
    {
        public readonly TutorialActionData[] Actions;
        public readonly string Title;
        public readonly string ThumbnailName;
        public readonly Type LevelLogicType;

        protected TutorialActionBase(string title, string thumbnailName, Type levelLogicType, TutorialActionData[] actions)
        {
            LevelLogicType = levelLogicType;
            Actions = actions;
            Title = title;
            ThumbnailName = thumbnailName;
        }

        public TutorialQuadDataPack TutorialQuadDataPack => new TutorialQuadDataPack(Title, "Play", ThumbnailName);
    }

    //这些不要弄ID什么的，最好写名字，要不然插队会很费劲。
    public class TutorialActionBasicControl : TutorialActionBase
    {
        public TutorialActionBasicControl() : base(
            "Basic Control",
            "Thumbnail_BasicControl",
            typeof(TutorialLevelBasicControlMgr),
            new[]
            {
                new TutorialActionData(0, TutorialActionType.ShowText),
                new TutorialActionData(0, TutorialActionType.Text, "你好，欢迎来到R.O.O.T.教程。这是一款基于棋盘的模拟经营游戏。"),
                new TutorialActionData(1, TutorialActionType.Text, "首先，这个是游戏中最重要的元素，称其为" + TmpBracketAndBold("单元") + ""),
                new TutorialActionData(1, TutorialActionType.CreateUnit, "", new Vector2Int(2, 3), CoreType.Processor,
                    new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(2, TutorialActionType.Text, "然后，这个是你的光标。"),
                new TutorialActionData(2, TutorialActionType.CreateCursor, "", new Vector2Int(2, 3)), //放光标
                new TutorialActionData(3, TutorialActionType.Text, "使用" + TmpBracketAndBold("方向键") + "移动，按住" + TmpBracketAndBold("空格") + "拖动。" + TmpBracketAndBold("左Shift") + "旋转。"),
                new TutorialActionData(4, TutorialActionType.Text, "我再多放几个" + TmpBracketAndBold("单元") + "，可以熟悉一下基本操作。"),
                new TutorialActionData(5, TutorialActionType.HideText),
                new TutorialActionData(5, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.HardDrive,
                    new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(5, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.HardDrive,
                    new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(5, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.HardDrive,
                    new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(6, TutorialActionType.ShowText),
                new TutorialActionData(6, TutorialActionType.Text, "好的，这就是这个游戏的基本操作。"),
                new TutorialActionData(7, TutorialActionType.End),
            })
        {
        }
    }

    public class TutorialActionSignalBasic : TutorialActionBase
    {
        public TutorialActionSignalBasic() : base(
            "Signal Basic",
            "Thumbnail_SignalBasic",
            typeof(TutorialSignalBasicMgr),
            new[]
            {
                new TutorialActionData(0, TutorialActionType.ShowText),
                new TutorialActionData(0, TutorialActionType.Text, "游戏中另一个重要元素就是" + TmpBracketAndBold("数据") + "，目前游戏中有两种数据：" + TMPNormalDataCompo() + "和" + TMPNetworkDataCompo() + "。"),
                new TutorialActionData(1, TutorialActionType.Text, "既然提到数据，就一定有发射端，和接收端。从形状上来看，很容易区别。【方形】是发射端，【圆形】是接收端。"),
                new TutorialActionData(2, TutorialActionType.Text, "当然，除了形状，上面的图案也很重要；你已经接触过的是【处理器和硬盘】这一组发射端和接收端。负责处理一般数据。"),
                new TutorialActionData(2, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.Processor, new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(2, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.HardDrive, new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(2, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.HardDrive, new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.NoConnection}),
                new TutorialActionData(3, TutorialActionType.Text, "现在给你打开" + TMPNormalDataCompo() + "的传输。"),
                new TutorialActionData(4, TutorialActionType.HideText),
                new TutorialActionData(5, TutorialActionType.ShowText),
                new TutorialActionData(5, TutorialActionType.Text, "来，这时另外一组。这组称为" + TmpBracketAndBold("服务器和网线") + "，负责处理" + TMPNetworkDataCompo() + "。"),
                new TutorialActionData(6, TutorialActionType.Text, "话说，信号的提示是不是有点晃眼？先帮你隐藏掉了，按住" + TmpBracketAndBold("TAB") + "可以再显示。"),
                new TutorialActionData(7, TutorialActionType.Text, "你再试试另一类的数据。来，稍微看看数据的特点"),
                new TutorialActionData(8, TutorialActionType.HideText),
                new TutorialActionData(8, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.Server, new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(8, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.NetworkCable, new[] {SideType.NoConnection, SideType.NoConnection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(8, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.NetworkCable, new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(9, TutorialActionType.ShowText),
                new TutorialActionData(9, TutorialActionType.Text, "虽说需要对应的" + TmpBracketAndBold("单元") + "才能处理对应的数据。但是有一点要注意的是：他们可以传递任意数据。"),
                new TutorialActionData(10, TutorialActionType.Text, "来试试，使用网线模组链接一个硬盘和处理器。"),
                new TutorialActionData(11, TutorialActionType.HideText),
                new TutorialActionData(12, TutorialActionType.ShowText),
                new TutorialActionData(12, TutorialActionType.Text, "总之，一切" + TmpBracketAndBold("单元") + "都可以传递一切数据，但是能否处理某些数据，就是需要特定的对应关系的单元和数据了。"),
                new TutorialActionData(13, TutorialActionType.Text, "这么处理后，你的整个网络是不是可以更加紧凑和灵活了呢~？"),
                new TutorialActionData(14, TutorialActionType.Text, "这就是这个游戏中信号的基本了。"),
                new TutorialActionData(15, TutorialActionType.End),
            })
        {
        }
    }

    public class TutorialActionGoalAndCycle : TutorialActionBase
    {
        public TutorialActionGoalAndCycle() : base(
            "Goal/Cycle",
            "Thumbnail_GoalCycle",
            typeof(TutorialGoalAndCycleMgr),
            new[]
            {
                new TutorialActionData(0, TutorialActionType.ShowText),
                new TutorialActionData(0, TutorialActionType.Text, "说了这么半天，传递、处理数据有什么用呢？就要说回游戏的目标了。"),
                new TutorialActionData(1, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.Server, new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(1, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.NetworkCable, new[] {SideType.NoConnection, SideType.NoConnection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(1, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.NetworkCable, new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(1, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.Processor, new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(1, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.HardDrive, new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(1, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.HardDrive, new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.NoConnection}),
                new TutorialActionData(1, TutorialActionType.Text, "咱们先往棋盘上放一些东西。"),
                new TutorialActionData(2, TutorialActionType.Text, "顺带说，之后的指引和正式游戏中随时可以按动H键来显示操作提示。"),
                new TutorialActionData(3, TutorialActionType.Text, "用现有的单元先拼出一个基本的网络试试~"),
                new TutorialActionData(4, TutorialActionType.HideText),
                new TutorialActionData(5, TutorialActionType.ShowText),
                new TutorialActionData(5, TutorialActionType.Text, "此时看到右上侧，依次显示的是你的【现有金钱】【收入/损失】【剩余时间】。既然称为模拟游戏，这个游戏的目的还是赚钱。（当然，更不能破产）"),
                new TutorialActionData(6, TutorialActionType.Text, "只要一个单元在处理对应的信号，那么这个单元就会产生收入。如果你的收入大于成本，那么就能赚钱了。"),
                new TutorialActionData(7, TutorialActionType.Text, "成本哪里来？每个单元在棋盘上，就会有一个运营成本，你的总收入只要高于总成本就有收益了。这就是为什么【" + TmpColorXml("收入", Color.green * 0.8f) + "/" +TmpColorRedXml("损失") + "】分不同颜色。"),
                new TutorialActionData(8, TutorialActionType.Text, "话说，一般人上班都是一个月拿一次工资。这个收益也是以" + TmpBracketAndBold("周期") + "计算的；这里是游戏最后一个重要元素" + TmpBracketAndBold("周期") +
                                                                   "。"),
                new TutorialActionData(9, TutorialActionType.Text, "这个游戏的所有关键结算都是以" + TmpBracketAndBold("周期") + "计算的，你目前只接触了收益这一个，之后还会有别的。"),
                new TutorialActionData(10, TutorialActionType.Text, TmpBracketAndBold("周期") + "的演进其实不是全自动的，是由你【移动一次单位】而触发的半自动方式。"),
                new TutorialActionData(11, TutorialActionType.Text, "你只要不动单位，游戏就算是静止的，所以可以慢慢思考。啊对，旋转单位不算，可以随便转。"),
                new TutorialActionData(12, TutorialActionType.Text, "这就是游戏的目标"),
                new TutorialActionData(13, TutorialActionType.End),
            })
        {
        }
    }

    public class TutorialActionShop : TutorialActionBase
    {
        public TutorialActionShop() : base(
            "About Shop",
            "Thumbnail_Shop",
            typeof(TutorialShopMgr),
            new[]
            {
                new TutorialActionData(0, TutorialActionType.ShowText),
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
                new TutorialActionGoalAndCycle(),
                new TutorialActionShop(),
            };
        }
    }
    public sealed partial class LevelMasterManager : MonoBehaviour
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
            if (levelLogicType == typeof(TutorialSignalBasicMgr))
            {
                LoadLevelThenPlay<TutorialSignalBasicMgr>(nextScoreSet, nextPerMoveData);
                return;
            }
            if (levelLogicType == typeof(TutorialGoalAndCycleMgr))
            {
                LoadLevelThenPlay<TutorialGoalAndCycleMgr>(nextScoreSet, nextPerMoveData);
                return;
            }
            if (levelLogicType == typeof(TutorialShopMgr))
            {
                LoadLevelThenPlay<TutorialShopMgr>(nextScoreSet, nextPerMoveData);
                return;
            }
            throw new NotImplementedException();
        }
    }
}