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
    }
}
