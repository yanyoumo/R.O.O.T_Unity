using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ROOT.Common
{
    public static class Utils
    {
        public static void NormalizeDicVal<T>(ref Dictionary<T, float> lib)
        {
            float totalWeight = 0;
            foreach (var weight in lib.Values)
            {
                totalWeight += weight;
            }

            if (!(Mathf.Abs(totalWeight - 1) < 1e-3))
            {
                var keys = lib.Keys.ToArray().Clone() as T[];
                foreach (var coreType in keys)
                {
                    lib[coreType] /= totalWeight;
                }
            }
        }

        
        public static Quaternion RotationToQuaternion(RotationDirection direction)
        {
            switch (direction)
            {
                case RotationDirection.North:
                    return Quaternion.Euler(0, 0, 0);
                case RotationDirection.East:
                    return Quaternion.Euler(0, 90, 0);
                case RotationDirection.West:
                    return Quaternion.Euler(0, 270, 0);
                default:
                    return Quaternion.Euler(0, 180, 0);
            }
        }
        
        public static Vector2Int _V2ToV2Int(Vector2 pos) => new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));

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

        public static string PaddingNum4Digit(int input) => input >= 10000 ? "????" : input.ToString("D4");
        public static string PaddingNum3Digit(int input) => input >= 1000 ? "???" : input.ToString("D3");
        public static string PaddingNum2Digit(int input) => input >= 100 ? "??" : input.ToString("D2");
        
        public static readonly RotationDirection[] ROTATION_LIST =
        {
            RotationDirection.East,
            RotationDirection.North,
            RotationDirection.South,
            RotationDirection.West
        };

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
    }
}