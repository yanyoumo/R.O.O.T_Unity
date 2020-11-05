using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
    [CreateAssetMenu(fileName = "NewPixelateCirclePattern", menuName = "PixelateCircle/New PixelateCircle Pattern")]
    [Serializable]
    public class PixelateCirclePattern : ScriptableObject
    {
        [ReadOnly]
        public List<Vector2Int> PatternList;

        public int CircleRadius=1;
        public int TierLevel=0;

        public int CircleDiameter => CircleRadius * 2 + 1;

        [ShowInInspector]
        [TableMatrix]
        public bool[,] Order;

        private void CreateNullMatrix()
        {
            Order = new bool[CircleDiameter, CircleDiameter];
            for (var i = 0; i < CircleDiameter; i++)
            {
                for (var j = 0; j < CircleDiameter; j++)
                {
                    Order[i, j] = false;
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
            for (var i = 0; i < PatternList.Count; i++)
            {
                var tmp = PatternList[i];
                Order[tmp.x, tmp.y] = true;
            }
        }

        [Button(Name = "Save Mat To List")]
        public void SaveMat()
        {
            var unrollMat = new List<Vector2Int>();
            for (var i = 0; i < CircleDiameter; i++)
            {
                for (var j = 0; j < CircleDiameter; j++)
                {
                    if (Order[i, j])
                    {
                        unrollMat.Add(new Vector2Int(i, j));
                    }
                }
            }

            PatternList = unrollMat;
        }
    }
}