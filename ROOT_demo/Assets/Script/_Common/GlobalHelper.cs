using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;
// ReSharper disable IdentifierTypo

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
        public static readonly string INPUT_BUTTON_NAME_CYCLENEXT = "CycleNext";

        private static readonly string INPUT_BUTTON_NAME_FUNC0 = "Func0";
        private static readonly string INPUT_BUTTON_NAME_FUNC1 = "Func1";
        private static readonly string INPUT_BUTTON_NAME_FUNC2 = "Func2";
        private static readonly string INPUT_BUTTON_NAME_FUNC3 = "Func3";
        private static readonly string INPUT_BUTTON_NAME_FUNC4 = "Func4";
        private static readonly string INPUT_BUTTON_NAME_FUNC5 = "Func5";
        private static readonly string INPUT_BUTTON_NAME_FUNC6 = "Func6";
        private static readonly string INPUT_BUTTON_NAME_FUNC7 = "Func7";
        private static readonly string INPUT_BUTTON_NAME_FUNC8 = "Func8";
        private static readonly string INPUT_BUTTON_NAME_FUNC9 = "Func9";

        public static readonly string[] INPUT_BUTTON_NAME_SHOPBUYS =
        {
            //这个字符串数组的顺序不能变，它的顺序就是ShopID。
            INPUT_BUTTON_NAME_FUNC1,
            INPUT_BUTTON_NAME_FUNC2,
            INPUT_BUTTON_NAME_FUNC3,
            INPUT_BUTTON_NAME_FUNC4,
            INPUT_BUTTON_NAME_FUNC5,
            INPUT_BUTTON_NAME_FUNC6,
            INPUT_BUTTON_NAME_FUNC7,
            INPUT_BUTTON_NAME_FUNC8,
            INPUT_BUTTON_NAME_FUNC9,
            INPUT_BUTTON_NAME_FUNC0,
        };

        public static readonly string[] INPUT_BUTTON_NAME_SKILLS =
        {
            INPUT_BUTTON_NAME_FUNC1,
            INPUT_BUTTON_NAME_FUNC2,
            INPUT_BUTTON_NAME_FUNC3,
            INPUT_BUTTON_NAME_FUNC4,
            INPUT_BUTTON_NAME_FUNC5,
            INPUT_BUTTON_NAME_FUNC6,
            INPUT_BUTTON_NAME_FUNC7,
            INPUT_BUTTON_NAME_FUNC8,
            INPUT_BUTTON_NAME_FUNC9,
            INPUT_BUTTON_NAME_FUNC0,
        };


        public static readonly string INPUT_BUTTON_NAME_SHOPCANCELED = "ShopCancel";
        public static readonly string INPUT_BUTTON_NAME_CONFIRM = "Confirm";
        public static readonly string INPUT_BUTTON_NAME_SHOPRANDOM = "ShopRandom";
        public static readonly string INPUT_BUTTON_NAME_REMOVEUNIT = "RemoveUnit";

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
        public static readonly int SCENE_ID_CAREER = 6;


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
        /// TODO Digong
        /// <summary>
        /// 目的：提供一个棋盘上的坐标作为center以及半径，返回由坐标构成，像素化的圆。
        /// 并且随机选择一个圆上的位置。这个随机过程符合二维正态分布。
        /// 如果圆被棋盘边界阻挡，那么可能需要返回半圆或四分之一圆等残圆。
        /// </summary>
        /// <param name="center">输入的棋盘位置。</param>
        /// <param name="radius">像素圆的半径。</param>
        /// <param name="s_div">随机选择过程的标准差。</param>
        /// <param name="boardLength">棋盘宽度。</param>
        /// <param name="selected">所选结果在return中的index。</param>
        /// <returns>构成像素圆全部像素的坐标的Array。</returns>
        /// 生成圆形的pattern可以参考网页：https://donatstudios.com/PixelCircleGenerator
        ///     里面输入的Height/Width是直径，因为是像素化的圆，那里的直径是函数中的：radius*2+1.
        public static List<Vector2Int> PosisionRandomazation_NormalDistro(in Vector2Int center, in int radius, in float s_div, in int boardLength, out int selected)
        {
            int[][] mask = PixelCircleMask.GenerateMask(radius);
            var len = 2 * radius + 1;
            var possibility = new Dictionary<int, float>();
            var res = new List<Vector2Int>();
            var sum = 0f;
            for (int i = 0; i < len; ++i)
            {
                for (int j = 0; j < len; ++j)
                {
                    if (mask[i][j] == 1)
                    {
                        int x = i - radius, y = j - radius;
                        var now = new Vector2Int(x + center.x, y + center.y);
                        if (IsInBoard(now, boardLength))
                        {
                            sum += (possibility[res.Count] = (float)TwoDimensionalGaussianDistribution(x, y, s_div));
                            res.Add(now);
                        }
                    }
                }
            }
            //normalize
            for (int i = 0; i < res.Count; ++i)
            {
                possibility[i] *= 1 / sum;
            }
            selected = GenerateWeightedRandom(possibility);
            return res;
        }

        public static bool IsInBoard(Vector2Int pos, int len)
        {
            return (pos.x >= 0) && (pos.y >= 0) && (pos.x < len) && (pos.y < len);
        }
        //using two dimensional gaussian distribution at point(x,y) as possibility of chunk (x,y)
        public static double TwoDimensionalGaussianDistribution(in int x, in int y, in float s_div)
        {
            double s_div2 = 1.0 * s_div * s_div;
            return (2 * Math.PI * s_div2) * Math.Exp(-0.5 * (x * x / s_div2 + y * y / s_div2));
        }
        public static List<Vector2Int> PositionRandomization_Dummy(
            in Vector2Int center, in int radius,
            in float s_div, in int boardLength, out int selected)
        {
            var res = new List<Vector2Int>();
            res.Add(center);
            res.Add(center + Vector2Int.left);
            res.Add(center + Vector2Int.right);
            res.Add(center + Vector2Int.up);
            res.Add(center + Vector2Int.down);
            selected = Random.Range(0, res.Count() - 1);
            return res;
        }

        public static float EaseInOutCubic(float x)
        {
            return x < 0.5f ? 4.0f * x * x * x : 1.0f - Mathf.Pow(-2.0f * x + 2.0f, 3.0f) / 2.0f;
        }

        public static float LastRandom = 0.0f;

        public static T RandomItem<T>(IEnumerable<T> enumerable)
        {
            return enumerable.ElementAt(Random.Range(0, enumerable.Count() - 1));
        }

        /// <summary>
        /// Array的洗牌函数，将一个Array的内容重组。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">需要被洗牌的数组</param>
        /// <returns>进行洗牌完毕的数组</returns>
        public static T[] Shuffle<T>(T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = (int)(Random.value * (n--));
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }

            return array;
        }

        public static int UnrollVector2Int(Vector2Int pos, int width)
        {
            return width * pos.x + pos.y;
        }

        public static int GetUnitTierInt(Unit now)
        {
            return Mathf.RoundToInt(ROOT.ShopMgr.TierMultiplier(now.Tier).Item1);
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

        /// <summary>
        /// 将一个整数尽可能以目标计数以整数平均分解
        /// </summary>
        /// <param name="targetCount">目标计数，大于等于1</param>
        /// <param name="maxCount">总数，大于等于目标计数</param>
        /// <param name="sum">将切分结果求和积分结果，最后一个数值就是总数</param>
        /// <returns>将总数按照计数切分的结果</returns>
        public static int[] SpreadOutLaying(int targetCount, int maxCount, out int[] sum)
        {

            if (targetCount < 1 || targetCount > maxCount)
            {
                throw new ArgumentException("目标计数，大于等于1;总数，大于等于目标计数");
            }

            sum = new int[targetCount];
            var resDiv = new int[targetCount];
            var baseInterval = maxCount / targetCount;
            var residue = maxCount - (baseInterval * targetCount);
            for (int i = 0; i < targetCount; i++)
            {
                int interval = baseInterval;
                if (targetCount - i <= residue)
                {
                    interval++;
                }
                resDiv[i] = interval;
            }

            for (var i = 0; i < sum.Length; i++)
            {
                for (var j = 0; j <= i; j++)
                {
                    sum[i] += resDiv[j];
                }
            }

            return resDiv;
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
            //有这个东西啊，不要小看他，这个很牛逼的；各种分布都可以有的。
            float totalWeight = 0;
            foreach (var weight in lib.Values)
            {
                totalWeight += weight;
            }

            Debug.Assert((Mathf.Abs(totalWeight - 1) < 1e-3) && (lib.Count > 0), "totalWeight=" + totalWeight + "||lib.Count=" + lib.Count);
            var val = Random.value;
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

        public static string PaddingNum(int input, int digit)
        {
            switch (digit)
            {
                case 4:
                    return PaddingNum4Digit(input);
                case 3:
                    return PaddingNum3Digit(input);
                case 2:
                    return PaddingNum2Digit(input);
            }
            throw new ArgumentException();
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
}
