using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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
        public static readonly int SCENE_ID_GAMEPLAY = 1;
        public static readonly int SCENE_ID_GAMEOVER = 2;
        public static readonly int SCENE_ID_TUTORIAL = 3;


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

    public static class Utils
    {
        public static float LastRandom = 0.0f;

        public static T[] Shuffle<T>(T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = (int)(Random.value*(n--));
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

        public static RotationDirection RotateDirectionAfterRotation(RotationDirection direction,RotationDirection rotation)
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
        public static RotationDirection RotateDirectionBeforeRotation(RotationDirection direction, RotationDirection rotation)
        {
            if (direction == RotateDirectionAfterRotation(RotationDirection.North, rotation))return RotationDirection.North;
            if (direction == RotateDirectionAfterRotation(RotationDirection.South, rotation))return RotationDirection.South;
            if (direction == RotateDirectionAfterRotation(RotationDirection.East, rotation))return RotationDirection.East;
            if (direction == RotateDirectionAfterRotation(RotationDirection.West, rotation))return RotationDirection.West;
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
            Dictionary<T, float> _lib=new Dictionary<T, float>();
            Debug.Assert(lib.Length>0);
            foreach (var type in lib)
            {
                _lib.Add(type,1.00f/lib.Length);
            }

            return GenerateWeightedRandom(_lib);
        }


        [CanBeNull]
        public static T GenerateWeightedRandom<T>(Dictionary<T,float> lib)
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
                if (val<= totalWeight)
                {
                    return keyValuePair.Key;
                }
            }
            return default;
        }

        public static string  PaddingFloat4Digit(float input)
        {
            int inputInt = Mathf.FloorToInt(input);
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

        public static string PaddingFloat3Digit(float input)
        {
            int inputInt = Mathf.FloorToInt(input);
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

    public partial class TutorialMgr : MonoBehaviour
    {
        void Awake()
        {
            string TmpColorBlueXml(string content)
            {
                return TmpColorXml(content, Color.blue);
            }

            string TmpColorRedXml(string content)
            {
                return TmpColorXml(content,Color.red);
            }

            string TmpColorXml(string content,Color col)
            {
                var hexCol = ColorUtility.ToHtmlStringRGB(col);
                return "<color=#" + hexCol + "> " + content + " </color> ";
            }

            string TmpColorBold(string content)
            {
                return "<b> " + content + " </b> ";
            }

            _tutorialContent = new[]
            {
                /*000*/"你好，欢迎来到R.O.O.T.教程。",
                /*001*/"这是一个基于棋盘的模拟经营游戏。",
                /*002*/"首先，这个是游戏中最重要的元素，我们称为单位。",
                /*003*/"然后，这个是你的光标。",
                /*004*/"来，再给你几个单位，随便试试先，习惯一下操作。",
                /*005*/"",
                /*006*/"是不是很像水管工啊，当然，有点像。\n水管工中连接了水管，还要有水对吧。",
                /*007*/"这里也是，你目前只处理了物理上的链接，换句话说\n有点像接了网线但是还没网。",
                /*008*/"现在给你打开数据的传输。",
                /*009*/"目前游戏中有两种数据：\n\t【"+TmpColorRedXml("一般数据")+"】和【"+TmpColorBlueXml("网络数据")+"】",//"（希望之后还能有更多",
                /*010*/"既然提到数据，就一定有发射端，和接收端。\n从形状上来看，很容易区别。【方形】是发射端，【圆形】是接收端。",
                /*011*/"除了形状，上面的图案也很重要。\n你已经接触过的是【处理器和硬盘】这一组发射端和接收端。负责"+TmpColorBold("处理")+"【"+TmpColorRedXml("一般数据")+"】",
                /*012*/"来，这时另外一组。\n这组称为【服务器和网线】，负责"+TmpColorBold("处理")+"【"+TmpColorBlueXml("网络数据")+"】",
                /*013*/"话说，信号的提示是不是有点晃眼？先帮你隐藏掉了，按住"+TmpColorBold("TAB")+"可以再显示",
                /*014*/"先来自己试试。"/*（        
                    * 硬盘：只要链接到处理器单元上即可获得数据，并且可以串联。
                    * 网络：只有从服务器中连出的最长一串的网线单元。才能计分，并且只有最长的一串才能获得数据。（显示在外面）
                ）*/,
                /*015*/"",
                /*016*/"虽说需要对应的单元才能"+TmpColorBold("处理")+"对应的数据。但是他们可以传递任意数据。来试试，使用网线模组链接一个硬盘和处理器。",
                /*017*/"此时你就可以看到，虽然网线不能处理【"+TmpColorRedXml("一般数据")+"】，但是可以通过它传递到可以处理【"+TmpColorRedXml("一般数据")+"】的硬盘上。当然，反之亦然。",
                /*018*/"总之，一切单元都可以传递一切数据，但是能否处理某些数据，就是需要特定的对应关系的单元和数据了。",
                /*019*/"知道这个信息后，你的整个网络是不是可以更加紧凑和灵活了呢~？",
                /*020*/"",
                /*021*/"说了这么半天，传递、处理数据有什么用呢？就提到这个游戏的目标了",//【解释面板】
                /*022*/"所有获得数据的单元可以获得金钱（网线一般要比硬盘获得的多一些。）",
                /*0XX*/"好的，教程继续。",
                /*0XX*/"好的，教程继续。",
                /*0XX*/"好的，教程继续。",
                /*0XX*/"好的，教程继续。",
                /*0XX*/"好的，教程继续。",
                /*0XX*/"好的，教程继续。",
            };
        }
    }
}
