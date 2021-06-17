using System;
using UnityEngine;

namespace ROOT
{
    public enum FlagOperation
    {
        //Single
        Is,
        Has,

        //Mutiple
        Set,
        Unset,
        Toggle,
        Mask
    }

    public static class FlagOpWrapper
    {
        public static bool IsFlag<T>(this T FlagA, T FlagB) where T : Enum
        {
            return OperateFlag_Sig(FlagOperation.Is, FlagA, FlagB);
        }

        public static bool HaveFlag<T>(this T FlagA, T FlagB) where T : Enum
        {
            return OperateFlag_Sig(FlagOperation.Has, FlagA, FlagB);
        }

        public static T SetFlag<T>(this T FlagA, T FlagB) where T : Enum
        {
            return OperateFlag_Mut(FlagOperation.Set, FlagA, FlagB);
        }

        public static T UnsetFlag<T>(this T FlagA, T FlagB) where T : Enum
        {
            return OperateFlag_Mut(FlagOperation.Unset, FlagA, FlagB);
        }

        public static T ToggleFlag<T>(this T FlagA, T FlagB) where T : Enum
        {
            return OperateFlag_Mut(FlagOperation.Toggle, FlagA, FlagB);
        }

        public static T MaxPriorityFlag<T>(this T flag) where T : Enum
        {
            var gFlag = Convert.ToUInt64(flag);
            if (gFlag == 0)
            {
                return (T) Enum.ToObject(typeof(T), 0);
            }
            byte needBitCount = 2;
            var flagLib = new[]//mask 和 这个mask没过的bit数量。
            {
                new Tuple<ulong, byte>(0xFFFF000000000000,64),
                new Tuple<ulong, byte>(0xFFFFFFFF00000000,48),
                new Tuple<ulong, byte>(0xFFFFFFFFFFFF0000,32),
                new Tuple<ulong, byte>(0xFFFFFFFFFFFFFF00,16),
                new Tuple<ulong, byte>(0xFFFFFFFFFFFFFFF0,8),
                new Tuple<ulong, byte>(0xFFFFFFFFFFFFFFFC,4),
            };
            foreach (var tuple in flagLib)
            {
                var zeroFlag = gFlag & tuple.Item1;
                if (zeroFlag != 0)
                {
                    needBitCount = tuple.Item2;
                    break;
                }
            }
            return (T) Enum.ToObject(typeof(T), flag.MaxPriorityFlag_Raw(needBitCount));
        }

        private static ulong MaxPriorityFlag_Raw_Core(this ulong flag, byte maxBitCount = 64)
        {
            if (flag == 0)
            {
                return 0;
            }

            var flagA = (ulong) 1 << (maxBitCount - 1);
            Debug.Log(flagA.ToString("X"));
            var bitCount = -1;
            do
            {
                bitCount++;
            } while (((flag << bitCount) & flagA) != flagA);

            return (ulong) (1 << (maxBitCount - bitCount - 1));
        }
        
        //这个是可以用二分法优化一下。套个壳。
        private static T MaxPriorityFlag_Raw<T>(this T flag, byte maxBitCount = 64) where T : Enum
        {
            var res = MaxPriorityFlag_Raw_Core(Convert.ToUInt64(flag), maxBitCount);
            return (T) Enum.ToObject(typeof(T), res);
        }

        private static bool OperateFlag_Sig<T>(FlagOperation ops, T FlagA, T FlagB) where T : Enum
        {
            var intA = Convert.ToUInt64(FlagA);
            var intB = Convert.ToUInt64(FlagB);
            switch (ops)
            {
                case FlagOperation.Has:
                    return HaveFlag(intA, intB);
                case FlagOperation.Is:
                    return IsFlag(intA, intB);
                default:
                    throw new ArgumentOutOfRangeException(nameof(ops), ops, null);
            }
        }

        private static T OperateFlag_Mut<T>(FlagOperation ops, T FlagA, T FlagB) where T : Enum
        {
            var intA = Convert.ToUInt64(FlagA);
            var intB = Convert.ToUInt64(FlagB);
            ulong res = 0;
            switch (ops)
            {
                case FlagOperation.Set:
                    res = _setFlag(intA, intB);
                    break;
                case FlagOperation.Unset:
                    res = _unsetFlag(intA, intB);
                    break;
                case FlagOperation.Toggle:
                    res = _toggleFlag(intA, intB);
                    break;
                case FlagOperation.Mask:
                    res = _maskFlag(intA, intB);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ops), ops, null);
            }

            return (T) Enum.ToObject(typeof(T), res);
        }

        private static bool IsFlag(ulong FlagA, ulong FlagB)
        {
            return FlagA == FlagB;
        }

        private static bool HaveFlag(ulong FlagA, ulong FlagB)
        {
            return (FlagA & FlagB) == FlagB;
        }

        private static ulong _maskFlag(ulong FlagA, ulong FlagB)
        {
            return FlagA & FlagB;
        }

        private static ulong _setFlag(ulong FlagA, ulong FlagB)
        {
            return FlagA | FlagB;
        }

        private static ulong _unsetFlag(ulong FlagA, ulong FlagB)
        {
            return FlagA & ~FlagB;
        }

        private static ulong _toggleFlag(ulong FlagA, ulong FlagB)
        {
            return FlagA ^ FlagB;
        }
    }
}