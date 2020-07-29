using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ROOT
{
    public static class StaticName
    {
        public static readonly string INPUT_BUTTON_NAME_CURSORUP = "CursorUp";
        public static readonly string INPUT_BUTTON_NAME_CURSORDOWN = "CursorDown";
        public static readonly string INPUT_BUTTON_NAME_CURSORLEFT = "CursorLeft";
        public static readonly string INPUT_BUTTON_NAME_CURSORRIGHT = "CursorRight";

        public static readonly string INPUT_BUTTON_NAME_MOVEUNIT = "MoveUnit";
        public static readonly string INPUT_BUTTON_NAME_ROTATEUNIT = "RotateUnit";

        public static readonly string INPUT_BUTTON_NAME_HINTHDD = "HintHDD";
        public static readonly string INPUT_BUTTON_NAME_HINTNET = "HintNetwork";
        public static readonly string INPUT_BUTTON_NAME_HINTCTRL = "HintControl";

        public static readonly string INPUT_BUTTON_NAME_SHOPBUY1 = "ShopBuy1";
        public static readonly string INPUT_BUTTON_NAME_SHOPBUY2 = "ShopBuy2";
        public static readonly string INPUT_BUTTON_NAME_SHOPBUY3 = "ShopBuy3";
        public static readonly string INPUT_BUTTON_NAME_SHOPBUY4 = "ShopBuy4";

        public static readonly string INPUT_BUTTON_NAME_QUIT = "Quit";
        public static readonly string INPUT_BUTTON_NAME_NEXT = "Next";
#if UNITY_EDITOR
        public static readonly string DEBUG_INPUT_BUTTON_NAME_FORCESTEP = "DebugForceStep";
#endif
        //
        public static readonly string NAME_CORE_PCB = "NoConnection";
        public static readonly string NAME_CORE_NETCABLE = "NetworkCable";
        public static readonly string NAME_CORE_SERVER = "Server";
        public static readonly string NAME_CORE_BRIDGE = "Bridge";
        public static readonly string NAME_CORE_DRIVER = "HardDrive";
        public static readonly string NAME_CORE_CPU = "Processor";
        public static readonly string NAME_CORE_COOLER = "Cooler";

        public static readonly string NAME_CORE_BACKPLATE = "BackPlate";

        //
        public static readonly int SCENE_ID_START = 0;
        public static readonly int SCENE_ID_LEVELMASTER = 1;
        public static readonly int SCENE_ID_GAMEOVER = 2;
        public static readonly int SCENE_ID_ADDTIVEVISUAL = 3;
        public static readonly int SCENE_ID_ADDTIVELOGIC = 4;
        public static readonly int SCENE_ID_TUTORIAL = 5;


        public static readonly string SOURCE_CORE_NODE_NAME = "SourceCore";
        public static readonly string DEST_CORE_NODE_NAME = "DestCore";

        public static readonly string CORE_MESH_MASTER_NODE_NAME = "CoreMesh";

        public static readonly string SOURCE_CONNECTOR_MASTER_NODE_NAME = "SourceConnector";
        public static readonly string DEST_CONNECTOR_MASTER_NODE_NAME = "DestConnector";

        public static readonly string LOCAL_NORTH_SIDE_MESH_RENDERER_NAME = "LocalNorthSide";
        public static readonly string LOCAL_EAST_SIDE_MESH_RENDERER_NAME = "LocalEastSide";
        public static readonly string LOCAL_SOUTH_SIDE_MESH_RENDERER_NAME = "LocalSouthSide";
        public static readonly string LOCAL_WEST_SIDE_MESH_RENDERER_NAME = "LocalWestSide";
    }
    public static class RootEVENT
    { 
        public delegate void GameMajorEvent();
        public delegate void TutorialStartEvent(TutorialActionBase tutorialAction);
        public delegate void GameStartEvent(ScoreSet scoreSet, PerMoveData _perMoveData,Type gameStateType);
    }

    public static class Utils
    {
        public static float LastRandom = 0.0f;

        public static T[] Shuffle<T>(T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = (int) (Random.value * (n--));
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }

            return array;
        }

        public static RotationDirection GetInvertDirection(RotationDirection orgRotationDirection)
        {
            switch (orgRotationDirection)
            {
                case RotationDirection.North:
                    return RotationDirection.South;
                case RotationDirection.East:
                    return RotationDirection.West;
                case RotationDirection.West:
                    return RotationDirection.East;
                case RotationDirection.South:
                    return RotationDirection.North;
                default:
                    throw new ArgumentOutOfRangeException(nameof(orgRotationDirection), orgRotationDirection, null);
            }
        }

        public static RotationDirection GetCWDirection(RotationDirection orgRotationDirection)
        {
            switch (orgRotationDirection)
            {
                case RotationDirection.North:
                    return RotationDirection.East;
                case RotationDirection.East:
                    return RotationDirection.South;
                case RotationDirection.West:
                    return RotationDirection.North;
                case RotationDirection.South:
                    return RotationDirection.West;
                default:
                    throw new ArgumentOutOfRangeException(nameof(orgRotationDirection), orgRotationDirection, null);
            }
        }

        public static RotationDirection GetCCWDirection(RotationDirection orgRotationDirection)
        {
            switch (orgRotationDirection)
            {
                case RotationDirection.North:
                    return RotationDirection.West;
                case RotationDirection.East:
                    return RotationDirection.North;
                case RotationDirection.West:
                    return RotationDirection.South;
                case RotationDirection.South:
                    return RotationDirection.East;
                default:
                    throw new ArgumentOutOfRangeException(nameof(orgRotationDirection), orgRotationDirection, null);
            }
        }

        public static RotationDirection RotateDirectionAfterRotation(RotationDirection direction,
            RotationDirection rotation)
        {
            switch (rotation)
            {
                case RotationDirection.North:
                    return direction;
                case RotationDirection.East:
                    switch (direction)
                    {
                        case RotationDirection.North:
                            return RotationDirection.East;
                        case RotationDirection.East:
                            return RotationDirection.South;
                        case RotationDirection.West:
                            return RotationDirection.North;
                        case RotationDirection.South:
                            return RotationDirection.West;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                    }
                case RotationDirection.West:
                    switch (direction)
                    {
                        case RotationDirection.North:
                            return RotationDirection.West;
                        case RotationDirection.East:
                            return RotationDirection.North;
                        case RotationDirection.West:
                            return RotationDirection.South;
                        case RotationDirection.South:
                            return RotationDirection.East;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                    }
                case RotationDirection.South:
                    switch (direction)
                    {
                        case RotationDirection.North:
                            return RotationDirection.South;
                        case RotationDirection.East:
                            return RotationDirection.West;
                        case RotationDirection.West:
                            return RotationDirection.East;
                        case RotationDirection.South:
                            return RotationDirection.North;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(rotation), rotation, null);
            }
        }

        public static RotationDirection RotateDirectionBeforeRotation(RotationDirection direction,
            RotationDirection rotation)
        {
            if (direction == RotateDirectionAfterRotation(RotationDirection.North, rotation))
                return RotationDirection.North;
            if (direction == RotateDirectionAfterRotation(RotationDirection.South, rotation))
                return RotationDirection.South;
            if (direction == RotateDirectionAfterRotation(RotationDirection.East, rotation))
                return RotationDirection.East;
            if (direction == RotateDirectionAfterRotation(RotationDirection.West, rotation))
                return RotationDirection.West;
            throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }

        public static Vector2Int ConvertDirectionToBoardPosOffset(RotationDirection direction)
        {
            switch (direction)
            {
                case RotationDirection.North:
                    return new Vector2Int(0, 1);
                case RotationDirection.East:
                    return new Vector2Int(1, 0);
                case RotationDirection.West:
                    return new Vector2Int(-1, 0);
                case RotationDirection.South:
                    return new Vector2Int(0, -1);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public static int GetSideCount(SideType side, SideType[] sides)
        {
            return sides.Count(sideType => sideType == side);
        }

        [CanBeNull]
        public static T GenerateWeightedRandom<T>(T[] lib)
        {
            Dictionary<T, float> _lib = new Dictionary<T, float>();
            Debug.Assert(lib.Length > 0);
            foreach (var type in lib)
            {
                _lib.Add(type, 1.00f / lib.Length);
            }

            return GenerateWeightedRandom(_lib);
        }


        [CanBeNull]
        public static T GenerateWeightedRandom<T>(Dictionary<T, float> lib)
        {
            //有这个东西啊，不要小看他，这个很牛逼的。
            //各种分布都可以有的。
            float totalWeight = 0;
            foreach (var weight in lib.Values)
            {
                totalWeight += weight;
            }

            Debug.Assert((Mathf.Abs(totalWeight - 1) < 1e-3) && (lib.Count > 0),
                "totalWeight=" + totalWeight + "||lib.Count=" + lib.Count);
            float val = Random.value;
            LastRandom = val;
            totalWeight = 0;
            foreach (var keyValuePair in lib)
            {
                totalWeight += keyValuePair.Value;
                if (val <= totalWeight)
                {
                    return keyValuePair.Key;
                }
            }

            return default;
        }

        public static string PaddingNum4Digit(float input)
        {
            return PaddingNum4Digit(Mathf.FloorToInt(input));
        }

        public static string PaddingNum4Digit(int input)
        {
            int inputInt = input;
            if (inputInt >= 10000)
            {
                return "????";
            }
            else if (inputInt >= 1000)
            {
                return inputInt.ToString();
            }
            else if (inputInt >= 100)
            {
                return "0" + inputInt;
            }
            else if (inputInt >= 10)
            {
                return "00" + inputInt;
            }
            else
            {
                return "000" + inputInt;
            }
        }

        public static string PaddingNum3Digit(float input)
        {
            return PaddingNum3Digit(Mathf.FloorToInt(input));
        }

        public static string PaddingNum3Digit(int input)
        {
            int inputInt = input;
            if (inputInt >= 1000)
            {
                return "???";
            }
            else if (inputInt >= 100)
            {
                return inputInt.ToString();
            }
            else if (inputInt >= 10)
            {
                return "0" + inputInt;
            }
            else
            {
                return "00" + inputInt;
            }
        }

        public static string PaddingNum2Digit(float input)
        {
            return PaddingNum2Digit(Mathf.FloorToInt(input));
        }

        public static string PaddingNum2Digit(int input)
        {
            int inputInt = input;
            if (inputInt >= 100)
            {
                return "???";
            }
            else if (inputInt >= 10)
            {
                return "" + inputInt;
            }
            else
            {
                return "0" + inputInt;
            }
        }

        public static ConnectionMeshType GetRelationNoConnection(CoreGenre SrcGenre)
        {
            return GetRelationBetweenGenre(SrcGenre, null);
        }


        public static ConnectionMeshType GetRelationBetweenGenre(CoreGenre SrcGenre, CoreGenre? OtherGenre)
        {
            switch (SrcGenre)
            {
                case CoreGenre.Source:
                    return ConnectionMeshType.NoChange;
                case CoreGenre.Destination:
                    if (OtherGenre == null)
                    {
                        return ConnectionMeshType.NoConnectionMesh;
                    }
                    else
                    {
                        if (OtherGenre == CoreGenre.Source)
                        {
                            return ConnectionMeshType.DtSConnectedMesh;
                        }

                        if (OtherGenre == CoreGenre.Destination)
                        {
                            return ConnectionMeshType.DtoDConnectedMesh;
                        }
                    }

                    break;
                case CoreGenre.Support:
                    return ConnectionMeshType.NoChange;
                case CoreGenre.Other:
                    return ConnectionMeshType.NoChange;
                default:
                    throw new ArgumentOutOfRangeException(nameof(SrcGenre), SrcGenre, null);
            }

            return ConnectionMeshType.NoChange;
        }
    }


    public abstract partial class TutorialActionBase
    {
        public static string TmpColorBlueXml(string content)
        {
            return TmpColorXml(content, Color.blue);
        }

        public static string TmpColorRedXml(string content)
        {
            return TmpColorXml(content, Color.red);
        }

        public static string TmpColorXml(string content, Color col)
        {
            var hexCol = ColorUtility.ToHtmlStringRGB(col);
            return "<color=#" + hexCol + ">" + content + "</color>";
        }

        public static string TmpColorBold(string content)
        {
            return "<b>" + content + "</b>";
        }

        public static string TmpBracket(string content)
        {
            return "[" + content + "]";
        }

        public static string TmpBracketAndBold(string content)
        {
            return TmpColorBold("[" + content + "]");
        }

        public static string TMPNormalDataCompo()
        {
            return TmpBracketAndBold(TmpColorRedXml("一般数据"));
        }

        public static string TMPNetworkDataCompo()
        {
            return TmpBracketAndBold(TmpColorBlueXml("网络数据"));
        }
    }

    public sealed partial class TutorialMgr : MonoBehaviour
    {

        string TmpColorBlueXml(string content)
        {
            return TmpColorXml(content, Color.blue);
        }

        string TmpColorRedXml(string content)
        {
            return TmpColorXml(content, Color.red);
        }

        string TmpColorXml(string content, Color col)
        {
            var hexCol = ColorUtility.ToHtmlStringRGB(col);
            return "<color=#" + hexCol + "> " + content + " </color> ";
        }

        string TmpColorBold(string content)
        {
            return "<b> " + content + " </b> ";
        }

        string TmpBracket(string content)
        {
            return "[" + content + "]";
        }

        string TmpBracketAndBold(string content)
        {
            return "[" + TmpColorBold(content) + "]";
        }

        string TMPNormalDataCompo()
        {
            return TmpBracketAndBold(TmpColorRedXml("一般数据"));
        }

        string TMPNetworkDataCompo()
        {
            return TmpBracketAndBold(TmpColorBlueXml("网络数据"));
        }

        void Awake()
        {
            string[] _tutorialContent = new[]
            {
                /*000*/"你好，欢迎来到R.O.O.T.教程。这是一款基于棋盘的模拟经营游戏。",
                /*001*/"首先，这个是游戏中最重要的元素，称其为" + TmpBracketAndBold("单元") + "。",
                /*002*/"然后，这个是你的光标。",
                /*003*/"我再多放几个单位，可以熟悉一下基本操作。",
                /*004*/"",
                /*005*/"好的，你已经学会了操作" + TmpBracketAndBold("单元") + "的基本操作。\n但是，你目前只处理了物理上的链接，换句话说有点像接了网线但是还没网。",
                /*006*/
                "游戏中另一个重要元素就是" + TmpBracketAndBold("数据") + "；目前游戏中有两种数据：" + TMPNormalDataCompo() + "和" +
                TMPNetworkDataCompo() + "。",
                /*007*/"既然提到数据，就一定有发射端，和接收端。\n从形状上来看，很容易区别。【方形】是发射端，【圆形】是接收端。",
                /*008*/"现在给你打开" + TMPNormalDataCompo() + "的传输。",
                /*009*/"",
                /*010*/"当然，除了形状，上面的图案也很重要；\n你已经接触过的是【处理器和硬盘】这一组发射端和接收端。负责处理" + TMPNormalDataCompo() + "。",
                /*011*/"来，这时另外一组。\n这组称为【服务器和网线】，负责处理" + TMPNetworkDataCompo() + "。",
                /*012*/"话说，信号的提示是不是有点晃眼？先帮你隐藏掉了，按住" + TmpColorBold("TAB") + "可以再显示。",
                /*013*/"你再试试另一类的数据。来，稍微看看数据的特点",
                /*014*/"",
                /*015*/"虽说需要对应的" + TmpBracketAndBold("单元") + "才能处理对应的数据。但是有一点要注意的是：他们可以传递任意数据。",
                /*016*/"来试试，使用网线模组链接一个硬盘和处理器。",
                /*017*/"",
                /*018*/"总之，一切单元都可以传递一切数据，但是能否处理某些数据，就是需要特定的对应关系的单元和数据了。\n这么处理后，你的整个网络是不是可以更加紧凑和灵活了呢~？",
                /*019*/"说了这么半天，传递、处理数据有什么用呢？就要说回游戏的目标了。",
                /*020*/"为了显示正常UI，要先把提示隐藏了，在之后的指引和正式游戏中随时可以按动" + TmpBracketAndBold("H") + "键来显示操作提示。",
                /*021*/"看到右上侧，依次显示的是你的【现有金钱】【收入/损失】【剩余时间】。既然称为模拟游戏，这个游戏的目的还是赚钱。（当然，更不能破产）",
                /*022*/"只要一个单元在处理对应的信号，那么这个单元就会产生收入。如果你的收入大于成本，那么就能赚钱了。",
                /*023*/
                "成本哪里来？每个单元在棋盘上，就会有一个运营成本，你的总收入只要高于总成本就有收益了。这就是为什么【" + TmpColorXml("收入", Color.green * 0.8f) + "/" +
                TmpColorRedXml("损失") + "】分不同颜色。",
                /*024*/
                "话说，一般人上班都是一个月拿一次工资。这个收益也是以" + TmpBracketAndBold("周期") + "计算的；这里是游戏最后一个重要元素" + TmpBracketAndBold("周期") +
                "。",
                /*025*/"这个游戏的所有关键结算都是以" + TmpBracketAndBold("周期") + "计算的，你目前只接触了收益这一个，之后还会有别的。",
                /*026*/TmpBracketAndBold("周期") + "的演进其实不是全自动的，是由你【移动一次单位】而触发的半自动方式。",
                /*027*/"你只要不动单位，游戏就算是静止的，所以可以慢慢思考。啊对，旋转单位不算，可以随便转。",
                /*028*/"话说回来，赚了钱之后不用只存在银行里，可以按需扩大生产的，就是通过这里右下角的商店。",
                /*029*/"在商店中，同时会有4个" + TmpBracketAndBold("单元") + "可供购买；",
                /*030*/"使用数字键购买，当然，需要按照下面价格支付金钱~",
                /*031*/"但是每个" + TmpBracketAndBold("周期") + "你只能在商店购买一个" + TmpBracketAndBold("单元") + "。",
                /*032*/"商店刷新的方式是这样的：1号位上面的" + TmpBracketAndBold("单元") + "会被移除。",
                /*033*/ "后面进行补位，而且越接近1号位，它的价格会越便宜。",
                /*034*/"但是千万不要图便宜就都买下来啊，别忘了" + TmpBracketAndBold("单元") + "的运营费用。",
                /*035*/"来，你自己买几个试试吧。",
                /*036*/"",
                /*037*/"最后，你不会认为你就可以这么安稳的玩下去吧，看这个红色标记。",
                /*038*/"若干个" + TmpBracketAndBold("周期") + "后，下面的单元就会被摧毁。",
                /*039*/"黄色状态是提前一" + TmpBracketAndBold("周期") + "的预警。",
                /*040*/"当然，即使是红色标记，如果你及时挪开下面的" + TmpBracketAndBold("单元") + "，就可以回避掉。",
                /*041*/"但是，如果原来不在标记下的" + TmpBracketAndBold("单元") + "，你给挪过去，也是会被摧毁的。",
                /*042*/"嘛，这么做有什么好处就需要你自己考虑考虑了。",
                /*043*/"这种标记在游戏中会随机出现，但是永远都是有预警的。至于怎么利用和回避，就靠你的发挥了。",
                /*044*/"嘛，总之这就是目前全部的系统了，内容还是有点多哈",
                /*045*/"在游戏中，别忘了按" + TmpBracketAndBold("H") + "随时可以显示提示噢。",
                /*046*/"最后再重复一下游戏结束的条件，如果你没有钱了，或者时间没有了，游戏就结束了。",
                /*047*/"如果刚刚开始玩的话，先争取规定时间内不要破产吧；之后在争取尽可能的多赚钱吧~",
            };
        }
    }
}
