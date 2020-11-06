using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT
{
    [CreateAssetMenu(fileName = "NewHeatSinkDiminishing", menuName = "HeatSink/New HeatSink Diminishing")]
    [Serializable]
    public class HeatSinkDiminishing : ScriptableObject
    {
        [ReadOnly]
        public List<Vector2Int> DiminishingList;

        [Range(0,35)]
        public int CutOffCount = 30;

        [ShowInInspector]
        [TableMatrix]
        public int[,] Order;

        private void CreateNullMatrix()
        {
            Order = new int[Board.BoardLength, Board.BoardLength];
            for (var i = 0; i < Board.BoardLength; i++)
            {
                for (var j = 0; j < Board.BoardLength; j++)
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
            for (var i = 0; i < Board.BoardLength; i++)
            {
                for (var j = 0; j < Board.BoardLength; j++)
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