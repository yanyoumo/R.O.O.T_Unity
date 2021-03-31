using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ROOT.Consts;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

// ReSharper disable IdentifierTypo

namespace ROOT
{
    //partial这个关键字只能在同一个ASM里面用
    public static partial class Utils
    {

    }
}

namespace ROOT
{
    public static partial class Utils
    {
        public static int RoundToFive(decimal val) => Mathf.RoundToInt((float) val / 5.0f) * 5;
        public static float GetCustomizedDistance(Vector2 from, Vector2 to)
        {
            var width = Screen.width;
            var height = Screen.height;
            from.x /= width;
            from.y /= height;
            to.x /= width;
            to.y /= height;
            return (from - to).magnitude;
        }
        public static bool IsOnGrid(RaycastHit hitInfo)
        {
            return Board.WorldPosToXZGrid(hitInfo.point).HasValue;
        }
        public static Unit GetUnit(GameObject pressObj)
        {
            return TryFindUnitInUpperHirearchy(pressObj);
        }

        public static bool IsUnit(GameObject pressObj)
        {
            return pressObj != null && GetUnit(pressObj) != null;
        }

        public static bool IsBoardUint(GameObject pressObj)
        {
            return IsUnit(pressObj) && GetUnit(pressObj).ShopID == -1;
        }
        public static bool IsSkillPalette(GameObject pressObj)
        {
            return pressObj != null && pressObj.CompareTag(StaticTagName.TAG_NAME_SKILL_PALETTE);
        }

        public static SkillPalette GetSkillPalette(GameObject pressObj)
        {
            return pressObj.GetComponent<SkillPalette>();
        }

        public static int SignalChannelSplit(float a, float b, int n, float x)
        {
            Debug.Assert(n >= 0);
            return Mathf.FloorToInt(((x - a) / (b - a)) * n);
        }

        public static bool HasUnitInUpperHirearchy(GameObject go)
        {
            return TryFindUnitInUpperHirearchy(go) != null;
        }

        public static Unit TryFindUnitInUpperHirearchy(GameObject go)
        {
            var tmpGo = go;
            do
            {
                var tmpUnit = tmpGo.GetComponent<Unit>();
                if (tmpUnit != null)
                {
                    return tmpUnit;
                }

                if (tmpGo.transform.parent == null)
                {
                    return null;
                }
                var tmpTmpGo = tmpGo.transform.parent.gameObject;
                if (tmpTmpGo == tmpGo)
                {
                    return null;
                }

                tmpGo = tmpTmpGo;
            } while (true);
        }

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
        public static List<Vector2Int> PositionRandomization_NormalDistro(
            in Vector2Int center, in int radius,
            in float s_div, in int boardLength,
            out int selected)
        {
            //考虑想辙把possibility也传出来？
            if (radius == 0)
            {
                var res0 = new List<Vector2Int>();
                res0.Add(center);
                selected = 0;
                return res0;
            }

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

        public static void DebugLogArray(int[] Array)
        {
            var res = "[";
            foreach (var i in Array)
            {
                res += i.ToString() + ",";
            }
            res += "]";
            Debug.Log(res);
        }

        public static int GetSideCount(SideType side, SideType[] sides)
        {
            return sides.Count(sideType => sideType == side);
        }

        /// <summary>
        /// 根据指定波动，生成特定随机数列，但是保证整个数组和为0。
        /// </summary>
        /// <param name="length">长度</param>
        /// <param name="variation">指定波动</param>
        /// <returns>随机数列</returns>
        public static int[] SumZeroRandomArray(int length, int variation)
        {
            var res = new int[length];
            for (int i = 0; i < length; i++)
            {
                res[i] = Random.Range(-variation, variation);
            }
            var sum = res.Sum();
            if (sum != 0)
            {
                if (Mathf.Abs(sum) < length)
                {
                    var sign = Mathf.Sign(sum) > 0;
                    var absSum = Mathf.Abs(sum);
                    for (var i = 0; i < absSum; i++)
                    {
                        //如果sum数值小于总数，就将前sum个数量的值中都敲掉1就行了。
                        res[i] = res[i] - (sign ? 1 : -1);
                    }
                }
                else
                {
                    var sign = Mathf.Sign(sum) > 0;
                    var absSum = Mathf.Abs(sum);
                    var offset = SpreadOutLaying(length, absSum, out var sum1);
                    for (var i = 0; i < length; i++)
                    {
                        //是减法，是因为需要和去掉sum的数量。
                        res[i] = res[i] - (sign ? 1 : -1) * offset[i];
                    }
                }

                Debug.Assert(res.Sum() == 0, "sum not Zero!");
            }

            return res;
        }

        /// <summary>
        /// 将一个整数以特定数量切分，并且每个数值可以会有特定的随机波动。
        /// </summary>
        /// <param name="length">目标计数，大于等于1</param>
        /// <param name="sum">总数，大于等于目标计数</param>
        /// <param name="variationRatio">随机波动的强度：0~1</param>
        /// <returns>将总数按照计数随机切分的结果</returns>
        public static int[] SpreadOutLayingWRandomization(int length, int sum, float variationRatio)
        {
            var averagedList = SpreadOutLaying(length, sum, out var avgSum);
            var maxElement = averagedList.Select(Mathf.Abs).Max();
            var variation = Mathf.RoundToInt(maxElement * variationRatio);
            var offset = SumZeroRandomArray(length, variation);
            var res = new int[length];
            for (int i = 0; i < length; i++)
            {
                res[i] = averagedList[i] + offset[i];
            }

            Debug.Assert(res.Sum() == sum);
            return res;
        }

        /// <summary>
        /// 将一个整数尽可能以目标计数以整数平均分解
        /// </summary>
        /// <param name="length">目标计数，大于等于1</param>
        /// <param name="sum">总数，大于等于目标计数</param>
        /// <param name="sumArray">将切分结果求和积分结果，最后一个数值就是总数</param>
        /// <returns>将总数按照计数切分的结果</returns>
        public static int[] SpreadOutLaying(int length, int sum, out int[] sumArray)
        {

            if (length < 1 || length > sum)
            {
                throw new ArgumentException("目标计数，大于等于1;总数，大于等于目标计数");
            }

            sumArray = new int[length];
            var resDiv = new int[length];
            var baseInterval = sum / length;
            var residue = sum - (baseInterval * length);
            for (int i = 0; i < length; i++)
            {
                int interval = baseInterval;
                if (length - i <= residue)
                {
                    interval++;
                }
                resDiv[i] = interval;
            }

            for (var i = 0; i < sumArray.Length; i++)
            {
                for (var j = 0; j <= i; j++)
                {
                    sumArray[i] += resDiv[j];
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

        public static ConnectionMeshType GetRelationNoConnection(HardwareType SrcGenre)
        {
            return GetRelationBetweenGenre(SrcGenre, null);
        }

        public static ConnectionMeshType GetRelationBetweenGenre(HardwareType SrcGenre, HardwareType? OtherGenre)
        {
            switch (SrcGenre)
            {
                case HardwareType.Core:
                    return ConnectionMeshType.NoChange;
                case HardwareType.Field:
                    if (OtherGenre == null)
                    {
                        return ConnectionMeshType.NoConnectionMesh;
                    }
                    else
                    {
                        if (OtherGenre == HardwareType.Core)
                        {
                            return ConnectionMeshType.DtSConnectedMesh;
                        }

                        if (OtherGenre == HardwareType.Field)
                        {
                            return ConnectionMeshType.DtoDConnectedMesh;
                        }
                    }

                    break;
                case HardwareType.Support:
                    return ConnectionMeshType.NoChange;
                case HardwareType.Other:
                    return ConnectionMeshType.NoChange;
                default:
                    throw new ArgumentOutOfRangeException(nameof(SrcGenre), SrcGenre, null);
            }

            return ConnectionMeshType.NoChange;
        }

        private static PixelateCirclePatternLib PixelateCirclePatternLibCache = null;

        public static PixelateCirclePattern GetPixelateCircle_Tier(int tier)
        {
            if (PixelateCirclePatternLibCache == null)
            {
                PixelateCirclePatternLibCache = Resources.Load<PixelateCirclePatternLib>("PixelateCircleLib/DefaultPixelateCirclePatternLib");
            }
            if (tier >= PixelateCirclePatternLibCache.Lib.Count)
            {
                Debug.LogWarning("PixelateCircle Tier Maxed-out");
                tier = PixelateCirclePatternLibCache.Lib.Count - 1;
            }
            return PixelateCirclePatternLibCache.Lib[tier];
        }

        public struct Matrix2x2
        {
            internal float A00;
            internal float A01;
            internal float A10;
            internal float A11;

            public Matrix2x2(float _A00, float _A01, float _A10, float _A11)
            {
                A00 = _A00;
                A01 = _A01;
                A10 = _A10;
                A11 = _A11;
            }

            public Matrix2x2(float[] content = null)
            {
                if (content == null)
                {
                    A00 = 1;
                    A01 = 0;
                    A10 = 0;
                    A11 = 1;
                }
                else
                {
                    A00 = content[0];
                    A01 = content[1];
                    A10 = content[2];
                    A11 = content[3];
                }
            }

            public static Matrix2x2 operator +(Matrix2x2 a, Matrix2x2 b)
            {
                var content =
                new[]{
                    b.A00+a.A00, b.A01+a.A01, b.A10+a.A10, b.A11+a.A11
                };
                return new Matrix2x2(content);
            }

            public static Matrix2x2 operator -(Matrix2x2 a, Matrix2x2 b)
            {
                var content =
                    new[]{
                        a.A00-b.A00, a.A01-b.A01, a.A10-b.A10, a.A11-b.A11
                    };
                return new Matrix2x2(content);
            }

            public static Matrix2x2 operator *(Matrix2x2 a, Matrix2x2 b)
            {
                Vector2 rowA0 = new Vector2(a.A00, a.A01);
                Vector2 rowA1 = new Vector2(a.A10, a.A11);
                Vector2 rowB0 = new Vector2(b.A00, b.A01);
                Vector2 rowB1 = new Vector2(b.A10, b.A11);

                var content =
                    new[]{
                        Vector2.Dot(rowA0,rowB0),
                        Vector2.Dot(rowA0,rowB1),
                        Vector2.Dot(rowA1,rowB0),
                        Vector2.Dot(rowA1,rowB1),
                    };
                return new Matrix2x2(content);
            }

            public static Vector2 operator *(Matrix2x2 a, Vector2 b)
            {
                Vector2 rowA0 = new Vector2(a.A00, a.A01);
                Vector2 rowA1 = new Vector2(a.A10, a.A11);

                return new Vector2(
                    Vector2.Dot(rowA0, b),
                    Vector2.Dot(rowA1, b));
            }

            public static Vector2 operator *(Vector2 a, Matrix2x2 b)
            {
                return b * a;
            }
        }

        public static Vector2Int V2toV2Int(Vector2 a)
        {
            return new Vector2Int(Mathf.RoundToInt(a.x), Mathf.RoundToInt(a.y));
        }

        public static Vector2Int PermutateV2I(Vector2Int inVec, int maxLength, PatternPermutation permutation)
        {
            Debug.Assert(maxLength >= 1);
            if (maxLength == 1)
            {
                return inVec;
            }
            Vector2 center = new Vector2(maxLength / 2.0f, maxLength / 2.0f);
            Vector2 normalizedIn = new Vector2(inVec.x, inVec.y) - center;
            Matrix2x2 rhs = new Matrix2x2();
            switch (permutation)
            {
                case PatternPermutation.None:
                    return inVec;
                case PatternPermutation.RotateR:
                    rhs = new Matrix2x2(0, 1, -1, 0);
                    break;
                case PatternPermutation.RotateL:
                    rhs = new Matrix2x2(0, -1, 1, 0);
                    break;
                case PatternPermutation.RotateH:
                    rhs = new Matrix2x2(-1, 0, 0, -1);
                    break;
                case PatternPermutation.FlipX:
                    rhs = new Matrix2x2(1, 0, 0, -1);
                    break;
                case PatternPermutation.FlipY:
                    rhs = new Matrix2x2(-1, 0, 0, 1);
                    break;
                case PatternPermutation.FlipXY:
                    rhs = new Matrix2x2(0, 1, 1, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(permutation), permutation, null);
            }

            return V2toV2Int(normalizedIn * rhs + center);
        }
    }
}
