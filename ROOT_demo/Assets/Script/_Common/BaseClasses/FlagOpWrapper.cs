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
        public static bool IsFlag<T>(T FlagA, T FlagB) where T : IConvertible
        {
            return OperateFlag_Sig(FlagOperation.Is, FlagA, FlagB);
        }

        public static bool HasFlag<T>(T FlagA, T FlagB) where T : IConvertible
        {
            return OperateFlag_Sig(FlagOperation.Has, FlagA, FlagB);
        }

        public static T SetFlag<T>(T FlagA, T FlagB) where T : IConvertible
        {
            return OperateFlag_Mut(FlagOperation.Set, FlagA, FlagB);
        }

        public static T UnsetFlag<T>(T FlagA, T FlagB) where T : IConvertible
        {
            return OperateFlag_Mut(FlagOperation.Unset, FlagA, FlagB);
        }

        public static T ToggleFlag<T>(T FlagA, T FlagB) where T : IConvertible
        {
            return OperateFlag_Mut(FlagOperation.Toggle, FlagA, FlagB);
        }

        private static bool OperateFlag_Sig<T>(FlagOperation ops, T FlagA, T FlagB) where T : IConvertible
        {
            var intA = Convert.ToInt32(FlagA);
            var intB = Convert.ToInt32(FlagB);
            switch (ops)
            {
                case FlagOperation.Has:
                    return HasFlag(intA, intB);
                case FlagOperation.Is:
                    return IsFlag(intA, intB);
                default:
                    throw new ArgumentOutOfRangeException(nameof(ops), ops, null);
            }
        }

        private static T OperateFlag_Mut<T>(FlagOperation ops, T FlagA, T FlagB) where T : IConvertible
        {
            var intA = Convert.ToInt32(FlagA);
            var intB = Convert.ToInt32(FlagA);
            var res = 0;
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

            return (T) Convert.ChangeType(res, typeof(T));
        }

        private static bool IsFlag(int FlagA, int FlagB)
        {
            return FlagA == FlagB;
        }

        private static bool HasFlag(int FlagA, int FlagB)
        {
            return (FlagA & FlagB) == FlagB;
        }

        private static int _maskFlag(int FlagA, int FlagB)
        {
            return FlagA & FlagB;
        }

        private static int _setFlag(int FlagA, int FlagB)
        {
            return FlagA | FlagB;
        }

        private static int _unsetFlag(int FlagA, int FlagB)
        {
            return FlagA & ~FlagB;
        }

        private static int _toggleFlag(int FlagA, int FlagB)
        {
            return FlagA ^ FlagB;
        }
    }
}