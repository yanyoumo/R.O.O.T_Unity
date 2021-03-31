using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ROOT;

namespace Tests
{
    public class V2IPermutationTestScript
    {
        [Test]
        public void V2IPermutationTestScriptSimplePasses()
        {
            var sets = new List<Tuple<Vector2Int, int, PatternPermutation, Vector2Int>>
            {
                new Tuple<Vector2Int, int, PatternPermutation, Vector2Int>(new Vector2Int(0, 0), 1, PatternPermutation.None,new Vector2Int(0,0)),
                new Tuple<Vector2Int, int, PatternPermutation, Vector2Int>(new Vector2Int(1, 2), 4, PatternPermutation.None,new Vector2Int(1,2)),
                new Tuple<Vector2Int, int, PatternPermutation, Vector2Int>(new Vector2Int(3, 5), 7, PatternPermutation.FlipX,new Vector2Int(3,2)),
                new Tuple<Vector2Int, int, PatternPermutation, Vector2Int>(new Vector2Int(3, 4), 8, PatternPermutation.FlipY,new Vector2Int(5,4)),
                new Tuple<Vector2Int, int, PatternPermutation, Vector2Int>(new Vector2Int(1, 4), 6, PatternPermutation.FlipXY,new Vector2Int(4,1)),
                new Tuple<Vector2Int, int, PatternPermutation, Vector2Int>(new Vector2Int(1, 5), 7, PatternPermutation.RotateL,new Vector2Int(2,1)),
                new Tuple<Vector2Int, int, PatternPermutation, Vector2Int>(new Vector2Int(9, 1), 6, PatternPermutation.RotateR,new Vector2Int(1,-3)),
                new Tuple<Vector2Int, int, PatternPermutation, Vector2Int>(new Vector2Int(1, -4), 6, PatternPermutation.RotateH,new Vector2Int(5,10)),
                new Tuple<Vector2Int, int, PatternPermutation, Vector2Int>(new Vector2Int(-4, 6), 11, PatternPermutation.FlipY,new Vector2Int(15,6)),
                new Tuple<Vector2Int, int, PatternPermutation, Vector2Int>(new Vector2Int(3, -5), 8, PatternPermutation.RotateR,new Vector2Int(-5,5)),
                new Tuple<Vector2Int, int, PatternPermutation, Vector2Int>(new Vector2Int(-4, 0), 6, PatternPermutation.FlipX,new Vector2Int(-4,6)),
                new Tuple<Vector2Int, int, PatternPermutation, Vector2Int>(new Vector2Int(4, -3), 10, PatternPermutation.FlipXY,new Vector2Int(-3,4)),
                new Tuple<Vector2Int, int, PatternPermutation, Vector2Int>(new Vector2Int(0, 4), 5, PatternPermutation.RotateL,new Vector2Int(1,0)),
                new Tuple<Vector2Int, int, PatternPermutation, Vector2Int>(new Vector2Int(4, -2), 5, PatternPermutation.RotateH,new Vector2Int(1,7)),
            };

            foreach (var (item1, item2, item3, item4) in sets)
            {
                var res = Utils.PermutateV2I(item1, item2, item3);
                Debug.Log(item1 + ":" + res);
                Assert.AreEqual(res, item4);
            }
        }
    }
}
