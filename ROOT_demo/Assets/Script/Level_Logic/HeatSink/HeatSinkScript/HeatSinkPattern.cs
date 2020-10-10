using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
    [CreateAssetMenu(fileName = "NewHeatSinkPattern", menuName = "HeatSink/New HeatSink Pattern")]
    [Serializable]
    public class HeatSinkPattern : ScriptableObject
    {
        public List<Vector2Int> Lib;
        public int Count => Lib.Count;

        [ShowInInspector]
        [ReadOnly]
        [TableMatrix(SquareCells = true)]
        public bool[,] Pattern;

        [Button(Name = "Invert Pattern")]
        public void InvertList()
        {
            var newLib=new List<Vector2Int>();
            for (int i = 0; i < Board.BoardLength; i++)
            {
                for (int j = 0; j < Board.BoardLength; j++)
                {
                    var tmp = new Vector2Int(i, j);
                    if (!Lib.Contains(tmp))
                    {
                        newLib.Add(tmp);
                    }
                }
            }

            Lib = newLib;
            UpdatePattern();
        }

        [Button(Name = "Update Pattern")]
        public void UpdatePattern()
        {
            Pattern=new bool[Board.BoardLength, Board.BoardLength];
            foreach (var vector2Int in Lib)
            {
                Pattern[vector2Int.x, vector2Int.y] = true;
            }
        }
    }
}