using System;
using System.Collections.Generic;
using System.Linq;
using ROOT.Consts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
    public enum PatternPermutation
    {
        None,
        RotateL,
        RotateR,
        RotateH,
        FlipX,
        FlipY,
        FlipXY,
    }
    
    [CreateAssetMenu(fileName = "NewHeatSinkDiminishing", menuName = "HeatSink/New HeatSink Diminishing")]
    [Serializable]
    public class HeatSinkDiminishing : ScriptableObject
    {
        private static int BoardLength=> StaticNumericData.BoardLength;

        [ReadOnly]
        public List<Vector2Int> DiminishingList;

        [Range(0,35)]
        public int CutOffCount = 30;//这个的设计是比这个数据大都返回-1，但是现在没有特别好的不删掉原始数据的方法。

        [ShowInInspector]
        [TableMatrix]
        public int[,] Order;

        private void CreateNullMatrix()
        {
            Order = new int[BoardLength, BoardLength];
            for (var i = 0; i < BoardLength; i++)
            {
                for (var j = 0; j < BoardLength; j++)
                {
                    Order[i, j] = -1;
                }
            }
        }

        [Button(Name = "Create New Pattern")]
        public void CreateMatrix()
        {
            CreateNullMatrix();
        }

        private Comparison<Tuple<int, Vector2Int>> unrollMat()
        {
            return null;
        }

        [Button(Name = "Load List To Mat")]
        public void LoadMat()
        {
            CreateNullMatrix();
            for (var i = 0; i < DiminishingList.Count; i++)
            {
                var tmp = DiminishingList[i];
                Order[tmp.x, tmp.y] = i;
            }
        }

        [Button(Name = "Save Mat To List")]
        public void SaveMat()
        {
            var unrollMat=new List<Tuple<int, Vector2Int>>();
            for (var i = 0; i < BoardLength; i++)
            {
                for (var j = 0; j < BoardLength; j++)
                {
                    if (Order[i, j] != -1)
                    {
                        unrollMat.Add(new Tuple<int, Vector2Int>(Order[i, j],new Vector2Int(i,j)));
                    }
                }
            }
            unrollMat.Sort((x, y) => x.Item1.CompareTo(y.Item1));
            DiminishingList = unrollMat.Select(tmp => tmp.Item2).ToList();
        }
    }
}