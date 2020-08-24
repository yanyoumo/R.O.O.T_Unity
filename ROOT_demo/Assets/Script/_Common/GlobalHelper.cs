﻿using JetBrains.Annotations;
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
        public static readonly string INPUT_BUTTON_NAME_CYCLENEXT = "CycleNext";

        public static readonly string INPUT_BUTTON_NAME_SHOPBUY1 = "ShopBuy1";
        public static readonly string INPUT_BUTTON_NAME_SHOPBUY2 = "ShopBuy2";
        public static readonly string INPUT_BUTTON_NAME_SHOPBUY3 = "ShopBuy3";
        public static readonly string INPUT_BUTTON_NAME_SHOPBUY4 = "ShopBuy4";
        public static readonly string INPUT_BUTTON_NAME_SHOPCANCELED = "ShopCancel";
        public static readonly string INPUT_BUTTON_NAME_SHOPCONFIRM = "ShopConfirm";
        public static readonly string INPUT_BUTTON_NAME_SHOPRANDOM = "ShopRandom";

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
    /*public static class RootEVENT
    { 
        public delegate void GameMajorEvent();
        public delegate void TutorialStartEvent(TutorialActionBase tutorialAction);
        public delegate void GameStartEvent(ScoreSet scoreSet, PerMoveData _perMoveData,Type gameStateType);
    }*/

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

            Debug.Assert((Mathf.Abs(totalWeight - 1) < 1e-3) && (lib.Count > 0), "totalWeight=" + totalWeight + "||lib.Count=" + lib.Count);
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


    /*public abstract partial class TutorialActionBase
    {
        public static string TmpColorBlueXml(string content)
        {
            return TmpColorXml(content, Color.blue);
        }

        public static string TmpColorGreenXml(string content)
        {
            return TmpColorXml(content, Color.green * 0.35f);
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
            return TmpBracketAndBold(TmpColorGreenXml("一般数据"));
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

            };
        }
    }*/
}
