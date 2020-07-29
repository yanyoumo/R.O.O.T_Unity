using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
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
        /*PlayerTry,
        Function,*/
        End,
    }

    [Serializable]
    public struct TutorialActionData
    {
        public int ActionIdx;
        [EnumToggleButtons]
        public TutorialActionType ActionType;
        [ShowIf("ActionType", TutorialActionType.Text)]
        public string Text;
        [ShowIf("ActionType", TutorialActionType.CreateUnit)]
        [ShowIf("@this.ActionType==TutorialActionType.CreateUnit||this.ActionType==TutorialActionType.CreateCursor")]
        public Vector2Int Pos;
        [ShowIf("ActionType", TutorialActionType.CreateUnit)]
        public CoreType Core;
        [ShowIf("ActionType", TutorialActionType.CreateUnit)]
        public SideType[] Sides;

        [StringFormatMethod("actionIdx")]
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
        public readonly string TitleTerm;
        public readonly string ThumbnailName;
        public readonly Type LevelLogicType;

        protected TutorialActionBase(string titleTerm, string thumbnailName, Type levelLogicType, TutorialActionData[] actions)
        {
            LevelLogicType = levelLogicType;
            Actions = actions;
            TitleTerm = titleTerm;
            ThumbnailName = thumbnailName;
        }

        public TutorialQuadDataPack TutorialQuadDataPack => new TutorialQuadDataPack(TitleTerm, "Play", ThumbnailName);
    }

    /// TODO tutorial开始的部分加一个加载进度条。
    /// TODO Tutorial文字的部分要对【H】键有一个对应

    //这些不要弄ID什么的，最好写名字，要不然插队会很费劲。
    public class TutorialActionBasicControl : TutorialActionBase
    {
        public TutorialActionBasicControl() : base(
            ScriptTerms.TutorialBasicControl,
            "Thumbnail_BasicControl",
            typeof(TutorialLevelBasicControlMgr),
            new[]
            {
                new TutorialActionData(0, TutorialActionType.ShowText),
                new TutorialActionData(0, TutorialActionType.Text, "你好,欢迎来到<b>[ROOT]</b>的教程;这是一款基于棋盘的模拟经营类游戏"),
                new TutorialActionData(1, TutorialActionType.Text, "首先,这个是"+TmpBracketAndBold("单元")+",游戏中最重要的元素之一"),
                new TutorialActionData(1, TutorialActionType.CreateUnit, "", new Vector2Int(2, 1), CoreType.Processor,new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(2, TutorialActionType.Text, "然后,这个是你的光标"),
                new TutorialActionData(2, TutorialActionType.CreateCursor, "", new Vector2Int(3, 2)),
                new TutorialActionData(3, TutorialActionType.Text, "使用" + TmpBracketAndBold("方向键") + "移动,按住" + TmpBracketAndBold("空格") + "拖动"+TmpBracketAndBold("单元")+",\n" + TmpBracketAndBold("左Shift") + "旋转"),
                new TutorialActionData(4, TutorialActionType.Text, "话说，在之后和正式游戏中随时可以按住<b>[H]</b>键来显示操作提示。"),
                new TutorialActionData(5, TutorialActionType.Text, "我再多放几个" + TmpBracketAndBold("单元") + ",熟悉一下基本操作"),
                new TutorialActionData(6, TutorialActionType.HideText),
                new TutorialActionData(6, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.HardDrive,new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(6, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.HardDrive,new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(6, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.HardDrive,new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(7, TutorialActionType.ShowText),
                new TutorialActionData(7, TutorialActionType.Text, "以上,这就是这个游戏的基本操作"),
                new TutorialActionData(8, TutorialActionType.End),
            })
        {
        }
    }

    public class TutorialActionBasicControlTouch : TutorialActionBase
    {
        public TutorialActionBasicControlTouch() : base(
            ScriptTerms.TutorialBasicControl,
            "Thumbnail_BasicControl_Touch",
            typeof(TutorialLevelBasicControlMgr),
            new[]
            {
                new TutorialActionData(0, TutorialActionType.ShowText),
                new TutorialActionData(0, TutorialActionType.Text, "你好,欢迎来到<b>[ROOT]</b>的教程;这是一款基于棋盘的模拟经营类游戏"),
                new TutorialActionData(1, TutorialActionType.Text, "首先,这个是"+TmpBracketAndBold("单元")+",游戏中最重要的元素之一"),
                new TutorialActionData(1, TutorialActionType.CreateUnit, "", new Vector2Int(2, 1), CoreType.Processor,new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(2, TutorialActionType.Text, "通过触摸屏就可以操作"+TmpBracketAndBold("单元")),
                new TutorialActionData(2, TutorialActionType.CreateCursor, "", new Vector2Int(3, 2)),//先这样……
                new TutorialActionData(3, TutorialActionType.Text, "拖动就可以移动"+TmpBracketAndBold("单元")+",双击就可以旋转"+TmpBracketAndBold("单元")),
                new TutorialActionData(4, TutorialActionType.Text, "话说,在之后和正式游戏中随时可以按住右下角的方框来显示操作提示."),
                new TutorialActionData(5, TutorialActionType.Text, "我再多放几个" + TmpBracketAndBold("单元") + ",熟悉一下基本操作"),
                new TutorialActionData(6, TutorialActionType.HideText),
                new TutorialActionData(6, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.HardDrive,new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(6, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.HardDrive,new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(6, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.HardDrive,new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(7, TutorialActionType.ShowText),
                new TutorialActionData(7, TutorialActionType.Text, "以上,这就是这个游戏的基本操作"),
                new TutorialActionData(8, TutorialActionType.End),
            })
        {
        }
    }

    public class TutorialActionSignalBasic : TutorialActionBase
    {
        public TutorialActionSignalBasic() : base(
            ScriptTerms.TutorialSignalBasic,
            "Thumbnail_SignalBasic",
            typeof(TutorialSignalBasicMgr),
            new[]
            {
                new TutorialActionData(0, TutorialActionType.ShowText),
                new TutorialActionData(0, TutorialActionType.Text, "游戏中另一个重要元素就是" + TmpBracketAndBold("数据") + ",目前游戏中有两种数据:" + TMPNormalDataCompo() + "和" + TMPNetworkDataCompo() + "."),
                new TutorialActionData(1, TutorialActionType.Text, "有数据,就会有发射端及接收端,"+TmpBracketAndBold("方形")+"的"+TmpBracketAndBold("单元")+"是发射端,"+TmpBracketAndBold("圆形")+"的"+TmpBracketAndBold("单元")+"是接收端."),
                new TutorialActionData(1, TutorialActionType.CreateUnit, "", new Vector2Int(0, 1), CoreType.Processor, new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(1, TutorialActionType.CreateUnit, "", new Vector2Int(2, 1), CoreType.HardDrive, new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(1, TutorialActionType.CreateUnit, "", new Vector2Int(3, 2), CoreType.HardDrive, new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.NoConnection}),
                new TutorialActionData(2, TutorialActionType.Text, "除了形状,"+TmpBracketAndBold("单元")+"上的图案也很重要;目前棋盘上的是" + TmpBracketAndBold("处理器和硬盘") + "这一组发射段及接收端,可以处理" + TMPNormalDataCompo() + "."),
                new TutorialActionData(3, TutorialActionType.Text, "现在给你打开" + TMPNormalDataCompo() + "的传输."),
                new TutorialActionData(4, TutorialActionType.HideText),
                new TutorialActionData(5, TutorialActionType.ShowText),
                new TutorialActionData(5, TutorialActionType.Text, "除了这些,还有另外一组,这组称为" + TmpBracketAndBold("服务器和网线") + ",负责处理" + TMPNetworkDataCompo() + "."),
                new TutorialActionData(5, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.Server, new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(5, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.NetworkCable, new[] {SideType.NoConnection, SideType.NoConnection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(5, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.NetworkCable, new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(6, TutorialActionType.Text, "话说,信号的提示是不是有点晃眼?先帮你隐藏掉了,按住" + TmpBracketAndBold("TAB") + "会显示."),
                new TutorialActionData(7, TutorialActionType.Text, "现在有了两组数据,试试同时有两种数据的情况."),
                new TutorialActionData(8, TutorialActionType.HideText),
                new TutorialActionData(9, TutorialActionType.ShowText),
                new TutorialActionData(9, TutorialActionType.Text, "数据需要对应的" + TmpBracketAndBold("单元") + "处理;但是,数据可以通过所有的" + TmpBracketAndBold("单元") + "传递."),
                new TutorialActionData(10, TutorialActionType.Text, "其实可以使用网线模组来链接一对硬盘和处理器,来试试~"),
                new TutorialActionData(11, TutorialActionType.HideText),
                new TutorialActionData(12, TutorialActionType.ShowText),
                new TutorialActionData(12, TutorialActionType.Text, "总之,所有" + TmpBracketAndBold("单元") + "都可以传递两种数据;但是需要特定单元,处理特定数据."),
                new TutorialActionData(13, TutorialActionType.Text, "这么处理后,你的整个网络是不是可以更加紧凑和灵活了呢?"),
                new TutorialActionData(14, TutorialActionType.Text, "这就是这个游戏中信号的基本了."),
                new TutorialActionData(15, TutorialActionType.End),
            })
        {
        }
    }
    public class TutorialActionGoalAndCycle : TutorialActionBase
    {
        public TutorialActionGoalAndCycle() : base(
            ScriptTerms.TutorialGoalAndCycle,
            "Thumbnail_GoalCycle",
            typeof(TutorialGoalAndCycleMgr),
            new[]
            {
                new TutorialActionData(0, TutorialActionType.ShowText),
                new TutorialActionData(0, TutorialActionType.Text, "说了这么半天;传递,处理数据有什么用呢?就要说回游戏的目标了."),
                new TutorialActionData(1, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.Server, new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(1, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.NetworkCable, new[] {SideType.NoConnection, SideType.NoConnection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(1, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.NetworkCable, new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(1, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.Processor, new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(1, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.HardDrive, new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(1, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.HardDrive, new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.NoConnection}),
                new TutorialActionData(1, TutorialActionType.Text, "咱们先往棋盘上放一些东西~"),
                new TutorialActionData(2, TutorialActionType.Text, "然后用棋盘上的单元先拼出一个基本的网络试试~"),
                new TutorialActionData(3, TutorialActionType.HideText),
                new TutorialActionData(4, TutorialActionType.ShowText),
                new TutorialActionData(4, TutorialActionType.Text, "此时看到右上显示器,依次显示的是你的" + TmpBracketAndBold("现有金钱") + TmpBracketAndBold(TmpColorXml("收入", Color.green * 0.6f) + "/" +TmpColorRedXml("损失")) + TmpBracketAndBold("剩余时间") + ";自然,赚钱就是目标（当然，更不能破产）"),
                new TutorialActionData(5, TutorialActionType.Text, "只要一个单元在处理对应的信号,那么这个单元就可以收入;但是每个单元在棋盘上,都会有运营成本"),
                new TutorialActionData(6, TutorialActionType.Text, "简单的数学题,总收入高于总成本就是收益,反过来就是亏损;这也是为什么"+ TmpBracketAndBold(TmpColorXml("收入", Color.green * 0.6f) + "/" +TmpColorRedXml("损失")) +"分不同颜色"),
                new TutorialActionData(7, TutorialActionType.Text, "话说,很多商业上的结算经常根据某个周期的,例如\"一个月一结\";这个游戏中的收益也是以\n" + TmpBracketAndBold("周期") + "计算的;这就是另一重要元素:" + TmpBracketAndBold("周期")),
                new TutorialActionData(8, TutorialActionType.Text, "这个游戏的所有关键结算都是以" + TmpBracketAndBold("周期") + "计算的，你目前只接触了收益这一个，之后还会有别的"),
                new TutorialActionData(9, TutorialActionType.Text, TmpBracketAndBold("周期") + "的演进并不是全自动的,是由\n" + TmpBracketAndBold("每移动一次单位") + "而触发的半自动方式"),
                new TutorialActionData(10, TutorialActionType.Text, "你只要不动单位,游戏就算是静止的,所以可以慢慢思考;"),
                new TutorialActionData(11, TutorialActionType.Text, "但是别忘了,右上的显示器还有第三行数据:\n"+TmpBracketAndBold("剩余时间")+",并且这个就是按照" + TmpBracketAndBold("周期") + "计算的!"),
                new TutorialActionData(12, TutorialActionType.Text, "你整场游戏只有这么多个" + TmpBracketAndBold("周期") + "可以用,等计数变为0后(当然前提是之前你没有破产)那么游戏就会结束,结算你赚了还是赔了"),
                new TutorialActionData(13, TutorialActionType.Text, "这就是游戏的目标和" + TmpBracketAndBold("周期") + "!"),
                new TutorialActionData(14, TutorialActionType.End),
            })
        {
        }
    }
    public class TutorialActionShop : TutorialActionBase
    {
        public TutorialActionShop() : base(
            ScriptTerms.TutorialShop,
            "Thumbnail_Shop",
            typeof(TutorialShopMgr),
            new[]
            {
                new TutorialActionData(0, TutorialActionType.ShowText),
                new TutorialActionData(0, TutorialActionType.Text, "话说回来,赚了钱之后不用只存在银行里;可以按需扩大生产,那就是通过商店"),
                new TutorialActionData(1, TutorialActionType.Text, "右下角的面板就是商店,里面同时会有\n4个" + TmpBracketAndBold("单元") + "可供购买;"),
                new TutorialActionData(2, TutorialActionType.Text, "使用数字键购买,当然,需要按照下面价格支付金钱~"),
                new TutorialActionData(3, TutorialActionType.Text, "来,先买个试试~"),
                new TutorialActionData(4, TutorialActionType.HideText),
                new TutorialActionData(5, TutorialActionType.ShowText),
                new TutorialActionData(5, TutorialActionType.Text, "但是每个" + TmpBracketAndBold("周期") + "你只能购买一次;并且每个\n" + TmpBracketAndBold("周期") + "商店都会刷新"),
                new TutorialActionData(6, TutorialActionType.Text, "商店是这样刷新的:1号位上面的" + TmpBracketAndBold("单元") + "会被移除;后面进行补位,而且越接近1号位,它的价格会越便宜!"),
                new TutorialActionData(7, TutorialActionType.Text, "但是千万不要图便宜就都买下来,别忘了" + TmpBracketAndBold("单元") + "都有运营成本"),
                new TutorialActionData(8, TutorialActionType.Text, "来,再试试吧~"),
                new TutorialActionData(9, TutorialActionType.HideText),
                new TutorialActionData(10, TutorialActionType.ShowText),
                new TutorialActionData(10, TutorialActionType.Text, "这就是这个游戏的商店~"),
                new TutorialActionData(11, TutorialActionType.End),
            })
        {
        }
    }
    public class TutorialActionDestroyer : TutorialActionBase
    {
        public TutorialActionDestroyer() : base(
            ScriptTerms.TutorialDestroyer,
            "Thumbnail_Destroyer",
            typeof(TutorialDestroyerMgr),
            new[]
            {
                new TutorialActionData(0, TutorialActionType.ShowText),
                new TutorialActionData(0, TutorialActionType.Text, "最后,你不会认为你就可以这么安稳的玩下去吧,看这个红色标记!"),
                new TutorialActionData(0, TutorialActionType.CreateUnit, "", new Vector2Int(4, 1), CoreType.Server, new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(0, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.NetworkCable, new[] {SideType.NoConnection, SideType.NoConnection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(0, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.NetworkCable, new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(0, TutorialActionType.CreateUnit, "", new Vector2Int(-1, -1), CoreType.NetworkCable, new[] {SideType.NoConnection, SideType.Connection, SideType.Connection, SideType.Connection}),
                new TutorialActionData(1, TutorialActionType.Text, "几个" + TmpBracketAndBold("周期") + "后,标记下面的单元就会被摧毁"),
                new TutorialActionData(2, TutorialActionType.Text, "黄色状态是提前一" + TmpBracketAndBold("周期") + "的预警"),
                new TutorialActionData(3, TutorialActionType.Text, "当然,即使是红色标记;如果你及时挪开下面的" + TmpBracketAndBold("单元") + ",就可以回避掉"),
                new TutorialActionData(4, TutorialActionType.Text, "来,躲开他!"),
                new TutorialActionData(5, TutorialActionType.HideText),
                new TutorialActionData(6, TutorialActionType.ShowText),
                new TutorialActionData(6, TutorialActionType.Text, "但是,如果原来不在标记下的" + TmpBracketAndBold("单元") + ",你给挪过去,也是会被摧毁的"),
                new TutorialActionData(7, TutorialActionType.Text, "嘛~这么做有什么好处就需要你自己考虑考虑了"),
                new TutorialActionData(8, TutorialActionType.Text, "这种标记在游戏中会随机出现,但是永远都是有预警的;至于怎么利用和回避,就靠你的发挥了!"),
                new TutorialActionData(9, TutorialActionType.End),
            })
        {
        }
    }

    public sealed partial class TutorialMasterMgr : MonoBehaviour
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
                new TutorialActionDestroyer(),
            };

            if (StartGameMgr.UseTouchScreen)
            {
                tutorialActions[0]=new TutorialActionBasicControlTouch();
            }
        }
    }

    public sealed partial class LevelMasterManager : MonoBehaviour
    {
        public void LoadLevelThenPlay(Type levelLogicType)
        {
            LoadLevelThenPlay(levelLogicType, new ScoreSet(), new PerMoveData());
        }

        public void LoadNextTutorialLevelThenPlay(Type levelLogicType)
        {
            LoadLevelThenPlay(GetNextTutorialLevel(levelLogicType), new ScoreSet(), new PerMoveData());
        }

        public static Type GetNextTutorialLevel(Type levelLogicType)
        {
            if (levelLogicType == typeof(TutorialLevelBasicControlMgr))
            {
                return typeof(TutorialSignalBasicMgr);
            }
            if (levelLogicType == typeof(TutorialSignalBasicMgr))
            {
                return typeof(TutorialGoalAndCycleMgr);
            }
            if (levelLogicType == typeof(TutorialGoalAndCycleMgr))
            {
                return typeof(TutorialShopMgr);
            }
            if (levelLogicType == typeof(TutorialShopMgr))
            {
                return typeof(TutorialDestroyerMgr);
            }
            throw new ArgumentOutOfRangeException();
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
            if (levelLogicType == typeof(TutorialDestroyerMgr))
            {
                LoadLevelThenPlay<TutorialDestroyerMgr>(nextScoreSet, nextPerMoveData);
                return;
            }
            throw new NotImplementedException();
        }
    }
}