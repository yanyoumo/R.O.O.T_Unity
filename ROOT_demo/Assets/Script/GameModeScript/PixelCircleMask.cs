using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace ROOT
{
    public static class PixelCircleMask
    {
        private static int[][] mask =
        {
            new []{0},
            new []{2, 7, 2},
            new []{0x0, 0xE, 0xE, 0xE, 0x0},
            //new []{0x1c, 0x3E, 0x7F, 0x7F, 0x7F, 0x3E, 0x1c},
            //new []{0x7c, 0xFE, 0x1FF, 0x1FF, 0x1FF, 0x1FF, 0x1FF, 0xFE, 0x7c},
            //new []{0xf8, 0x18c, 0x306, 0x603, 0x401, 0x401, 0x401, 0x603, 0x306, 0x18c, 0xf8},
            //new []{0x1f0, 0x71c, 0xc06, 0x401, 0x1803, 0x1001, 0x1001, 0x1001, 0x1803, 0x401, 0xc06, 0x71c, 0x1f0}
        };
        public static int[][] GenerateMask(in int radius)
        {
            Debug.Assert(radius <3);
            var len = radius * 2 + 1;
            var res = new int[len][];
            for (var i = 0; i < len; ++i)
            {
                res[i] = new int[len];
                for (var j = 0; j < len; ++j)
                {
                    res[i][j] = mask[radius][i] >> j & 1;
                }
            }
            return res;
        }
    }
}
