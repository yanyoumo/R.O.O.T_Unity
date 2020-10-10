using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    [CreateAssetMenu(fileName = "NewHeatSinkDiminishing", menuName = "HeatSink/New HeatSink Diminishing")]
    [Serializable]
    public class HeatSinkDiminishing : ScriptableObject
    {
        public List<Vector2Int> Lib;
        public int Count => Lib.Count;
    }
}