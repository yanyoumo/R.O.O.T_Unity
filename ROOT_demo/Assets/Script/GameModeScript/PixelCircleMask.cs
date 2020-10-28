using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace ROOT
{
    [Serializable]
    [CreateAssetMenu(fileName = "NewPixcelCircleMask", menuName = "ActionAsset/New PixcelCircleMask")]
    public class PixcelCircleMask : ScriptableObject
    {
        private int[][] mask =
        {
            new []{0},
            new []{7, 5, 7},
            new []{0xe, 0x1b, 0x11, 0x1b, 0xe},
            new []{0x1c, 0x36, 0x63, 0x41, 0x63, 0x36, 0x1c},
            new []{0x7c, 0xc6, 0x183, 0x101, 0x101, 0x101, 0x183, 0xc6, 0x7c},
            new []{0xf8, 0x18c, 0x306, 0x603, 0x401, 0x401, 0x401, 0x603, 0x306, 0x18c, 0xf8},
            new []{0x1f0, 0x71c, 0xc06, 0x401, 0x1803, 0x1001, 0x1001, 0x1001, 0x1803, 0x401, 0xc06, 0x71c, 0x1f0}
        };
        PixcelCircleMask()
        {
        }

        public int[][] GenerateMask(in int radius)
        {
            Debug.Assert(radius <= 6);
            var len = radius * 2 + 1;
            var res = new int[len][];
            for (var i = 0; i < len; ++i)
            {
                res[i] = new int[len];
                for (var j = 0; j < len; ++j)
                {
                    res[i][j] = mask[radius][i] & (1 << j);
                }
            }
            return res;
        }
    }
}
