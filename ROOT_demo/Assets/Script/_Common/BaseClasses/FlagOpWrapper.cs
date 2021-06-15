using System;

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